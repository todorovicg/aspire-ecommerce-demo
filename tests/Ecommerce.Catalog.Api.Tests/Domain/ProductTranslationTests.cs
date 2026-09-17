using Ecommerce.Catalog.Api.Domain;

namespace Ecommerce.Catalog.Api.Tests.Domain;

public sealed class ProductTranslationTests
{
    [Fact]
    public void Create_TrimsTextAndNormalizesTheLanguageCode()
    {
        var translation = ProductTranslation.Create(" SR ", " Šolja ", " Opis ", " Posuđe ");

        Assert.Equal("sr", translation.Language);
        Assert.Equal("Šolja", translation.Name);
        Assert.Equal("Opis", translation.Description);
        Assert.Equal("Posuđe", translation.Category);
    }

    [Fact]
    public void Create_WithBlankLanguage_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ProductTranslation.Create(" ", "Šolja", "Opis", "Posuđe"));
    }

    [Fact]
    public void Create_WithBlankName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ProductTranslation.Create("sr", " ", "Opis", "Posuđe"));
    }
}
