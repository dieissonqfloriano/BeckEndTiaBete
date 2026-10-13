using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService? _emailService;
        private readonly ConfiguracaoConfirmacaoEmail _confirmacao;

        public UsuarioService(
            IUsuarioRepository repository,
            ITokenService tokenService,
            IEmailService? emailService = null,
            ConfiguracaoConfirmacaoEmail? confirmacao = null)
        {
            _repository = repository;
            _tokenService = tokenService;
            _emailService = emailService;
            _confirmacao = confirmacao ?? new ConfiguracaoConfirmacaoEmail();
        }

        public async Task<(ResultadoCriacaoUsuario Resultado, UsuarioOutputDto? Usuario)> CreateAsync(UsuarioCreateDto dto)
        {
            var email = Usuario.NormalizarEmail(dto.Email);

            if (await _repository.GetByEmailAsync(email) != null)
            {
                return (ResultadoCriacaoUsuario.EmailJaExiste, null);
            }

            TipoDiabetesConversor.TentarConverter(dto.TipoDiabetes, out var tipoDiabetes);

            var usuario = new Usuario
            {
                Name = dto.Name.Trim(),
                Email = email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                TipoDiabetes = tipoDiabetes,
                Idade = dto.Idade,
                Celular = LimparTexto(dto.Celular),
                FatorSensibilidade = dto.FatorSensibilidade,
                HgtAlvo = dto.HgtAlvo,
                Role = "Usuario",
                EmailConfirmado = !_confirmacao.Exigir,
                TermosAceitosEm = DateTime.UtcNow,
                VersaoTermos = Usuario.VersaoTermosAtual,
                ConsentimentoSaudeEm = DateTime.UtcNow
            };

            if (dto.Idade is < Usuario.MaioridadeLegal)
            {
                usuario.ResponsavelNome = dto.ResponsavelNome?.Trim();
                usuario.ConsentimentoResponsavelEm = DateTime.UtcNow;
            }

            var codigo = _confirmacao.Exigir ? GerarNovoCodigo(usuario) : null;

            await _repository.AddAsync(usuario);

            try
            {
                await _repository.SaveChangesAsync();
            }
            catch (EmailJaExisteException)
            {
                // Dois cadastros simultâneos com o mesmo e-mail: o índice único do banco barra o segundo.
                return (ResultadoCriacaoUsuario.EmailJaExiste, null);
            }

            if (codigo != null)
            {
                await EnviarCodigoAsync(usuario, codigo);
            }

            return (ResultadoCriacaoUsuario.Sucesso, MapearParaOutput(usuario));
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var usuario = await _repository.GetByEmailAsync(Usuario.NormalizarEmail(dto.Email));

            if (usuario == null || !usuario.Ativo)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            {
                return null;
            }

            // Só avisa sobre o e-mail depois da senha correta (não revela se a conta existe).
            if (_confirmacao.Exigir && !usuario.EmailConfirmado)
            {
                throw new EmailNaoConfirmadoException();
            }

            return new LoginResponseDto
            {
                Token = _tokenService.GenerateToken(usuario),
                Usuario = MapearParaOutput(usuario)
            };
        }

        public Task<bool> DeleteAsync(int id) => _repository.DesativarAsync(id);

        public async Task<bool> ReativarContaAsync(ReativarContaDto dto)
        {
            var usuario = await _repository.GetByEmailAsync(Usuario.NormalizarEmail(dto.Email));

            if (usuario == null || usuario.Ativo)
            {
                return false;
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            {
                return false;
            }

            usuario.Ativo = true;
            usuario.DesativadoEm = null;

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<List<UsuarioOutputDto>> GetAllAsync()
        {
            var usuarios = await _repository.GetAllAsync();

            return usuarios.Select(MapearParaOutput).ToList();
        }

        public async Task<UsuarioOutputDto?> GetByIdAsync(int id)
        {
            var usuario = await _repository.GetByIdSomenteLeituraAsync(id);

            return usuario == null ? null : MapearParaOutput(usuario);
        }

        public async Task<ResultadoPatchUsuario> PatchAsync(int id, UsuarioPatchDto dto)
        {
            var usuario = await _repository.GetByIdAsync(id);

            if (usuario == null)
            {
                return ResultadoPatchUsuario.UsuarioNaoEncontrado;
            }

            if (dto.Name != null)
            {
                usuario.Name = dto.Name.Trim();
            }

            if (dto.Email != null)
            {
                var email = Usuario.NormalizarEmail(dto.Email);
                var outro = await _repository.GetByEmailAsync(email);

                if (outro != null && outro.Id != usuario.Id)
                {
                    return ResultadoPatchUsuario.EmailJaExiste;
                }

                usuario.Email = email;
            }

            if (dto.TipoDiabetes != null &&
                TipoDiabetesConversor.TentarConverter(dto.TipoDiabetes, out var tipo))
            {
                usuario.TipoDiabetes = tipo;
            }

            if (dto.Idade != null)
            {
                usuario.Idade = dto.Idade;
            }

            if (dto.Celular != null)
            {
                usuario.Celular = LimparTexto(dto.Celular);
            }

            if (dto.FatorSensibilidade != null)
            {
                usuario.FatorSensibilidade = dto.FatorSensibilidade.Value;
            }

            if (dto.HgtAlvo != null)
            {
                usuario.HgtAlvo = dto.HgtAlvo.Value;
            }

            try
            {
                await _repository.SaveChangesAsync();
            }
            catch (EmailJaExisteException)
            {
                return ResultadoPatchUsuario.EmailJaExiste;
            }

            return ResultadoPatchUsuario.Sucesso;
        }

        public Task<bool> UsuarioAtivoAsync(int id) => _repository.ExisteAtivoAsync(id);

        public async Task<ResultadoConfirmacaoEmail> ConfirmarEmailAsync(ConfirmarEmailDto dto)
        {
            var email = Usuario.NormalizarEmail(dto.Email);
            var usuario = await _repository.GetByEmailAsync(email);

            if (usuario == null)
            {
                return ResultadoConfirmacaoEmail.CodigoInvalido;
            }

            if (usuario.EmailConfirmado)
            {
                return ResultadoConfirmacaoEmail.JaConfirmado;
            }

            if (usuario.TentativasCodigo >= _confirmacao.MaximoTentativas)
            {
                return ResultadoConfirmacaoEmail.MuitasTentativas;
            }

            if (usuario.CodigoConfirmacaoHash == null ||
                usuario.CodigoConfirmacaoExpiraEm == null ||
                usuario.CodigoConfirmacaoExpiraEm < DateTime.UtcNow)
            {
                return ResultadoConfirmacaoEmail.CodigoExpirado;
            }

            var esperado = Convert.FromHexString(usuario.CodigoConfirmacaoHash);
            var informado = Convert.FromHexString(HashCodigo(email, dto.Codigo.Trim()));

            if (!CryptographicOperations.FixedTimeEquals(esperado, informado))
            {
                usuario.TentativasCodigo++;
                await _repository.SaveChangesAsync();
                return usuario.TentativasCodigo >= _confirmacao.MaximoTentativas
                    ? ResultadoConfirmacaoEmail.MuitasTentativas
                    : ResultadoConfirmacaoEmail.CodigoInvalido;
            }

            usuario.EmailConfirmado = true;
            usuario.CodigoConfirmacaoHash = null;
            usuario.CodigoConfirmacaoExpiraEm = null;
            usuario.TentativasCodigo = 0;
            await _repository.SaveChangesAsync();

            return ResultadoConfirmacaoEmail.Sucesso;
        }

        // Sempre termina "com sucesso" para quem chamou: não revela se o e-mail existe.
        public async Task ReenviarCodigoAsync(ReenviarCodigoDto dto)
        {
            var usuario = await _repository.GetByEmailAsync(Usuario.NormalizarEmail(dto.Email));

            if (usuario == null || usuario.EmailConfirmado || !usuario.Ativo)
            {
                return;
            }

            var codigo = GerarNovoCodigo(usuario);
            await _repository.SaveChangesAsync();
            await EnviarCodigoAsync(usuario, codigo);
        }

        // Exclui a conta e todos os dados, depois de confirmar a senha.
        public async Task<bool> ExcluirDefinitivamenteAsync(int id, ExcluirContaDto dto)
        {
            var usuario = await _repository.GetByIdSomenteLeituraAsync(id);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            {
                return false;
            }

            return await _repository.ExcluirDefinitivamenteAsync(id);
        }

        private string GerarNovoCodigo(Usuario usuario)
        {
            var codigo = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            usuario.CodigoConfirmacaoHash = HashCodigo(usuario.Email, codigo);
            usuario.CodigoConfirmacaoExpiraEm = DateTime.UtcNow.AddMinutes(_confirmacao.ValidadeMinutos);
            usuario.TentativasCodigo = 0;
            return codigo;
        }

        private async Task EnviarCodigoAsync(Usuario usuario, string codigo)
        {
            if (_emailService == null)
            {
                return;
            }

            try
            {
                await _emailService.EnviarCodigoConfirmacaoAsync(usuario.Email, usuario.Name, codigo);
            }
            catch
            {
                // Falha no envio não desfaz o cadastro: a pessoa pode pedir "reenviar código".
            }
        }

        private static string HashCodigo(string email, string codigo) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{email}:{codigo}")));

        private static string? LimparTexto(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static UsuarioOutputDto MapearParaOutput(Usuario usuario) => new()
        {
            Id = usuario.Id,
            Name = usuario.Name,
            Email = usuario.Email,
            TipoDiabetes = TipoDiabetesConversor.ParaTexto(usuario.TipoDiabetes),
            Idade = usuario.Idade,
            Celular = usuario.Celular,
            FatorSensibilidade = usuario.FatorSensibilidade,
            HgtAlvo = usuario.HgtAlvo
        };
    }
}
