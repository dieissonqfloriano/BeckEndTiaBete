using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services
{
    /// <summary>
    /// Guarda por pouco tempo a informação "usuário X está ativo".
    /// Só resultados positivos entram no cache: um usuário inativo sempre é conferido no banco,
    /// então a reativação vale na hora. A desativação chama Invalidar().
    /// </summary>
    public class UsuarioAtivoCache : IUsuarioAtivoCache
    {
        private static readonly TimeSpan Duracao = TimeSpan.FromMinutes(2);

        private readonly IMemoryCache _cache;
        private readonly IUsuarioService _usuarioService;

        public UsuarioAtivoCache(IMemoryCache cache, IUsuarioService usuarioService)
        {
            _cache = cache;
            _usuarioService = usuarioService;
        }

        public async Task<bool> UsuarioAtivoAsync(int usuarioId)
        {
            var chave = Chave(usuarioId);

            if (_cache.TryGetValue(chave, out _))
            {
                return true;
            }

            var ativo = await _usuarioService.UsuarioAtivoAsync(usuarioId);

            if (ativo)
            {
                _cache.Set(chave, true, Duracao);
            }

            return ativo;
        }

        public void Invalidar(int usuarioId) => _cache.Remove(Chave(usuarioId));

        private static string Chave(int usuarioId) => $"usuario-ativo:{usuarioId}";
    }
}
