using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Tests
{
    public class NovoBancoTests
    {
        [Theory]
        [InlineData("☕ Café da Manhã", TipoRefeicao.CafeDaManha)]
        [InlineData("🍛 Almoço", TipoRefeicao.Almoco)]
        [InlineData("Almoço", TipoRefeicao.Almoco)]
        [InlineData("🥪 Lanche", TipoRefeicao.Lanche)]
        [InlineData("Janta", TipoRefeicao.Jantar)]
        [InlineData("CafeDaManha", TipoRefeicao.CafeDaManha)]
        public void Refeicao_DeveSerNormalizada(string texto, TipoRefeicao esperado)
        {
            var ok = TipoRefeicaoConversor.TentarConverter(texto, out var refeicao);

            Assert.True(ok);
            Assert.Equal(esperado, refeicao);
        }

        [Fact]
        public void Refeicao_Invalida_DeveGerarErroDeValidacao()
        {
            var dto = CriarDtoValido();
            dto.Refeicao = "Sobremesa";

            Assert.False(Validar(dto));
        }

        [Fact]
        public void Dose_ComMeiaUnidade_DeveSerValida()
        {
            var dto = CriarDtoValido();
            dto.Dose = 4.5m;

            Assert.True(Validar(dto));
        }

        [Fact]
        public void Dose_ComDuasCasasDecimais_DeveSerInvalida()
        {
            var dto = CriarDtoValido();
            dto.Dose = 4.25m;

            Assert.False(Validar(dto));
        }

        [Fact]
        public async Task Cadastro_DeveSalvarEmailNormalizado_ETipoDiabetesComoEnum()
        {
            var repositorio = new Mock<IUsuarioRepository>();
            Usuario? salvo = null;

            repositorio
                .Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => salvo = u)
                .Returns(Task.CompletedTask);

            var service = new UsuarioService(repositorio.Object, Mock.Of<ITokenService>());

            var (resultado, usuario) = await service.CreateAsync(new UsuarioCreateDto
            {
                Name = " Maria ",
                Email = "  Maria@Teste.COM ",
                Senha = "SenhaForte123",
                TipoDiabetes = "Tipo 1",
                FatorSensibilidade = 45.5m,
                HgtAlvo = 110
            });

            Assert.Equal(ResultadoCriacaoUsuario.Sucesso, resultado);
            Assert.NotNull(salvo);
            Assert.Equal("maria@teste.com", salvo!.Email);
            Assert.Equal("Maria", salvo.Name);
            Assert.Equal(TipoDiabetes.Tipo1, salvo.TipoDiabetes);
            Assert.NotEqual("SenhaForte123", salvo.SenhaHash);
            Assert.Equal("Tipo 1", usuario!.TipoDiabetes);
        }

        [Fact]
        public async Task CacheUsuarioAtivo_DeveConsultarBancoUmaVez_EVoltarAConsultarAposInvalidar()
        {
            var usuarioService = new Mock<IUsuarioService>();
            usuarioService.Setup(s => s.UsuarioAtivoAsync(7)).ReturnsAsync(true);

            using var memoria = new MemoryCache(new MemoryCacheOptions());
            var cache = new UsuarioAtivoCache(memoria, usuarioService.Object);

            Assert.True(await cache.UsuarioAtivoAsync(7));
            Assert.True(await cache.UsuarioAtivoAsync(7));
            usuarioService.Verify(s => s.UsuarioAtivoAsync(7), Times.Once);

            cache.Invalidar(7);
            usuarioService.Setup(s => s.UsuarioAtivoAsync(7)).ReturnsAsync(false);

            Assert.False(await cache.UsuarioAtivoAsync(7));
            usuarioService.Verify(s => s.UsuarioAtivoAsync(7), Times.Exactly(2));
        }

        private static RegistroGlicemiaCreateDto CriarDtoValido() => new()
        {
            Glicemia = 120,
            GlicemiaAcimaDoLimite = false,
            Dose = 5,
            Hora = new TimeSpan(12, 0, 0),
            Refeicao = "Almoço",
            Data = DateOnly.FromDateTime(DateTime.Today)
        };

        private static bool Validar(object dto) =>
            Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true);
    }
}
