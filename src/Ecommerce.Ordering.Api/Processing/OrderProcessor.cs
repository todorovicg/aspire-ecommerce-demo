using Ecommerce.Ordering.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace Ecommerce.Ordering.Api.Processing;

// Polls for due orders and hands each claimed one to the processing service; safe to run on several instances
public sealed class OrderProcessor(
    IServiceProvider services,
    IOptions<OrderProcessorOptions> options,
    TimeProvider timeProvider,
    ILogger<OrderProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[{Prefix}] Started with poll interval [{PollInterval}]", nameof(OrderProcessor), options.Value.PollInterval);
        using var timer = new PeriodicTimer(options.Value.PollInterval, timeProvider);
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueOrdersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "[{Prefix}] Processing tick failed", nameof(OrderProcessor));
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task ProcessDueOrdersAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IOrderStore>();
        var processing = scope.ServiceProvider.GetRequiredService<OrderProcessingService>();

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = timeProvider.GetUtcNow();
            var order = await store.ClaimDueAsync(now, now + options.Value.LeaseDuration, cancellationToken);
            if (order is null)
            {
                return;
            }

            await processing.ProcessAsync(order, cancellationToken);
        }
    }
}
