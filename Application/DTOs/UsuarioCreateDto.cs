using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class UsuarioCreateDto : IValidatableObject
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(72, MinimumLength = 8)] // BCrypt ignora o que passar de 72 bytes
        public string Senha { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^(Tipo 1|Tipo 2|Gestacional|Outro)$",
            ErrorMessage = "Tipo de diabetes inválido.")]
        public string TipoDiabetes { get; set; } = string.Empty;

        [Required(ErrorMessage = "A idade é obrigatória.")]
        [Range(1, 120)]
        public int? Idade { get; set; }

        // Aceite dos Termos de Uso e da Política de Privacidade
        [Range(typeof(bool), "true", "true", ErrorMessage = "É preciso aceitar os Termos de Uso e a Política de Privacidade.")]
        public bool AceitouTermos { get; set; }

        // Consentimento específico e destacado para dados de saúde (LGPD art. 11, I)
        [Range(typeof(bool), "true", "true", ErrorMessage = "É preciso autorizar o tratamento dos dados de saúde para usar o GlicHelp.")]
        public bool ConsentiuDadosSaude { get; set; }

        // Obrigatórios só para menores de 18 anos (LGPD art. 14)
        [StringLength(100)]
        public string? ResponsavelNome { get; set; }

        public bool ConsentimentoResponsavel { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Idade is < 18)
            {
                if (string.IsNullOrWhiteSpace(ResponsavelNome))
                {
                    yield return new ValidationResult(
                        "Para menores de 18 anos, informe o nome do pai, mãe ou responsável legal.",
                        new[] { nameof(ResponsavelNome) });
                }

                if (!ConsentimentoResponsavel)
                {
                    yield return new ValidationResult(
                        "Para menores de 18 anos, o responsável legal precisa autorizar o cadastro.",
                        new[] { nameof(ConsentimentoResponsavel) });
                }
            }
        }

        [StringLength(20)]
        public string? Celular { get; set; }

        [Range(typeof(decimal), "1", "600",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public decimal FatorSensibilidade { get; set; }

        [Range(1, 600)]
        public int HgtAlvo { get; set; }
    }
}
