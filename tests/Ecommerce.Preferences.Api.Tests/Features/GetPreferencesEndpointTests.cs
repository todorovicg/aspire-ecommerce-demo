using Ecommerce.ApiDefaults;
using Ecommerce.Preferences.Api.Features;
using Ecommerce.Preferences.Api.Features.GetPreferences;
using Ecommerce.Preferences.Api.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Preferences.Api.Tests.Features;

public sealed class GetPreferencesEndpointTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));

    [Fact]
    public async Task HandleAsync_WithoutStoredPreferences_ReturnsEnglishEnvelope()
    {
        var service = new PreferencesService(new InMemoryPreferencesStore(), NullLogger<PreferencesService>.Instance);

        var result = await GetPreferencesEndpoint.HandleAsync(Buyer, service, TestContext.Current.CancellationToken);

        Assert.NotNull(result.Value);
        Assert.True(result.Value.Success);
        Assert.Equal("en", result.Value.Data?.Language);
    }
}
