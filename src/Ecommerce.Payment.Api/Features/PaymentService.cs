using Microsoft.Extensions.Options;

namespace Ecommerce.Payment.Api.Features;

public sealed record PaymentResult(Guid PaymentId, bool Approved, string? Reason);

public sealed class PaymentService(IOptions<PaymentOptions> options, TimeProvider timeProvider, ILogger<PaymentService> logger)
{
    public async Task<PaymentResult> AuthorizeAsync(Guid orderId, decimal amount, string currency, string cardNumber, CancellationToken cancellationToken)
    {
        await Task.Delay(options.Value.ProcessingDelay, timeProvider, cancellationToken);

        var paymentId = Guid.CreateVersion7();
        if (PaymentDecision.IsDeclined(cardNumber))
        {
            logger.LogWarning("[{Prefix}] Declined payment [{PaymentId}] for order [{OrderId}] amount [{Amount}] [{Currency}]", nameof(PaymentService), paymentId, orderId, amount, currency);
            return new PaymentResult(paymentId, false, PaymentDecision.DeclinedReason);
        }

        logger.LogInformation("[{Prefix}] Approved payment [{PaymentId}] for order [{OrderId}] amount [{Amount}] [{Currency}]", nameof(PaymentService), paymentId, orderId, amount, currency);
        return new PaymentResult(paymentId, true, null);
    }
}
