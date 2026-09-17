using Ecommerce.Catalog.Api.Domain;

namespace Ecommerce.Catalog.Api.Tests.Domain;

public sealed class ProductTests
{
    private static readonly Guid ValidId = Guid.Parse("a0000000-0000-4000-8000-000000000001");

    [Fact]
    public void Create_WithValidValues_SetsAllProperties()
    {
        var product = Product.Create(ValidId, "Aspire Ceramic Mug", "A mug", "Drinkware", 12.50m, 25, "mug");

        Assert.Equal(ValidId, product.Id);
        Assert.Equal("Aspire Ceramic Mug", product.Name);
        Assert.Equal("A mug", product.Description);
        Assert.Equal("Drinkware", product.Category);
        Assert.Equal(12.50m, product.Price);
        Assert.Equal(25, product.AvailableStock);
        Assert.Equal("mug", product.ImageKey);
    }

    [Fact]
    public void Create_TrimsTextFields()
    {
        var product = Product.Create(ValidId, "  Mug ", " A mug ", " Drinkware ", 1m, 1, " mug ");

        Assert.Equal("Mug", product.Name);
        Assert.Equal("A mug", product.Description);
        Assert.Equal("Drinkware", product.Category);
        Assert.Equal("mug", product.ImageKey);
    }

    [Fact]
    public void Create_WithZeroStock_IsAllowed()
    {
        var product = Product.Create(ValidId, "Mug", "A mug", "Drinkware", 1m, 0, "mug");

        Assert.Equal(0, product.AvailableStock);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Product.Create(Guid.Empty, "Mug", "A mug", "Drinkware", 1m, 1, "mug"));
    }

    [Fact]
    public void Create_WithBlankName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Product.Create(ValidId, " ", "A mug", "Drinkware", 1m, 1, "mug"));
    }

    [Fact]
    public void Create_WithZeroPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create(ValidId, "Mug", "A mug", "Drinkware", 0m, 1, "mug"));
    }

    [Fact]
    public void Create_WithNegativeStock_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Product.Create(ValidId, "Mug", "A mug", "Drinkware", 1m, -1, "mug"));
    }

    [Fact]
    public void WithTranslation_AddsOnePerLanguageAndReplacesTheSameLanguage()
    {
        var product = Product.Create(ValidId, "Mug", "A mug", "Drinkware", 1m, 1, "mug")
            .WithTranslation("sr", "Šolja", "Jedna šolja", "Posuđe")
            .WithTranslation("sr", "Keramička šolja", "Jedna šolja", "Posuđe");

        var translation = Assert.Single(product.Translations);
        Assert.Equal("Keramička šolja", translation.Name);
    }

    [Fact]
    public void Localize_WithTranslation_ReturnsTheTranslatedText()
    {
        var product = Product.Create(ValidId, "Mug", "A mug", "Drinkware", 1m, 1, "mug")
            .WithTranslation("sr", "Šolja", "Jedna šolja", "Posuđe");

        var text = product.Localize("sr");

        Assert.Equal(new ProductText("Šolja", "Jedna šolja", "Posuđe"), text);
    }

    [Fact]
    public void Localize_WithoutTranslation_FallsBackToTheProductText()
    {
        var product = Product.Create(ValidId, "Mug", "A mug", "Drinkware", 1m, 1, "mug")
            .WithTranslation("sr", "Šolja", "Jedna šolja", "Posuđe");

        Assert.Equal(new ProductText("Mug", "A mug", "Drinkware"), product.Localize("en"));
        Assert.Equal(new ProductText("Mug", "A mug", "Drinkware"), product.Localize("de"));
    }
}
