using Ecommerce.Catalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Api.Tests.Infrastructure;

public sealed class CatalogDbContextTests
{
    [Fact]
    public async Task SaveChanges_ThenQuery_RoundTripsProduct()
    {
        using var db = new SqliteCatalogDb();
        var id = Guid.Parse("a0000000-0000-4000-8000-000000000099");
        db.Context.Products.Add(Product.Create(id, "Aspire Ceramic Mug", "A mug", "Drinkware", 12.50m, 25, "mug"));
        await db.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        db.Context.ChangeTracker.Clear();

        var loaded = await db.Context.Products.SingleAsync(p => p.Id == id, TestContext.Current.CancellationToken);

        Assert.Equal("Aspire Ceramic Mug", loaded.Name);
        Assert.Equal("Drinkware", loaded.Category);
        Assert.Equal(12.50m, loaded.Price);
        Assert.Equal(25, loaded.AvailableStock);
        Assert.Equal("mug", loaded.ImageKey);
    }

    [Fact]
    public async Task SaveChanges_ThenQuery_RoundTripsTranslations()
    {
        using var db = new SqliteCatalogDb();
        var id = Guid.Parse("a0000000-0000-4000-8000-000000000098");
        db.Context.Products.Add(Product.Create(id, "Aspire Ceramic Mug", "A mug", "Drinkware", 12.50m, 25, "mug")
            .WithTranslation("sr", "Aspire keramička šolja", "Jedna šolja", "Posuđe za piće"));
        await db.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        db.Context.ChangeTracker.Clear();

        var loaded = await db.Context.Products.SingleAsync(p => p.Id == id, TestContext.Current.CancellationToken);

        var translation = Assert.Single(loaded.Translations);
        Assert.Equal("sr", translation.Language);
        Assert.Equal("Aspire keramička šolja", translation.Name);
        Assert.Equal("Aspire keramička šolja", loaded.Localize("sr").Name);
    }
}
