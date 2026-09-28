namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarCodigoConfirmacaoAsync(string emailDestino, string nome, string codigo);
    }
}
