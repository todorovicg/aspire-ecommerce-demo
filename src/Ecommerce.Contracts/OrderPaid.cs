namespace Ecommerce.Contracts;

public sealed record OrderPaidLine(Guid ProductId, int Quantity);

public sealed record OrderPaid(Guid OrderId, IReadOnlyList<OrderPaidLine> Lines, DateTimeOffset PaidAt);
