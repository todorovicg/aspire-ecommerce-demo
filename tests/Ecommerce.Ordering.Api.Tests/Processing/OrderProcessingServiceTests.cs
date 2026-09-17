using Ecommerce.Ordering.Api.Domain;
using Ecommerce.Ordering.Api.Processing;
using Ecommerce.Ordering.Api.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Ordering.Api.Tests.Processing;

public sealed class OrderProcessingServiceTests
{
    private static readonly OrderProcessorOptions Options = new()
    {
        PollInterval = TimeSpan.FromSeconds(1),
        LeaseDuration = TimeSpan.FromSeconds(30),
        PaidToShippedDelay = TimeSpan.FromSeconds(5),
        PaymentRetryDelay = TimeSpan.FromSeconds(5),
    };

    private readonly InMemoryOrderStore _store = new();
    private readonly FakePaymentClient _payment = new();
    private readonly FakeOrderPaidPublisher _publisher = new();
    private readonly FakeTimeProvider _time = new(TestOrders.PlacedAt.AddSeconds(2));

    private OrderProcessingService CreateService() =>
        new(_store, _payment, _publisher, Microsoft.Extensions.Options.Options.Create(Options), _time, NullLogger<OrderProcessingService>.Instance);

    private Order SubmittedOrder()
    {
        var order = Order.FromPlaced(TestOrders.Placed(), _time.GetUtcNow());
        _store.Orders[order.Id] = order;
        return order;
    }

    [Fact]
    public async Task ProcessAsync_SubmittedAndApproved_MarksPaidPublishesAndReleasesLease()
    {
        var order = SubmittedOrder();
        order.LeaseUntil = _time.GetUtcNow().AddSeconds(30);

        await CreateService().ProcessAsync(order, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(1, order.PaymentAttempts);
        Assert.Equal("d0000000-0000-4000-8000-000000000001", order.PaymentReference);
        Assert.True(order.PaidEventPublished);
        Assert.Null(order.PaymentCardNumber);
        Assert.Null(order.LeaseUntil);
        Assert.Equal(_time.GetUtcNow() + Options.PaidToShippedDelay, order.NextAttemptAt);
        var published = Assert.Single(_publisher.Published);
        Assert.Equal(order.Id, published.OrderId);
        Assert.Equal(2, published.Lines.Count);
        Assert.Equal("4242424242424242", Assert.Single(_payment.Calls).CardNumber);
    }

    [Fact]
    public async Task ProcessAsync_SubmittedAndDeclined_MarksPaymentFailedWithoutPublishing()
    {
        var order = SubmittedOrder();
        _payment.Decline = true;

        await CreateService().ProcessAsync(order, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.PaymentFailed, order.Status);
        Assert.Equal("card_declined", order.FailureReason);
        Assert.Empty(_publisher.Published);
        Assert.Null(order.LeaseUntil);
    }

    [Fact]
    public async Task ProcessAsync_SubmittedAndPaymentUnavailable_SchedulesRetryAndStaysSubmitted()
    {
        var order = SubmittedOrder();
        _payment.Unavailable = true;

        await CreateService().ProcessAsync(order, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(1, order.PaymentAttempts);
        Assert.Equal(_time.GetUtcNow() + Options.PaymentRetryDelay, order.NextAttemptAt);
        Assert.Equal("4242424242424242", order.PaymentCardNumber);
        Assert.Null(order.LeaseUntil);
    }

    [Fact]
    public async Task ProcessAsync_PaidAndOutboxUnavailable_KeepsPaidAndRetriesPublishLater()
    {
        var order = SubmittedOrder();
        _publisher.Unavailable = true;

        await CreateService().ProcessAsync(order, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.False(order.PaidEventPublished);
        Assert.Equal(_time.GetUtcNow() + Options.PaymentRetryDelay, order.NextAttemptAt);
    }

    [Fact]
    public async Task ProcessAsync_PaidWithUnpublishedEvent_PublishesBeforeShipping()
    {
        var order = SubmittedOrder();
        order.MarkPaid("pay-1", _time.GetUtcNow(), Options.PaidToShippedDelay);
        order.ScheduleRetry(_time.GetUtcNow());

        await CreateService().ProcessAsync(order, TestContext.Current.CancellationToken);

        Assert.Single(_publisher.Published);
        Assert.True(order.PaidEventPublished);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Equal(order.PaidAt + Options.PaidToShippedDelay, order.NextAttemptAt);
    }

    [Fact]
    public async Task ProcessAsync_PaidAndDue_MarksShipped()
    {
        var order = SubmittedOrder();
        order.MarkPaid("pay-1", _time.GetUtcNow(), Options.PaidToShippedDelay);
        order.MarkPaidEventPublished(Options.PaidToShippedDelay);
        _time.Advance(Options.PaidToShippedDelay);

        await CreateService().ProcessAsync(order, TestContext.Current.CancellationToken);

        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(_time.GetUtcNow(), order.ShippedAt);
        Assert.Empty(_publisher.Published);
    }

    [Fact]
    public async Task ClaimDueAsync_SkipsLeasedAndNotYetDueOrders()
    {
        var due = SubmittedOrder();
        var later = Order.FromPlaced(TestOrders.Placed(Guid.NewGuid()), _time.GetUtcNow().AddMinutes(1));
        _store.Orders[later.Id] = later;

        var first = await _store.ClaimDueAsync(_time.GetUtcNow(), _time.GetUtcNow().AddSeconds(30), TestContext.Current.CancellationToken);
        var second = await _store.ClaimDueAsync(_time.GetUtcNow(), _time.GetUtcNow().AddSeconds(30), TestContext.Current.CancellationToken);

        Assert.Same(due, first);
        Assert.Null(second);
    }
}
