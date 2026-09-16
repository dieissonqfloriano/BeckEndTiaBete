using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class RegistroGlicemiaCreateDto
    {
        public int Glicemia {  get; set; }
        public int Dose { get; set; }
        public TimeSpan Hora { get; set; }
        public string Refeicao { get; set; } = string.Empty;
        public DateTime Data {  get; set; }
        public int UsuarioId { get; set; }
 
    }
}
