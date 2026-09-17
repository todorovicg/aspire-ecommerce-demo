using System.Net.Http.Json;
using Ecommerce.ApiDefaults;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Ecommerce.Ordering.Api.Infrastructure;

public sealed class PaymentClient(HttpClient httpClient) : IPaymentClient
{
    private sealed record AuthorizeRequest(Guid OrderId, decimal Amount, string Currency, string CardNumber, string CardHolder);

    private sealed record AuthorizeResponse(Guid PaymentId, string Status, string? Reason);

    public async Task<PaymentOutcome> AuthorizeAsync(Guid orderId, decimal amount, string currency, string cardNumber, string cardHolder, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.PostAsJsonAsync("/api/payments", new AuthorizeRequest(orderId, amount, currency, cardNumber, cardHolder), cancellationToken);
            response.EnsureSuccessStatusCode();
            var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<AuthorizeResponse>>(cancellationToken);
            var data = envelope?.Data ?? throw new HttpRequestException("The payment service returned an empty response");
            return new PaymentOutcome(data.Status == "Approved", data.PaymentId, data.Reason);
        }
        catch (Exception exception) when (exception is HttpRequestException or TimeoutRejectedException or BrokenCircuitException)
        {
            throw new PaymentUnavailableException(exception);
        }
    }
}
