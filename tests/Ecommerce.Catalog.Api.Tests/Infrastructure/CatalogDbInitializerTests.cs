using Ecommerce.Catalog.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Api.Tests.Infrastructure;

public sealed class CatalogDbInitializerTests
{
    [Fact]
    public async Task SeedAsync_OnEmptyDatabase_InsertsAllProducts()
    {
        using var db = new SqliteCatalogDb();

        var inserted = await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        Assert.Equal(12, inserted);
        Assert.Equal(12, await db.Context.Products.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SeedAsync_WhenAlreadySeeded_InsertsNothing()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var inserted = await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        Assert.Equal(0, inserted);
        Assert.Equal(12, await db.Context.Products.CountAsync(TestContext.Current.CancellationToken));
    }
}
