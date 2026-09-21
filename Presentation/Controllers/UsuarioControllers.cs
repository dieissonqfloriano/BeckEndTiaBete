using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/usuario")]
    public class UsuarioControllers : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuarioControllers(IUsuarioService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(UsuarioCreateDto dto)
        {
            var usuarioCriado = await _service.CreateAsync(dto);

            return StatusCode(201, usuarioCriado);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var usuario = await _service.LoginAsync(dto);

            if (usuario == null)
            { 
                return Unauthorized("Email ou Senha invalidos");
            }

            return Ok(usuario);
        }

        [HttpPost("reativar")]
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
        [HttpGet("{perfil}")]
        public async Task<IActionResult> GetPerfil()
        {
            var usuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioClaim.Value);

            var usuario = await _service.GetByIdAsync(usuarioId);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario );
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.GetAllAsync();

            return Ok(usuarios);
        }

        [Authorize]
        [HttpPut("perfil")]
        public async Task<IActionResult> UpdatePerfil(UsuarioUpdateDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var atualizado = await _service.UpdateAsync(usuarioId, dto);

            if (!atualizado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{perfil}")]
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

            return NoContent();

        }
    }
}
