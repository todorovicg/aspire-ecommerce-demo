namespace Ecommerce.Ordering.Api.Infrastructure;

public sealed record PaymentOutcome(bool Approved, Guid PaymentId, string? Reason);

// Thrown when the payment service cannot be reached or answers with an error after the resilience handler gave up
public sealed class PaymentUnavailableException(Exception inner) : Exception("The payment service is unavailable", inner);

public interface IPaymentClient
{
    Task<PaymentOutcome> AuthorizeAsync(Guid orderId, decimal amount, string currency, string cardNumber, string cardHolder, CancellationToken cancellationToken);
}
