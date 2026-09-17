using Ecommerce.Contracts;
using Ecommerce.Ordering.Api.Domain;
using Ecommerce.Ordering.Api.Infrastructure;
using Microsoft.Extensions.Options;

namespace Ecommerce.Ordering.Api.Processing;

// Drives one claimed order through its next transition; the store persists the outcome and releases the lease
public sealed class OrderProcessingService(
    IOrderStore store,
    IPaymentClient payment,
    IOrderPaidPublisher publisher,
    IOptions<OrderProcessorOptions> options,
    TimeProvider timeProvider,
    ILogger<OrderProcessingService> logger)
{
    public async Task ProcessAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            switch (order.Status)
            {
                case OrderStatus.Submitted:
                    await PayAsync(order, cancellationToken);
                    break;
                case OrderStatus.Paid:
                    await ContinuePaidAsync(order, cancellationToken);
                    break;
            }
        }
        finally
        {
            order.ReleaseLease();
            await store.UpdateAsync(order, cancellationToken);
        }
    }

    private async Task PayAsync(Order order, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        order.PaymentAttempts++;
        PaymentOutcome outcome;
        try
        {
            outcome = await payment.AuthorizeAsync(order.Id, order.Total, order.Currency, order.PaymentCardNumber ?? string.Empty, order.Buyer.Name, cancellationToken);
        }
        catch (PaymentUnavailableException exception)
        {
            order.ScheduleRetry(now + options.Value.PaymentRetryDelay);
            logger.LogWarning(exception, "[{Prefix}] Payment unavailable for order [{OrderId}], attempt [{Attempt}], retrying at [{NextAttemptAt}]", nameof(OrderProcessingService), order.Id, order.PaymentAttempts, order.NextAttemptAt);
            return;
        }

        if (!outcome.Approved)
        {
            order.MarkPaymentFailed(outcome.Reason ?? "declined");
            logger.LogWarning("[{Prefix}] Payment declined for order [{OrderId}] reason [{Reason}]", nameof(OrderProcessingService), order.Id, order.FailureReason);
            return;
        }

        order.MarkPaid(outcome.PaymentId.ToString(), now, options.Value.PaidToShippedDelay);
        await store.UpdateAsync(order, cancellationToken);
        logger.LogInformation("[{Prefix}] Order [{OrderId}] paid with reference [{PaymentReference}]", nameof(OrderProcessingService), order.Id, order.PaymentReference);
        await PublishPaidAsync(order, cancellationToken);
    }

    private async Task ContinuePaidAsync(Order order, CancellationToken cancellationToken)
    {
        if (!order.PaidEventPublished)
        {
            await PublishPaidAsync(order, cancellationToken);
        }

        var now = timeProvider.GetUtcNow();
        if (order.IsShippingDue(now))
        {
            order.MarkShipped(now);
            logger.LogInformation("[{Prefix}] Order [{OrderId}] shipped", nameof(OrderProcessingService), order.Id);
        }
    }

    private async Task PublishPaidAsync(Order order, CancellationToken cancellationToken)
    {
        var message = new OrderPaid(order.Id, order.Lines.Select(line => new OrderPaidLine(line.ProductId, line.Quantity)).ToList(), order.PaidAt ?? timeProvider.GetUtcNow());
        try
        {
            await publisher.PublishAsync(message, cancellationToken);
            order.MarkPaidEventPublished(options.Value.PaidToShippedDelay);
            logger.LogInformation("[{Prefix}] Published OrderPaid for order [{OrderId}] with [{Lines}] lines", nameof(OrderProcessingService), order.Id, message.Lines.Count);
        }
        catch (OutboxUnavailableException exception)
        {
            order.ScheduleRetry(timeProvider.GetUtcNow() + options.Value.PaymentRetryDelay);
            logger.LogWarning(exception, "[{Prefix}] Could not store OrderPaid for order [{OrderId}], retrying at [{NextAttemptAt}]", nameof(OrderProcessingService), order.Id, order.NextAttemptAt);
        }
    }
}
