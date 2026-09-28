using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class RegistroGlicemiaConfiguration : IEntityTypeConfiguration<RegistroGlicemia>
    {
        public void Configure(EntityTypeBuilder<RegistroGlicemia> builder)
        {
            builder.ToTable("RegistrosGlicemia", tabela =>
            {
                tabela.HasCheckConstraint(
                    "CK_Registros_GlicemiaFaixa",
                    "`Glicemia` IS NULL OR `Glicemia` BETWEEN 20 AND 600");

                // Leitura "HI" do aparelho não tem valor numérico; leitura normal sempre tem.
                tabela.HasCheckConstraint(
                    "CK_Registros_HI",
                    "(`GlicemiaAcimaDoLimite` = 1 AND `Glicemia` IS NULL) OR (`GlicemiaAcimaDoLimite` = 0 AND `Glicemia` IS NOT NULL)");

                tabela.HasCheckConstraint(
                    "CK_Registros_Dose",
                    "`Dose` BETWEEN 0 AND 100");
            });

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Hora)
                .HasPrecision(0);

            builder.Property(r => r.Dose)
                .HasPrecision(4, 1);

            builder.Property(r => r.Refeicao)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(r => r.Observacao)
                .HasMaxLength(500);

            builder.Property(r => r.CriadoEm).HasPrecision(6);
            builder.Property(r => r.AtualizadoEm).HasPrecision(6);

            // Índice principal do sistema: toda consulta é "registros de UM usuário, por data".
            // Também atende a chave estrangeira (UsuarioId é a primeira coluna).
            builder.HasIndex(r => new { r.UsuarioId, r.Data, r.Hora })
                .HasDatabaseName("IX_RegistrosGlicemia_UsuarioId_Data_Hora");
        }
    }
}
