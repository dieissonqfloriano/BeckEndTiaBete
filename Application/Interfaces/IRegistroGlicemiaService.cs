using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    namespace Application.Interfaces
    {
        public interface IRegistroGlicemiaService
        {
            Task<RegistroGlicemia> CreateAsync(RegistroGlicemia registro);

            Task<RegistroGlicemia?> GetByIdAsync(int id);

            Task<List<RegistroGlicemia>> GetAllAsync();

            Task<bool> UpdateAsync(RegistroGlicemia registro);

            Task<bool> DeleteAsync(int id);
        }
    }
}
