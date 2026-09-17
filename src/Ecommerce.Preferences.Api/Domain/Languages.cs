namespace Ecommerce.Preferences.Api.Domain;

public static class Languages
{
    public const string Default = "en";

    public static readonly IReadOnlyList<string> Supported = ["en", "sr"];

    public static bool IsSupported(string? language) =>
        language is not null && Supported.Contains(language, StringComparer.Ordinal);
}
