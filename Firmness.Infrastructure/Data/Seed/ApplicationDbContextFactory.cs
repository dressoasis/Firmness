using Firmness.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Firmness.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Cambia por tu cadena real de Railway
            optionsBuilder.UseNpgsql(
                "Host=mainline.proxy.rlwy.net;Port=12358;Database=railway;Username=postgres;Password=dsxkZNHyryWphybiFMauuHKQxZLqdDuo"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}