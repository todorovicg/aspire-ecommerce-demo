using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Ecommerce.IntegrationTests;

[Collection(AppCollection.Name)]
public sealed class PreferencesTests(AppFixture app)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Language_DefaultsToEnglish_AndSurvivesAnUpdate()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var buyerId = Guid.NewGuid();

        Assert.Equal("en", await GetLanguageAsync(buyerId, cancellationToken));

        using var update = await SendAsync(HttpMethod.Put, buyerId, new { language = "sr" }, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        Assert.Equal("sr", await GetLanguageAsync(buyerId, cancellationToken));
    }

    [Fact]
    public async Task Language_Unsupported_IsRejectedWithTheEnvelopeCode()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await SendAsync(HttpMethod.Put, Guid.NewGuid(), new { language = "de" }, cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.Equal("preferences.unsupported_language", body.RootElement.GetProperty("error").GetProperty("code").GetString());
    }

    private async Task<string?> GetLanguageAsync(Guid buyerId, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, buyerId, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        return body.RootElement.GetProperty("data").GetProperty("language").GetString();
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, Guid buyerId, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, "/api/preferences");
        request.Headers.Add("X-Buyer-Id", buyerId.ToString());
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: Json);
        }

        return await app.Gateway.SendAsync(request, cancellationToken);
    }
}
