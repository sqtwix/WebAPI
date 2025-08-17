using Microsoft.EntityFrameworkCore;
using CsvImportApi.Models; // Мои модели


namespace CsvImportApi
{
    public class AppDbContext : DbContext

    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
      : base(options)
        {
        }

        public DbSet<Result> Results { get; set; }
        public DbSet<Values> Values { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конфигурация для Values
            modelBuilder.Entity<Values>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Date).IsRequired();
                entity.Property(v => v.ExecutionTime).IsRequired();
                entity.Property(v => v.Value).IsRequired();
                entity.Property(v => v.FileName)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            // Конфигурация для Result
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.FileName)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(r => r.MinDate).IsRequired();
            });

            modelBuilder.Entity<Values>(entity =>
            {
                entity.Property(e => e.Date)
                    .HasColumnType("timestamp with time zone");
            });
        }
    }
}
