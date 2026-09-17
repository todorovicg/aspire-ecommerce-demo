using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Features;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Basket.Api.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Basket.Api.Tests.Features;

public sealed class BasketServiceTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));
    private static readonly CatalogProduct Mug = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", 12.50m, 25);
    private static readonly CatalogProduct Poster = new(Guid.Parse("a0000000-0000-4000-8000-000000000010"), "Aspire Launch Poster", 15.00m, 2);
    private static readonly CatalogProduct LimitedHoodie = new(Guid.Parse("a0000000-0000-4000-8000-000000000006"), "Aspire Limited Edition Hoodie", 59.00m, 0);

    private readonly InMemoryBasketStore _store = new();
    private readonly FakeCatalogClient _catalog = new FakeCatalogClient().With(Mug).With(Poster).With(LimitedHoodie);

    private BasketService CreateService() => new(_store, _catalog, NullLogger<BasketService>.Instance);

    private async Task SeedBasketAsync(params (CatalogProduct Product, int Quantity)[] lines)
    {
        var basket = new ShoppingBasket(Buyer.Value);
        foreach (var (product, quantity) in lines)
        {
            basket.UpsertItem(product.Id, product.Price, quantity, 99);
        }

        await _store.SaveAsync(basket, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GetAsync_WithoutStoredBasket_ReturnsEmptyBasketWithoutAskingTheCatalog()
    {
        var result = await CreateService().GetAsync(Buyer, TestContext.Current.CancellationToken);

        var success = Assert.IsType<BasketResult.Success>(result);
        Assert.Equal(Buyer.Value, success.Basket.Basket.BuyerId);
        Assert.Empty(success.Basket.Basket.Items);
        Assert.Equal(0, _catalog.LookupCount);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task GetAsync_WithLines_NamesThemFromTheCatalogInOneCall()
    {
        await SeedBasketAsync((Mug, 2), (Poster, 1));

        var result = await CreateService().GetAsync(Buyer, TestContext.Current.CancellationToken);

        var success = Assert.IsType<BasketResult.Success>(result);
        Assert.Equal("Aspire Ceramic Mug", success.Basket.ProductNames[Mug.Id]);
        Assert.Equal("Aspire Launch Poster", success.Basket.ProductNames[Poster.Id]);
        Assert.Equal(1, _catalog.LookupCount);
    }

    [Fact]
    public async Task GetAsync_WhenCatalogUnavailable_ReturnsCatalogUnavailable()
    {
        await SeedBasketAsync((Mug, 1));
        _catalog.Unavailable = true;

        var result = await CreateService().GetAsync(Buyer, TestContext.Current.CancellationToken);

        Assert.IsType<BasketResult.CatalogUnavailable>(result);
    }

    [Fact]
    public async Task UpsertItemAsync_WithKnownProduct_StoresLineWithCatalogPriceAndNamesEveryLine()
    {
        await SeedBasketAsync((Poster, 1));

        var result = await CreateService().UpsertItemAsync(Buyer, Mug.Id, 2, TestContext.Current.CancellationToken);

        var success = Assert.IsType<UpsertItemResult.Success>(result);
        Assert.False(success.Capped);
        Assert.Equal(2, success.AppliedQuantity);
        var line = success.Basket.Basket.Items.Single(item => item.ProductId == Mug.Id);
        Assert.Equal(12.50m, line.UnitPrice);
        Assert.Equal("Aspire Ceramic Mug", success.Basket.ProductNames[Mug.Id]);
        Assert.Equal("Aspire Launch Poster", success.Basket.ProductNames[Poster.Id]);
        Assert.Equal(1, _catalog.LookupCount);
        Assert.Equal(2, _store.SaveCount);
    }

    [Fact]
    public async Task UpsertItemAsync_BeyondStock_CapsAndFlags()
    {
        var result = await CreateService().UpsertItemAsync(Buyer, Poster.Id, 5, TestContext.Current.CancellationToken);

        var success = Assert.IsType<UpsertItemResult.Success>(result);
        Assert.True(success.Capped);
        Assert.Equal(5, success.RequestedQuantity);
        Assert.Equal(2, success.AppliedQuantity);
    }

    [Fact]
    public async Task UpsertItemAsync_WithUnknownProduct_ReturnsProductNotFound()
    {
        var unknown = Guid.Parse("a0000000-0000-4000-8000-0000000000ff");

        var result = await CreateService().UpsertItemAsync(Buyer, unknown, 1, TestContext.Current.CancellationToken);

        var notFound = Assert.IsType<UpsertItemResult.ProductNotFound>(result);
        Assert.Equal(unknown, notFound.ProductId);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task UpsertItemAsync_WithOutOfStockProduct_ReturnsOutOfStock()
    {
        var result = await CreateService().UpsertItemAsync(Buyer, LimitedHoodie.Id, 1, TestContext.Current.CancellationToken);

        var outOfStock = Assert.IsType<UpsertItemResult.OutOfStock>(result);
        Assert.Equal("Aspire Limited Edition Hoodie", outOfStock.ProductName);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task UpsertItemAsync_WhenCatalogUnavailable_ReturnsCatalogUnavailable()
    {
        _catalog.Unavailable = true;

        var result = await CreateService().UpsertItemAsync(Buyer, Mug.Id, 1, TestContext.Current.CancellationToken);

        Assert.IsType<UpsertItemResult.CatalogUnavailable>(result);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task RemoveItemAsync_RemovesLineSavesAndNamesTheRest()
    {
        await SeedBasketAsync((Mug, 2), (Poster, 1));

        var result = await CreateService().RemoveItemAsync(Buyer, Mug.Id, TestContext.Current.CancellationToken);

        var success = Assert.IsType<BasketResult.Success>(result);
        var remaining = Assert.Single(success.Basket.Basket.Items);
        Assert.Equal(Poster.Id, remaining.ProductId);
        Assert.Equal("Aspire Launch Poster", success.Basket.ProductNames[Poster.Id]);
        Assert.Equal(2, _store.SaveCount);
    }

    [Fact]
    public async Task RemoveItemAsync_WithUnknownLine_ReturnsBasketWithoutSaving()
    {
        var result = await CreateService().RemoveItemAsync(Buyer, Mug.Id, TestContext.Current.CancellationToken);

        var success = Assert.IsType<BasketResult.Success>(result);
        Assert.Empty(success.Basket.Basket.Items);
        Assert.Equal(0, _store.SaveCount);
    }
}
