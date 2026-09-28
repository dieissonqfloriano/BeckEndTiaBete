using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UsuarioPatchDto
    {
        [StringLength(100, MinimumLength = 1)]
        public string? Name { get; set; }

        [EmailAddress]
        [StringLength(254)]
        public string? Email { get; set; }

        [RegularExpression(
            "^(Tipo 1|Tipo 2|Gestacional|Outro)$",
            ErrorMessage = "Tipo de diabetes inválido.")]
        public string? TipoDiabetes { get; set; }

        [Range(1, 120)]
        public int? Idade { get; set; }

        [StringLength(20)]
        public string? Celular { get; set; }

        [Range(typeof(decimal), "1", "600",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public decimal? FatorSensibilidade { get; set; }

        [Range(1, 600)]
        public int? HgtAlvo { get; set; }
    }
}
