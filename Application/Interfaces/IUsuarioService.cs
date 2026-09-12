using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task <Usuario> CreateAsync(Usuario usuario);

        Task <Usuario?> GetByIdAsync(int id);

        Task <List<Usuario>> GetAllAsync();

        Task <bool> DeleteAsync(int id);

        Task <bool> UpdateAsync(Usuario usuario);
        
    }
}
