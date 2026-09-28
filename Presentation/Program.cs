using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// No ambiente de teste de carga os segredos locais (connection string, chave JWT)
// também precisam ser lidos — por padrão o .NET só lê User Secrets em Development.
if (builder.Environment.IsEnvironment("LoadTest"))
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Versão fixa do MySQL: evita abrir uma conexão só para detectar a versão ao iniciar a API.
var versaoMySql = ServerVersion.Parse(
    builder.Configuration["Database:ServerVersion"] ?? "8.0.36-mysql");

// DbContextPool reaproveita instâncias do contexto entre requisições (menos alocação sob carga).
builder.Services.AddDbContextPool<AppDbContext>(
    options => options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        versaoMySql,
        mySql => mySql.EnableRetryOnFailure(maxRetryCount: 3)),
    poolSize: 256);

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<
    IRegistroGlicemiaRepository,
    RegistroGlicemiaRepository>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<
    IRegistroGlicemiaService,
    RegistroGlicemiaService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IUsuarioAtivoCache, UsuarioAtivoCache>();

// Confirmação de e-mail: com chave do Brevo envia e-mail de verdade;
// sem chave (desenvolvimento), mostra o código no console da API.
builder.Services.AddSingleton(
    builder.Configuration.GetSection("ConfirmacaoEmail").Get<ConfiguracaoConfirmacaoEmail>()
    ?? new ConfiguracaoConfirmacaoEmail());

if (string.IsNullOrWhiteSpace(builder.Configuration["Email:BrevoApiKey"]))
{
    builder.Services.AddSingleton<IEmailService, ConsoleEmailService>();
}
else
{
    builder.Services.AddSingleton<IEmailService, BrevoEmailService>();
}

builder.Services.AddScoped<
    IHistoricoPdfService,
    HistoricoPdfService>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Digite o token JWT."
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

// Autenticação JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    )
            };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var usuarioIdClaim =
                    context.Principal?
                        .FindFirst(
                            ClaimTypes.NameIdentifier
                        );

                if (usuarioIdClaim == null)
                {
                    context.Fail(
                        "Usuário inválido."
                    );

                    return;
                }

                if (!int.TryParse(
                        usuarioIdClaim.Value,
                        out var usuarioId))
                {
                    context.Fail(
                        "Usuário inválido."
                    );

                    return;
                }

                var usuarioAtivoCache =
                    context.HttpContext
                        .RequestServices
                        .GetRequiredService<
                            IUsuarioAtivoCache>();

                var usuarioAtivo =
                    await usuarioAtivoCache
                        .UsuarioAtivoAsync(
                            usuarioId
                        );

                if (!usuarioAtivo)
                {
                    context.Fail(
                        "Usuário inativo."
                    );
                }
            }
        };
    });


builder.Services.AddAuthorization();

// Limites configuráveis: o ambiente LoadTest (appsettings.LoadTest.json) usa valores altos
// para medir a API e o banco, e não o limitador.
var limiteGeral = builder.Configuration.GetValue("RateLimiting:Geral:PermitLimit", 60);
var limiteLogin = builder.Configuration.GetValue("RateLimiting:Login:PermitLimit", 5);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "fixed",
        httpContext =>
        {
            var chave =
                httpContext.User.Identity?
                    .IsAuthenticated == true
                    ? httpContext.User
                        .FindFirst(
                            ClaimTypes.NameIdentifier
                        )?.Value
                    : httpContext.Connection
                        .RemoteIpAddress?
                        .ToString();

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey:
                        chave ?? "desconhecido",

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limiteGeral,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0,

                            AutoReplenishment = true
                        });
        });

    options.AddPolicy(
        "login",
        httpContext =>
        {
            var ip =
                httpContext.Connection
                    .RemoteIpAddress?
                    .ToString()
                ?? "desconhecido";

            return RateLimitPartition
                .GetFixedWindowLimiter(
                    partitionKey: ip,

                    factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = limiteLogin,

                            Window =
                                TimeSpan.FromMinutes(1),

                            QueueLimit = 0,

                            AutoReplenishment = true
                        });
        });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Na hospedagem (Render etc.) a API fica atrás de um proxy: o IP real do usuário
// chega no cabeçalho X-Forwarded-For. Sem isso, o limite de login valeria para
// todos os usuários juntos (todos pareceriam ter o IP do proxy).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

// Cria/atualiza as tabelas automaticamente ao iniciar, se configurado
// (variável de ambiente Database__AplicarMigracoesAoIniciar=true na hospedagem).
if (app.Configuration.GetValue("Database:AplicarMigracoesAoIniciar", false))
{
    using var escopo = app.Services.CreateScope();
    escopo.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

// Rota simples para verificar se a API está no ar (usada pelo Render e para "acordar" o servidor)
app.MapGet("/api/saude", () => Results.Ok(new { status = "ok" }));

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseRouting();

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }