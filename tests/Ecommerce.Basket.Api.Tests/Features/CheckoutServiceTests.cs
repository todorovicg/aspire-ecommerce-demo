using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Features.Checkout;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Basket.Api.Tests.Fakes;
using Ecommerce.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Basket.Api.Tests.Features;

public sealed class CheckoutServiceTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));
    private static readonly CatalogProduct Mug = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", 12.50m, 25);
    private static readonly CatalogProduct Poster = new(Guid.Parse("a0000000-0000-4000-8000-000000000010"), "Aspire Launch Poster", 15.00m, 2);
    private static readonly CheckoutDetails Details = new(
        new OrderBuyer("Goran", "goran@example.com"),
        new OrderAddress("Main Street 1", "Belgrade", "11000", "Serbia"),
        new OrderCard("4242424242424242", "Goran"));
    private static readonly DateTimeOffset Now = new(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);

    private readonly InMemoryBasketStore _store = new();
    private readonly FakeCatalogClient _catalog = new FakeCatalogClient().With(Mug).With(Poster);
    private readonly FakeOrderPlacedPublisher _publisher = new();

    private CheckoutService CreateService() =>
        new(_store, _catalog, _publisher, new FakeTimeProvider(Now), NullLogger<CheckoutService>.Instance);

    private async Task SeedBasketAsync(params (CatalogProduct Product, int Quantity, decimal StoredPrice)[] lines)
    {
        var basket = new ShoppingBasket(Buyer.Value);
        foreach (var (product, quantity, storedPrice) in lines)
        {
            basket.UpsertItem(product.Id, storedPrice, quantity, 99);
        }

        await _store.SaveAsync(basket, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task CheckoutAsync_WithoutBasket_ReturnsEmptyBasket()
    {
        var result = await CreateService().CheckoutAsync(Buyer, Details, TestContext.Current.CancellationToken);

        Assert.IsType<CheckoutResult.EmptyBasket>(result);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task CheckoutAsync_PublishesOrderWithCurrentCatalogPricesAndClearsBasket()
    {
        await SeedBasketAsync((Mug, 2, 10.00m), (Poster, 1, 15.00m));

        var result = await CreateService().CheckoutAsync(Buyer, Details, TestContext.Current.CancellationToken);

        var placed = Assert.IsType<CheckoutResult.Placed>(result);
        var message = Assert.Single(_publisher.Published);
        Assert.Equal(placed.OrderId, message.OrderId);
        Assert.Equal(Buyer.Value, message.BuyerId);
        Assert.Equal(40.00m, placed.Total);
        Assert.Equal(40.00m, message.Total);
        Assert.Equal("EUR", message.Currency);
        Assert.Equal(Now, message.PlacedAt);
        Assert.Equal(12.50m, message.Lines.Single(line => line.ProductId == Mug.Id).UnitPrice);
        Assert.Equal("4242424242424242", message.Card.Number);
        Assert.Null(await _store.GetAsync(Buyer.Value, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CheckoutAsync_WithLineBeyondStock_ReturnsOutOfStockAndKeepsBasket()
    {
        await SeedBasketAsync((Mug, 1, 12.50m), (Poster, 3, 15.00m));

        var result = await CreateService().CheckoutAsync(Buyer, Details, TestContext.Current.CancellationToken);

        var outOfStock = Assert.IsType<CheckoutResult.OutOfStock>(result);
        var line = Assert.Single(outOfStock.Lines);
        Assert.Equal(Poster.Id, line.ProductId);
        Assert.Equal(3, line.Requested);
        Assert.Equal(2, line.Available);
        Assert.Empty(_publisher.Published);
        Assert.NotNull(await _store.GetAsync(Buyer.Value, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CheckoutAsync_WithProductNoLongerInCatalog_ReportsItWithZeroAvailable()
    {
        var vanished = new CatalogProduct(Guid.Parse("a0000000-0000-4000-8000-0000000000ee"), "Vanished", 1m, 1);
        await SeedBasketAsync((vanished, 1, 1m));

        var result = await CreateService().CheckoutAsync(Buyer, Details, TestContext.Current.CancellationToken);

        var outOfStock = Assert.IsType<CheckoutResult.OutOfStock>(result);
        Assert.Equal(0, Assert.Single(outOfStock.Lines).Available);
    }

    [Fact]
    public async Task CheckoutAsync_WhenCatalogUnavailable_ReturnsCatalogUnavailable()
    {
        await SeedBasketAsync((Mug, 1, 12.50m));
        _catalog.Unavailable = true;

        var result = await CreateService().CheckoutAsync(Buyer, Details, TestContext.Current.CancellationToken);

        Assert.IsType<CheckoutResult.CatalogUnavailable>(result);
        Assert.NotNull(await _store.GetAsync(Buyer.Value, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CheckoutAsync_WhenOutboxUnavailable_ReturnsOutboxUnavailableAndKeepsBasket()
    {
        await SeedBasketAsync((Mug, 1, 12.50m));
        _publisher.Unavailable = true;

        var result = await CreateService().CheckoutAsync(Buyer, Details, TestContext.Current.CancellationToken);

        Assert.IsType<CheckoutResult.OutboxUnavailable>(result);
        Assert.NotNull(await _store.GetAsync(Buyer.Value, TestContext.Current.CancellationToken));
    }
}
