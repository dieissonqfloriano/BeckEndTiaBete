namespace Application.Interfaces
{
    /// <summary>
    /// Evita ir ao banco em toda requisição autenticada só para saber se o usuário está ativo.
    /// </summary>
    public interface IUsuarioAtivoCache
    {
        Task<bool> UsuarioAtivoAsync(int usuarioId);
        void Invalidar(int usuarioId);
    }
}
