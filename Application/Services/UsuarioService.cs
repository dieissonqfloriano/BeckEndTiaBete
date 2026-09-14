using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<UsuarioOutputDto> CreateAsync(UsuarioCreateDto dto)
        {
            var usuario = new Usuario
            {
                Name = dto.Name,
                Email = dto.Email,
                Senha = dto.Senha,
                TipoDiabetes = dto.TipoDiabetes,
                Idade = dto.Idade,
                Celular = dto.Celular,
                FatorSensibilidade = dto.FatorSensibilidade,
                HgtAlvo = dto.HgtAlvo
            };

            await _repository.AddAsync(usuario);
            await _repository.SaveChangesAsync();

            return new UsuarioOutputDto
            {
                Id = usuario.Id,
                Name = usuario.Name,
                Email= usuario.Email,
                TipoDiabetes = usuario.TipoDiabetes,
                Idade= usuario.Idade,
                Celular = usuario.Celular,
                FatorSensibilidade = usuario.FatorSensibilidade,
                HgtAlvo = usuario.HgtAlvo
            };

        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);

            if (usuario == null)
            {
                return false;
            }

            _repository.Delete(usuario);

            await _repository.SaveChangesAsync();
            return true;

        }

        public async Task<List<UsuarioOutputDto>> GetAllAsync()
        {
           var usuario = await _repository.GetAllAsync();

            return usuario.Select(usuario => new UsuarioOutputDto
            {
                Id = usuario.Id,
                Name = usuario.Name,
                Email = usuario.Email,
                TipoDiabetes = usuario.TipoDiabetes,
                Idade = usuario.Idade,
                Celular = usuario.Celular,
                FatorSensibilidade = usuario.FatorSensibilidade,
                HgtAlvo = usuario.HgtAlvo
            }).ToList();
        }

        public async Task<UsuarioOutputDto?> GetByIdAsync(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);

            if (usuario == null)
            {
                return null;
            }

            return new UsuarioOutputDto
            {
                Id = usuario.Id,
                Name = usuario.Name,
                Email = usuario.Email,
                TipoDiabetes = usuario.TipoDiabetes,
                Idade = usuario.Idade,
                Celular = usuario.Celular,
                FatorSensibilidade = usuario.FatorSensibilidade,
                HgtAlvo = usuario.HgtAlvo
            };
        }

        public async Task<bool> UpdateAsync(int id, UsuarioUpdateDto dto)
        {
            var usuarioexiste = await _repository.GetByIdAsync(id);

            if (usuarioexiste == null)
            {
                return false;
            }

            usuarioexiste.Name = dto.Name;
            usuarioexiste.Email = dto.Email;
            usuarioexiste.Idade = dto.Idade;
            usuarioexiste.Celular = dto.Celular;
            usuarioexiste.TipoDiabetes = dto.TipoDiabetes;
            usuarioexiste.FatorSensibilidade = dto.FatorSensibilidade;
            usuarioexiste.HgtAlvo = dto.HgtAlvo;

            _repository.Update(usuarioexiste);

            await _repository.SaveChangesAsync();
            return true;

        }
    }
}
