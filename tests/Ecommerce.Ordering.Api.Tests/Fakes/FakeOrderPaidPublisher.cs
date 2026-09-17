using Ecommerce.Contracts;
using Ecommerce.Ordering.Api.Infrastructure;

namespace Ecommerce.Ordering.Api.Tests.Fakes;

public sealed class FakeOrderPaidPublisher : IOrderPaidPublisher
{
    public bool Unavailable { get; set; }

    public List<OrderPaid> Published { get; } = [];

    public Task PublishAsync(OrderPaid orderPaid, CancellationToken cancellationToken)
    {
        if (Unavailable)
        {
            throw new OutboxUnavailableException(new InvalidOperationException("Message store unreachable"));
        }

        Published.Add(orderPaid);
        return Task.CompletedTask;
    }
}
