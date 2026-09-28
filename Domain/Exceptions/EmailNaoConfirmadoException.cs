namespace Domain.Exceptions
{
    public class EmailNaoConfirmadoException : Exception
    {
        public EmailNaoConfirmadoException()
            : base("Confirme seu e-mail antes de entrar. Enviamos um código para a sua caixa de entrada.")
        {
        }
    }
}
