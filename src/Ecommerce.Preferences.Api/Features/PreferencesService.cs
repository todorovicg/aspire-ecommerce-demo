using Ecommerce.ApiDefaults;
using Ecommerce.Preferences.Api.Domain;
using Ecommerce.Preferences.Api.Infrastructure;

namespace Ecommerce.Preferences.Api.Features;

public abstract record UpdatePreferencesResult
{
    public sealed record Updated(BuyerPreferences Preferences) : UpdatePreferencesResult;

    public sealed record UnsupportedLanguage(string Language) : UpdatePreferencesResult;
}

public sealed class PreferencesService(IPreferencesStore store, ILogger<PreferencesService> logger)
{
    public async Task<BuyerPreferences> GetAsync(BuyerId buyerId, CancellationToken cancellationToken) =>
        await store.GetAsync(buyerId.Value, cancellationToken) ?? BuyerPreferences.Default;

    public async Task<UpdatePreferencesResult> UpdateAsync(BuyerId buyerId, string language, CancellationToken cancellationToken)
    {
        if (!Languages.IsSupported(language))
        {
            return new UpdatePreferencesResult.UnsupportedLanguage(language);
        }

        var preferences = new BuyerPreferences(language);
        await store.SaveAsync(buyerId.Value, preferences, cancellationToken);
        logger.LogInformation("[{Prefix}] Set language [{Language}] for buyer [{BuyerId}]", nameof(PreferencesService), language, buyerId.Value);

        return new UpdatePreferencesResult.Updated(preferences);
    }
}
