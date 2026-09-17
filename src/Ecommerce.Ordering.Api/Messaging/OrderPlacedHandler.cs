using Ecommerce.Contracts;
using Ecommerce.Ordering.Api.Domain;
using Ecommerce.Ordering.Api.Infrastructure;

namespace Ecommerce.Ordering.Api.Messaging;

public sealed class OrderPlacedHandler
{
    public static async Task HandleAsync(
        OrderPlaced message,
        IOrderStore store,
        TimeProvider timeProvider,
        ILogger<OrderPlacedHandler> logger,
        CancellationToken cancellationToken)
    {
        var order = Order.FromPlaced(message, timeProvider.GetUtcNow());
        var inserted = await store.InsertIfAbsentAsync(order, cancellationToken);
        if (inserted)
        {
            logger.LogInformation("[{Prefix}] Stored order [{OrderId}] for buyer [{BuyerId}] with [{Lines}] lines total [{Total}]", nameof(OrderPlacedHandler), order.Id, order.BuyerId, order.Lines.Count, order.Total);
        }
        else
        {
            logger.LogInformation("[{Prefix}] Order [{OrderId}] already stored, redelivery ignored", nameof(OrderPlacedHandler), order.Id);
        }
    }
}
