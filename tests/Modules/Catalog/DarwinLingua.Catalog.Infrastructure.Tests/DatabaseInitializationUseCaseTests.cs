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
            Assert.Equal(
                0,
                await verificationContext.ExpressionEntries.CountAsync(cancellationToken: CancellationToken.None));
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
    /// Verifies that server startup retrofits Expressions tables for existing PostgreSQL databases.
    /// </summary>
    [Fact]
    public void DatabaseInitializer_ShouldRetainPostgresExpressionEntryRetrofitSchema()
    {
        string sourcePath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "BuildingBlocks",
            "DarwinLingua.Infrastructure",
            "Persistence",
            "DarwinLinguaDatabaseInitializer.cs");
        string source = File.ReadAllText(sourcePath);

        Assert.Contains("EnsureExpressionEntrySchemaAsync", source, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE IF NOT EXISTS \"ExpressionEntries\"", source, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE IF NOT EXISTS \"ExpressionMeanings\"", source, StringComparison.Ordinal);
        Assert.Contains("CREATE TABLE IF NOT EXISTS \"ExpressionExampleTranslations\"", source, StringComparison.Ordinal);
        Assert.Contains("CREATE UNIQUE INDEX IF NOT EXISTS \"IX_ExpressionEntries_Slug\"", source, StringComparison.Ordinal);
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
                11,
                await verificationContext.Languages.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                5,
                await verificationContext.Topics.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                55,
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
    /// Verifies that first-run initialization on a clean install path creates and seeds the local database.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldCreateAndSeedCleanInstallDatabase()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-clean-install-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            Assert.False(File.Exists(databasePath));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await databaseInitializer.InitializeAsync(CancellationToken.None);

            Assert.True(File.Exists(databasePath));

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            Assert.True(await verificationContext.Database.CanConnectAsync(CancellationToken.None));
            Assert.Equal(
                11,
                await verificationContext.Languages.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                5,
                await verificationContext.Topics.CountAsync(cancellationToken: CancellationToken.None));
            Assert.Equal(
                55,
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
    /// Finds the repository root without depending on the test output path.
    /// </summary>
    private static string FindRepositoryRoot()
    {
        foreach (string startPath in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            DirectoryInfo? directory = new(startPath);

            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DarwinLingua.slnx")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Could not find the DarwinLingua repository root.");
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
