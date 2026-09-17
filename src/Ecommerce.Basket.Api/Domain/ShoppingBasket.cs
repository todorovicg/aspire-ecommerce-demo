namespace Ecommerce.Basket.Api.Domain;

public sealed class ShoppingBasket
{
    public const int MaxQuantityPerLine = 99;

    private readonly List<BasketItem> _items;

    public ShoppingBasket(Guid buyerId)
        : this(buyerId, [])
    {
    }

    public ShoppingBasket(Guid buyerId, IEnumerable<BasketItem> items)
    {
        BuyerId = buyerId;
        _items = [.. items];
    }

    public Guid BuyerId { get; }

    public IReadOnlyList<BasketItem> Items => _items;

    public int ItemCount => _items.Sum(item => item.Quantity);

    public decimal Total => _items.Sum(item => item.LineTotal);

    // Sets the line quantity, capped at the available stock, and returns the quantity applied
    public int UpsertItem(Guid productId, decimal unitPrice, int requestedQuantity, int availableStock)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requestedQuantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(availableStock);

        var applied = Math.Min(Math.Min(requestedQuantity, availableStock), MaxQuantityPerLine);
        var item = new BasketItem(productId, unitPrice, applied);
        var index = _items.FindIndex(existing => existing.ProductId == productId);
        if (index >= 0)
        {
            _items[index] = item;
        }
        else
        {
            _items.Add(item);
        }

        return applied;
    }

    public bool RemoveItem(Guid productId) => _items.RemoveAll(item => item.ProductId == productId) > 0;
}
