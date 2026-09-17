using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Basket.Api.Features.RemoveItem;

public static class RemoveItemEndpoint
{
    public static RouteGroupBuilder MapRemoveItem(this RouteGroupBuilder group)
    {
        group.MapDelete("/items/{productId:guid}", HandleAsync).WithName("RemoveBasketItem");
        return group;
    }

    public static async Task<Results<Ok<ApiResponse<BasketResponse>>, JsonHttpResult<ApiResponse>>> HandleAsync(
        BuyerId buyerId,
        Guid productId,
        BasketService service,
        CancellationToken cancellationToken)
    {
        var result = await service.RemoveItemAsync(buyerId, productId, cancellationToken);

        return result switch
        {
            BasketResult.Success success => ApiResults.Ok(BasketResponse.From(success.Basket)),
            BasketResult.CatalogUnavailable => ApiResults.ServiceUnavailable(ErrorCodes.CatalogUnavailable, "The catalog service is unavailable"),
            _ => throw new InvalidOperationException($"Unhandled result [{result.GetType().Name}]"),
        };
    }
}
