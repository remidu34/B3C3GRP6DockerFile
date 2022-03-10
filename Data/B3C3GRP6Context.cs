using B3C3GRP6.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B3C3GRP6.Data
{
    public partial class B3C3GRP6Context : DbContext
    {
        public B3C3GRP6Context()
        {
        }

        public B3C3GRP6Context(DbContextOptions<B3C3GRP6Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Compte> Comptes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Compte>(entity =>
            {
                entity.HasKey(e => e.IdCompte)
                    .HasName("Compte_PK");

                entity.ToTable("Compte");

                entity.Property(e => e.BrowserName)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.IncrementDelay)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.IpPublic)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Login)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
