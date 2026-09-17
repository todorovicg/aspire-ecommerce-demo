using Ecommerce.Contracts;

namespace Ecommerce.Basket.Api.Infrastructure;

// Thrown when the event cannot be handed to the durable outbox, which is the only way a checkout cannot be submitted
public sealed class OutboxUnavailableException(Exception inner) : Exception("The order could not be stored for delivery", inner);

public interface IOrderPlacedPublisher
{
    Task PublishAsync(OrderPlaced orderPlaced, CancellationToken cancellationToken);
}
