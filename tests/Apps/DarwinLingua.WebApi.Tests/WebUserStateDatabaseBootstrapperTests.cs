using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebUserStateDatabaseBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_RejectsNonPostgresProvider()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<WebIdentityDbContext> options = new DbContextOptionsBuilder<WebIdentityDbContext>()
            .UseSqlite(connection)
            .Options;

        await using WebIdentityDbContext dbContext = new(options);
        WebUserStateDatabaseBootstrapper bootstrapper = new(dbContext);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            bootstrapper.InitializeAsync(CancellationToken.None));
        Assert.Contains("PostgreSQL", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
