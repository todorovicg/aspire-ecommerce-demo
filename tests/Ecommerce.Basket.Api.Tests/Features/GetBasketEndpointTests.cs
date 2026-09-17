using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Features;
using Ecommerce.Basket.Api.Features.GetBasket;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Basket.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Basket.Api.Tests.Features;

public sealed class GetBasketEndpointTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));
    private static readonly CatalogProduct Mug = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire keramička šolja", 12.50m, 25);

    private readonly InMemoryBasketStore _store = new();
    private readonly FakeCatalogClient _catalog = new FakeCatalogClient().With(Mug);

    private BasketService CreateService() => new(_store, _catalog, NullLogger<BasketService>.Instance);

    private async Task SeedMugAsync()
    {
        var basket = new ShoppingBasket(Buyer.Value);
        basket.UpsertItem(Mug.Id, Mug.Price, 2, 25);
        await _store.SaveAsync(basket, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleAsync_NamesLinesAsTheCatalogReturnsThem()
    {
        await SeedMugAsync();

        var result = await GetBasketEndpoint.HandleAsync(Buyer, CreateService(), TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<BasketResponse>>>(result.Result);
        var item = Assert.Single(ok.Value?.Data?.Items ?? []);
        Assert.Equal("Aspire keramička šolja", item.ProductName);
        Assert.Equal(25.00m, item.LineTotal);
    }

    [Fact]
    public async Task HandleAsync_WhenCatalogUnavailable_ReturnsServiceUnavailableEnvelope()
    {
        await SeedMugAsync();
        _catalog.Unavailable = true;

        var result = await GetBasketEndpoint.HandleAsync(Buyer, CreateService(), TestContext.Current.CancellationToken);

        var unavailable = Assert.IsType<JsonHttpResult<ApiResponse>>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, unavailable.StatusCode);
        Assert.Equal("basket.catalog_unavailable", unavailable.Value?.Error?.Code);
    }
}
