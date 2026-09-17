using Ecommerce.ApiDefaults;
using Ecommerce.Catalog.Api.Features;
using Ecommerce.Catalog.Api.Features.GetProduct;
using Ecommerce.Catalog.Api.Infrastructure;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ecommerce.Catalog.Api.Tests.Features;

public sealed class GetProductEndpointTests
{
    private static readonly Guid MugId = Guid.Parse("a0000000-0000-4000-8000-000000000001");

    [Fact]
    public async Task HandleAsync_WithExistingId_ReturnsProduct()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await GetProductEndpoint.HandleAsync(MugId, RequestLanguage.English, db.Context, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<ProductResponse>>>(result.Result);
        Assert.NotNull(ok.Value?.Data);
        Assert.Equal(MugId, ok.Value.Data.Id);
        Assert.Equal("Aspire Ceramic Mug", ok.Value.Data.Name);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownId_ReturnsNotFoundEnvelope()
    {
        using var db = new SqliteCatalogDb();
        var unknownId = Guid.Parse("a0000000-0000-4000-8000-0000000000ff");

        var result = await GetProductEndpoint.HandleAsync(unknownId, RequestLanguage.English, db.Context, TestContext.Current.CancellationToken);

        var notFound = Assert.IsType<NotFound<ApiResponse>>(result.Result);
        Assert.NotNull(notFound.Value?.Error);
        Assert.False(notFound.Value.Success);
        Assert.Equal("catalog.product_not_found", notFound.Value.Error.Code);
        Assert.Equal($"Product [{unknownId}] not found", notFound.Value.Error.Message);
    }

    [Fact]
    public async Task HandleAsync_WithSerbian_ReturnsTranslatedProduct()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await GetProductEndpoint.HandleAsync(MugId, new RequestLanguage("sr"), db.Context, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<ProductResponse>>>(result.Result);
        Assert.Equal("Aspire keramička šolja", ok.Value?.Data?.Name);
        Assert.Equal("Posuđe za piće", ok.Value?.Data?.Category);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownLanguage_FallsBackToEnglish()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);

        var result = await GetProductEndpoint.HandleAsync(MugId, new RequestLanguage("de"), db.Context, TestContext.Current.CancellationToken);

        var ok = Assert.IsType<Ok<ApiResponse<ProductResponse>>>(result.Result);
        Assert.Equal("Aspire Ceramic Mug", ok.Value?.Data?.Name);
    }
}
