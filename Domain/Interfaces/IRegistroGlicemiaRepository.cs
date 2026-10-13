using Domain.Entities;

namespace Domain.Interfaces
{
    public interface IRegistroGlicemiaRepository
    {
        Task AddAsync(RegistroGlicemia registro);
        Task<RegistroGlicemia?> GetByIdAsync(int id, int usuarioId);
        Task<RegistroGlicemia?> GetByIdSomenteLeituraAsync(int id, int usuarioId);
        Task<List<RegistroGlicemia>> GetPaginaAsync(int usuarioId, int pagina, int tamanho);
        Task<List<RegistroGlicemia>> GetByPeriodoAsync(int usuarioId, DateOnly dataInicial, DateOnly dataFinal);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task SaveChangesAsync();
    }
}
