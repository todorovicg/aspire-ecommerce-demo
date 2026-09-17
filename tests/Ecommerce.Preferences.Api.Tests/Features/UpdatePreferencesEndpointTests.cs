using Ecommerce.ApiDefaults;
using Ecommerce.Preferences.Api.Features;
using Ecommerce.Preferences.Api.Features.UpdatePreferences;
using Ecommerce.Preferences.Api.Tests.Fakes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Preferences.Api.Tests.Features;

public sealed class UpdatePreferencesEndpointTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));

    private readonly PreferencesService _service = new(new InMemoryPreferencesStore(), NullLogger<PreferencesService>.Instance);

    [Fact]
    public async Task HandleAsync_WithSupportedLanguage_ReturnsOkEnvelope()
    {
        var result = await UpdatePreferencesEndpoint.HandleAsync(Buyer, new UpdatePreferencesRequest("sr"), _service, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<PreferencesResponse>>>(result.Result);
        Assert.Equal("sr", ok.Value?.Data?.Language);
    }

    [Fact]
    public async Task HandleAsync_WithUnsupportedLanguage_ReturnsBadRequestEnvelope()
    {
        var result = await UpdatePreferencesEndpoint.HandleAsync(Buyer, new UpdatePreferencesRequest("de"), _service, TestContext.Current.CancellationToken);

        var badRequest = Assert.IsType<BadRequest<ApiResponse>>(result.Result);
        Assert.Equal("preferences.unsupported_language", badRequest.Value?.Error?.Code);
        Assert.Equal("Language [de] is not supported", badRequest.Value?.Error?.Message);
    }
}
