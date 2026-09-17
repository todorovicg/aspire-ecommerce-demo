using Ecommerce.ApiDefaults;
using Ecommerce.Ordering.Api.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Ordering.Api.Features.ListOrders;

public static class ListOrdersEndpoint
{
    public static RouteGroupBuilder MapListOrders(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync).WithName("ListOrders");
        return group;
    }

    public static async Task<Ok<ApiResponse<IReadOnlyList<OrderResponse>>>> HandleAsync(
        BuyerId buyerId,
        IOrderStore store,
        CancellationToken cancellationToken)
    {
        var orders = await store.ListForBuyerAsync(buyerId.Value, cancellationToken);
        IReadOnlyList<OrderResponse> response = orders.Select(OrderResponse.From).ToList();
        return ApiResults.Ok(response);
    }
}
