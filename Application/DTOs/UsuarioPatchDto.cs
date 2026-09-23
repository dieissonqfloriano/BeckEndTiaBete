using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UsuarioPatchDto
    {
        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? TipoDiabetes { get; set; }

        public int? Idade { get; set; }

        public string? Celular { get; set; }

        public int? FatorSensibilidade { get; set; }

        public int? HgtAlvo { get; set; }
    }
}
