namespace Ecommerce.Basket.Api.Infrastructure;

public sealed record CatalogProduct(Guid Id, string Name, decimal Price, int AvailableStock);

// Thrown when the catalog cannot be reached after the resilience handler gave up
public sealed class CatalogUnavailableException(Exception inner) : Exception("The catalog service is unavailable", inner);

public interface ICatalogClient
{
    // Returns only the products the catalog knows, named in the request's language; throws CatalogUnavailableException when the catalog cannot be reached
    Task<IReadOnlyList<CatalogProduct>> GetProductsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);
}
