using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace Tests
{
    public class LgpdTests
    {
        private static UsuarioCreateDto Cadastro(int idade) => new()
        {
            Name = "Ana", Email = "ana@teste.com", Senha = "SenhaForte1",
            TipoDiabetes = "Tipo 1", Idade = idade, FatorSensibilidade = 40, HgtAlvo = 100,
            AceitouTermos = true, ConsentiuDadosSaude = true
        };

        private static bool Valido(object dto) =>
            Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true);

        [Fact]
        public void Cadastro_SemAceitarTermos_DeveSerInvalido()
        {
            var dto = Cadastro(30);
            dto.AceitouTermos = false;
            Assert.False(Valido(dto));
        }

        [Fact]
        public void Cadastro_SemConsentimentoDeSaude_DeveSerInvalido()
        {
            var dto = Cadastro(30);
            dto.ConsentiuDadosSaude = false;
            Assert.False(Valido(dto));
        }

        [Fact]
        public void Menor_SemResponsavel_DeveSerInvalido()
        {
            Assert.False(Valido(Cadastro(10)));
        }

        [Fact]
        public void Menor_ComResponsavelAutorizando_DeveSerValido()
        {
            var dto = Cadastro(10);
            dto.ResponsavelNome = "Maria Responsável";
            dto.ConsentimentoResponsavel = true;
            Assert.True(Valido(dto));
        }

        [Fact]
        public async Task Cadastro_DeveRegistrarDataEVersaoDoConsentimento()
        {
            Usuario? salvo = null;
            var repositorio = new Mock<IUsuarioRepository>();
            repositorio.Setup(r => r.AddAsync(It.IsAny<Usuario>()))
                .Callback<Usuario>(u => salvo = u).Returns(Task.CompletedTask);

            var dto = Cadastro(10);
            dto.ResponsavelNome = "Maria Responsável";
            dto.ConsentimentoResponsavel = true;

            await new UsuarioService(repositorio.Object, Mock.Of<ITokenService>()).CreateAsync(dto);

            Assert.NotNull(salvo!.TermosAceitosEm);
            Assert.NotNull(salvo.ConsentimentoSaudeEm);
            Assert.Equal(Usuario.VersaoTermosAtual, salvo.VersaoTermos);
            Assert.Equal("Maria Responsável", salvo.ResponsavelNome);
            Assert.NotNull(salvo.ConsentimentoResponsavelEm);
        }

        [Fact]
        public async Task ExcluirConta_ComSenhaErrada_NaoDeveApagar()
        {
            var repositorio = new Mock<IUsuarioRepository>();
            repositorio.Setup(r => r.GetByIdSomenteLeituraAsync(1)).ReturnsAsync(new Usuario
            {
                Id = 1, SenhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaCerta1")
            });

            var servico = new UsuarioService(repositorio.Object, Mock.Of<ITokenService>());
            var excluiu = await servico.ExcluirDefinitivamenteAsync(1, new ExcluirContaDto { Senha = "errada" });

            Assert.False(excluiu);
            repositorio.Verify(r => r.ExcluirDefinitivamenteAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExcluirConta_ComSenhaCerta_DeveApagarDefinitivamente()
        {
            var repositorio = new Mock<IUsuarioRepository>();
            repositorio.Setup(r => r.GetByIdSomenteLeituraAsync(1)).ReturnsAsync(new Usuario
            {
                Id = 1, SenhaHash = BCrypt.Net.BCrypt.HashPassword("SenhaCerta1")
            });
            repositorio.Setup(r => r.ExcluirDefinitivamenteAsync(1)).ReturnsAsync(true);

            var servico = new UsuarioService(repositorio.Object, Mock.Of<ITokenService>());
            var excluiu = await servico.ExcluirDefinitivamenteAsync(1, new ExcluirContaDto { Senha = "SenhaCerta1" });

            Assert.True(excluiu);
            repositorio.Verify(r => r.ExcluirDefinitivamenteAsync(1), Times.Once);
        }
    }
}
