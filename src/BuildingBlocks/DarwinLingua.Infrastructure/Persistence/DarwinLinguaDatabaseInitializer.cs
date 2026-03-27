using System.Data.Common;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DarwinLingua.Infrastructure.Persistence;

/// <summary>
/// Creates the local SQLite database and executes all registered seed workflows.
/// </summary>
internal sealed class DarwinLinguaDatabaseInitializer : IDatabaseInitializer
{
    private readonly IDbContextFactory<DarwinLinguaDbContext> _dbContextFactory;
    private readonly IReadOnlyCollection<IDatabaseSeeder> _databaseSeeders;

    /// <summary>
    /// Initializes a new instance of the <see cref="DarwinLinguaDatabaseInitializer"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The context factory used to create database sessions.</param>
    /// <param name="databaseSeeders">The registered module seeders.</param>
    public DarwinLinguaDatabaseInitializer(
        IDbContextFactory<DarwinLinguaDbContext> dbContextFactory,
        IEnumerable<IDatabaseSeeder> databaseSeeders)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);
        ArgumentNullException.ThrowIfNull(databaseSeeders);

        _dbContextFactory = dbContextFactory;
        _databaseSeeders = databaseSeeders.ToArray();
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await EnsureDatabaseSchemaAsync(cancellationToken).ConfigureAwait(false);
        await SeedReferenceDataAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task EnsureDatabaseSchemaAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await _dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        await PrepareSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
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

        string[] availableMigrations = dbContext.Database
            .GetMigrations()
            .ToArray();

        if (availableMigrations.Length == 0)
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await BaselineLegacyEnsureCreatedDatabaseAsync(dbContext, availableMigrations[0], cancellationToken)
            .ConfigureAwait(false);
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts the initial migration history row when an older EnsureCreated database already contains the schema.
    /// </summary>
    /// <param name="dbContext">The active database context.</param>
    /// <param name="baselineMigrationId">The first migration identifier that matches the legacy schema.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task BaselineLegacyEnsureCreatedDatabaseAsync(
        DarwinLinguaDbContext dbContext,
        string baselineMigrationId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(baselineMigrationId);

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

        string productVersion = dbContext.Model.GetProductVersion()
            ?? throw new InvalidOperationException("The EF Core relational model does not expose a product version.");
        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            SELECT {baselineMigrationId}, {productVersion}
            WHERE NOT EXISTS (
                SELECT 1
                FROM "__EFMigrationsHistory"
                WHERE "MigrationId" = {baselineMigrationId});
            """,
            cancellationToken).ConfigureAwait(false);
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
            command.CommandText =
                """
                SELECT 1
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $tableName
                LIMIT 1;
                """;

            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = "$tableName";
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
