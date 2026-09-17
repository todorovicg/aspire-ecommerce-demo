using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Infrastructure;
using Ecommerce.Contracts;

namespace Ecommerce.Basket.Api.Features.Checkout;

public sealed record CheckoutDetails(OrderBuyer Buyer, OrderAddress ShippingAddress, OrderCard Card);

public sealed record UnavailableLine(Guid ProductId, string ProductName, int Requested, int Available);

public abstract record CheckoutResult
{
    public sealed record Placed(Guid OrderId, decimal Total) : CheckoutResult;

    public sealed record EmptyBasket : CheckoutResult;

    public sealed record OutOfStock(IReadOnlyList<UnavailableLine> Lines) : CheckoutResult;

    public sealed record CatalogUnavailable : CheckoutResult;

    public sealed record OutboxUnavailable : CheckoutResult;
}

public sealed class CheckoutService(
    IBasketStore store,
    ICatalogClient catalog,
    IOrderPlacedPublisher publisher,
    TimeProvider timeProvider,
    ILogger<CheckoutService> logger)
{
    public const string Currency = "EUR";

    public async Task<CheckoutResult> CheckoutAsync(BuyerId buyerId, CheckoutDetails details, CancellationToken cancellationToken)
    {
        var basket = await store.GetAsync(buyerId.Value, cancellationToken);
        if (basket is null || basket.Items.Count == 0)
        {
            return new CheckoutResult.EmptyBasket();
        }

        IReadOnlyList<CatalogProduct> products;
        try
        {
            products = await catalog.GetProductsAsync(basket.Items.Select(item => item.ProductId).ToList(), cancellationToken);
        }
        catch (CatalogUnavailableException exception)
        {
            logger.LogWarning(exception, "[{Prefix}] Catalog unavailable during checkout for buyer [{BuyerId}]", nameof(CheckoutService), buyerId.Value);
            return new CheckoutResult.CatalogUnavailable();
        }

        var byId = products.ToDictionary(product => product.Id);
        var unavailable = basket.Items
            .Select(item => byId.TryGetValue(item.ProductId, out var product)
                ? new UnavailableLine(item.ProductId, product.Name, item.Quantity, product.AvailableStock)
                : new UnavailableLine(item.ProductId, item.ProductId.ToString(), item.Quantity, 0))
            .Where(line => line.Available < line.Requested)
            .ToList();
        if (unavailable.Count > 0)
        {
            logger.LogWarning("[{Prefix}] Checkout blocked for buyer [{BuyerId}], [{Count}] lines exceed stock", nameof(CheckoutService), buyerId.Value, unavailable.Count);
            return new CheckoutResult.OutOfStock(unavailable);
        }

        var lines = basket.Items
            .Select(item => new OrderLine(item.ProductId, byId[item.ProductId].Name, byId[item.ProductId].Price, item.Quantity))
            .ToList();
        var orderPlaced = new OrderPlaced(
            Guid.CreateVersion7(),
            buyerId.Value,
            details.Buyer,
            details.ShippingAddress,
            details.Card,
            lines,
            lines.Sum(line => line.UnitPrice * line.Quantity),
            Currency,
            timeProvider.GetUtcNow());

        try
        {
            await publisher.PublishAsync(orderPlaced, cancellationToken);
        }
        catch (OutboxUnavailableException exception)
        {
            logger.LogWarning(exception, "[{Prefix}] Outbox unavailable, order [{OrderId}] for buyer [{BuyerId}] not placed, basket kept", nameof(CheckoutService), orderPlaced.OrderId, buyerId.Value);
            return new CheckoutResult.OutboxUnavailable();
        }

        await store.DeleteAsync(buyerId.Value, cancellationToken);
        logger.LogInformation("[{Prefix}] Published OrderPlaced for order [{OrderId}] buyer [{BuyerId}] total [{Total}]", nameof(CheckoutService), orderPlaced.OrderId, buyerId.Value, orderPlaced.Total);
        return new CheckoutResult.Placed(orderPlaced.OrderId, orderPlaced.Total);
    }
}
