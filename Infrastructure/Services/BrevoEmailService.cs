using System.Net.Http.Json;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    /// <summary>
    /// Envia e-mails pela API HTTP do Brevo (plano gratuito ~300 e-mails/dia).
    /// Usa HTTP e não SMTP porque o Render gratuito bloqueia as portas SMTP.
    /// Configuração: Email:BrevoApiKey, Email:Remetente (e-mail verificado no Brevo), Email:NomeRemetente.
    /// </summary>
    public class BrevoEmailService : IEmailService
    {
        private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

        private readonly string _apiKey;
        private readonly string _remetente;
        private readonly string _nomeRemetente;

        public BrevoEmailService(IConfiguration configuration)
        {
            _apiKey = configuration["Email:BrevoApiKey"] ?? string.Empty;
            _remetente = configuration["Email:Remetente"] ?? string.Empty;
            _nomeRemetente = configuration["Email:NomeRemetente"] ?? "GlicHelp";
        }

        public async Task EnviarCodigoConfirmacaoAsync(string emailDestino, string nome, string codigo)
        {
            var primeiroNome = (nome ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

            var corpo = new
            {
                sender = new { name = _nomeRemetente, email = _remetente },
                to = new[] { new { email = emailDestino, name = nome } },
                subject = $"Seu código GlicHelp: {codigo}",
                htmlContent = $"""
                    <div style="font-family:Arial,sans-serif;max-width:480px;margin:auto;padding:24px;color:#222">
                      <h2 style="color:#c0392b;margin:0 0 12px">GlicHelp</h2>
                      <p>Olá, {System.Net.WebUtility.HtmlEncode(primeiroNome)}!</p>
                      <p>Use o código abaixo para confirmar seu e-mail:</p>
                      <p style="font-size:32px;font-weight:bold;letter-spacing:8px;color:#0d9e6e;margin:16px 0">{codigo}</p>
                      <p style="color:#666;font-size:13px">O código vale por 15 minutos. Se você não criou uma conta no GlicHelp, ignore este e-mail.</p>
                    </div>
                    """
            };

            using var requisicao = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = JsonContent.Create(corpo)
            };
            requisicao.Headers.Add("api-key", _apiKey);
            requisicao.Headers.Add("accept", "application/json");

            using var resposta = await Http.SendAsync(requisicao);
            if (!resposta.IsSuccessStatusCode)
            {
                var detalhe = await resposta.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Brevo recusou o envio ({(int)resposta.StatusCode}): {detalhe}");
            }
        }
    }
}
