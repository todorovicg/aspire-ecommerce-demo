using Ecommerce.Ordering.Api.Infrastructure;

namespace Ecommerce.Ordering.Api.Tests.Fakes;

public sealed class FakePaymentClient : IPaymentClient
{
    public bool Unavailable { get; set; }

    public bool Decline { get; set; }

    public List<(Guid OrderId, decimal Amount, string CardNumber)> Calls { get; } = [];

    public Task<PaymentOutcome> AuthorizeAsync(Guid orderId, decimal amount, string currency, string cardNumber, string cardHolder, CancellationToken cancellationToken)
    {
        if (Unavailable)
        {
            throw new PaymentUnavailableException(new HttpRequestException("Payment unreachable"));
        }

        Calls.Add((orderId, amount, cardNumber));
        return Task.FromResult(Decline
            ? new PaymentOutcome(false, Guid.NewGuid(), "card_declined")
            : new PaymentOutcome(true, Guid.Parse("d0000000-0000-4000-8000-000000000001"), null));
    }
}
