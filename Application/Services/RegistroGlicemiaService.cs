using Application.DTOs;
using Application.Interfaces.Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services
{
    public class RegistroGlicemiaService : IRegistroGlicemiaService
    {
        private readonly IRegistroGlicemiaRepository _repository;

        public RegistroGlicemiaService(IRegistroGlicemiaRepository repository)
        {
            _repository = repository;
        }

        public async Task<RegistroGlicemiaOutputDto> CreateAsync(RegistroGlicemiaCreateDto dto, int usuarioId)
        {

            var registro = new RegistroGlicemia
            {
                Glicemia = dto.Glicemia,
                GlicemiaAcimaDoLimite = dto.GlicemiaAcimaDoLimite,
                Dose = dto.Dose,
                Hora = dto.Hora,
                Refeicao = dto.Refeicao,
                Data = dto.Data,
                UsuarioId = usuarioId
            };

            await _repository.AddAsync(registro);
            await _repository.SaveChangesAsync();

            return new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                GlicemiaAcimaDoLimite = registro.GlicemiaAcimaDoLimite,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId
            };
        }

        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var registro = await _repository.GetByIdAsync(id, usuarioId);

            if (registro == null)
            {
                return false;
            }

            _repository.Delete(registro);

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<List<RegistroGlicemiaOutputDto>> GetAllAsync(int usuarioId)
        {
            var registros = await _repository.GetAllUsuarioIdAsync(usuarioId);

            return registros.Select(registro => new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                GlicemiaAcimaDoLimite = registro.GlicemiaAcimaDoLimite,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId
            }).ToList();
        }

        public async Task<RegistroGlicemiaOutputDto?> GetByIdAsync(int id, int usuarioId)
        {
            var registro = await _repository.GetByIdAsync(id, usuarioId);

            if (registro == null)
            {
                return null;
            }

            return new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                GlicemiaAcimaDoLimite = registro.GlicemiaAcimaDoLimite,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId
            };

        }

        public async Task<bool> UpdateAsync(int id, RegistroGlicemiaUpdateDto dto, int usuarioId)
        {

            var registroExiste = await _repository.GetByIdAsync(id, usuarioId);

            if (registroExiste == null)
            {
                return false;
            }

            registroExiste.Glicemia = dto.Glicemia;
            registroExiste.GlicemiaAcimaDoLimite = dto.GlicemiaAcimaDoLimite;
            registroExiste.Dose = dto.Dose;
            registroExiste.Hora = dto.Hora;
            registroExiste.Refeicao = dto.Refeicao;
            registroExiste.Data = dto.Data;

            _repository.Update(registroExiste);

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<List<RegistroGlicemiaOutputDto>> GetByPeriodoAsync(int usuarioId, DateTime inicio, DateTime fimExclusivo)
        {
            var registros = await _repository.GetByPeriodoAsync(
                usuarioId,
                inicio,
                fimExclusivo);

            return registros.Select(registro => new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                GlicemiaAcimaDoLimite = registro.GlicemiaAcimaDoLimite,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId
            }).ToList();
        }
    }
}
