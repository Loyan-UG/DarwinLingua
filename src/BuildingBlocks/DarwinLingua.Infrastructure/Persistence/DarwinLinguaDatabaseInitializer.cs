using System.Data.Common;
using System.Text;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Infrastructure.Persistence.Options;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Infrastructure.Persistence;

/// <summary>
/// Creates the local SQLite database and executes all registered seed workflows.
/// </summary>
internal sealed class DarwinLinguaDatabaseInitializer : IDatabaseInitializer
{
    private const int DefaultBusyTimeoutSeconds = 5;
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;
    private readonly IReadOnlyCollection<IDatabaseSeeder> _databaseSeeders;
    private readonly SqliteDatabaseOptions? _sqliteDatabaseOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="DarwinLinguaDatabaseInitializer"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The context factory used to create database sessions.</param>
    /// <param name="databaseSeeders">The registered module seeders.</param>
    public DarwinLinguaDatabaseInitializer(
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
        IEnumerable<IDatabaseSeeder> databaseSeeders,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);
        ArgumentNullException.ThrowIfNull(databaseSeeders);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _dbContextFactory = dbContextFactory;
        _databaseSeeders = databaseSeeders.ToArray();
        _sqliteDatabaseOptions = serviceProvider.GetService<SqliteDatabaseOptions>();
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseSchemaAsync(cancellationToken).ConfigureAwait(false);
        await SeedReferenceDataAsync(cancellationToken).ConfigureAwait(false);
        await BootstrapCatalogContentAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task EnsureDatabaseSchemaAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await PrepareSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
        await EnsureRetrofitSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
        await ApplySqliteOperationalIndexesAsync(dbContext, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SeedReferenceDataAsync(CancellationToken cancellationToken)
    {
        foreach (IDatabaseSeeder databaseSeeder in _databaseSeeders)
        {
            await databaseSeeder.SeedAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Prepares the database schema using migrations and baselines legacy EnsureCreated databases when required.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task PrepareSchemaAsync(DarwinLinguaDbContext dbContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (!dbContext.Database.IsSqlite())
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        string[] availableMigrations = dbContext.Database
            .GetMigrations()
            .ToArray();

        if (availableMigrations.Length == 0)
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await BaselineLegacyEnsureCreatedDatabaseAsync(dbContext, availableMigrations, cancellationToken)
            .ConfigureAwait(false);
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts the initial migration history row when an older EnsureCreated database already contains the schema.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <param name="availableMigrations">The available migration identifiers in apply order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task BaselineLegacyEnsureCreatedDatabaseAsync(
        DarwinLinguaDbContext dbContext,
        IReadOnlyList<string> availableMigrations,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(availableMigrations);

        if (availableMigrations.Count == 0)
        {
            return;
        }

        if (await TableExistsAsync(dbContext, "__EFMigrationsHistory", cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        bool hasLegacySchema =
            await TableExistsAsync(dbContext, "Languages", cancellationToken).ConfigureAwait(false) ||
            await TableExistsAsync(dbContext, "WordEntries", cancellationToken).ConfigureAwait(false) ||
            await TableExistsAsync(dbContext, "ContentPackages", cancellationToken).ConfigureAwait(false);

        if (!hasLegacySchema)
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" TEXT NOT NULL
            );
            """,
            cancellationToken).ConfigureAwait(false);

        string[] migrationsToBaseline = await DatabaseMatchesCurrentModelTablesAsync(dbContext, cancellationToken).ConfigureAwait(false)
            ? availableMigrations.ToArray()
            : [availableMigrations[0]];

        string productVersion = dbContext.Model.GetProductVersion()
            ?? throw new InvalidOperationException("The EF Core relational model does not expose a product version.");

        foreach (string migrationId in migrationsToBaseline)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT {migrationId}, {productVersion}
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "__EFMigrationsHistory"
                    WHERE "MigrationId" = {migrationId});
                """,
                cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Checks whether the current SQLite database already contains every mapped table in the current model.
    /// </summary>
    private static async Task<bool> DatabaseMatchesCurrentModelTablesAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        string[] requiredTableNames = dbContext.Model
            .GetEntityTypes()
            .Select(entityType => entityType.GetTableName())
            .Where(tableName => !string.IsNullOrWhiteSpace(tableName))
            .Distinct(StringComparer.Ordinal)
            .ToArray()!;

        foreach (string tableName in requiredTableNames)
        {
            if (!await TableExistsAsync(dbContext, tableName, cancellationToken).ConfigureAwait(false))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Applies operational SQLite indexes that must also exist for databases created before newer model changes.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task ApplySqliteOperationalIndexesAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (!dbContext.Database.IsSqlite())
        {
            return;
        }

        // EnsureCreated does not retrofit new indexes onto an existing database file, so critical
        // read-path indexes are applied idempotently during every startup initialization.
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS IX_WordEntries_Search_NormalizedLemma
            ON WordEntries (NormalizedLemma);
            """,
            cancellationToken).ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS IX_WordEntries_Search_ActiveNormalizedLemma
            ON WordEntries (PublicationStatus, NormalizedLemma);
            """,
            cancellationToken).ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS IX_WordEntries_Browse_Cefr_NormalizedLemma
            ON WordEntries (PrimaryCefrLevel, NormalizedLemma);
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureRetrofitSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (await TableExistsAsync(dbContext, "WordLexicalForms", cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        if (dbContext.Database.IsSqlite())
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS WordLexicalForms (
                    Id TEXT NOT NULL PRIMARY KEY,
                    WordEntryId TEXT NOT NULL,
                    PartOfSpeech TEXT NOT NULL,
                    Article TEXT NULL,
                    PluralForm TEXT NULL,
                    InfinitiveForm TEXT NULL,
                    SortOrder INTEGER NOT NULL,
                    IsPrimary INTEGER NOT NULL,
                    CreatedAtUtc TEXT NOT NULL,
                    UpdatedAtUtc TEXT NOT NULL,
                    CONSTRAINT FK_WordLexicalForms_WordEntries_WordEntryId
                        FOREIGN KEY (WordEntryId) REFERENCES WordEntries (Id) ON DELETE CASCADE
                );

                CREATE UNIQUE INDEX IF NOT EXISTS IX_WordLexicalForms_WordEntryId_PartOfSpeech
                    ON WordLexicalForms (WordEntryId, PartOfSpeech);

                CREATE UNIQUE INDEX IF NOT EXISTS IX_WordLexicalForms_WordEntryId_SortOrder
                    ON WordLexicalForms (WordEntryId, SortOrder);

                CREATE UNIQUE INDEX IF NOT EXISTS IX_WordLexicalForms_PrimaryPerWordEntry
                    ON WordLexicalForms (WordEntryId)
                    WHERE IsPrimary = 1;
                """,
                cancellationToken).ConfigureAwait(false);

            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "WordLexicalForms" (
                "Id" uuid NOT NULL,
                "WordEntryId" uuid NOT NULL,
                "PartOfSpeech" character varying(32) NOT NULL,
                "Article" character varying(32),
                "PluralForm" character varying(256),
                "InfinitiveForm" character varying(256),
                "SortOrder" integer NOT NULL,
                "IsPrimary" boolean NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_WordLexicalForms" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_WordLexicalForms_WordEntries_WordEntryId"
                    FOREIGN KEY ("WordEntryId") REFERENCES "WordEntries" ("Id") ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordLexicalForms_WordEntryId_PartOfSpeech"
                ON "WordLexicalForms" ("WordEntryId", "PartOfSpeech");

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordLexicalForms_WordEntryId_SortOrder"
                ON "WordLexicalForms" ("WordEntryId", "SortOrder");

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordLexicalForms_PrimaryPerWordEntry"
                ON "WordLexicalForms" ("WordEntryId")
                WHERE "IsPrimary" = TRUE;
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task BootstrapCatalogContentAsync(CancellationToken cancellationToken)
    {
        SqliteDatabaseOptions? sqliteDatabaseOptions = _sqliteDatabaseOptions;
        if (sqliteDatabaseOptions is null ||
            string.IsNullOrWhiteSpace(sqliteDatabaseOptions.DatabasePath) ||
            string.IsNullOrWhiteSpace(sqliteDatabaseOptions.SeedDatabasePath))
        {
            return;
        }

        string runtimeDatabasePath = sqliteDatabaseOptions.DatabasePath;
        string seedDatabasePath = sqliteDatabaseOptions.SeedDatabasePath;

        if (!File.Exists(runtimeDatabasePath) || !File.Exists(seedDatabasePath))
        {
            return;
        }

        if (string.Equals(
            Path.GetFullPath(runtimeDatabasePath),
            Path.GetFullPath(seedDatabasePath),
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await using SqliteConnection connection = CreateMaintenanceConnection(runtimeDatabasePath);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await ExecuteNonQueryAsync(connection, $"PRAGMA busy_timeout = {DefaultBusyTimeoutSeconds * 1000};", cancellationToken)
            .ConfigureAwait(false);

        bool seedAttached = false;

        try
        {
            await using SqliteCommand attachCommand = connection.CreateCommand();
            attachCommand.CommandText = "ATTACH DATABASE $seedPath AS seed;";
            attachCommand.Parameters.AddWithValue("$seedPath", seedDatabasePath);
            await attachCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            seedAttached = true;

            int pendingPackages = await ExecuteScalarIntAsync(
                connection,
                """
                SELECT COUNT(*)
                FROM seed.ContentPackages AS seedPackages
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM ContentPackages AS localPackages
                    WHERE localPackages.PackageId = seedPackages.PackageId);
                """,
                cancellationToken).ConfigureAwait(false);

            if (pendingPackages == 0)
            {
                return;
            }

            await using SqliteTransaction transaction = (SqliteTransaction)await connection
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
            await ExecuteNonQueryAsync(connection, CreateSeedMergeScript(), cancellationToken, transaction).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (seedAttached)
            {
                await using SqliteCommand detachCommand = connection.CreateCommand();
                detachCommand.CommandText = "DETACH DATABASE seed;";
                await detachCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static SqliteConnection CreateMaintenanceConnection(string databasePath)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = databasePath,
            Pooling = false,
            DefaultTimeout = DefaultBusyTimeoutSeconds,
            Mode = SqliteOpenMode.ReadWriteCreate,
        };

        return new SqliteConnection(builder.ToString());
    }

    private static async Task<int> ExecuteScalarIntAsync(
        SqliteConnection connection,
        string commandText,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToInt32(result);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        string commandText,
        CancellationToken cancellationToken,
        SqliteTransaction? transaction = null)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = commandText;
        command.Transaction = transaction;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static string CreateSeedMergeScript()
    {
        StringBuilder scriptBuilder = new();

        scriptBuilder.AppendLine(
            """
            CREATE TEMP TABLE IF NOT EXISTS NewSeedPackages AS
            SELECT seedPackages.Id,
                   seedPackages.PackageId
            FROM seed.ContentPackages AS seedPackages
            WHERE NOT EXISTS (
                SELECT 1
                FROM ContentPackages AS localPackages
                WHERE localPackages.PackageId = seedPackages.PackageId);

            CREATE TEMP TABLE IF NOT EXISTS NewSeedWordPublicIds AS
            SELECT DISTINCT seedEntries.ImportedWordEntryPublicId AS PublicId
            FROM seed.ContentPackageEntries AS seedEntries
            WHERE seedEntries.ContentPackageId IN (SELECT Id FROM NewSeedPackages)
              AND seedEntries.ImportedWordEntryPublicId IS NOT NULL;

            INSERT INTO Languages
            SELECT *
            FROM seed.Languages AS seedLanguages
            WHERE NOT EXISTS (
                SELECT 1
                FROM Languages AS localLanguages
                WHERE localLanguages.Code = seedLanguages.Code);

            INSERT INTO Topics
            SELECT *
            FROM seed.Topics AS seedTopics
            WHERE NOT EXISTS (
                SELECT 1
                FROM Topics AS localTopics
                WHERE localTopics.[Key] = seedTopics.[Key]);

            INSERT INTO TopicLocalizations
            SELECT *
            FROM seed.TopicLocalizations AS seedTopicLocalizations
            WHERE NOT EXISTS (
                SELECT 1
                FROM TopicLocalizations AS localTopicLocalizations
                WHERE localTopicLocalizations.TopicId = seedTopicLocalizations.TopicId
                  AND localTopicLocalizations.LanguageCode = seedTopicLocalizations.LanguageCode);

            INSERT INTO ContentPackages
            SELECT *
            FROM seed.ContentPackages AS seedPackages
            WHERE seedPackages.Id IN (SELECT Id FROM NewSeedPackages);

            INSERT INTO WordEntries
            SELECT seedWords.*
            FROM seed.WordEntries AS seedWords
            INNER JOIN NewSeedWordPublicIds AS selectedWords
                ON selectedWords.PublicId = seedWords.PublicId
            WHERE NOT EXISTS (
                SELECT 1
                FROM WordEntries AS localWords
                WHERE localWords.PublicId = seedWords.PublicId);

            INSERT INTO WordSenses
            SELECT seedSenses.*
            FROM seed.WordSenses AS seedSenses
            WHERE seedSenses.WordEntryId IN (
                SELECT localWords.Id
                FROM WordEntries AS localWords
                WHERE localWords.PublicId IN (SELECT PublicId FROM NewSeedWordPublicIds))
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordSenses AS localSenses
                  WHERE localSenses.Id = seedSenses.Id);

            INSERT INTO SenseTranslations
            SELECT seedTranslations.*
            FROM seed.SenseTranslations AS seedTranslations
            WHERE seedTranslations.WordSenseId IN (
                SELECT seedSenses.Id
                FROM seed.WordSenses AS seedSenses
                WHERE seedSenses.WordEntryId IN (
                    SELECT seedWords.Id
                    FROM seed.WordEntries AS seedWords
                    INNER JOIN NewSeedWordPublicIds AS selectedWords
                        ON selectedWords.PublicId = seedWords.PublicId))
              AND NOT EXISTS (
                  SELECT 1
                  FROM SenseTranslations AS localTranslations
                  WHERE localTranslations.Id = seedTranslations.Id);

            INSERT INTO ExampleSentences
            SELECT seedExamples.*
            FROM seed.ExampleSentences AS seedExamples
            WHERE seedExamples.WordSenseId IN (
                SELECT seedSenses.Id
                FROM seed.WordSenses AS seedSenses
                WHERE seedSenses.WordEntryId IN (
                    SELECT seedWords.Id
                    FROM seed.WordEntries AS seedWords
                    INNER JOIN NewSeedWordPublicIds AS selectedWords
                        ON selectedWords.PublicId = seedWords.PublicId))
              AND NOT EXISTS (
                  SELECT 1
                  FROM ExampleSentences AS localExamples
                  WHERE localExamples.Id = seedExamples.Id);

            INSERT INTO ExampleTranslations
            SELECT seedExampleTranslations.*
            FROM seed.ExampleTranslations AS seedExampleTranslations
            WHERE seedExampleTranslations.ExampleSentenceId IN (
                SELECT seedExamples.Id
                FROM seed.ExampleSentences AS seedExamples
                WHERE seedExamples.WordSenseId IN (
                    SELECT seedSenses.Id
                    FROM seed.WordSenses AS seedSenses
                    WHERE seedSenses.WordEntryId IN (
                        SELECT seedWords.Id
                        FROM seed.WordEntries AS seedWords
                        INNER JOIN NewSeedWordPublicIds AS selectedWords
                            ON selectedWords.PublicId = seedWords.PublicId)))
              AND NOT EXISTS (
                  SELECT 1
                  FROM ExampleTranslations AS localExampleTranslations
                  WHERE localExampleTranslations.Id = seedExampleTranslations.Id);

            INSERT INTO WordTopics
            SELECT seedWordTopics.*
            FROM seed.WordTopics AS seedWordTopics
            WHERE seedWordTopics.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordTopics AS localWordTopics
                  WHERE localWordTopics.Id = seedWordTopics.Id);

            INSERT INTO WordLabels
            SELECT seedLabels.*
            FROM seed.WordLabels AS seedLabels
            WHERE seedLabels.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordLabels AS localLabels
                  WHERE localLabels.Id = seedLabels.Id);

            INSERT INTO WordGrammarNotes
            SELECT seedGrammarNotes.*
            FROM seed.WordGrammarNotes AS seedGrammarNotes
            WHERE seedGrammarNotes.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordGrammarNotes AS localGrammarNotes
                  WHERE localGrammarNotes.Id = seedGrammarNotes.Id);

            INSERT INTO WordCollocations
            SELECT seedCollocations.*
            FROM seed.WordCollocations AS seedCollocations
            WHERE seedCollocations.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordCollocations AS localCollocations
                  WHERE localCollocations.Id = seedCollocations.Id);

            INSERT INTO WordFamilyMembers
            SELECT seedFamilyMembers.*
            FROM seed.WordFamilyMembers AS seedFamilyMembers
            WHERE seedFamilyMembers.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordFamilyMembers AS localFamilyMembers
                  WHERE localFamilyMembers.Id = seedFamilyMembers.Id);

            INSERT INTO WordRelations
            SELECT seedRelations.*
            FROM seed.WordRelations AS seedRelations
            WHERE seedRelations.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordRelations AS localRelations
                  WHERE localRelations.Id = seedRelations.Id);

            INSERT INTO WordLexicalForms
            SELECT seedLexicalForms.*
            FROM seed.WordLexicalForms AS seedLexicalForms
            WHERE seedLexicalForms.WordEntryId IN (
                SELECT seedWords.Id
                FROM seed.WordEntries AS seedWords
                INNER JOIN NewSeedWordPublicIds AS selectedWords
                    ON selectedWords.PublicId = seedWords.PublicId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM WordLexicalForms AS localLexicalForms
                  WHERE localLexicalForms.Id = seedLexicalForms.Id);

            INSERT INTO ContentPackageEntries
            SELECT seedEntries.*
            FROM seed.ContentPackageEntries AS seedEntries
            WHERE seedEntries.ContentPackageId IN (SELECT Id FROM NewSeedPackages)
              AND NOT EXISTS (
                  SELECT 1
                  FROM ContentPackageEntries AS localEntries
                  WHERE localEntries.Id = seedEntries.Id);

            DROP TABLE IF EXISTS NewSeedWordPublicIds;
            DROP TABLE IF EXISTS NewSeedPackages;
            """);

        return scriptBuilder.ToString();
    }

    /// <summary>
    /// Checks whether the requested SQLite table already exists in the current database.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <param name="tableName">The table name to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the table exists; otherwise <see langword="false"/>.</returns>
    private static async Task<bool> TableExistsAsync(
        DarwinLinguaDbContext dbContext,
        string tableName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        DbConnection connection = dbContext.Database.GetDbConnection();
        bool shouldCloseConnection = connection.State != System.Data.ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            bool isSqlite = dbContext.Database.IsSqlite();
            command.CommandText = isSqlite
                ? """
                  SELECT 1
                  FROM sqlite_master
                  WHERE type = 'table'
                    AND name = $tableName
                  LIMIT 1;
                  """
                : """
                  SELECT 1
                  FROM information_schema.tables
                  WHERE table_schema = current_schema()
                    AND table_name = @tableName
                  LIMIT 1;
                  """;

            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = isSqlite ? "$tableName" : "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return result is not null;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync().ConfigureAwait(false);
            }
        }
    }
}
