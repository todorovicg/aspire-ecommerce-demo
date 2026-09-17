using Ecommerce.Catalog.Api.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Ecommerce.Catalog.Api.Tests.Features;

public sealed class RequestLanguageTests
{
    [Fact]
    public async Task BindAsync_WithoutNegotiatedCulture_IsEnglish()
    {
        var language = await RequestLanguage.BindAsync(new DefaultHttpContext());

        Assert.Equal("en", language?.Code);
    }

    [Theory]
    [InlineData("sr-Latn", "sr")]
    [InlineData("sr", "sr")]
    [InlineData("en", "en")]
    public async Task BindAsync_WithNegotiatedCulture_UsesItsTwoLetterCode(string culture, string expected)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<IRequestCultureFeature>(new RequestCultureFeature(new RequestCulture(culture), null));

        var language = await RequestLanguage.BindAsync(context);

        Assert.Equal(expected, language?.Code);
    }
}
