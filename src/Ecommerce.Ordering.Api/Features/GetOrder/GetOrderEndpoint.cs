using Ecommerce.ApiDefaults;
using Ecommerce.Ordering.Api.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Ordering.Api.Features.GetOrder;

public static class GetOrderEndpoint
{
    public static RouteGroupBuilder MapGetOrder(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", HandleAsync).WithName("GetOrder");
        return group;
    }

    public static async Task<Results<Ok<ApiResponse<OrderResponse>>, NotFound<ApiResponse>>> HandleAsync(
        BuyerId buyerId,
        Guid id,
        IOrderStore store,
        CancellationToken cancellationToken)
    {
        var order = await store.GetAsync(buyerId.Value, id, cancellationToken);
        if (order is null)
        {
            return ApiResults.NotFound(ErrorCodes.OrderNotFound, $"Order [{id}] not found");
        }

        return ApiResults.Ok(OrderResponse.From(order));
    }
}
