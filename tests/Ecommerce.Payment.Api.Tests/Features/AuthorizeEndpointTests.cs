using Ecommerce.Payment.Api.Features;
using Ecommerce.Payment.Api.Features.Authorize;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace Ecommerce.Payment.Api.Tests.Features;

public sealed class AuthorizeEndpointTests
{
    private static PaymentService CreateService() =>
        new(Options.Create(new PaymentOptions { ProcessingDelay = TimeSpan.Zero }), new FakeTimeProvider(), NullLogger<PaymentService>.Instance);

    [Fact]
    public async Task HandleAsync_MapsApprovalToStatusText()
    {
        var request = new AuthorizeRequest(Guid.NewGuid(), 40m, "EUR", "4242424242424242", "Goran");

        var result = await AuthorizeEndpoint.HandleAsync(request, CreateService(), TestContext.Current.CancellationToken);

        Assert.NotNull(result.Value?.Data);
        Assert.Equal("Approved", result.Value.Data.Status);
        Assert.Null(result.Value.Data.Reason);
    }

    [Fact]
    public async Task HandleAsync_MapsDeclineToStatusTextAndReason()
    {
        var request = new AuthorizeRequest(Guid.NewGuid(), 40m, "EUR", "4000000000000002", "Goran");

        var result = await AuthorizeEndpoint.HandleAsync(request, CreateService(), TestContext.Current.CancellationToken);

        Assert.NotNull(result.Value?.Data);
        Assert.Equal("Declined", result.Value.Data.Status);
        Assert.Equal("card_declined", result.Value.Data.Reason);
    }
}
