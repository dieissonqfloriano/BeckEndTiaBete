using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class RegistroGlicemiaRepository : IRegistroGlicemiaRepository
    {
        private readonly AppDbContext _context;

        public RegistroGlicemiaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RegistroGlicemia registro)
        {
            await _context.RegistrosGlicemia.AddAsync(registro);
        }

        public void Delete(RegistroGlicemia registro)
        {
            _context.RegistrosGlicemia.Remove(registro);
        }

        public async Task<List<RegistroGlicemia>> GetAllAsync()
        {
            return await _context.RegistrosGlicemia.ToListAsync();
        }

        public async Task<RegistroGlicemia?> GetByIdAsync(int id)
        {
            return await _context.RegistrosGlicemia
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Update(RegistroGlicemia registro)
        {
            _context.RegistrosGlicemia.Update(registro);
        }
    }
}