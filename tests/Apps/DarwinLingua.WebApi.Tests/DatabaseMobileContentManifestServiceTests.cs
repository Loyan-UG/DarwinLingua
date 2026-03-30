using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class DatabaseMobileContentManifestServiceTests
{
    [Fact]
    public async Task Bootstrapper_SeedsConfiguredProductsAndPackagesAsync()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ServerContentDbContext> dbOptions = new DbContextOptionsBuilder<ServerContentDbContext>()
            .UseSqlite(connection)
            .Options;

        ServerContentOptions options = CreateOptions();

        await using (ServerContentDbContext dbContext = new(dbOptions))
        {
            ServerContentDatabaseBootstrapper bootstrapper = new(dbContext, Options.Create(options));
            await bootstrapper.InitializeAsync(CancellationToken.None);
        }

        await using (ServerContentDbContext dbContext = new(dbOptions))
        {
            Assert.Equal(1, await dbContext.ClientProducts.CountAsync());
            Assert.Equal(2, await dbContext.ContentStreams.CountAsync());
            Assert.Equal(2, await dbContext.PublishedPackages.CountAsync());
        }
    }

    [Fact]
    public async Task DatabaseService_ReturnsCefrScopedPackagesAsync()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ServerContentDbContext> dbOptions = new DbContextOptionsBuilder<ServerContentDbContext>()
            .UseSqlite(connection)
            .Options;

        ServerContentOptions options = CreateOptions();

        await using (ServerContentDbContext seedContext = new(dbOptions))
        {
            ServerContentDatabaseBootstrapper bootstrapper = new(seedContext, Options.Create(options));
            await bootstrapper.InitializeAsync(CancellationToken.None);
        }

        await using (ServerContentDbContext queryContext = new(dbOptions))
        {
            DatabaseMobileContentManifestService service = new(queryContext, Options.Create(options));

            var manifest = service.GetCefrManifest("darwin-deutsch", "A1");

            Assert.Single(manifest.Packages);
            Assert.Equal("cefr:a1", manifest.SliceKey);
            Assert.Equal("darwin-deutsch-catalog-a1-v1", manifest.Packages[0].PackageId);
        }
    }

    private static ServerContentOptions CreateOptions()
    {
        ServerContentOptions options = new()
        {
            PublicBaseUrl = "http://localhost:5099",
        };

        options.ClientProducts.Add(new ClientProductOptions
        {
            Key = "darwin-deutsch",
            DisplayName = "Darwin Deutsch",
            LearningLanguageCode = "de",
            DefaultUiLanguageCode = "en",
            IsActive = true,
        });

        options.Packages.Add(new PublishedPackageOptions
        {
            PackageId = "darwin-deutsch-catalog-full-v1",
            ClientProductKey = "darwin-deutsch",
            ContentAreaKey = "catalog",
            SliceKey = "full",
            PackageType = "full-catalog",
            Version = "2026.03.30.1",
            SchemaVersion = 1,
            MinimumAppSchemaVersion = 1,
            Checksum = "checksum-full",
            EntryCount = 41,
            WordCount = 41,
            CreatedAtUtc = new DateTimeOffset(2026, 03, 30, 10, 0, 0, TimeSpan.Zero),
            RelativeDownloadPath = "/downloads/packages/darwin-deutsch-catalog-full-v1.json",
        });

        options.Packages.Add(new PublishedPackageOptions
        {
            PackageId = "darwin-deutsch-catalog-a1-v1",
            ClientProductKey = "darwin-deutsch",
            ContentAreaKey = "catalog",
            SliceKey = "cefr:a1",
            PackageType = "catalog-cefr",
            Version = "2026.03.30.1",
            SchemaVersion = 1,
            MinimumAppSchemaVersion = 1,
            Checksum = "checksum-a1",
            EntryCount = 12,
            WordCount = 12,
            CreatedAtUtc = new DateTimeOffset(2026, 03, 30, 10, 0, 0, TimeSpan.Zero),
            RelativeDownloadPath = "/downloads/packages/darwin-deutsch-catalog-a1-v1.json",
        });

        return options;
    }
}
