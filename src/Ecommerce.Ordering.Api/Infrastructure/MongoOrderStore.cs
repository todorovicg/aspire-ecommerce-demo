using Ecommerce.Ordering.Api.Domain;
using MongoDB.Driver;

namespace Ecommerce.Ordering.Api.Infrastructure;

public sealed class MongoOrderStore(IMongoDatabase database) : IOrderStore
{
    public const string CollectionName = "orders";

    private readonly IMongoCollection<Order> _orders = database.GetCollection<Order>(CollectionName);

    public async Task<bool> InsertIfAbsentAsync(Order order, CancellationToken cancellationToken)
    {
        try
        {
            await _orders.InsertOneAsync(order, cancellationToken: cancellationToken);
            return true;
        }
        catch (MongoWriteException exception) when (exception.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<Order>> ListForBuyerAsync(Guid buyerId, CancellationToken cancellationToken) =>
        await _orders.Find(order => order.BuyerId == buyerId)
            .SortByDescending(order => order.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetAsync(Guid buyerId, Guid orderId, CancellationToken cancellationToken) =>
        _orders.Find(order => order.Id == orderId && order.BuyerId == buyerId).FirstOrDefaultAsync(cancellationToken)!;

    public Task<Order?> ClaimDueAsync(DateTimeOffset now, DateTimeOffset leaseUntil, CancellationToken cancellationToken)
    {
        var filter = Builders<Order>.Filter.And(
            Builders<Order>.Filter.In(order => order.Status, [OrderStatus.Submitted, OrderStatus.Paid]),
            Builders<Order>.Filter.Lte(order => order.NextAttemptAt, now),
            Builders<Order>.Filter.Or(
                Builders<Order>.Filter.Eq(order => order.LeaseUntil, null),
                Builders<Order>.Filter.Lt(order => order.LeaseUntil, now)));
        var update = Builders<Order>.Update.Set(order => order.LeaseUntil, leaseUntil);
        var options = new FindOneAndUpdateOptions<Order>
        {
            ReturnDocument = ReturnDocument.After,
            Sort = Builders<Order>.Sort.Ascending(order => order.NextAttemptAt),
        };

        return _orders.FindOneAndUpdateAsync(filter, update, options, cancellationToken)!;
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken) =>
        _orders.ReplaceOneAsync(existing => existing.Id == order.Id, order, cancellationToken: cancellationToken);
}
