using Ecommerce.Contracts;

namespace Ecommerce.Ordering.Api.Tests;

public static class TestOrders
{
    public static readonly Guid BuyerId = Guid.Parse("b0000000-0000-4000-8000-000000000001");
    public static readonly Guid OrderId = Guid.Parse("c0000000-0000-4000-8000-000000000001");
    public static readonly DateTimeOffset PlacedAt = new(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);

    public static OrderPlaced Placed(Guid? orderId = null) =>
        new(
            orderId ?? OrderId,
            BuyerId,
            new OrderBuyer("Goran", "goran@example.com"),
            new OrderAddress("Main Street 1", "Belgrade", "11000", "Serbia"),
            new OrderCard("4242424242424242", "Goran"),
            [
                new OrderLine(Guid.Parse("a0000000-0000-4000-8000-000000000001"), "Aspire Ceramic Mug", 12.50m, 2),
                new OrderLine(Guid.Parse("a0000000-0000-4000-8000-000000000010"), "Aspire Launch Poster", 15.00m, 1),
            ],
            40.00m,
            "EUR",
            PlacedAt);
}
