using Ecommerce.Catalog.Api.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalog.Api.Tests;

// A real EF Core provider on a private in-memory SQLite database, one per test
public sealed class SqliteCatalogDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteCatalogDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new CatalogDbContext(options);
        Context.Database.EnsureCreated();
    }

    public CatalogDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
