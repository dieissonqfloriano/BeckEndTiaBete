using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RegistroGlicemiaUpdateDto : IValidatableObject
    {
        [Range(20, 600, ErrorMessage = "A glicemia deve estar entre 20 e 600 mg/dL.")]
        public int? Glicemia { get; set; }

        public bool GlicemiaAcimaDoLimite { get; set; }

        [Range(typeof(decimal), "0", "100",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "A dose deve estar entre 0 e 100 unidades.")]
        public decimal Dose { get; set; }

        public TimeSpan Hora { get; set; }

        [Required]
        [StringLength(40)]
        public string Refeicao { get; set; } = string.Empty;

        public DateOnly Data { get; set; }

        [StringLength(500, ErrorMessage = "A observação deve ter no máximo 500 caracteres.")]
        public string? Observacao { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) =>
            RegistroGlicemiaValidacao.Validar(Data, Glicemia, GlicemiaAcimaDoLimite, Dose, Refeicao);
    }
}
