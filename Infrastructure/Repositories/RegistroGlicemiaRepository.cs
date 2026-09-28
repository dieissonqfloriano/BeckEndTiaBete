using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        // Rastreada: usada quando o registro vai ser alterado.
        public Task<RegistroGlicemia?> GetByIdAsync(int id, int usuarioId) =>
            _context.RegistrosGlicemia
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);

        public Task<RegistroGlicemia?> GetByIdSomenteLeituraAsync(int id, int usuarioId) =>
            _context.RegistrosGlicemia
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);

        public Task<List<RegistroGlicemia>> GetPaginaAsync(int usuarioId, int pagina, int tamanho) =>
            _context.RegistrosGlicemia
                .AsNoTracking()
                .Where(r => r.UsuarioId == usuarioId)
                .OrderByDescending(r => r.Data)
                .ThenByDescending(r => r.Hora)
                .Skip((pagina - 1) * tamanho)
                .Take(tamanho)
                .ToListAsync();

        public Task<List<RegistroGlicemia>> GetByPeriodoAsync(int usuarioId, DateOnly dataInicial, DateOnly dataFinal) =>
            _context.RegistrosGlicemia
                .AsNoTracking()
                .Where(r => r.UsuarioId == usuarioId && r.Data >= dataInicial && r.Data <= dataFinal)
                .OrderBy(r => r.Data)
                .ThenBy(r => r.Hora)
                .ToListAsync();

        // Um único DELETE no banco, sem buscar o registro antes.
        public async Task<bool> DeleteAsync(int id, int usuarioId)
        {
            var apagados = await _context.RegistrosGlicemia
                .Where(r => r.Id == id && r.UsuarioId == usuarioId)
                .ExecuteDeleteAsync();

            return apagados > 0;
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
