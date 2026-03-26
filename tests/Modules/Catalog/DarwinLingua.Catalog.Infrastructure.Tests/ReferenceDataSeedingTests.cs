using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies that Phase 1 reference data seeding is present and idempotent.
/// </summary>
public sealed class ReferenceDataSeedingTests
{
    /// <summary>
    /// Verifies that startup initialization seeds expected stable languages and topics.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldSeedExpectedReferenceData()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-seed-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            int activeLanguageCount = await verificationContext.Languages
                .CountAsync(cancellationToken: CancellationToken.None);
            int topicCount = await verificationContext.Topics
                .CountAsync(cancellationToken: CancellationToken.None);
            int topicLocalizationCount = await verificationContext.TopicLocalizations
                .CountAsync(cancellationToken: CancellationToken.None);

            Assert.Equal(2, activeLanguageCount);
            Assert.Equal(5, topicCount);
            Assert.Equal(10, topicLocalizationCount);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(databasePath);
        }
    }

    /// <summary>
    /// Verifies that running the startup initialization repeatedly does not duplicate seeded rows.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldKeepReferenceDataSeedingIdempotent()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-seed-idempotent-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await databaseInitializer.InitializeAsync(CancellationToken.None);
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            int activeLanguageCount = await verificationContext.Languages
                .CountAsync(cancellationToken: CancellationToken.None);
            int topicCount = await verificationContext.Topics
                .CountAsync(cancellationToken: CancellationToken.None);
            int topicLocalizationCount = await verificationContext.TopicLocalizations
                .CountAsync(cancellationToken: CancellationToken.None);

            Assert.Equal(2, activeLanguageCount);
            Assert.Equal(5, topicCount);
            Assert.Equal(10, topicLocalizationCount);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            TryDeleteFile(databasePath);
        }
    }

    /// <summary>
    /// Builds the service provider used by the reference-seeding tests.
    /// </summary>
    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogInfrastructure()
            .AddLocalizationInfrastructure();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Deletes a temporary test database file while safely ignoring lock races.
    /// </summary>
    private static void TryDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException)
        {
            // Ignore transient file-lock races during test cleanup.
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore cleanup failures caused by external process locks.
        }
    }
}
