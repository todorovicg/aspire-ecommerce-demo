using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Contracts;

namespace Ecommerce.Basket.Api.Tests.Fakes;

public sealed class FakeOrderPlacedPublisher : IOrderPlacedPublisher
{
    public List<OrderPlaced> Published { get; } = [];

    public bool Unavailable { get; set; }

    public Task PublishAsync(OrderPlaced orderPlaced, CancellationToken cancellationToken)
    {
        if (Unavailable)
        {
            throw new OutboxUnavailableException(new InvalidOperationException("Message store unreachable"));
        }

        Published.Add(orderPlaced);
        return Task.CompletedTask;
    }
}
