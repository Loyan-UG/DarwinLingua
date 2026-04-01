using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Models;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
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

    [Fact]
    public async Task DatabaseService_ExcludesDraftPackagesFromGlobalManifestAsync()
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

            ContentStreamEntity stream = await seedContext.ContentStreams.SingleAsync(existingStream => existingStream.ContentAreaKey == "catalog" && existingStream.SliceKey == "full");
            seedContext.PublishedPackages.Add(new PublishedPackageEntity
            {
                Id = Guid.NewGuid(),
                PackageId = "darwin-deutsch-catalog-full-draft",
                ContentStreamId = stream.Id,
                ContentStream = stream,
                PackageType = "full-catalog",
                Version = "2026.03.30.2",
                PublicationBatchId = "draft-batch",
                PublicationStatus = PackagePublicationStatus.Draft,
                SchemaVersion = 1,
                MinimumAppSchemaVersion = 1,
                Checksum = "draft-checksum",
                EntryCount = 41,
                WordCount = 41,
                CreatedAtUtc = new DateTimeOffset(2026, 03, 30, 11, 0, 0, TimeSpan.Zero),
                UpdatedAtUtc = new DateTimeOffset(2026, 03, 30, 11, 0, 0, TimeSpan.Zero),
                RelativeDownloadPath = "/downloads/packages/darwin-deutsch-catalog-full-draft.json",
            });
            await seedContext.SaveChangesAsync();
        }

        await using (ServerContentDbContext queryContext = new(dbOptions))
        {
            DatabaseMobileContentManifestService service = new(queryContext, Options.Create(options));

            MobileContentManifestResponse manifest = service.GetAreaManifest("darwin-deutsch", "catalog");

            Assert.DoesNotContain(manifest.Packages, package => package.PackageId == "darwin-deutsch-catalog-full-draft");
        }
    }

    [Fact]
    public async Task DatabaseService_ThrowsInvalidOperation_WhenClientProductKeyIsMissingAndMultipleProductsAreActiveAsync()
    {
        await using SqliteConnection connection = new("Data Source=:memory:");
        await connection.OpenAsync();

        DbContextOptions<ServerContentDbContext> dbOptions = new DbContextOptionsBuilder<ServerContentDbContext>()
            .UseSqlite(connection)
            .Options;

        ServerContentOptions options = CreateOptions();
        options.ClientProducts.Add(new ClientProductOptions
        {
            Key = "darwin-persian",
            DisplayName = "Darwin Persian",
            LearningLanguageCode = "fa",
            DefaultUiLanguageCode = "fa",
            IsActive = true,
        });

        await using (ServerContentDbContext seedContext = new(dbOptions))
        {
            ServerContentDatabaseBootstrapper bootstrapper = new(seedContext, Options.Create(options));
            await bootstrapper.InitializeAsync(CancellationToken.None);
        }

        await using (ServerContentDbContext queryContext = new(dbOptions))
        {
            DatabaseMobileContentManifestService service = new(queryContext, Options.Create(options));

            Assert.Throws<InvalidOperationException>(() => service.GetGlobalManifest(null));
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
