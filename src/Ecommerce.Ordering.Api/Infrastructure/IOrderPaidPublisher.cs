using Ecommerce.Contracts;

namespace Ecommerce.Ordering.Api.Infrastructure;

// Thrown when the event cannot be handed to the durable outbox
public sealed class OutboxUnavailableException(Exception inner) : Exception("The order event could not be stored for delivery", inner);

public interface IOrderPaidPublisher
{
    Task PublishAsync(OrderPaid orderPaid, CancellationToken cancellationToken);
}
