using Ecommerce.Preferences.Api.Domain;
using Ecommerce.Preferences.Api.Infrastructure;

namespace Ecommerce.Preferences.Api.Tests.Fakes;

public sealed class InMemoryPreferencesStore : IPreferencesStore
{
    private readonly Dictionary<Guid, BuyerPreferences> _preferences = [];

    public int SaveCount { get; private set; }

    public Task<BuyerPreferences?> GetAsync(Guid buyerId, CancellationToken cancellationToken) =>
        Task.FromResult(_preferences.GetValueOrDefault(buyerId));

    public Task SaveAsync(Guid buyerId, BuyerPreferences preferences, CancellationToken cancellationToken)
    {
        _preferences[buyerId] = preferences;
        SaveCount++;
        return Task.CompletedTask;
    }
}
