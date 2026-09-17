using Ecommerce.Catalog.Api.Domain;

namespace Ecommerce.Catalog.Api.Features;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int AvailableStock,
    string ImageKey)
{
    public static ProductResponse From(Product product, string language)
    {
        var text = product.Localize(language);
        return new(product.Id, text.Name, text.Description, text.Category, product.Price, product.AvailableStock, product.ImageKey);
    }
}
