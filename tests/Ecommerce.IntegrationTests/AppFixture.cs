using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Ecommerce.IntegrationTests;

public sealed class AppFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromMinutes(5);

    private DistributedApplication? app;

    public HttpClient Gateway { get; private set; } = null!;

    public HttpClient Frontend { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        using var startup = new CancellationTokenSource(StartupTimeout);

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Ecommerce_AppHost>(startup.Token);
        app = await builder.BuildAsync(startup.Token);
        await app.StartAsync(startup.Token);

        await app.ResourceNotifications.WaitForResourceHealthyAsync("gateway", startup.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("payment-api", startup.Token);
        await app.ResourceNotifications.WaitForResourceHealthyAsync("frontend", startup.Token);

        Gateway = app.CreateHttpClient("gateway", "http");
        Frontend = app.CreateHttpClient("frontend", "http");
    }

    public async ValueTask DisposeAsync()
    {
        Gateway?.Dispose();
        Frontend?.Dispose();

        if (app is not null)
        {
            await app.DisposeAsync();
        }
    }
}
