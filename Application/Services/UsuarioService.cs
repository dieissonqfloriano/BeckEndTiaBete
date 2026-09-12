using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;

        public UsuarioService(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            await _repository.AddAsync(usuario);
            await _repository.SaveChangesAsync();

            return usuario;
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

        public async Task<List<Usuario>> GetAllAsync()
        {
           return await _repository.GetAllAsync();
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            var usuarioexiste = await _repository.GetByIdAsync(usuario.Id);

            if (usuarioexiste == null)
            {
                return false;
            }

            usuarioexiste.Name = usuario.Name;
            usuarioexiste.Email = usuario.Email;
            usuarioexiste.Idade = usuario.Idade;
            usuarioexiste.Celular = usuario.Celular;
            usuarioexiste.TipoDiabetes = usuario.TipoDiabetes;
            usuarioexiste.FatorSensibilidade = usuario.FatorSensibilidade;
            usuarioexiste.HgtAlvo = usuario.HgtAlvo;

            _repository.Update(usuarioexiste);

            await _repository.SaveChangesAsync();
            return true;

        }
    }
}
