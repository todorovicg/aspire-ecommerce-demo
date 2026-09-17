using Microsoft.AspNetCore.Localization;

namespace Ecommerce.Catalog.Api.Features;

// Bound from the culture that request localization negotiated from Accept-Language; English when nothing supported was asked for
public sealed record RequestLanguage(string Code)
{
    public const string Default = "en";

    public static readonly RequestLanguage English = new(Default);

    public static ValueTask<RequestLanguage?> BindAsync(HttpContext context)
    {
        var culture = context.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture;
        var language = culture is null ? Default : culture.TwoLetterISOLanguageName;
        return ValueTask.FromResult<RequestLanguage?>(new RequestLanguage(language));
    }
}
