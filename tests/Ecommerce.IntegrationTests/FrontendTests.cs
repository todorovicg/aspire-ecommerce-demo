using System.Net;
using System.Text.Json;

namespace Ecommerce.IntegrationTests;

[Collection(AppCollection.Name)]
public sealed class FrontendTests(AppFixture app)
{
    [Fact]
    public async Task DevServer_ForwardsApiCallsToTheGateway()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var response = await app.Frontend.GetAsync("/api/catalog/products", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        Assert.True(body.RootElement.GetProperty("success").GetBoolean());
        Assert.NotEqual(0, body.RootElement.GetProperty("data").GetArrayLength());
    }
}
