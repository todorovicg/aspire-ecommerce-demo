using System.ComponentModel.DataAnnotations;
using Ecommerce.ApiDefaults;
using Ecommerce.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Basket.Api.Features.Checkout;

public sealed record CheckoutBuyer(
    [property: Required, StringLength(100, MinimumLength = 2)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email);

public sealed record CheckoutAddress(
    [property: Required, StringLength(200, MinimumLength = 2)] string Street,
    [property: Required, StringLength(100, MinimumLength = 2)] string City,
    [property: Required, StringLength(20, MinimumLength = 2)] string PostalCode,
    [property: Required, StringLength(100, MinimumLength = 2)] string Country);

public sealed record CheckoutCard(
    [property: Required, RegularExpression(@"^\d{16}$", ErrorMessage = "Card number must be 16 digits")] string Number,
    [property: Required, StringLength(100, MinimumLength = 2)] string HolderName);

public sealed record CheckoutRequest(
    [property: Required] CheckoutBuyer Buyer,
    [property: Required] CheckoutAddress ShippingAddress,
    [property: Required] CheckoutCard Card);

public sealed record CheckoutResponse(Guid OrderId, decimal Total, string Currency);

public static class CheckoutEndpoint
{
    public static RouteGroupBuilder MapCheckout(this RouteGroupBuilder group)
    {
        group.MapPost("/checkout", HandleAsync).WithName("Checkout");
        return group;
    }

    public static async Task<Results<Accepted<ApiResponse<CheckoutResponse>>, BadRequest<ApiResponse>, Conflict<ApiResponse>, JsonHttpResult<ApiResponse>>> HandleAsync(
        BuyerId buyerId,
        CheckoutRequest request,
        CheckoutService service,
        CancellationToken cancellationToken)
    {
        var details = new CheckoutDetails(
            new OrderBuyer(request.Buyer.Name, request.Buyer.Email),
            new OrderAddress(request.ShippingAddress.Street, request.ShippingAddress.City, request.ShippingAddress.PostalCode, request.ShippingAddress.Country),
            new OrderCard(request.Card.Number, request.Card.HolderName));

        var result = await service.CheckoutAsync(buyerId, details, cancellationToken);

        return result switch
        {
            CheckoutResult.Placed placed => ApiResults.Accepted(new CheckoutResponse(placed.OrderId, placed.Total, CheckoutService.Currency)),
            CheckoutResult.EmptyBasket => ApiResults.BadRequest(ErrorCodes.CheckoutEmpty, "The basket is empty"),
            CheckoutResult.OutOfStock outOfStock => ApiResults.Conflict(ErrorCodes.CheckoutOutOfStock, $"[{outOfStock.Lines.Count}] lines exceed the available stock", outOfStock.Lines),
            CheckoutResult.CatalogUnavailable => ApiResults.ServiceUnavailable(ErrorCodes.CatalogUnavailable, "The catalog service is unavailable"),
            CheckoutResult.OutboxUnavailable => ApiResults.ServiceUnavailable(ErrorCodes.OutboxUnavailable, "The order could not be submitted, the basket was kept"),
            _ => throw new InvalidOperationException($"Unhandled result [{result.GetType().Name}]"),
        };
    }
}
