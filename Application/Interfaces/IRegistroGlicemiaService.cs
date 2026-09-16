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
                RegistroGlicemiaCreateDto dto);

            Task<RegistroGlicemiaOutputDto?> GetByIdAsync(int id);

            Task<List<RegistroGlicemiaOutputDto>> GetAllAsync();

            Task<bool> UpdateAsync(
                int id, RegistroGlicemiaUpdateDto dto);

            Task<bool> DeleteAsync(int id);
        }
    }
}
