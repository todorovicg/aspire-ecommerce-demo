using Ecommerce.Ordering.Api.Domain;

namespace Ecommerce.Ordering.Api.Features;

public sealed record OrderLineResponse(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);

public sealed record OrderResponse(
    Guid Id,
    string Status,
    decimal Total,
    string Currency,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    DateTimeOffset? ShippedAt,
    string? FailureReason,
    string CardLast4,
    OrderBuyerInfo Buyer,
    OrderAddressInfo ShippingAddress,
    IReadOnlyList<OrderLineResponse> Lines)
{
    public static OrderResponse From(Order order) =>
        new(
            order.Id,
            order.Status.ToString(),
            order.Total,
            order.Currency,
            order.CreatedAt,
            order.PaidAt,
            order.ShippedAt,
            order.FailureReason,
            order.CardLast4,
            order.Buyer,
            order.ShippingAddress,
            order.Lines.Select(line => new OrderLineResponse(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity, line.LineTotal)).ToList());
}
