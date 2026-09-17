using Ecommerce.Ordering.Api.Messaging;
using Ecommerce.Ordering.Api.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Ordering.Api.Tests.Messaging;

public sealed class OrderPlacedHandlerTests
{
    private readonly InMemoryOrderStore _store = new();
    private readonly FakeTimeProvider _time = new(TestOrders.PlacedAt.AddSeconds(2));

    [Fact]
    public async Task HandleAsync_StoresTheOrder()
    {
        await OrderPlacedHandler.HandleAsync(TestOrders.Placed(), _store, _time, NullLogger<OrderPlacedHandler>.Instance, TestContext.Current.CancellationToken);

        var order = Assert.Single(_store.Orders.Values);
        Assert.Equal(TestOrders.OrderId, order.Id);
        Assert.Equal(_time.GetUtcNow(), order.NextAttemptAt);
    }

    [Fact]
    public async Task HandleAsync_IgnoresRedelivery()
    {
        await OrderPlacedHandler.HandleAsync(TestOrders.Placed(), _store, _time, NullLogger<OrderPlacedHandler>.Instance, TestContext.Current.CancellationToken);
        _store.Orders[TestOrders.OrderId].Status = Ecommerce.Ordering.Api.Domain.OrderStatus.Paid;

        await OrderPlacedHandler.HandleAsync(TestOrders.Placed(), _store, _time, NullLogger<OrderPlacedHandler>.Instance, TestContext.Current.CancellationToken);

        Assert.Single(_store.Orders);
        Assert.Equal(Ecommerce.Ordering.Api.Domain.OrderStatus.Paid, _store.Orders[TestOrders.OrderId].Status);
    }
}
