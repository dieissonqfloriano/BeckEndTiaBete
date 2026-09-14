using Application.DTOs;
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
        Task <UsuarioOutputDto> CreateAsync(UsuarioCreateDto usuario);

        Task <UsuarioOutputDto?> GetByIdAsync(int id);

        Task <List<UsuarioOutputDto>> GetAllAsync();

        Task <bool> DeleteAsync(int id);

        Task <bool> UpdateAsync(int id,UsuarioUpdateDto dto);
        
    }
}
