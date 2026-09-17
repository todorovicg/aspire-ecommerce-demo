using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Basket.Api.Features.GetBasket;

public static class GetBasketEndpoint
{
    public static RouteGroupBuilder MapGetBasket(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync).WithName("GetBasket");
        return group;
    }

    public static async Task<Results<Ok<ApiResponse<BasketResponse>>, JsonHttpResult<ApiResponse>>> HandleAsync(
        BuyerId buyerId,
        BasketService service,
        CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(buyerId, cancellationToken);

        return result switch
        {
            BasketResult.Success success => ApiResults.Ok(BasketResponse.From(success.Basket)),
            BasketResult.CatalogUnavailable => ApiResults.ServiceUnavailable(ErrorCodes.CatalogUnavailable, "The catalog service is unavailable"),
            _ => throw new InvalidOperationException($"Unhandled result [{result.GetType().Name}]"),
        };
    }
}
