using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.Application.Interfaces;

namespace Application.Services
{
    public class RegistroGlicemiaService : IRegistroGlicemiaService
    {
        private readonly IRegistroGlicemiaRepository _repository;

        public RegistroGlicemiaService(IRegistroGlicemiaRepository repository)
        {
            _repository = repository;
        }
     
        public async Task<RegistroGlicemiaOutputDto> CreateAsync(RegistroGlicemiaCreateDto dto)
        {
            var registro = new RegistroGlicemia
            {
                Glicemia = dto.Glicemia,
                Dose = dto.Dose,
                Hora = dto.Hora,
                Refeicao = dto.Refeicao,
                Data = dto.Data,
                UsuarioId = dto.UsuarioId
            };

            await _repository.AddAsync(registro);
            await _repository.SaveChangesAsync();

            return new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var registro = await _repository.GetByIdAsync(id);

            if (registro == null)
            {
                return false;
            }

            _repository.Delete(registro);

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<List<RegistroGlicemiaOutputDto>> GetAllAsync()
        {
            var registros = await _repository.GetAllAsync();

            return registros.Select(registro => new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId

            }).ToList();
        }

        public async Task<RegistroGlicemiaOutputDto?> GetByIdAsync(int id)
        {
            var registro = await _repository.GetByIdAsync(id);

            if (registro == null)
            {
                return null;
            }

            return new RegistroGlicemiaOutputDto
            {
                Id = registro.Id,
                Glicemia = registro.Glicemia,
                Dose = registro.Dose,
                Hora = registro.Hora,
                Refeicao = registro.Refeicao,
                Data = registro.Data,
                UsuarioId = registro.UsuarioId
            };

        }

        public async Task<bool> UpdateAsync(int id, RegistroGlicemiaUpdateDto dto)
        {
            var registroExiste = await _repository.GetByIdAsync(id);

            if (registroExiste == null) 
            {
                return false;
            }

            registroExiste.Glicemia = dto.Glicemia;
            registroExiste.Dose = dto.Dose;
            registroExiste.Hora = dto.Hora;
            registroExiste.Refeicao = dto.Refeicao;
            registroExiste.Data = dto.Data;

            _repository.Update(registroExiste);

            await _repository.SaveChangesAsync();

            return true;

        }
    }
}
