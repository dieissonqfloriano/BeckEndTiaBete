using Application.DTOs;

namespace Application.Interfaces
{
    public interface IRegistroGlicemiaService
    {
        Task<RegistroGlicemiaOutputDto> CreateAsync(RegistroGlicemiaCreateDto dto, int usuarioId);
        Task<RegistroGlicemiaOutputDto?> GetByIdAsync(int id, int usuarioId);
        Task<List<RegistroGlicemiaOutputDto>> GetAllAsync(int usuarioId, int pagina = 1, int tamanho = RegistroGlicemiaPaginacao.TamanhoPadrao);
        Task<bool> UpdateAsync(int id, RegistroGlicemiaUpdateDto dto, int usuarioId);
        Task<bool> DeleteAsync(int id, int usuarioId);
        Task<List<RegistroGlicemiaOutputDto>> GetByPeriodoAsync(int usuarioId, DateOnly dataInicial, DateOnly dataFinal);
    }

    public static class RegistroGlicemiaPaginacao
    {
        public const int TamanhoPadrao = 100;
        public const int TamanhoMaximo = 500;
        public const int PeriodoMaximoDias = 366;
    }
}
