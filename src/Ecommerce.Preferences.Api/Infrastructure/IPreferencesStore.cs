using Ecommerce.Preferences.Api.Domain;

namespace Ecommerce.Preferences.Api.Infrastructure;

public interface IPreferencesStore
{
    Task<BuyerPreferences?> GetAsync(Guid buyerId, CancellationToken cancellationToken);

    Task SaveAsync(Guid buyerId, BuyerPreferences preferences, CancellationToken cancellationToken);
}
