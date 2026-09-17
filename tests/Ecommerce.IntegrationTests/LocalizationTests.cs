using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.IntegrationTests;

[Collection(AppCollection.Name)]
public sealed class LocalizationTests(AppFixture app)
{
    private static readonly Guid Mug = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Product_FollowsAcceptLanguage_AndFallsBackToEnglish()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        Assert.Equal("Aspire keramička šolja", await GetProductNameAsync("sr-Latn", cancellationToken));
        Assert.Equal("Aspire Ceramic Mug", await GetProductNameAsync("en", cancellationToken));
        Assert.Equal("Aspire Ceramic Mug", await GetProductNameAsync("de", cancellationToken));
    }

    [Fact]
    public async Task BasketLines_AreNamedInTheLanguageOfEachRead()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var buyerId = Guid.NewGuid();
        await new ShopClient(app.Gateway, buyerId).AddToBasketAsync(Mug, 1, cancellationToken);

        Assert.Equal("Aspire keramička šolja", await GetBasketLineNameAsync(buyerId, "sr-Latn", cancellationToken));
        Assert.Equal("Aspire Ceramic Mug", await GetBasketLineNameAsync(buyerId, "en", cancellationToken));
    }

    private async Task<string?> GetProductNameAsync(string language, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/catalog/products/{Mug}");
        request.Headers.Add("Accept-Language", language);
        using var response = await app.Gateway.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return body.RootElement.GetProperty("data").GetProperty("name").GetString();
    }

    private async Task<string?> GetBasketLineNameAsync(Guid buyerId, string language, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/basket");
        request.Headers.Add("X-Buyer-Id", buyerId.ToString());
        request.Headers.Add("Accept-Language", language);
        using var response = await app.Gateway.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return body.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("productName").GetString();
    }
}
