namespace Application.DTOs
{
    public class RegistroGlicemiaOutputDto
    {
        public int Id { get; set; }
        public int? Glicemia { get; set; }
        public bool GlicemiaAcimaDoLimite { get; set; }
        public decimal Dose { get; set; }
        public TimeSpan Hora { get; set; }
        public string Refeicao { get; set; } = string.Empty;
        public DateOnly Data { get; set; }
        public string? Observacao { get; set; }
        public int UsuarioId { get; set; }
    }
}
