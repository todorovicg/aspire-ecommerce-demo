using Ecommerce.Basket.Api.Infrastructure;

namespace Ecommerce.Basket.Api.Tests.Fakes;

public sealed class FakeCatalogClient : ICatalogClient
{
    private readonly Dictionary<Guid, CatalogProduct> _products = [];

    public bool Unavailable { get; set; }

    public int LookupCount { get; private set; }

    public FakeCatalogClient With(CatalogProduct product)
    {
        _products[product.Id] = product;
        return this;
    }

    public Task<IReadOnlyList<CatalogProduct>> GetProductsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        LookupCount++;
        if (Unavailable)
        {
            throw new CatalogUnavailableException(new HttpRequestException("Catalog unreachable"));
        }

        IReadOnlyList<CatalogProduct> found = productIds.Where(_products.ContainsKey).Select(id => _products[id]).ToList();
        return Task.FromResult(found);
    }
}
