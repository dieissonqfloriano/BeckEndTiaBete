using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task AddAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByIdSomenteLeituraAsync(int id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<List<Usuario>> GetAllAsync();
        Task<bool> ExisteAtivoAsync(int id);
        Task<bool> DesativarAsync(int id);
        Task<bool> ExcluirDefinitivamenteAsync(int id);
        Task SaveChangesAsync();
    }
}
