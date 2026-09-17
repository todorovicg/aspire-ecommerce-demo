using Ecommerce.Preferences.Api.Domain;

namespace Ecommerce.Preferences.Api.Tests.Domain;

public sealed class LanguagesTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("sr")]
    public void IsSupported_WithAgreedLanguages_ReturnsTrue(string language)
    {
        Assert.True(Languages.IsSupported(language));
    }

    [Theory]
    [InlineData("de")]
    [InlineData("EN")]
    [InlineData("sr-Latn")]
    [InlineData("")]
    [InlineData(null)]
    public void IsSupported_WithAnythingElse_ReturnsFalse(string? language)
    {
        Assert.False(Languages.IsSupported(language));
    }

    [Fact]
    public void Default_IsEnglish()
    {
        Assert.Equal("en", Languages.Default);
        Assert.Equal("en", BuyerPreferences.Default.Language);
    }
}
