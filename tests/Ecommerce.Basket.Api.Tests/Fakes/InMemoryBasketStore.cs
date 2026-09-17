using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Infrastructure;

namespace Ecommerce.Basket.Api.Tests.Fakes;

public sealed class InMemoryBasketStore : IBasketStore
{
    private readonly Dictionary<Guid, ShoppingBasket> _baskets = [];

    public int SaveCount { get; private set; }

    public Task<ShoppingBasket?> GetAsync(Guid buyerId, CancellationToken cancellationToken) =>
        Task.FromResult(_baskets.TryGetValue(buyerId, out var basket) ? new ShoppingBasket(basket.BuyerId, basket.Items) : null);

    public Task SaveAsync(ShoppingBasket basket, CancellationToken cancellationToken)
    {
        _baskets[basket.BuyerId] = new ShoppingBasket(basket.BuyerId, basket.Items);
        SaveCount++;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        _baskets.Remove(buyerId);
        return Task.CompletedTask;
    }
}
