using Ecommerce.Contracts;
using Wolverine;

namespace Ecommerce.Ordering.Api.Infrastructure;

// The order-paid exchange is a durable outbox endpoint in Program.cs, so PublishAsync persists the envelope before returning
public sealed class WolverineOrderPaidPublisher(IMessageBus bus) : IOrderPaidPublisher
{
    public async Task PublishAsync(OrderPaid orderPaid, CancellationToken cancellationToken)
    {
        try
        {
            await bus.PublishAsync(orderPaid);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            throw new OutboxUnavailableException(exception);
        }
    }
}
