using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Features;
using Ecommerce.Basket.Api.Features.UpsertItem;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Basket.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Basket.Api.Tests.Features;

public sealed class UpsertItemEndpointTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));
    private static readonly CatalogProduct Mug = new(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", 12.50m, 25);
    private static readonly CatalogProduct LimitedHoodie = new(Guid.Parse("a0000000-0000-4000-8000-000000000006"), "Aspire Limited Edition Hoodie", 59.00m, 0);

    private readonly FakeCatalogClient _catalog = new FakeCatalogClient().With(Mug).With(LimitedHoodie);

    private BasketService CreateService() => new(new InMemoryBasketStore(), _catalog, NullLogger<BasketService>.Instance);

    [Fact]
    public async Task HandleAsync_WithKnownProduct_ReturnsOkWithBasket()
    {
        var result = await UpsertItemEndpoint.HandleAsync(Buyer, new UpsertItemRequest(Mug.Id, 3), CreateService(), TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<UpsertItemResponse>>>(result.Result);
        Assert.NotNull(ok.Value?.Data);
        Assert.Equal(3, ok.Value.Data.AppliedQuantity);
        Assert.Equal(3, ok.Value.Data.Basket.ItemCount);
        Assert.Equal(37.50m, ok.Value.Data.Basket.Total);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownProduct_ReturnsNotFoundEnvelope()
    {
        var result = await UpsertItemEndpoint.HandleAsync(Buyer, new UpsertItemRequest(Guid.NewGuid(), 1), CreateService(), TestContext.Current.CancellationToken);

        var notFound = Assert.IsType<NotFound<ApiResponse>>(result.Result);
        Assert.Equal("basket.product_not_found", notFound.Value?.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WithOutOfStockProduct_ReturnsConflictEnvelope()
    {
        var result = await UpsertItemEndpoint.HandleAsync(Buyer, new UpsertItemRequest(LimitedHoodie.Id, 1), CreateService(), TestContext.Current.CancellationToken);

        var conflict = Assert.IsType<Conflict<ApiResponse>>(result.Result);
        Assert.Equal("basket.product_out_of_stock", conflict.Value?.Error?.Code);
    }

    [Fact]
    public async Task HandleAsync_WhenCatalogUnavailable_ReturnsServiceUnavailableEnvelope()
    {
        _catalog.Unavailable = true;

        var result = await UpsertItemEndpoint.HandleAsync(Buyer, new UpsertItemRequest(Mug.Id, 1), CreateService(), TestContext.Current.CancellationToken);

        var unavailable = Assert.IsType<JsonHttpResult<ApiResponse>>(result.Result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, unavailable.StatusCode);
        Assert.Equal("basket.catalog_unavailable", unavailable.Value?.Error?.Code);
    }
}
