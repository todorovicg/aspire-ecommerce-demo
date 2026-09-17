using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ecommerce.Catalog.Api.Infrastructure;

// Runs before the server starts listening, so the health check only passes on a migrated and seeded database
public sealed class CatalogDbInitializer(IServiceProvider services, ILogger<CatalogDbInitializer> logger) : IHostedService
{
    private static readonly ActivitySource ActivitySource = new("Ecommerce.Catalog.Api");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("Initialize catalog database", ActivityKind.Client);
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        // The Npgsql migrator reads the history table before creating it and logs that read as a failed command on a fresh database; creating the table first keeps the startup log clean
        await db.Database.ExecuteSqlRawAsync(db.GetService<IHistoryRepository>().GetCreateIfNotExistsScript(), cancellationToken);
        await db.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("[{Prefix}] Applied pending migrations", nameof(CatalogDbInitializer));

        var inserted = await SeedAsync(db, cancellationToken);
        if (inserted == 0)
        {
            logger.LogInformation("[{Prefix}] Catalog already seeded, skipping", nameof(CatalogDbInitializer));
            return;
        }

        logger.LogInformation("[{Prefix}] Seeded [{Count}] products", nameof(CatalogDbInitializer), inserted);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public static async Task<int> SeedAsync(CatalogDbContext db, CancellationToken cancellationToken)
    {
        if (await db.Products.AnyAsync(cancellationToken))
        {
            return 0;
        }

        var products = CatalogSeedData.CreateProducts();
        db.Products.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);
        return products.Count;
    }
}
