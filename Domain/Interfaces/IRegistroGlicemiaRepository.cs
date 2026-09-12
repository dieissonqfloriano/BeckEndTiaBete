using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRegistroGlicemiaRepository
    {
        Task AddAsync(RegistroGlicemia registro);
        void Update(RegistroGlicemia registro);
        void Delete(RegistroGlicemia registro);
        Task<RegistroGlicemia?> GetByIdAsync(int id);
        Task<List<RegistroGlicemia>> GetAllAsync();
        Task SaveChangesAsync();
    }
}