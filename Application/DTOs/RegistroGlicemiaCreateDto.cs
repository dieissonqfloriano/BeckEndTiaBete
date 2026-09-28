using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs
{
    public class RegistroGlicemiaCreateDto : IValidatableObject
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

    /// <summary>Regras compartilhadas entre criação e atualização de registro.</summary>
    internal static class RegistroGlicemiaValidacao
    {
        public static IEnumerable<ValidationResult> Validar(
            DateOnly data, int? glicemia, bool acimaDoLimite, decimal dose, string refeicao)
        {
            if (data == default)
            {
                yield return new ValidationResult("Informe uma data válida.", new[] { "Data" });
            }

            if (acimaDoLimite && glicemia != null)
            {
                yield return new ValidationResult(
                    "Quando o aparelho indicar HI, não informe um valor numérico de glicemia.",
                    new[] { "Glicemia", "GlicemiaAcimaDoLimite" });
            }

            if (!acimaDoLimite && glicemia == null)
            {
                yield return new ValidationResult("Informe o valor da glicemia.", new[] { "Glicemia" });
            }

            if (decimal.Round(dose, 1) != dose)
            {
                yield return new ValidationResult(
                    "A dose aceita no máximo uma casa decimal (ex.: 4,5).", new[] { "Dose" });
            }

            if (!TipoRefeicaoConversor.TentarConverter(refeicao, out _))
            {
                yield return new ValidationResult(
                    "Refeição inválida. Use: Café da Manhã, Almoço, Lanche, Jantar, Ceia ou Outro.",
                    new[] { "Refeicao" });
            }
        }
    }
}
