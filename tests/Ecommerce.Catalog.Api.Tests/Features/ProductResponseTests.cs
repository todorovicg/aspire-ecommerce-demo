using Ecommerce.Catalog.Api.Domain;
using Ecommerce.Catalog.Api.Features;

namespace Ecommerce.Catalog.Api.Tests.Features;

public sealed class ProductResponseTests
{
    private static readonly Guid Id = Guid.Parse("a0000000-0000-4000-8000-000000000001");

    private static Product CreateMug() =>
        Product.Create(Id, "Aspire Ceramic Mug", "A mug", "Drinkware", 12.50m, 25, "mug")
            .WithTranslation("sr", "Aspire keramička šolja", "Jedna šolja", "Posuđe za piće");

    [Fact]
    public void From_WithEnglish_MapsEveryField()
    {
        var response = ProductResponse.From(CreateMug(), "en");

        Assert.Equal(Id, response.Id);
        Assert.Equal("Aspire Ceramic Mug", response.Name);
        Assert.Equal("A mug", response.Description);
        Assert.Equal("Drinkware", response.Category);
        Assert.Equal(12.50m, response.Price);
        Assert.Equal(25, response.AvailableStock);
        Assert.Equal("mug", response.ImageKey);
    }

    [Fact]
    public void From_WithTranslatedLanguage_MapsTheTranslatedTextAndKeepsTheRest()
    {
        var response = ProductResponse.From(CreateMug(), "sr");

        Assert.Equal("Aspire keramička šolja", response.Name);
        Assert.Equal("Jedna šolja", response.Description);
        Assert.Equal("Posuđe za piće", response.Category);
        Assert.Equal(12.50m, response.Price);
        Assert.Equal("mug", response.ImageKey);
    }
}
