using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ecommerce.Catalog.Api.Infrastructure;

// Used only by dotnet ef at design time; the connection string is never used to connect
public sealed class CatalogDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CatalogDbContext>
{
    public CatalogDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql("Host=localhost;Database=catalogdb;Username=postgres;Password=postgres")
            .Options;

        return new CatalogDbContext(options);
    }
}
