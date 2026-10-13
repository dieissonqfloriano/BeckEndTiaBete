using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Usuario : IAuditavel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public TipoDiabetes TipoDiabetes { get; set; }
        public decimal FatorSensibilidade { get; set; }
        public int HgtAlvo { get; set; }
        public int? Idade { get; set; }
        public string? Celular { get; set; }
        public string Role { get; set; } = "Usuario";
        public bool Ativo { get; set; } = true;
        public DateTime? DesativadoEm { get; set; }

        // Confirmação de e-mail por código de 6 dígitos
        public bool EmailConfirmado { get; set; }
        public string? CodigoConfirmacaoHash { get; set; }
        public DateTime? CodigoConfirmacaoExpiraEm { get; set; }
        public int TentativasCodigo { get; set; }

        // Prova do consentimento (LGPD art. 8º §2º: cabe ao controlador provar o consentimento)
        public DateTime? TermosAceitosEm { get; set; }
        public string? VersaoTermos { get; set; }
        public DateTime? ConsentimentoSaudeEm { get; set; }

        // Menores de 18 anos: consentimento de um dos pais ou responsável legal (LGPD art. 14, §1º)
        public string? ResponsavelNome { get; set; }
        public DateTime? ConsentimentoResponsavelEm { get; set; }

        /// <summary>Versão vigente dos Termos de Uso e da Política de Privacidade.</summary>
        public const string VersaoTermosAtual = "1.0-2026-09";
        public const int MaioridadeLegal = 18;
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }

        public List<RegistroGlicemia> RegistroGlicemia { get; set; } = new();

        /// <summary>E-mail sempre salvo e comparado sem espaços e em minúsculas.</summary>
        public static string NormalizarEmail(string email) =>
            (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}
