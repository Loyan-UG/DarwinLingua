using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebUserStateDatabaseBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_CreatesWebUserStateTablesWhenIdentityTablesAlreadyExist()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<WebIdentityDbContext> options = new DbContextOptionsBuilder<WebIdentityDbContext>()
            .UseSqlite(connection)
            .Options;

        await using WebIdentityDbContext dbContext = new(options);
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE "AspNetUsers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY
            );
            """);

        WebUserStateDatabaseBootstrapper bootstrapper = new(dbContext);
        await bootstrapper.InitializeAsync(CancellationToken.None);
        await bootstrapper.InitializeAsync(CancellationToken.None);

        string[] tableNames =
        [
            "WebUserPreferences",
            "WebUserFavoriteWords",
            "WebUserWordStates",
        ];

        foreach (string tableName in tableNames)
        {
            int count = await CountTableAsync(connection, tableName);
            Assert.Equal(1, count);
        }
    }

    private static async Task<int> CountTableAsync(SqliteConnection connection, string tableName)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName";
        command.Parameters.AddWithValue("$tableName", tableName);

        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}
