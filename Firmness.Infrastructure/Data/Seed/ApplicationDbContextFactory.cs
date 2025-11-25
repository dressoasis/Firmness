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
                "Host=bri30qnelugmsdgm1zzi-postgresql.services.clever-cloud.com;Port=50013;Database=bri30qnelugmsdgm1zzi;Username=ufuxa3dn5n77udngrn2i;Password=xGXOYCeJGYOqvB24zxno8rOPFxqyGs"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}