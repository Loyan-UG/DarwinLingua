using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies explicit app-initialization use-case workflows for schema prep and reference seeding.
/// </summary>
public sealed class DatabaseInitializationUseCaseTests
{
    /// <summary>
    /// Verifies that the schema initialization use case creates the database without running seeders.
    /// </summary>
    [Fact]
    public async Task EnsureDatabaseSchemaAsync_ShouldPrepareSchemaWithoutSeedingReferenceData()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-init-schema-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            Assert.True(await verificationContext.Database.CanConnectAsync(CancellationToken.None));
            Assert.Equal(
                0,
                await verificationContext.Languages.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                0,
                await verificationContext.Topics.CountAsync(cancellationToken: CancellationToken.None));
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
    /// Verifies that the explicit reference-data use case seeds stable language and topic rows.
    /// </summary>
    [Fact]
    public async Task SeedReferenceDataAsync_ShouldSeedExpectedLanguageAndTopicRows()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-init-seed-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await databaseInitializer.EnsureDatabaseSchemaAsync(CancellationToken.None);
            await databaseInitializer.SeedReferenceDataAsync(CancellationToken.None);

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            Assert.Equal(
                2,
                await verificationContext.Languages.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                5,
                await verificationContext.Topics.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                10,
                await verificationContext.TopicLocalizations.CountAsync(cancellationToken: CancellationToken.None));
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
    /// Builds the service provider used by initialization use-case tests.
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
