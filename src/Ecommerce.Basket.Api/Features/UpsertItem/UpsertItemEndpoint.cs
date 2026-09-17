using System.ComponentModel.DataAnnotations;
using Ecommerce.ApiDefaults;
using Ecommerce.Basket.Api.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Basket.Api.Features.UpsertItem;

public sealed record UpsertItemRequest(Guid ProductId, [Range(1, ShoppingBasket.MaxQuantityPerLine)] int Quantity);

public sealed record UpsertItemResponse(BasketResponse Basket, int RequestedQuantity, int AppliedQuantity, bool Capped);

public static class UpsertItemEndpoint
{
    public static RouteGroupBuilder MapUpsertItem(this RouteGroupBuilder group)
    {
        group.MapPut("/items", HandleAsync).WithName("UpsertBasketItem");
        return group;
    }

    public static async Task<Results<Ok<ApiResponse<UpsertItemResponse>>, NotFound<ApiResponse>, Conflict<ApiResponse>, JsonHttpResult<ApiResponse>>> HandleAsync(
        BuyerId buyerId,
        UpsertItemRequest request,
        BasketService service,
        CancellationToken cancellationToken)
    {
        var result = await service.UpsertItemAsync(buyerId, request.ProductId, request.Quantity, cancellationToken);

        return result switch
        {
            UpsertItemResult.Success success => ApiResults.Ok(new UpsertItemResponse(BasketResponse.From(success.Basket), success.RequestedQuantity, success.AppliedQuantity, success.Capped)),
            UpsertItemResult.ProductNotFound notFound => ApiResults.NotFound(ErrorCodes.ProductNotFound, $"Product [{notFound.ProductId}] not found"),
            UpsertItemResult.OutOfStock outOfStock => ApiResults.Conflict(ErrorCodes.ProductOutOfStock, $"Product [{outOfStock.ProductName}] has [0] units available", new { outOfStock.ProductId }),
            UpsertItemResult.CatalogUnavailable => ApiResults.ServiceUnavailable(ErrorCodes.CatalogUnavailable, "The catalog service is unavailable"),
            _ => throw new InvalidOperationException($"Unhandled result [{result.GetType().Name}]"),
        };
    }
}
