using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Create(Usuario usuario)
        {
            var usuarioCriado = await _service.CreateAsync(usuario);

            return CreatedAtAction(
                nameof(GetById),
                new { id = usuarioCriado.Id },
                usuarioCriado
            );
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _service.GetByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.GetAllAsync();

            return Ok(usuarios);
        }

        [HttpPut("id")]
        public async Task<IActionResult> Update(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest("Id invalido ou nao encontrado");
            }

            var atualizado = await _service.UpdateAsync(usuario);

            if (!atualizado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> Delete(int id)
        {
            var deletado = await _service.DeleteAsync(id);

            if (!deletado)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
