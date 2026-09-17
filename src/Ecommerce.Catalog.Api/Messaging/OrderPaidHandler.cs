using Ecommerce.Catalog.Api.Infrastructure;
using Ecommerce.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Api.Messaging;

// Decrements stock once per paid order; the durable inbox keeps a redelivered event from running this twice
public sealed class OrderPaidHandler
{
    public static async Task HandleAsync(OrderPaid message, CatalogDbContext db, ILogger<OrderPaidHandler> logger, CancellationToken cancellationToken)
    {
        foreach (var line in message.Lines)
        {
            var decremented = await db.Products
                .Where(product => product.Id == line.ProductId && product.AvailableStock >= line.Quantity)
                .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.AvailableStock, product => product.AvailableStock - line.Quantity), cancellationToken);
            if (decremented == 1)
            {
                logger.LogInformation("[{Prefix}] Decremented stock of product [{ProductId}] by [{Quantity}] for order [{OrderId}]", nameof(OrderPaidHandler), line.ProductId, line.Quantity, message.OrderId);
                continue;
            }

            // The guard failed: either the product is gone or two orders raced for the last units, so the stock floors at zero
            var floored = await db.Products
                .Where(product => product.Id == line.ProductId && product.AvailableStock < line.Quantity)
                .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.AvailableStock, 0), cancellationToken);
            if (floored == 1)
            {
                logger.LogWarning("[{Prefix}] Stock shortfall for product [{ProductId}], order [{OrderId}] needed [{Quantity}], stock floored at [0]", nameof(OrderPaidHandler), line.ProductId, message.OrderId, line.Quantity);
            }
            else
            {
                logger.LogWarning("[{Prefix}] Product [{ProductId}] from order [{OrderId}] not found in the catalog", nameof(OrderPaidHandler), line.ProductId, message.OrderId);
            }
        }
    }
}
