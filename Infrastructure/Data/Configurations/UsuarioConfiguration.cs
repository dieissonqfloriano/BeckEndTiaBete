using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", tabela =>
            {
                tabela.HasCheckConstraint("CK_Usuarios_HgtAlvo", "`HgtAlvo` BETWEEN 1 AND 600");
                tabela.HasCheckConstraint("CK_Usuarios_FatorSensibilidade", "`FatorSensibilidade` BETWEEN 1 AND 600");
                tabela.HasCheckConstraint("CK_Usuarios_Idade", "`Idade` IS NULL OR `Idade` BETWEEN 1 AND 120");
            });

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.Email)
                .HasMaxLength(254)
                .IsRequired();

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UX_Usuarios_Email");

            // Hash BCrypt tem 60 caracteres; 100 deixa margem se o algoritmo mudar.
            builder.Property(u => u.SenhaHash)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(u => u.TipoDiabetes)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(u => u.FatorSensibilidade)
                .HasPrecision(5, 1);

            builder.Property(u => u.Celular)
                .HasMaxLength(20);

            builder.Property(u => u.Role)
                .HasMaxLength(20)
                .IsRequired();

            // SHA-256 em hexadecimal = 64 caracteres (o código em si nunca é salvo)
            builder.Property(u => u.CodigoConfirmacaoHash).HasMaxLength(64);
            builder.Property(u => u.CodigoConfirmacaoExpiraEm).HasPrecision(6);

            builder.Property(u => u.VersaoTermos).HasMaxLength(20);
            builder.Property(u => u.TermosAceitosEm).HasPrecision(6);
            builder.Property(u => u.ConsentimentoSaudeEm).HasPrecision(6);
            builder.Property(u => u.ResponsavelNome).HasMaxLength(100);
            builder.Property(u => u.ConsentimentoResponsavelEm).HasPrecision(6);

            builder.Property(u => u.CriadoEm).HasPrecision(6);
            builder.Property(u => u.AtualizadoEm).HasPrecision(6);
            builder.Property(u => u.DesativadoEm).HasPrecision(6);

            builder.HasMany(u => u.RegistroGlicemia)
                .WithOne(r => r.Usuario)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
