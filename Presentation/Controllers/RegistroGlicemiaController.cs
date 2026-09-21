using Application.DTOs;
using Application.Interfaces.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/registros-glicemia")]
    public class RegistroGlicemiaController : ControllerBase
    {
        private readonly IRegistroGlicemiaService _service;

        public RegistroGlicemiaController(IRegistroGlicemiaService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegistroGlicemiaCreateDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var registroCriado = await _service.CreateAsync(dto, usuarioId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = registroCriado.Id },
                registroCriado
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var registro = await _service.GetByIdAsync(id, usuarioId);

            if (registro == null)
            {
                return NotFound();
            }

            return Ok(registro);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var registros = await _service.GetAllAsync(usuarioId);

            return Ok(registros);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, RegistroGlicemiaUpdateDto dto)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var atualizado = await _service.UpdateAsync(id, dto, usuarioId);

            if (!atualizado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (usuarioIdClaim == null)
            {
                return Unauthorized();
            }

            var usuarioId = int.Parse(usuarioIdClaim.Value);

            var deletado = await _service.DeleteAsync(id, usuarioId);

            if (!deletado)
            {
                return NotFound();
            }

            return NoContent();
        }

    }
}
