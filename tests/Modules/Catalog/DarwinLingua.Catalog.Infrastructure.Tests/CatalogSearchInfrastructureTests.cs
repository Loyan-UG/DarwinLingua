using System.Data.Common;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Infrastructure.Tests;

/// <summary>
/// Verifies the SQLite-backed catalog search strategy and its operational indexes.
/// </summary>
public sealed class CatalogSearchInfrastructureTests
{
    /// <summary>
    /// Verifies that prefix matches are ranked ahead of contains-only matches.
    /// </summary>
    [Fact]
    public async Task SearchActiveByLemmaAsync_ShouldRankPrefixMatchesBeforeContainsMatches()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-catalog-search-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.WordEntries.AddRange(
                    CreateWord("Abendbahn", "evening rail"),
                    CreateWord("Bahnhof", "station"),
                    CreateWord("Bahnsteig", "platform"));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IWordEntryRepository repository = serviceProvider.GetRequiredService<IWordEntryRepository>();
            IReadOnlyList<WordListItemModel> words = await repository.SearchActiveByLemmaAsync("bahn", "en", CancellationToken.None);

            Assert.Equal(["Bahnhof", "Bahnsteig", "Abendbahn"], words.Select(word => word.Lemma).ToArray());
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
    /// Verifies that broad contains matches are skipped when prefix matches already fill the result limit.
    /// </summary>
    [Fact]
    public async Task SearchActiveByLemmaAsync_ShouldPreferPrefixWindowBeforeContainsFallback()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-catalog-search-window-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                List<WordEntry> prefixWords = Enumerable.Range(0, 55)
                    .Select(index => CreateWord($"Bahnwort{index:D2}", $"prefix {index:D2}"))
                    .ToList();

                dbContext.WordEntries.AddRange(prefixWords);
                dbContext.WordEntries.Add(CreateWord("Abendbahn", "contains fallback"));

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IWordEntryRepository repository = serviceProvider.GetRequiredService<IWordEntryRepository>();
            IReadOnlyList<WordListItemModel> words = await repository.SearchActiveByLemmaAsync("bahn", "en", CancellationToken.None);

            Assert.Equal(50, words.Count);
            Assert.DoesNotContain(words, word => word.Lemma == "Abendbahn");
            Assert.All(words, word => Assert.StartsWith("Bahn", word.Lemma, StringComparison.Ordinal));
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
    /// Verifies that startup initialization recreates the required operational search indexes.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldCreateOperationalSearchIndexes()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-catalog-indexes-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    "DROP INDEX IF EXISTS IX_WordEntries_Search_NormalizedLemma;",
                    CancellationToken.None);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "DROP INDEX IF EXISTS IX_WordEntries_Search_ActiveNormalizedLemma;",
                    CancellationToken.None);
                await dbContext.Database.ExecuteSqlRawAsync(
                    "DROP INDEX IF EXISTS IX_WordEntries_Browse_Cefr_NormalizedLemma;",
                    CancellationToken.None);
            }

            await databaseInitializer.InitializeAsync(CancellationToken.None);

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IReadOnlySet<string> indexNames = await ReadIndexNamesAsync(verificationContext, CancellationToken.None);

            Assert.Contains("IX_WordEntries_Search_NormalizedLemma", indexNames);
            Assert.Contains("IX_WordEntries_Search_ActiveNormalizedLemma", indexNames);
            Assert.Contains("IX_WordEntries_Browse_Cefr_NormalizedLemma", indexNames);
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
    /// Verifies that legacy EnsureCreated databases are baselined into EF migration history on startup.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_ShouldBaselineLegacyEnsureCreatedDatabase()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-catalog-legacy-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext legacyContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                await legacyContext.Database.EnsureCreatedAsync(CancellationToken.None);
            }

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            await using DarwinLinguaDbContext verificationContext = await dbContextFactory
                .CreateDbContextAsync(CancellationToken.None);

            IReadOnlyList<string> appliedMigrations = await ReadAppliedMigrationsAsync(verificationContext, CancellationToken.None);

            Assert.NotEmpty(appliedMigrations);
            Assert.Contains(appliedMigrations, migrationId => migrationId.Contains("InitialCreate", StringComparison.Ordinal));
            Assert.Contains(appliedMigrations, migrationId => migrationId.Contains("AddPracticeSchedulingState", StringComparison.Ordinal));
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
    /// Builds the shared DI container used by the SQLite-backed repository tests.
    /// </summary>
    /// <param name="databasePath">The temporary database file path.</param>
    /// <returns>The configured service provider.</returns>
    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogInfrastructure();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Reads the currently defined SQLite index names for the lexical-entry table.
    /// </summary>
    /// <param name="dbContext">The verification database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The set of discovered index names.</returns>
    private static async Task<IReadOnlySet<string>> ReadIndexNamesAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        DbConnection connection = dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using DbCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT name
            FROM sqlite_master
            WHERE type = 'index'
              AND name IN (
                  'IX_WordEntries_Search_NormalizedLemma',
                  'IX_WordEntries_Search_ActiveNormalizedLemma',
                  'IX_WordEntries_Browse_Cefr_NormalizedLemma')
            ORDER BY name;
            """;

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        HashSet<string> indexNames = [];

        while (await reader.ReadAsync(cancellationToken))
        {
            indexNames.Add(reader.GetString(0));
        }

        await connection.CloseAsync();

        return indexNames;
    }

    /// <summary>
    /// Reads the EF migration history rows currently stored in the SQLite database.
    /// </summary>
    /// <param name="dbContext">The verification database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The applied migration identifiers.</returns>
    private static async Task<IReadOnlyList<string>> ReadAppliedMigrationsAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        DbConnection connection = dbContext.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using DbCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT MigrationId
            FROM "__EFMigrationsHistory"
            ORDER BY MigrationId;
            """;

        await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        List<string> migrationIds = [];

        while (await reader.ReadAsync(cancellationToken))
        {
            migrationIds.Add(reader.GetString(0));
        }

        await connection.CloseAsync();

        return migrationIds;
    }

    /// <summary>
    /// Creates a minimal active lexical aggregate suitable for search repository tests.
    /// </summary>
    /// <param name="lemma">The visible German lemma.</param>
    /// <param name="translationText">The primary English translation.</param>
    /// <returns>A minimal but valid lexical aggregate.</returns>
    private static WordEntry CreateWord(string lemma, string translationText)
    {
        DateTime nowUtc = DateTime.UtcNow;
        WordEntry word = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            lemma,
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            nowUtc,
            article: "die");

        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, nowUtc);
        sense.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), translationText, true, nowUtc);

        return word;
    }

    /// <summary>
    /// Best-effort cleanup for temporary SQLite files that may remain locked briefly on Windows.
    /// </summary>
    /// <param name="path">The file path to delete when possible.</param>
    private static void TryDeleteFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // SQLite can keep a short-lived handle on the database file after disposal on Windows.
            // Failing cleanup must not fail a passing integration test.
        }
    }
}
