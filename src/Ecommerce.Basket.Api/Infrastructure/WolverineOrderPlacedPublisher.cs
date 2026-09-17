using Ecommerce.Contracts;
using Wolverine;

namespace Ecommerce.Basket.Api.Infrastructure;

// The exchange is a durable outbox endpoint in Program.cs: PublishAsync persists the envelope in the message store
// before returning, and Wolverine delivers it to the broker as soon as the broker is reachable
public sealed class WolverineOrderPlacedPublisher(IMessageBus bus) : IOrderPlacedPublisher
{
    public async Task PublishAsync(OrderPlaced orderPlaced, CancellationToken cancellationToken)
    {
        try
        {
            await bus.PublishAsync(orderPlaced);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new OutboxUnavailableException(exception);
        }
    }
}
