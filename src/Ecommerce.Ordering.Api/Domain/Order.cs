using Ecommerce.Contracts;

namespace Ecommerce.Ordering.Api.Domain;

public enum OrderStatus
{
    Submitted,
    Paid,
    Shipped,
    PaymentFailed,
}

public sealed record OrderBuyerInfo(string Name, string Email);

public sealed record OrderAddressInfo(string Street, string City, string PostalCode, string Country);

public sealed record OrderLineInfo(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}

// The point-in-time snapshot of a purchase; the MongoDB driver materializes it through the property setters
public sealed class Order
{
    public required Guid Id { get; init; }

    public required Guid BuyerId { get; init; }

    public required OrderBuyerInfo Buyer { get; init; }

    public required OrderAddressInfo ShippingAddress { get; init; }

    public required string CardLast4 { get; init; }

    // The simulator has no tokenization step: the test card number stays on the order only until the payment step completes
    public string? PaymentCardNumber { get; set; }

    public required IReadOnlyList<OrderLineInfo> Lines { get; init; }

    public required decimal Total { get; init; }

    public required string Currency { get; init; }

    public OrderStatus Status { get; set; } = OrderStatus.Submitted;

    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? PaidAt { get; set; }

    public DateTimeOffset? ShippedAt { get; set; }

    public string? FailureReason { get; set; }

    public string? PaymentReference { get; set; }

    public int PaymentAttempts { get; set; }

    public required DateTimeOffset NextAttemptAt { get; set; }

    public DateTimeOffset? LeaseUntil { get; set; }

    public bool PaidEventPublished { get; set; }

    public static Order FromPlaced(OrderPlaced placed, DateTimeOffset now) =>
        new()
        {
            Id = placed.OrderId,
            BuyerId = placed.BuyerId,
            Buyer = new OrderBuyerInfo(placed.Buyer.Name, placed.Buyer.Email),
            ShippingAddress = new OrderAddressInfo(placed.ShippingAddress.Street, placed.ShippingAddress.City, placed.ShippingAddress.PostalCode, placed.ShippingAddress.Country),
            CardLast4 = placed.Card.Number.Length >= 4 ? placed.Card.Number[^4..] : placed.Card.Number,
            PaymentCardNumber = placed.Card.Number,
            Lines = placed.Lines.Select(line => new OrderLineInfo(line.ProductId, line.ProductName, line.UnitPrice, line.Quantity)).ToList(),
            Total = placed.Total,
            Currency = placed.Currency,
            CreatedAt = placed.PlacedAt,
            NextAttemptAt = now,
        };

    public void MarkPaid(string paymentReference, DateTimeOffset now, TimeSpan shipAfter)
    {
        Status = OrderStatus.Paid;
        PaidAt = now;
        PaymentReference = paymentReference;
        PaymentCardNumber = null;
        PaidEventPublished = false;
        NextAttemptAt = now + shipAfter;
    }

    public void MarkPaymentFailed(string reason)
    {
        Status = OrderStatus.PaymentFailed;
        FailureReason = reason;
        PaymentCardNumber = null;
    }

    public void ScheduleRetry(DateTimeOffset nextAttemptAt) => NextAttemptAt = nextAttemptAt;

    public void MarkPaidEventPublished(TimeSpan shipAfter)
    {
        PaidEventPublished = true;
        NextAttemptAt = (PaidAt ?? NextAttemptAt) + shipAfter;
    }

    public bool IsShippingDue(DateTimeOffset now) => Status == OrderStatus.Paid && PaidEventPublished && NextAttemptAt <= now;

    public void MarkShipped(DateTimeOffset now)
    {
        Status = OrderStatus.Shipped;
        ShippedAt = now;
    }

    public void ReleaseLease() => LeaseUntil = null;
}
