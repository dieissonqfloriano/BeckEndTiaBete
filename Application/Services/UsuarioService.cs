using Application.DTOs;
using Application.Interfaces;
using BCrypt.Net;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Application.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repository;
        private readonly ITokenService _tokenService;

        public UsuarioService(IUsuarioRepository repository, ITokenService tokenService)
        {
            _repository = repository;
            _tokenService = tokenService;
        }

        public async Task<UsuarioOutputDto> CreateAsync(UsuarioCreateDto dto)
        {
            var usuarioexiste = await _repository.GetByEmailAsync(dto.Email);

            if (usuarioexiste != null)
            {
                throw new Exception("Já existe um usuário cadastrado com este e-mail.");
            }

            var usuario = new Usuario
            {
                Name = dto.Name,
                Email = dto.Email,
                Senha = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
                TipoDiabetes = dto.TipoDiabetes,
                Idade = dto.Idade,
                Celular = dto.Celular,
                FatorSensibilidade = dto.FatorSensibilidade,
                HgtAlvo = dto.HgtAlvo,
                Role = "Usuario"
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

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var usuario = await _repository.GetByEmailAsync(dto.Email);

            if (usuario == null)
            {
                return null;
            }

            if (!usuario.Ativo)
            {
                return null;
            }

            var senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha);

            if (!senhaCorreta)
            {
                return null;
            }

            var token = _tokenService.GenerateToken(usuario);

            var usuarioOutput = new UsuarioOutputDto
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

            return new LoginResponseDto
            {
                Token = token,
                Usuario = usuarioOutput
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);

            if (usuario == null)
            {
                return false;
            }

            usuario.Ativo = false;

            _repository.Update(usuario);

            await _repository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReativarContaAsync(ReativarContaDto dto)
        {
            var usuario = await _repository.GetByEmailAsync(dto.Email);

            if (usuario == null)
            {
                return false;
            }

            if (usuario.Ativo)
            {
                return false;
            }

            var senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.Senha);

            if (!senhaCorreta)
            {
                return false;
            }

            usuario.Ativo = true;

            _repository.Update(usuario);

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

        public async Task<bool> PatchAsync(int id, UsuarioPatchDto dto)
        {
            var usuario = await _repository.GetByIdAsync(id);

            if (usuario == null)
            {
                return false;
            }

            if (dto.Name != null)
            {
                usuario.Name = dto.Name;
            }

            if (dto.Email != null)
            {
                usuario.Email = dto.Email;
            }

            if (dto.TipoDiabetes != null)
            {
                usuario.TipoDiabetes = dto.TipoDiabetes;
            }

            if (dto.Idade != null)
            {
                usuario.Idade = dto.Idade;
            }

            if (dto.Celular != null)
            {
                usuario.Celular = dto.Celular;
            }

            if (dto.FatorSensibilidade != null)
            {
                usuario.FatorSensibilidade = dto.FatorSensibilidade.Value;
            }

            if (dto.HgtAlvo != null)
            {
                usuario.HgtAlvo = dto.HgtAlvo.Value;
            }

            _repository.Update(usuario);

            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
