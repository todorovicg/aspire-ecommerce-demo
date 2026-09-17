using Ecommerce.ApiDefaults;
using Ecommerce.Preferences.Api.Features;
using Ecommerce.Preferences.Api.Tests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Preferences.Api.Tests.Features;

public sealed class PreferencesServiceTests
{
    private static readonly BuyerId Buyer = new(Guid.Parse("b0000000-0000-4000-8000-000000000001"));

    private readonly InMemoryPreferencesStore _store = new();

    private PreferencesService CreateService() => new(_store, NullLogger<PreferencesService>.Instance);

    [Fact]
    public async Task GetAsync_WithoutStoredPreferences_ReturnsEnglish()
    {
        var preferences = await CreateService().GetAsync(Buyer, TestContext.Current.CancellationToken);

        Assert.Equal("en", preferences.Language);
        Assert.Equal(0, _store.SaveCount);
    }

    [Fact]
    public async Task UpdateAsync_WithSupportedLanguage_StoresAndReturnsIt()
    {
        var service = CreateService();

        var result = await service.UpdateAsync(Buyer, "sr", TestContext.Current.CancellationToken);

        var updated = Assert.IsType<UpdatePreferencesResult.Updated>(result);
        Assert.Equal("sr", updated.Preferences.Language);
        Assert.Equal(1, _store.SaveCount);
        Assert.Equal("sr", (await service.GetAsync(Buyer, TestContext.Current.CancellationToken)).Language);
    }

    [Fact]
    public async Task UpdateAsync_WithUnsupportedLanguage_ReturnsUnsupportedWithoutSaving()
    {
        var result = await CreateService().UpdateAsync(Buyer, "de", TestContext.Current.CancellationToken);

        var unsupported = Assert.IsType<UpdatePreferencesResult.UnsupportedLanguage>(result);
        Assert.Equal("de", unsupported.Language);
        Assert.Equal(0, _store.SaveCount);
    }
}
