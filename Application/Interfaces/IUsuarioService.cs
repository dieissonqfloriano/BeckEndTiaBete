using Application.DTOs;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<UsuarioOutputDto> CreateAsync(UsuarioCreateDto usuario);
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
        Task<UsuarioOutputDto?> GetByIdAsync(int id);
        Task<List<UsuarioOutputDto>> GetAllAsync();
        Task<bool> DeleteAsync(int id);
        Task<bool> ReativarContaAsync(ReativarContaDto dto);
        Task<ResultadoPatchUsuario> PatchAsync(int id, UsuarioPatchDto dto);
    }
}