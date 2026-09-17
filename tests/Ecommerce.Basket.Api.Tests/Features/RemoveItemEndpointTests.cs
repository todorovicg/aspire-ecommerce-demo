using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Features;
using Ecommerce.Basket.Api.Features.RemoveItem;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Basket.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Basket.Api.Tests.Features;

public sealed class RemoveItemEndpointTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));
    private static readonly CatalogProduct Mug = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", 12.50m, 25);
    private static readonly CatalogProduct Poster = new(Guid.Parse("a0000000-0000-4000-8000-000000000010"), "Aspire Launch Poster", 15.00m, 2);

    [Fact]
    public async Task HandleAsync_RemovesTheLineAndReturnsTheNamedRest()
    {
        var store = new InMemoryBasketStore();
        var basket = new ShoppingBasket(Buyer.Value);
        basket.UpsertItem(Mug.Id, Mug.Price, 1, 25);
        basket.UpsertItem(Poster.Id, Poster.Price, 1, 2);
        await store.SaveAsync(basket, TestContext.Current.CancellationToken);
        var service = new BasketService(store, new FakeCatalogClient().With(Mug).With(Poster), NullLogger<BasketService>.Instance);

        var result = await RemoveItemEndpoint.HandleAsync(Buyer, Mug.Id, service, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<BasketResponse>>>(result.Result);
        var item = Assert.Single(ok.Value?.Data?.Items ?? []);
        Assert.Equal("Aspire Launch Poster", item.ProductName);
    }
}
