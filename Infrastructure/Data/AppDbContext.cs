using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<RegistroGlicemia> RegistrosGlicemia => Set<RegistroGlicemia>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // utf8mb4 aceita qualquer caractere (acentos, emoji);
            // a collation _ai_ci compara sem diferenciar maiúsculas e acentos.
            modelBuilder
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_0900_ai_ci");

            // Todas as classes IEntityTypeConfiguration<T> da pasta Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            PreencherDatasDeAuditoria();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            PreencherDatasDeAuditoria();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void PreencherDatasDeAuditoria()
        {
            var agora = DateTime.UtcNow;

            foreach (var entrada in ChangeTracker.Entries<IAuditavel>())
            {
                if (entrada.State == EntityState.Added)
                {
                    entrada.Entity.CriadoEm = agora;
                    entrada.Entity.AtualizadoEm = agora;
                }
                else if (entrada.State == EntityState.Modified)
                {
                    entrada.Entity.AtualizadoEm = agora;
                }
            }
        }
    }
}
