namespace Domain.Enums
{
    public enum TipoDiabetes
    {
        Tipo1,
        Tipo2,
        Gestacional,
        Outro
    }

    /// <summary>
    /// Converte entre o enum e o texto usado na API ("Tipo 1", "Tipo 2", "Gestacional", "Outro").
    /// </summary>
    public static class TipoDiabetesConversor
    {
        public static string ParaTexto(TipoDiabetes tipo) => tipo switch
        {
            TipoDiabetes.Tipo1 => "Tipo 1",
            TipoDiabetes.Tipo2 => "Tipo 2",
            TipoDiabetes.Gestacional => "Gestacional",
            _ => "Outro"
        };

        public static bool TentarConverter(string? texto, out TipoDiabetes tipo)
        {
            var normalizado = (texto ?? string.Empty)
                .Replace(" ", string.Empty)
                .Trim()
                .ToLowerInvariant();

            switch (normalizado)
            {
                case "tipo1": tipo = TipoDiabetes.Tipo1; return true;
                case "tipo2": tipo = TipoDiabetes.Tipo2; return true;
                case "gestacional": tipo = TipoDiabetes.Gestacional; return true;
                case "outro": tipo = TipoDiabetes.Outro; return true;
                default: tipo = TipoDiabetes.Outro; return false;
            }
        }
    }
}
