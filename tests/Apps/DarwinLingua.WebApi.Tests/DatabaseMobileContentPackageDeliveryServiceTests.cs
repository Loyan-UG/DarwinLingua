using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class DatabaseMobileContentPackageDeliveryServiceTests
{
    [Fact]
    public async Task GetLatestFullPackage_ReturnsExistingPayloadAsync()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ServerContentDbContext> dbOptions = new DbContextOptionsBuilder<ServerContentDbContext>()
            .UseSqlite(connection)
            .Options;

        ServerContentOptions options = CreateOptions();
        TestWebHostEnvironment environment = new()
        {
            ContentRootPath = "D:\\_Projects\\DarwinLingua",
        };

        await using (ServerContentDbContext seedContext = new(dbOptions))
        {
            ServerContentDatabaseBootstrapper bootstrapper = new(seedContext, Options.Create(options));
            await bootstrapper.InitializeAsync(CancellationToken.None);
        }

        await using (ServerContentDbContext queryContext = new(dbOptions))
        {
            DatabaseMobileContentPackageDeliveryService service = new(queryContext, Options.Create(options), environment);

            var descriptor = service.GetLatestFullPackage("darwin-deutsch", clientSchemaVersion: 1);

            Assert.Equal("darwin-deutsch-all-full-v1", descriptor.PackageId);
            Assert.True(File.Exists(descriptor.FilePath));
        }
    }

    [Fact]
    public async Task GetLatestCefrPackage_RejectsIncompatibleClientSchemaAsync()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ServerContentDbContext> dbOptions = new DbContextOptionsBuilder<ServerContentDbContext>()
            .UseSqlite(connection)
            .Options;

        ServerContentOptions options = CreateOptions();
        options.Packages[2].MinimumAppSchemaVersion = 2;

        TestWebHostEnvironment environment = new()
        {
            ContentRootPath = "D:\\_Projects\\DarwinLingua",
        };

        await using (ServerContentDbContext seedContext = new(dbOptions))
        {
            ServerContentDatabaseBootstrapper bootstrapper = new(seedContext, Options.Create(options));
            await bootstrapper.InitializeAsync(CancellationToken.None);
        }

        await using (ServerContentDbContext queryContext = new(dbOptions))
        {
            DatabaseMobileContentPackageDeliveryService service = new(queryContext, Options.Create(options), environment);

            Assert.Throws<MobileContentSchemaCompatibilityException>(() =>
                service.GetLatestCefrPackage("darwin-deutsch", "A1", clientSchemaVersion: 1));
        }
    }

    private static ServerContentOptions CreateOptions()
    {
        ServerContentOptions options = new()
        {
            PublicBaseUrl = "http://localhost:5099",
            PackageStorage = new PackageStorageOptions
            {
                RootPath = "assets/ServerContent/PublishedPackages",
            },
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
            PackageId = "darwin-deutsch-all-full-v1",
            ClientProductKey = "darwin-deutsch",
            ContentAreaKey = "all",
            SliceKey = "full",
            PackageType = "full-database",
            Version = "2026.03.30.1",
            SchemaVersion = 1,
            MinimumAppSchemaVersion = 1,
            Checksum = "checksum-all",
            EntryCount = 41,
            WordCount = 41,
            CreatedAtUtc = new DateTimeOffset(2026, 03, 30, 10, 0, 0, TimeSpan.Zero),
            RelativeDownloadPath = "darwin-deutsch/darwin-deutsch-all-full-v1.json",
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
            RelativeDownloadPath = "darwin-deutsch/darwin-deutsch-catalog-full-v1.json",
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
            RelativeDownloadPath = "darwin-deutsch/darwin-deutsch-catalog-a1-v1.json",
        });

        return options;
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "DarwinLingua.WebApi.Tests";

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = Environments.Development;

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
