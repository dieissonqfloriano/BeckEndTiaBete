using Application.Interfaces.Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class RegistroGlicemiaService : IRegistroGlicemiaService
    {
        private readonly IRegistroGlicemiaRepository _repository;

        public RegistroGlicemiaService(IRegistroGlicemiaRepository repository)
        {
            _repository = repository;
        }

        public async Task<RegistroGlicemia> CreateAsync(RegistroGlicemia registro)
        {
            await _repository.AddAsync(registro);
            await _repository.SaveChangesAsync();

            return registro;
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

        public async Task<List<RegistroGlicemia>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public Task<RegistroGlicemia?> GetByIdAsync(int id)
        {
            return _repository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(RegistroGlicemia registro)
        {
            var registroExiste = await _repository.GetByIdAsync(registro.Id);

            if (registroExiste == null) 
            {
                return false;
            }

            registroExiste.Glicemia = registro.Glicemia;
            registroExiste.Dose = registro.Dose;
            registroExiste.Hora = registro.Hora;
            registroExiste.Refeicao = registro.Refeicao;
            registroExiste.Date = registro.Date;
            registroExiste.UsuarioId = registro.UsuarioId;

            _repository.Update(registroExiste);

            await _repository.SaveChangesAsync();

            return true;

        }
    }
}
