using System.Text.Json;
using Ecommerce.Preferences.Api.Domain;
using StackExchange.Redis;

namespace Ecommerce.Preferences.Api.Infrastructure;

// Preferences never expire; a buyer's choice must survive any number of refreshes and restarts
public sealed class RedisPreferencesStore(IConnectionMultiplexer redis) : IPreferencesStore
{
    public async Task<BuyerPreferences?> GetAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        var value = await redis.GetDatabase().StringGetAsync(KeyFor(buyerId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<BuyerPreferences>((string)value!, JsonSerializerOptions.Web);
    }

    public Task SaveAsync(Guid buyerId, BuyerPreferences preferences, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(preferences, JsonSerializerOptions.Web);
        return redis.GetDatabase().StringSetAsync(KeyFor(buyerId), json);
    }

    private static string KeyFor(Guid buyerId) => $"preferences:{buyerId}";
}
