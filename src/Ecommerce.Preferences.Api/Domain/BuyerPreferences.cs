namespace Ecommerce.Preferences.Api.Domain;

public sealed record BuyerPreferences(string Language)
{
    public static readonly BuyerPreferences Default = new(Languages.Default);
}
