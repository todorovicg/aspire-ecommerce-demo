using Ecommerce.Ordering.Api.Domain;
using Ecommerce.Ordering.Api.Infrastructure;

namespace Ecommerce.Ordering.Api.Tests.Fakes;

public sealed class InMemoryOrderStore : IOrderStore
{
    public Dictionary<Guid, Order> Orders { get; } = [];

    public int UpdateCount { get; private set; }

    public Task<bool> InsertIfAbsentAsync(Order order, CancellationToken cancellationToken) =>
        Task.FromResult(Orders.TryAdd(order.Id, order));

    public Task<IReadOnlyList<Order>> ListForBuyerAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Order> orders = Orders.Values.Where(order => order.BuyerId == buyerId).OrderByDescending(order => order.CreatedAt).ToList();
        return Task.FromResult(orders);
    }

    public Task<Order?> GetAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken) =>
        Task.FromResult(Orders.TryGetValue(orderId, out var order) && order.BuyerId == buyerId ? order : null);

    public Task<Order?> ClaimDueAsync(DateTimeOffset now, DateTimeOffset leaseUntil, CancellationToken cancellationToken)
    {
        var due = Orders.Values
            .Where(order => order.Status is OrderStatus.Submitted or OrderStatus.Paid)
            .Where(order => order.NextAttemptAt <= now)
            .Where(order => order.LeaseUntil is null || order.LeaseUntil < now)
            .OrderBy(order => order.NextAttemptAt)
            .FirstOrDefault();
        if (due is not null)
        {
            due.LeaseUntil = leaseUntil;
        }

        return Task.FromResult(due);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        Orders[order.Id] = order;
        UpdateCount++;
        return Task.CompletedTask;
    }
}
