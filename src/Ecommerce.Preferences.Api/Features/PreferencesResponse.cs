using Ecommerce.Preferences.Api.Domain;

namespace Ecommerce.Preferences.Api.Features;

public sealed record PreferencesResponse(string Language)
{
    public static PreferencesResponse From(BuyerPreferences preferences) => new(preferences.Language);
}
