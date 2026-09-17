using Ecommerce.Ordering.Api.Domain;
using MongoDB.Driver;

namespace Ecommerce.Ordering.Api.Infrastructure;

// Creates the indexes the read endpoints and the order processor rely on
public sealed class OrderingDbInitializer(IMongoDatabase database, ILogger<OrderingDbInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var orders = database.GetCollection<Order>(MongoOrderStore.CollectionName);
        var byBuyer = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(order => order.BuyerId).Descending(order => order.CreatedAt),
            new CreateIndexOptions { Name = "buyer_created" });
        var dueForProcessing = new CreateIndexModel<Order>(
            Builders<Order>.IndexKeys.Ascending(order => order.Status).Ascending(order => order.NextAttemptAt),
            new CreateIndexOptions { Name = "status_next_attempt" });

        await orders.Indexes.CreateManyAsync([byBuyer, dueForProcessing], cancellationToken);
        logger.LogInformation("[{Prefix}] Ensured indexes on collection [{Collection}]", nameof(OrderingDbInitializer), MongoOrderStore.CollectionName);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
