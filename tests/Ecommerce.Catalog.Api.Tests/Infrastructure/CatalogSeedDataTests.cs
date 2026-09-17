using Ecommerce.Catalog.Api.Infrastructure;

namespace Ecommerce.Catalog.Api.Tests.Infrastructure;

public sealed class CatalogSeedDataTests
{
    [Fact]
    public void CreateProducts_ReturnsTwelveProducts()
    {
        var products = CatalogSeedData.CreateProducts();

        Assert.Equal(12, products.Count);
    }

    [Fact]
    public void CreateProducts_HaveUniqueIdsAndNames()
    {
        var products = CatalogSeedData.CreateProducts();

        Assert.Equal(products.Count, products.Select(p => p.Id).Distinct().Count());
        Assert.Equal(products.Count, products.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void CreateProducts_ContainsExactlyOneOutOfStockProduct()
    {
        var products = CatalogSeedData.CreateProducts();

        Assert.Single(products, p => p.AvailableStock == 0);
    }

    [Fact]
    public void CreateProducts_ContainsExactlyOneProductWithStockOfTwo()
    {
        var products = CatalogSeedData.CreateProducts();

        Assert.Single(products, p => p.AvailableStock == 2);
    }

    [Fact]
    public void CreateProducts_ReturnsFreshInstancesOnEveryCall()
    {
        var first = CatalogSeedData.CreateProducts();
        var second = CatalogSeedData.CreateProducts();

        Assert.NotSame(first[0], second[0]);
        Assert.Equal(first[0].Id, second[0].Id);
    }

    [Fact]
    public void CreateProducts_UsesTheFourAgreedCategories()
    {
        var categories = CatalogSeedData.CreateProducts().Select(p => p.Category).Distinct(StringComparer.Ordinal).Order().ToList();

        Assert.Equal(["Accessories", "Apparel", "Books", "Drinkware"], categories);
    }

    [Fact]
    public void CreateProducts_EveryProductHasASerbianTranslation()
    {
        var products = CatalogSeedData.CreateProducts();

        Assert.All(products, product => Assert.Single(product.Translations, t => t.Language == CatalogSeedData.Serbian));
        Assert.Equal(products.Count, products.Select(p => p.Localize(CatalogSeedData.Serbian).Name).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void CreateProducts_TranslatesTheFourCategoriesIntoSerbian()
    {
        var categories = CatalogSeedData.CreateProducts().Select(p => p.Localize(CatalogSeedData.Serbian).Category).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList();

        Assert.Equal(["Dodaci", "Knjige", "Odeća", "Posuđe za piće"], categories);
    }
}
