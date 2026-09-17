using Ecommerce.Ordering.Api.Domain;

namespace Ecommerce.Ordering.Api.Infrastructure;

public interface IOrderStore
{
    // Returns false when an order with the same id already exists, so a redelivered event changes nothing
    Task<bool> InsertIfAbsentAsync(Order order, CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> ListForBuyerAsync(Guid buyerId, CancellationToken cancellationToken);

    Task<Order?> GetAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken);

    // Atomically leases the oldest due order (Submitted or Paid, NextAttemptAt reached, no live lease); null when nothing is due
    Task<Order?> ClaimDueAsync(DateTimeOffset now, DateTimeOffset leaseUntil, CancellationToken cancellationToken);

    Task UpdateAsync(Order order, CancellationToken cancellationToken);
}
