namespace Ecommerce.IntegrationTests;

[Collection(AppCollection.Name)]
public sealed class PurchaseTests(AppFixture app)
{
    private static readonly Guid Mug = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid Poster = Guid.Parse("a0000000-0000-4000-8000-000000000010");
    private static readonly TimeSpan OrderTimeout = TimeSpan.FromSeconds(90);
    private static readonly TimeSpan StockTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task Checkout_WithApprovingCard_ShipsOrderAndDecrementsStock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var shop = new ShopClient(app.Gateway, Guid.NewGuid());
        var stockBefore = await shop.GetStockAsync(Mug, cancellationToken);

        await shop.AddToBasketAsync(Mug, 2, cancellationToken);
        var orderId = await shop.CheckoutAsync("4242424242424242", cancellationToken);

        var order = await Poll.UntilAsync(() => shop.GetOrderAsync(orderId, cancellationToken), o => o?.Status == "Shipped", OrderTimeout, cancellationToken);
        Assert.NotNull(order);
        Assert.Equal("Shipped", order.Status);
        var stockAfter = await Poll.UntilAsync(() => shop.GetStockAsync(Mug, cancellationToken), s => s == stockBefore - 2, StockTimeout, cancellationToken);
        Assert.Equal(stockBefore - 2, stockAfter);
    }

    [Fact]
    public async Task Checkout_WithDecliningCard_FailsPaymentAndKeepsStock()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var shop = new ShopClient(app.Gateway, Guid.NewGuid());
        var stockBefore = await shop.GetStockAsync(Poster, cancellationToken);

        await shop.AddToBasketAsync(Poster, 1, cancellationToken);
        var orderId = await shop.CheckoutAsync("4000000000000002", cancellationToken);

        var order = await Poll.UntilAsync(() => shop.GetOrderAsync(orderId, cancellationToken), o => o?.Status == "PaymentFailed", OrderTimeout, cancellationToken);
        Assert.NotNull(order);
        Assert.Equal("PaymentFailed", order.Status);
        Assert.Equal("card_declined", order.FailureReason);
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        Assert.Equal(stockBefore, await shop.GetStockAsync(Poster, cancellationToken));
    }
}
