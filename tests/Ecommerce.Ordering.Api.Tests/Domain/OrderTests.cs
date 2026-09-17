using Ecommerce.Ordering.Api.Domain;

namespace Ecommerce.Ordering.Api.Tests.Domain;

public sealed class OrderTests
{
    private static readonly TimeSpan ShipAfter = TimeSpan.FromSeconds(5);

    [Fact]
    public void FromPlaced_CopiesTheSnapshotAndStartsSubmitted()
    {
        var now = TestOrders.PlacedAt.AddSeconds(1);

        var order = Order.FromPlaced(TestOrders.Placed(), now);

        Assert.Equal(TestOrders.OrderId, order.Id);
        Assert.Equal(TestOrders.BuyerId, order.BuyerId);
        Assert.Equal("Goran", order.Buyer.Name);
        Assert.Equal("Belgrade", order.ShippingAddress.City);
        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(25.00m, order.Lines[0].LineTotal);
        Assert.Equal(40.00m, order.Total);
        Assert.Equal("EUR", order.Currency);
        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(TestOrders.PlacedAt, order.CreatedAt);
        Assert.Equal(now, order.NextAttemptAt);
        Assert.Null(order.LeaseUntil);
        Assert.False(order.PaidEventPublished);
    }

    [Fact]
    public void FromPlaced_KeepsTheCardNumberOnlyUntilPayment()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);

        Assert.Equal("4242", order.CardLast4);
        Assert.Equal("4242424242424242", order.PaymentCardNumber);
    }

    [Fact]
    public void MarkPaid_SetsPaidStateSchedulesShippingAndDropsTheCardNumber()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);
        var now = TestOrders.PlacedAt.AddSeconds(3);

        order.MarkPaid("pay-1", now, ShipAfter);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(now, order.PaidAt);
        Assert.Equal("pay-1", order.PaymentReference);
        Assert.Null(order.PaymentCardNumber);
        Assert.False(order.PaidEventPublished);
        Assert.Equal(now + ShipAfter, order.NextAttemptAt);
    }

    [Fact]
    public void MarkPaymentFailed_SetsReasonAndDropsTheCardNumber()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);

        order.MarkPaymentFailed("card_declined");

        Assert.Equal(OrderStatus.PaymentFailed, order.Status);
        Assert.Equal("card_declined", order.FailureReason);
        Assert.Null(order.PaymentCardNumber);
    }

    [Fact]
    public void IsShippingDue_RequiresPaidPublishedAndElapsedDelay()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);
        var paidAt = TestOrders.PlacedAt.AddSeconds(3);
        order.MarkPaid("pay-1", paidAt, ShipAfter);

        Assert.False(order.IsShippingDue(paidAt + ShipAfter));
        order.MarkPaidEventPublished(ShipAfter);
        Assert.False(order.IsShippingDue(paidAt + ShipAfter - TimeSpan.FromSeconds(1)));
        Assert.True(order.IsShippingDue(paidAt + ShipAfter));
    }

    [Fact]
    public void MarkShipped_SetsShippedState()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), TestOrders.PlacedAt);
        var shippedAt = TestOrders.PlacedAt.AddSeconds(10);

        order.MarkShipped(shippedAt);

        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(shippedAt, order.ShippedAt);
    }
}
