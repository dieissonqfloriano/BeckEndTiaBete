using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Moq;

namespace Tests
{
    public class ConfirmacaoEmailTests
    {
        private sealed class EmailFalso : IEmailService
        {
            public string? Codigo { get; private set; }
            public Task EnviarCodigoConfirmacaoAsync(string emailDestino, string nome, string codigo)
            {
                Codigo = codigo;
                return Task.CompletedTask;
            }
        }

        private static (UsuarioService Servico, EmailFalso Email, Func<Usuario?> Salvo) Criar()
        {
            Usuario? salvo = null;
            var repositorio = new Mock<IUsuarioRepository>();
            repositorio.Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => salvo = u).Returns(Task.CompletedTask);
            repositorio.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((string e) => salvo != null && salvo.Email == e ? salvo : null);

            var email = new EmailFalso();
            var servico = new UsuarioService(repositorio.Object, Mock.Of<ITokenService>(), email,
                new ConfiguracaoConfirmacaoEmail { Exigir = true });

            return (servico, email, () => salvo);
        }

        private static UsuarioCreateDto NovoUsuario() => new()
        {
            Name = "Ana", Email = "ana@teste.com", Senha = "SenhaForte1",
            TipoDiabetes = "Tipo 1", Idade = 30, FatorSensibilidade = 40, HgtAlvo = 100
        };

        [Fact]
        public async Task Login_DeveSerBloqueado_AteConfirmarEmail()
        {
            var (servico, email, _) = Criar();
            await servico.CreateAsync(NovoUsuario());

            await Assert.ThrowsAsync<EmailNaoConfirmadoException>(() =>
                servico.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "SenhaForte1" }));

            var resultado = await servico.ConfirmarEmailAsync(
                new ConfirmarEmailDto { Email = "ana@teste.com", Codigo = email.Codigo! });

            Assert.Equal(ResultadoConfirmacaoEmail.Sucesso, resultado);
            Assert.NotNull(await servico.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "SenhaForte1" }));
        }

        [Fact]
        public async Task CodigoErrado_DeveSerRecusado_ENaoSalvarCodigoEmTextoPuro()
        {
            var (servico, email, salvo) = Criar();
            await servico.CreateAsync(NovoUsuario());

            var errado = email.Codigo == "000000" ? "111111" : "000000";
            var resultado = await servico.ConfirmarEmailAsync(
                new ConfirmarEmailDto { Email = "ana@teste.com", Codigo = errado });

            Assert.Equal(ResultadoConfirmacaoEmail.CodigoInvalido, resultado);
            Assert.False(salvo()!.EmailConfirmado);
            Assert.NotEqual(email.Codigo, salvo()!.CodigoConfirmacaoHash);
            Assert.Equal(1, salvo()!.TentativasCodigo);
        }
    }
}
