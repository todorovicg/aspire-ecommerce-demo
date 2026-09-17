using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Features.Checkout;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Basket.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Basket.Api.Tests.Features;

public sealed class CheckoutEndpointTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));
    private static readonly CatalogProduct Mug = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", 12.50m, 25);
    private static readonly CheckoutRequest Request = new(
        new CheckoutBuyer("Goran", "goran@example.com"),
        new CheckoutAddress("Main Street 1", "Belgrade", "11000", "Serbia"),
        new CheckoutCard("4242424242424242", "Goran"));

    private readonly InMemoryBasketStore _store = new();
    private readonly FakeOrderPlacedPublisher _publisher = new();

    private CheckoutService CreateService() =>
        new(_store, new FakeCatalogClient().With(Mug), _publisher, new FakeTimeProvider(), NullLogger<CheckoutService>.Instance);

    [Fact]
    public async Task HandleAsync_WithEmptyBasket_ReturnsBadRequestEnvelope()
    {
        var result = await CheckoutEndpoint.HandleAsync(Buyer, Request, CreateService(), TestContext.Current.CancellationToken);

        var badRequest = Assert.IsType<BadRequest<ApiResponse>>(result.Result);
        Assert.Equal("basket.checkout_empty", badRequest.Value?.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WithItems_ReturnsAcceptedWithOrderId()
    {
        var basket = new ShoppingBasket(Buyer.Value);
        basket.UpsertItem(Mug.Id, Mug.Price, 2, Mug.AvailableStock);
        await _store.SaveAsync(basket, TestContext.Current.CancellationToken);

        var result = await CheckoutEndpoint.HandleAsync(Buyer, Request, CreateService(), TestContext.Current.CancellationToken);

        var accepted = Assert.IsType<Accepted<ApiResponse<CheckoutResponse>>>(result.Result);
        Assert.Equal(StatusCodes.Status202Accepted, accepted.StatusCode);
        Assert.NotNull(accepted.Value?.Data);
        Assert.Equal(25.00m, accepted.Value.Data.Total);
        Assert.Equal("EUR", accepted.Value.Data.Currency);
        Assert.Equal(accepted.Value.Data.OrderId, Assert.Single(_publisher.Published).OrderId);
    }

    [Fact]
    public async Task HandleAsync_WhenOutboxUnavailable_ReturnsServiceUnavailableEnvelope()
    {
        var basket = new ShoppingBasket(Buyer.Value);
        basket.UpsertItem(Mug.Id, Mug.Price, 1, Mug.AvailableStock);
        await _store.SaveAsync(basket, TestContext.Current.CancellationToken);
        _publisher.Unavailable = true;

        var result = await CheckoutEndpoint.HandleAsync(Buyer, Request, CreateService(), TestContext.Current.CancellationToken);

        var unavailable = Assert.IsType<JsonHttpResult<ApiResponse>>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, unavailable.StatusCode);
        Assert.Equal("basket.outbox_unavailable", unavailable.Value?.Error?.Code);
    }
}
