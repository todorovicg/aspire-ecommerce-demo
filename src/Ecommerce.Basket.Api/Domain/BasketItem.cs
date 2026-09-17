namespace Ecommerce.Basket.Api.Domain;

// Product names are not stored; Catalog names every line in the request's language when the basket is read
public sealed record BasketItem(Guid ProductId, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
