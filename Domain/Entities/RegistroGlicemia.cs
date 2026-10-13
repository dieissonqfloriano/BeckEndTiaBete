using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class RegistroGlicemia : IAuditavel
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public DateOnly Data { get; set; }
        public TimeSpan Hora { get; set; }
        public int? Glicemia { get; set; }
        public bool GlicemiaAcimaDoLimite { get; set; }
        public decimal Dose { get; set; }
        public TipoRefeicao Refeicao { get; set; }
        public string? Observacao { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }
}
