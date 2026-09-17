using Ecommerce.ApiDefaults;
using Ecommerce.Ordering.Api.Domain;
using Ecommerce.Ordering.Api.Features;
using Ecommerce.Ordering.Api.Features.GetOrder;
using Ecommerce.Ordering.Api.Features.ListOrders;
using Ecommerce.Ordering.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Ordering.Api.Tests.Features;

public sealed class OrderEndpointsTests
{
    private static readonly BuyerId Buyer = new(TestOrders.BuyerId);
    private readonly InMemoryOrderStore _store = new();

    [Fact]
    public async Task ListOrders_ReturnsTheBuyersOrdersNewestFirst()
    {
        var older = Order.FromPlaced(TestOrders.Placed(Guid.Parse("c0000000-0000-4000-8000-000000000001")), TestOrders.PlacedAt);
        var newer = Order.FromPlaced(TestOrders.Placed(Guid.Parse("c0000000-0000-4000-8000-000000000002")) with { PlacedAt = TestOrders.PlacedAt.AddMinutes(5) }, TestOrders.PlacedAt);
        _store.Orders[older.Id] = older;
        _store.Orders[newer.Id] = newer;

        var result = await ListOrdersEndpoint.HandleAsync(Buyer, _store, TestContext.Current.CancellationToken);

        Assert.NotNull(result.Value?.Data);
        Assert.Equal([newer.Id, older.Id], result.Value.Data.Select(order => order.Id));
        Assert.Equal("Submitted", result.Value.Data[0].Status);
    }

    [Fact]
    public async Task GetOrder_WithAnotherBuyersOrder_ReturnsNotFound()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);
        _store.Orders[order.Id] = order;

        var result = await GetOrderEndpoint.HandleAsync(new BuyerId(Guid.NewGuid()), order.Id, _store, TestContext.Current.CancellationToken);

        var notFound = Assert.IsType<NotFound<ApiResponse>>(result.Result);
        Assert.Equal("orders.not_found", notFound.Value?.Error?.Code);
    }

    [Fact]
    public async Task GetOrder_WithOwnOrder_ReturnsIt()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);
        _store.Orders[order.Id] = order;

        var result = await GetOrderEndpoint.HandleAsync(Buyer, order.Id, _store, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<OrderResponse>>>(result.Result);
        Assert.Equal("4242", ok.Value?.Data?.CardLast4);
        Assert.Equal(2, ok.Value?.Data?.Lines.Count);
    }
}
