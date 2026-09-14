using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UsuarioCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string TipoDiabetes { get; set; } = string.Empty;
        public int? Idade { get; set; }
        public string? Celular { get; set; }
        public decimal FatorSensibilidade { get; set; }
        public int HgtAlvo { get; set; }
    }
}
