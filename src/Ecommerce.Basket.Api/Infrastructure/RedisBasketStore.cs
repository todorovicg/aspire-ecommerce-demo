using System.Text.Json;
using Ecommerce.Basket.Api.Domain;
using StackExchange.Redis;

namespace Ecommerce.Basket.Api.Infrastructure;

public sealed class RedisBasketStore(IConnectionMultiplexer redis) : IBasketStore
{
    private static readonly TimeSpan Expiry = TimeSpan.FromDays(7);

    public async Task<ShoppingBasket?> GetAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        var value = await redis.GetDatabase().StringGetAsync(KeyFor(buyerId));
        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var stored = JsonSerializer.Deserialize<StoredBasket>((string)value!, JsonSerializerOptions.Web);
        return stored is null ? null : new ShoppingBasket(stored.BuyerId, stored.Items);
    }

    public Task SaveAsync(ShoppingBasket basket, CancellationToken cancellationToken)
    {
        var stored = new StoredBasket(basket.BuyerId, basket.Items);
        var json = JsonSerializer.Serialize(stored, JsonSerializerOptions.Web);
        return redis.GetDatabase().StringSetAsync(KeyFor(basket.BuyerId), json, Expiry);
    }

    public Task DeleteAsync(Guid buyerId, CancellationToken cancellationToken) =>
        redis.GetDatabase().KeyDeleteAsync(KeyFor(buyerId));

    private static string KeyFor(Guid buyerId) => $"basket:{buyerId}";

    private sealed record StoredBasket(Guid BuyerId, IReadOnlyList<BasketItem> Items);
}
