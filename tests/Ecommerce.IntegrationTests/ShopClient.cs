using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.IntegrationTests;

public sealed class ShopClient(HttpClient gateway, Guid buyerId)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<int> GetStockAsync(Guid productId, CancellationToken cancellationToken)
    {
        var envelope = await gateway.GetFromJsonAsync<Envelope<ProductView>>($"/api/catalog/products/{productId}", Json, cancellationToken);

        return envelope!.Data!.AvailableStock;
    }

    public async Task AddToBasketAsync(Guid productId, int quantity, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, "/api/basket/items")
        {
            Content = JsonContent.Create(new { productId, quantity }, options: Json),
        };
        request.Headers.Add("X-Buyer-Id", buyerId.ToString());

        using var response = await gateway.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Guid> CheckoutAsync(string cardNumber, CancellationToken cancellationToken)
    {
        var body = new
        {
            buyer = new { name = "Integration Test", email = "test@example.com" },
            shippingAddress = new { street = "Main Street 1", city = "Belgrade", postalCode = "11000", country = "Serbia" },
            card = new { number = cardNumber, holderName = "Integration Test" },
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/basket/checkout")
        {
            Content = JsonContent.Create(body, options: Json),
        };
        request.Headers.Add("X-Buyer-Id", buyerId.ToString());

        using var response = await gateway.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<CheckoutView>>(Json, cancellationToken);

        return envelope!.Data!.OrderId;
    }

    // The order is created asynchronously from the checkout event, so a 404 right after checkout means "not yet"
    public async Task<OrderView?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/orders/{orderId}");
        request.Headers.Add("X-Buyer-Id", buyerId.ToString());

        using var response = await gateway.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var envelope = await response.Content.ReadFromJsonAsync<Envelope<OrderView>>(Json, cancellationToken);

        return envelope!.Data!;
    }

    private sealed record Envelope<T>(bool Success, T? Data);

    private sealed record ProductView(Guid Id, int AvailableStock);

    private sealed record CheckoutView(Guid OrderId);

    public sealed record OrderView(Guid Id, string Status, string? FailureReason);
}
