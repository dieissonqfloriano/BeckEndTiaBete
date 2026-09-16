using Application.DTOs;
using Application.Interfaces.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
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
            var registroCriado = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = registroCriado.Id },
                registroCriado
                );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var registros = await _service.GetByIdAsync(id);

            if (registros == null)
            {
                return NotFound();
            }

            return Ok(registros);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var registros = await _service.GetAllAsync();

            return Ok(registros);
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, RegistroGlicemiaUpdateDto dto)
        {
            var atualizado = await _service.UpdateAsync(id, dto);

            if (!atualizado)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete]
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
