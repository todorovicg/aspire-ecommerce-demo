using Ecommerce.Catalog.Api.Features;
using Ecommerce.Catalog.Api.Features.ListProducts;
using Ecommerce.Catalog.Api.Infrastructure;

namespace Ecommerce.Catalog.Api.Tests.Features;

public sealed class ListProductsEndpointTests
{
    private static readonly Guid MugId = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid CapId = Guid.Parse("a0000000-0000-4000-8000-000000000007");

    [Fact]
    public async Task HandleAsync_WithoutIds_ReturnsAllProductsOrderedByName()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await ListProductsEndpoint.HandleAsync(null, RequestLanguage.English, db.Context, TestContext.Current.CancellationToken);

        Assert.NotNull(result.Value);
        Assert.True(result.Value.Success);
        var products = result.Value.Data;
        Assert.NotNull(products);
        Assert.Equal(12, products.Count);
        Assert.Equal(products.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Id), products.Select(p => p.Id));
    }

    [Fact]
    public async Task HandleAsync_WithIds_ReturnsOnlyMatchingProducts()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await ListProductsEndpoint.HandleAsync([MugId, CapId], RequestLanguage.English, db.Context, TestContext.Current.CancellationToken);

        var products = result.Value?.Data;
        Assert.NotNull(products);
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p.Id == MugId);
        Assert.Contains(products, p => p.Id == CapId);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownIds_ReturnsEmptyList()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await ListProductsEndpoint.HandleAsync([Guid.NewGuid()], RequestLanguage.English, db.Context, TestContext.Current.CancellationToken);

        var products = result.Value?.Data;
        Assert.NotNull(products);
        Assert.Empty(products);
    }

    [Fact]
    public async Task HandleAsync_WithSerbian_ReturnsTranslatedTextOrderedByTranslatedName()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await ListProductsEndpoint.HandleAsync(null, new RequestLanguage("sr"), db.Context, TestContext.Current.CancellationToken);

        var products = result.Value?.Data;
        Assert.NotNull(products);
        Assert.Equal(12, products.Count);
        Assert.Equal("Aspire keramička šolja", products.Single(p => p.Id == MugId).Name);
        Assert.Equal(["Dodaci", "Knjige", "Odeća", "Posuđe za piće"], products.Select(p => p.Category).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal));
        Assert.Equal(products.OrderBy(p => p.Name, StringComparer.Ordinal).Select(p => p.Id), products.Select(p => p.Id));
    }
}
