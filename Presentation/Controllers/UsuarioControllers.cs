using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    [EnableRateLimiting("fixed")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;
        private readonly IUsuarioAtivoCache _usuarioAtivoCache;

        public UsuarioController(IUsuarioService service, IUsuarioAtivoCache usuarioAtivoCache)
        {
            _service = service;
            _usuarioAtivoCache = usuarioAtivoCache;
        }

        [HttpPost]
        public async Task<IActionResult> Create(UsuarioCreateDto dto)
        {
            var resultado = await _service.CreateAsync(dto);

            if (resultado.Resultado == ResultadoCriacaoUsuario.EmailJaExiste)
            {
                return Conflict(
                    "Já existe um usuário cadastrado com este e-mail.");
            }

            return StatusCode(201, resultado.Usuario);
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            LoginResponseDto? usuario;

            try
            {
                usuario = await _service.LoginAsync(dto);
            }
            catch (EmailNaoConfirmadoException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }

            if (usuario == null)
            {
                return Unauthorized("Email ou senha inválidos.");
            }

            return Ok(usuario);
        }

        [HttpPost("confirmar-email")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ConfirmarEmail(ConfirmarEmailDto dto)
        {
            var resultado = await _service.ConfirmarEmailAsync(dto);

            return resultado switch
            {
                ResultadoConfirmacaoEmail.Sucesso => Ok("E-mail confirmado."),
                ResultadoConfirmacaoEmail.JaConfirmado => Ok("Este e-mail já estava confirmado."),
                ResultadoConfirmacaoEmail.CodigoExpirado => BadRequest("O código expirou. Peça um novo código."),
                ResultadoConfirmacaoEmail.MuitasTentativas => BadRequest("Muitas tentativas erradas. Peça um novo código."),
                _ => BadRequest("Código inválido.")
            };
        }

        [HttpPost("reenviar-codigo")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ReenviarCodigo(ReenviarCodigoDto dto)
        {
            await _service.ReenviarCodigoAsync(dto);
            return Ok("Se o e-mail estiver cadastrado e ainda não confirmado, enviamos um novo código.");
        }

        [HttpPost("reativar")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ReativarConta(ReativarContaDto dto)
        {
            var reativado = await _service.ReativarContaAsync(dto);

            if (!reativado)
            {
                return BadRequest("Não foi possível reativar a conta.");
            }

            return Ok("Conta reativada com sucesso.");
        }

        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> GetPerfil()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var usuario = await _service.GetByIdAsync(usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [Authorize]
        [HttpPatch("perfil")]
        public async Task<IActionResult> PatchPerfil(UsuarioPatchDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var resultado = await _service.PatchAsync(usuarioId, dto);

            if (resultado == ResultadoPatchUsuario.UsuarioNaoEncontrado)
            {
                return NotFound();
            }

            if (resultado == ResultadoPatchUsuario.EmailJaExiste)
            {
                return Conflict("Já existe um usuário cadastrado com este e-mail.");
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("perfil")]
        public async Task<IActionResult> DeletePerfil()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var deletado = await _service.DeleteAsync(usuarioId);

            if (!deletado)
            {
                return NotFound();
            }

            // Tokens já emitidos param de valer imediatamente.
            _usuarioAtivoCache.Invalidar(usuarioId);

            return NoContent();
        }

        /// <summary>
        /// Exclui definitivamente a conta e todos os registros (LGPD art. 18, VI).
        /// Exige a senha para evitar exclusão acidental ou por quem pegou o celular desbloqueado.
        /// </summary>
        [Authorize]
        [HttpPost("excluir-conta")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ExcluirConta(ExcluirContaDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);
            var excluido = await _service.ExcluirDefinitivamenteAsync(usuarioId, dto);

            if (!excluido)
            {
                return BadRequest("Senha incorreta. A conta não foi excluída.");
            }

            _usuarioAtivoCache.Invalidar(usuarioId);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.GetAllAsync();

            return Ok(usuarios);
        }
    }
}