namespace Domain.Common
{
    /// <summary>
    /// Entidades que registram automaticamente quando foram criadas e alteradas (UTC).
    /// Os valores são preenchidos pelo AppDbContext ao salvar.
    /// </summary>
    public interface IAuditavel
    {
        DateTime CriadoEm { get; set; }
        DateTime AtualizadoEm { get; set; }
    }
}
