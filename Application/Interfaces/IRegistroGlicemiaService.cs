using Application.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    namespace Application.Interfaces
    {
        public interface IRegistroGlicemiaService
        {
            Task<RegistroGlicemiaOutputDto> CreateAsync(
                RegistroGlicemiaCreateDto dto, int usuarioId);

            Task<RegistroGlicemiaOutputDto?> GetByIdAsync(int id, int usxuarioId);

            Task<List<RegistroGlicemiaOutputDto>> GetAllAsync(int usuarioId);

            Task<bool> UpdateAsync(
                int id, RegistroGlicemiaUpdateDto dto, int usuarioId);

            Task<bool> DeleteAsync(int id, int usuarioId);
        }
    }
}