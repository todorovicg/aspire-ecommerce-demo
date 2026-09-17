namespace Ecommerce.Contracts;

public sealed record OrderBuyer(string Name, string Email);

public sealed record OrderAddress(string Street, string City, string PostalCode, string Country);

// The full test card number travels here because the simulator has no tokenization step; no service persists it
public sealed record OrderCard(string Number, string HolderName);

public sealed record OrderLine(Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);

public sealed record OrderPlaced(
    Guid OrderId,
    Guid BuyerId,
    OrderBuyer Buyer,
    OrderAddress ShippingAddress,
    OrderCard Card,
    IReadOnlyList<OrderLine> Lines,
    decimal Total,
    string Currency,
    DateTimeOffset PlacedAt);
