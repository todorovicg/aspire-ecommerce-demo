using Ecommerce.Basket.Api.Domain;

namespace Ecommerce.Basket.Api.Tests.Domain;

public sealed class ShoppingBasketTests
{
    private static readonly Guid BuyerId = Guid.Parse("b0000000-0000-4000-8000-000000000001");
    private static readonly Guid MugId = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid CapId = Guid.Parse("a0000000-0000-4000-8000-000000000007");

    [Fact]
    public void UpsertItem_AddsLineWithRequestedQuantity()
    {
        var basket = new ShoppingBasket(BuyerId);

        var applied = basket.UpsertItem(MugId, 12.50m, 2, 25);

        Assert.Equal(2, applied);
        var item = Assert.Single(basket.Items);
        Assert.Equal(MugId, item.ProductId);
        Assert.Equal(25.00m, item.LineTotal);
    }

    [Fact]
    public void UpsertItem_CapsAtAvailableStock()
    {
        var basket = new ShoppingBasket(BuyerId);

        var applied = basket.UpsertItem(MugId, 12.50m, 10, 3);

        Assert.Equal(3, applied);
        Assert.Equal(3, Assert.Single(basket.Items).Quantity);
    }

    [Fact]
    public void UpsertItem_CapsAtMaxQuantityPerLine()
    {
        var basket = new ShoppingBasket(BuyerId);

        var applied = basket.UpsertItem(MugId, 12.50m, 99, 500);

        Assert.Equal(ShoppingBasket.MaxQuantityPerLine, applied);
    }

    [Fact]
    public void UpsertItem_ReplacesExistingLineInsteadOfAdding()
    {
        var basket = new ShoppingBasket(BuyerId);
        basket.UpsertItem(MugId, 12.50m, 2, 25);

        basket.UpsertItem(MugId, 12.50m, 5, 25);

        Assert.Equal(5, Assert.Single(basket.Items).Quantity);
    }

    [Fact]
    public void ItemCountAndTotal_SumAcrossLines()
    {
        var basket = new ShoppingBasket(BuyerId);
        basket.UpsertItem(MugId, 12.50m, 2, 25);
        basket.UpsertItem(CapId, 19.50m, 1, 18);

        Assert.Equal(3, basket.ItemCount);
        Assert.Equal(44.50m, basket.Total);
    }

    [Fact]
    public void RemoveItem_ReturnsWhetherALineWasRemoved()
    {
        var basket = new ShoppingBasket(BuyerId);
        basket.UpsertItem(MugId, 12.50m, 2, 25);

        Assert.True(basket.RemoveItem(MugId));
        Assert.False(basket.RemoveItem(MugId));
        Assert.Empty(basket.Items);
    }

    [Fact]
    public void UpsertItem_WithZeroQuantity_Throws()
    {
        var basket = new ShoppingBasket(BuyerId);

        Assert.Throws<ArgumentOutOfRangeException>(() => basket.UpsertItem(MugId, 12.50m, 0, 25));
    }
}
