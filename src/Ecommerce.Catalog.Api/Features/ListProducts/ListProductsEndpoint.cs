using Ecommerce.ApiDefaults;
using Ecommerce.Catalog.Api.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Api.Features.ListProducts;

public static class ListProductsEndpoint
{
    public static RouteGroupBuilder MapListProducts(this RouteGroupBuilder group)
    {
        group.MapGet("/products", HandleAsync).WithName("ListProducts");
        return group;
    }

    public static async Task<Ok<ApiResponse<IReadOnlyList<ProductResponse>>>> HandleAsync(
        Guid[]? ids,
        RequestLanguage language,
        CatalogDbContext db,
        CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking();
        if (ids is { Length: > 0 })
        {
            query = query.Where(p => ids.Contains(p.Id));
        }

        var products = await query.ToListAsync(cancellationToken);
        IReadOnlyList<ProductResponse> response = products
            .Select(product => ProductResponse.From(product, language.Code))
            .OrderBy(product => product.Name, StringComparer.Ordinal)
            .ToList();

        return ApiResults.Ok(response);
    }
}
