using Microsoft.EntityFrameworkCore;
using CsvImportApi.Models; // Мои модели


namespace CsvImportApi
{
    public class AppDbContext : DbContext
    {
        public DbSet<Result> Results { get; set; }
    }
}
