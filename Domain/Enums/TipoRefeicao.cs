using System.Globalization;
using System.Text;

namespace Domain.Enums
{
    public enum TipoRefeicao
    {
        CafeDaManha,
        Almoco,
        Lanche,
        Jantar,
        Ceia,
        Outro
    }

    /// <summary>
    /// Converte o texto enviado pelo front (com ou sem emoji/acento, ex.: "☕ Café da Manhã", "Almoço", "Janta")
    /// para o enum salvo no banco, e o enum de volta para um texto amigável.
    /// </summary>
    public static class TipoRefeicaoConversor
    {
        public static string ParaTexto(TipoRefeicao refeicao) => refeicao switch
        {
            TipoRefeicao.CafeDaManha => "Café da Manhã",
            TipoRefeicao.Almoco => "Almoço",
            TipoRefeicao.Lanche => "Lanche",
            TipoRefeicao.Jantar => "Jantar",
            TipoRefeicao.Ceia => "Ceia",
            _ => "Outro"
        };

        public static bool TentarConverter(string? texto, out TipoRefeicao refeicao)
        {
            var chave = Normalizar(texto);

            switch (chave)
            {
                case "cafedamanha":
                case "cafe":
                    refeicao = TipoRefeicao.CafeDaManha; return true;
                case "almoco":
                    refeicao = TipoRefeicao.Almoco; return true;
                case "lanche":
                case "lanchedatarde":
                    refeicao = TipoRefeicao.Lanche; return true;
                case "jantar":
                case "janta":
                    refeicao = TipoRefeicao.Jantar; return true;
                case "ceia":
                    refeicao = TipoRefeicao.Ceia; return true;
                case "outro":
                    refeicao = TipoRefeicao.Outro; return true;
                default:
                    refeicao = TipoRefeicao.Outro; return false;
            }
        }

        // Remove emoji, acentos, espaços e maiúsculas: "☕ Café da Manhã" -> "cafedamanha"
        private static string Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return string.Empty;
            }

            var decomposto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposto.Length);

            foreach (var c in decomposto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (c < 128 && char.IsLetter(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }

            return sb.ToString();
        }
    }
}
