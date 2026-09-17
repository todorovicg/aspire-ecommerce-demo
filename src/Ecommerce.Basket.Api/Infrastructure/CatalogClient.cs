using System.Net.Http.Json;
using Ecommerce.ApiDefaults;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Ecommerce.Basket.Api.Infrastructure;

// The Accept-Language header of the current request travels along through header propagation, so names come back in the buyer's language
public sealed class CatalogClient(HttpClient httpClient) : ICatalogClient
{
    public async Task<IReadOnlyList<CatalogProduct>> GetProductsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return [];
        }

        var query = string.Join("&", productIds.Select(id => $"ids={id}"));
        try
        {
            var envelope = await httpClient.GetFromJsonAsync<ApiResponse<List<CatalogProduct>>>($"/api/catalog/products?{query}", cancellationToken);
            return envelope?.Data ?? [];
        }
        catch (Exception exception) when (IsTransport(exception))
        {
            throw new CatalogUnavailableException(exception);
        }
    }

    private static bool IsTransport(Exception exception) =>
        exception is HttpRequestException or TimeoutRejectedException or BrokenCircuitException;
}
