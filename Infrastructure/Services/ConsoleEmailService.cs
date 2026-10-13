using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    /// <summary>
    /// Usado quando não há chave do Brevo configurada (desenvolvimento local):
    /// em vez de enviar o e-mail, mostra o código no console da API.
    /// </summary>
    public class ConsoleEmailService : IEmailService
    {
        private readonly ILogger<ConsoleEmailService> _logger;

        public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
        {
            _logger = logger;
        }

        public Task EnviarCodigoConfirmacaoAsync(string emailDestino, string nome, string codigo)
        {
            _logger.LogWarning("[E-MAIL NÃO ENVIADO — modo local] Código de confirmação para {Email}: {Codigo}", emailDestino, codigo);
            return Task.CompletedTask;
        }
    }
}
