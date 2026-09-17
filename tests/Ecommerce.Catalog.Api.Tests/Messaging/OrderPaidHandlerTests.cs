using Ecommerce.Catalog.Api.Infrastructure;
using Ecommerce.Catalog.Api.Messaging;
using Ecommerce.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Ecommerce.Catalog.Api.Tests.Messaging;

public sealed class OrderPaidHandlerTests
{
    private static readonly Guid MugId = Guid.Parse("a0000000-0000-4000-8000-000000000001");
    private static readonly Guid PosterId = Guid.Parse("a0000000-0000-4000-8000-000000000010");
    private static readonly DateTimeOffset PaidAt = new(2026, 9, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_DecrementsEachLine()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);
        var message = new OrderPaid(Guid.NewGuid(), [new OrderPaidLine(MugId, 2), new OrderPaidLine(PosterId, 1)], PaidAt);

        await OrderPaidHandler.HandleAsync(message, db.Context, NullLogger<OrderPaidHandler>.Instance, TestContext.Current.CancellationToken);

        Assert.Equal(23, await StockAsync(db.Context, MugId));
        Assert.Equal(1, await StockAsync(db.Context, PosterId));
    }

    [Fact]
    public async Task HandleAsync_FloorsAtZeroWhenStockIsShort()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);
        var message = new OrderPaid(Guid.NewGuid(), [new OrderPaidLine(PosterId, 5)], PaidAt);

        await OrderPaidHandler.HandleAsync(message, db.Context, NullLogger<OrderPaidHandler>.Instance, TestContext.Current.CancellationToken);

        Assert.Equal(0, await StockAsync(db.Context, PosterId));
    }

    [Fact]
    public async Task HandleAsync_IgnoresUnknownProducts()
    {
        using var db = new SqliteCatalogDb();
        await CatalogDbInitializer.SeedAsync(db.Context, TestContext.Current.CancellationToken);
        var message = new OrderPaid(Guid.NewGuid(), [new OrderPaidLine(Guid.NewGuid(), 1), new OrderPaidLine(MugId, 1)], PaidAt);

        await OrderPaidHandler.HandleAsync(message, db.Context, NullLogger<OrderPaidHandler>.Instance, TestContext.Current.CancellationToken);

        Assert.Equal(24, await StockAsync(db.Context, MugId));
    }

    private static Task<int> StockAsync(CatalogDbContext db, Guid productId) =>
        db.Products.AsNoTracking().Where(product => product.Id == productId).Select(product => product.AvailableStock).SingleAsync(TestContext.Current.CancellationToken);
}
