namespace Ecommerce.Basket.Api.Features;

public sealed record BasketItemResponse(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record BasketResponse(Guid BuyerId, IReadOnlyList<BasketItemResponse> Items, int ItemCount, decimal Total)
{
    // A product the catalog no longer knows is shown by its id
    public static BasketResponse From(NamedBasket named) =>
        new(
            named.Basket.BuyerId,
            named.Basket.Items
                .Select(item => new BasketItemResponse(
                    item.ProductId,
                    named.ProductNames.GetValueOrDefault(item.ProductId, item.ProductId.ToString()),
                    item.UnitPrice,
                    item.Quantity,
                    item.LineTotal))
                .ToList(),
            named.Basket.ItemCount,
            named.Basket.Total);
}
