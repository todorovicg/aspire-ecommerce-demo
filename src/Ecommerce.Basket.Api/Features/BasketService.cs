using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Ecommerce.Basket.Api.Infrastructure;

namespace Ecommerce.Basket.Api.Features;

public abstract record BasketResult
{
    public sealed record Success(NamedBasket Basket) : BasketResult;

    public sealed record CatalogUnavailable : BasketResult;
}

public abstract record UpsertItemResult
{
    public sealed record Success(NamedBasket Basket, int RequestedQuantity, int AppliedQuantity) : UpsertItemResult
    {
        public bool Capped => AppliedQuantity < RequestedQuantity;
    }

    public sealed record ProductNotFound(Guid ProductId) : UpsertItemResult;

    public sealed record OutOfStock(Guid ProductId, string ProductName) : UpsertItemResult;

    public sealed record CatalogUnavailable : UpsertItemResult;
}

public sealed class BasketService(IBasketStore store, ICatalogClient catalog, ILogger<BasketService> logger)
{
    public async Task<BasketResult> GetAsync(BuyerId buyerId, CancellationToken cancellationToken)
    {
        var basket = await LoadAsync(buyerId, cancellationToken);
        return await NameAsync(basket, cancellationToken);
    }

    public async Task<UpsertItemResult> UpsertItemAsync(BuyerId buyerId, Guid productId, int quantity, CancellationToken cancellationToken)
    {
        var basket = await LoadAsync(buyerId, cancellationToken);

        // One catalog call checks the requested product and names every line of the response
        IReadOnlyList<CatalogProduct> products;
        try
        {
            products = await catalog.GetProductsAsync(basket.Items.Select(item => item.ProductId).Append(productId).Distinct().ToList(), cancellationToken);
        }
        catch (CatalogUnavailableException exception)
        {
            logger.LogWarning(exception, "[{Prefix}] Catalog unavailable while adding product [{ProductId}] for buyer [{BuyerId}]", nameof(BasketService), productId, buyerId.Value);
            return new UpsertItemResult.CatalogUnavailable();
        }

        var product = products.FirstOrDefault(candidate => candidate.Id == productId);
        if (product is null)
        {
            return new UpsertItemResult.ProductNotFound(productId);
        }

        if (product.AvailableStock == 0)
        {
            return new UpsertItemResult.OutOfStock(product.Id, product.Name);
        }

        var applied = basket.UpsertItem(product.Id, product.Price, quantity, product.AvailableStock);
        await store.SaveAsync(basket, cancellationToken);

        if (applied < quantity)
        {
            logger.LogWarning("[{Prefix}] Capped product [{ProductId}] at [{Applied}] of requested [{Requested}] for buyer [{BuyerId}]", nameof(BasketService), product.Id, applied, quantity, buyerId.Value);
        }
        else
        {
            logger.LogInformation("[{Prefix}] Set product [{ProductId}] quantity [{Quantity}] for buyer [{BuyerId}]", nameof(BasketService), product.Id, applied, buyerId.Value);
        }

        return new UpsertItemResult.Success(new NamedBasket(basket, NamesOf(products)), quantity, applied);
    }

    public async Task<BasketResult> RemoveItemAsync(BuyerId buyerId, Guid productId, CancellationToken cancellationToken)
    {
        var basket = await LoadAsync(buyerId, cancellationToken);
        if (basket.RemoveItem(productId))
        {
            await store.SaveAsync(basket, cancellationToken);
            logger.LogInformation("[{Prefix}] Removed product [{ProductId}] for buyer [{BuyerId}]", nameof(BasketService), productId, buyerId.Value);
        }

        return await NameAsync(basket, cancellationToken);
    }

    private async Task<ShoppingBasket> LoadAsync(BuyerId buyerId, CancellationToken cancellationToken) =>
        await store.GetAsync(buyerId.Value, cancellationToken) ?? new ShoppingBasket(buyerId.Value);

    // An empty basket needs no catalog call
    private async Task<BasketResult> NameAsync(ShoppingBasket basket, CancellationToken cancellationToken)
    {
        if (basket.Items.Count == 0)
        {
            return new BasketResult.Success(new NamedBasket(basket, new Dictionary<Guid, string>()));
        }

        try
        {
            var products = await catalog.GetProductsAsync(basket.Items.Select(item => item.ProductId).ToList(), cancellationToken);
            return new BasketResult.Success(new NamedBasket(basket, NamesOf(products)));
        }
        catch (CatalogUnavailableException exception)
        {
            logger.LogWarning(exception, "[{Prefix}] Catalog unavailable while naming the basket of buyer [{BuyerId}]", nameof(BasketService), basket.BuyerId);
            return new BasketResult.CatalogUnavailable();
        }
    }

    private static Dictionary<Guid, string> NamesOf(IReadOnlyList<CatalogProduct> products) =>
        products.ToDictionary(product => product.Id, product => product.Name);
}
