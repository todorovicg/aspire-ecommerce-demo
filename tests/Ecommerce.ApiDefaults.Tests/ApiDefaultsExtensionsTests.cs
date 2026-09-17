using Ecommerce.ApiDefaults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.ApiDefaults.Tests;

public sealed class ApiDefaultsExtensionsTests
{
    [Fact]
    public void AddApiDefaults_RegistersEnvelopeWriterAheadOfTheFrameworkWriter()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddApiDefaults();
        using var app = builder.Build();

        var writers = app.Services.GetServices<IProblemDetailsWriter>().ToList();

        Assert.True(writers.Count >= 2);
        Assert.IsType<ApiEnvelopeProblemDetailsWriter>(writers[0]);
        Assert.NotNull(app.Services.GetService<IProblemDetailsService>());
    }

    [Fact]
    public void AddApiErrorHandling_RegistersEnvelopeWriterWithoutOpenApi()
    {
        var builder = WebApplication.CreateBuilder();
        builder.AddApiErrorHandling();
        using var app = builder.Build();

        var writers = app.Services.GetServices<IProblemDetailsWriter>().ToList();

        Assert.IsType<ApiEnvelopeProblemDetailsWriter>(writers[0]);
        Assert.NotNull(app.Services.GetService<IProblemDetailsService>());
    }
}
