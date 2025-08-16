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
    }
}
