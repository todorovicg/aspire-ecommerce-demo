using Ecommerce.Payment.Api.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Payment.Api.Tests.Features;

public sealed class PaymentServiceTests
{
    private static readonly Guid OrderId = Guid.Parse("c0000000-0000-4000-8000-000000000001");

    private static PaymentService CreateService(FakeTimeProvider time, TimeSpan delay) =>
        new(Options.Create(new PaymentOptions { ProcessingDelay = delay }), time, NullLogger<PaymentService>.Instance);

    [Fact]
    public async Task AuthorizeAsync_WithApprovingCard_ReturnsApprovedAfterTheDelay()
    {
        var time = new FakeTimeProvider();
        var service = CreateService(time, TimeSpan.FromSeconds(2));

        var pending = service.AuthorizeAsync(OrderId, 40m, "EUR", "4242424242424242", TestContext.Current.CancellationToken);
        Assert.False(pending.IsCompleted);
        time.Advance(TimeSpan.FromSeconds(2));
        var result = await pending;

        Assert.True(result.Approved);
        Assert.Null(result.Reason);
        Assert.NotEqual(Guid.Empty, result.PaymentId);
    }

    [Fact]
    public async Task AuthorizeAsync_WithDecliningCard_ReturnsDeclinedWithReason()
    {
        var service = CreateService(new FakeTimeProvider(), TimeSpan.Zero);

        var result = await service.AuthorizeAsync(OrderId, 40m, "EUR", "4000000000000002", TestContext.Current.CancellationToken);

        Assert.False(result.Approved);
        Assert.Equal("card_declined", result.Reason);
    }
}
