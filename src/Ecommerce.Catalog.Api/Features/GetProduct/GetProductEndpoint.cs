using Ecommerce.ApiDefaults;
using Ecommerce.Catalog.Api.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Api.Features.GetProduct;

public static class GetProductEndpoint
{
    public static RouteGroupBuilder MapGetProduct(this RouteGroupBuilder group)
    {
        group.MapGet("/products/{id:guid}", HandleAsync).WithName("GetProduct");
        return group;
    }

    public static async Task<Results<Ok<ApiResponse<ProductResponse>>, NotFound<ApiResponse>>> HandleAsync(
        Guid id,
        RequestLanguage language,
        CatalogDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ApiResults.NotFound(ErrorCodes.ProductNotFound, $"Product [{id}] not found");
        }

        return ApiResults.Ok(ProductResponse.From(product, language.Code));
    }
}
