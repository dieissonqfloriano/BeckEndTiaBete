using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ConfirmarEmailDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "O código tem 6 números.")]
        public string Codigo { get; set; } = string.Empty;
    }

    public class ReenviarCodigoDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ExcluirContaDto
    {
        [Required(ErrorMessage = "Confirme sua senha para excluir a conta.")]
        public string Senha { get; set; } = string.Empty;
    }

    public enum ResultadoConfirmacaoEmail
    {
        Sucesso,
        JaConfirmado,
        CodigoInvalido,
        CodigoExpirado,
        MuitasTentativas
    }

    /// <summary>Configuração lida do appsettings (seção "ConfirmacaoEmail").</summary>
    public class ConfiguracaoConfirmacaoEmail
    {
        public bool Exigir { get; set; }
        public int ValidadeMinutos { get; set; } = 15;
        public int MaximoTentativas { get; set; } = 5;
    }
}
