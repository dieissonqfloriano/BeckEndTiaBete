using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private const int MySqlChaveDuplicada = 1062;

        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
        }

        // Rastreada: usada quando o usuário vai ser alterado.
        public Task<Usuario?> GetByIdAsync(int id) =>
            _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        public Task<Usuario?> GetByIdSomenteLeituraAsync(int id) =>
            _context.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

        public Task<Usuario?> GetByEmailAsync(string email) =>
            _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        public Task<List<Usuario>> GetAllAsync() =>
            _context.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .ToListAsync();

        // SELECT EXISTS(...) — não carrega a linha inteira.
        public Task<bool> ExisteAtivoAsync(int id) =>
            _context.Usuarios.AnyAsync(u => u.Id == id && u.Ativo);

        // Soft delete em um único UPDATE.
        public async Task<bool> DesativarAsync(int id)
        {
            var agora = DateTime.UtcNow;

            var alterados = await _context.Usuarios
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(u => u.Ativo, false)
                    .SetProperty(u => u.DesativadoEm, (DateTime?)agora)
                    .SetProperty(u => u.AtualizadoEm, agora));

            return alterados > 0;
        }

        // Exclusão definitiva (LGPD art. 18, VI): apaga o usuário; os registros de glicemia
        // saem junto pela chave estrangeira com ON DELETE CASCADE.
        public async Task<bool> ExcluirDefinitivamenteAsync(int id)
        {
            var apagados = await _context.Usuarios
                .Where(u => u.Id == id)
                .ExecuteDeleteAsync();

            return apagados > 0;
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is MySqlException mysql && mysql.Number == MySqlChaveDuplicada)
            {
                throw new EmailJaExisteException();
            }
        }
    }
}
