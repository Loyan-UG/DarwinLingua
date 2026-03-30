using System.Data.Common;
using DarwinLingua.WebApi.Configuration;
using DarwinLingua.WebApi.Persistence;
using DarwinLingua.WebApi.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Creates the server-content database schema and imports configured bootstrap metadata.
/// </summary>
public sealed class ServerContentDatabaseBootstrapper(
    ServerContentDbContext dbContext,
    IOptions<ServerContentOptions> options) : IServerContentDatabaseBootstrapper
{
    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(options);

        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        await ApplyPublishedPackageCompatibilityUpdatesAsync(cancellationToken).ConfigureAwait(false);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        Dictionary<string, ClientProductEntity> productsByKey = await dbContext.ClientProducts
            .Include(product => product.ContentStreams)
            .ThenInclude(stream => stream.PublishedPackages)
            .ToDictionaryAsync(product => product.Key, StringComparer.OrdinalIgnoreCase, cancellationToken)
            .ConfigureAwait(false);

        foreach (ClientProductOptions configuredProduct in options.Value.ClientProducts)
        {
            if (!productsByKey.TryGetValue(configuredProduct.Key, out ClientProductEntity? productEntity))
            {
                productEntity = new ClientProductEntity
                {
                    Id = Guid.NewGuid(),
                    Key = configuredProduct.Key.Trim(),
                    CreatedAtUtc = now,
                };

                dbContext.ClientProducts.Add(productEntity);
                productsByKey[productEntity.Key] = productEntity;
            }

            productEntity.DisplayName = configuredProduct.DisplayName.Trim();
            productEntity.LearningLanguageCode = configuredProduct.LearningLanguageCode.Trim();
            productEntity.DefaultUiLanguageCode = configuredProduct.DefaultUiLanguageCode.Trim();
            productEntity.IsActive = configuredProduct.IsActive;
            productEntity.UpdatedAtUtc = now;
        }

        foreach (PublishedPackageOptions configuredPackage in options.Value.Packages)
        {
            ClientProductEntity product = productsByKey[configuredPackage.ClientProductKey];

            ContentStreamEntity? stream = product.ContentStreams.FirstOrDefault(existingStream =>
                existingStream.ContentAreaKey.Equals(configuredPackage.ContentAreaKey, StringComparison.OrdinalIgnoreCase) &&
                existingStream.SliceKey.Equals(configuredPackage.SliceKey, StringComparison.OrdinalIgnoreCase));

            if (stream is null)
            {
                stream = new ContentStreamEntity
                {
                    Id = Guid.NewGuid(),
                    ClientProductId = product.Id,
                    ClientProduct = product,
                    ContentAreaKey = configuredPackage.ContentAreaKey.Trim(),
                    SliceKey = configuredPackage.SliceKey.Trim(),
                    LearningLanguageCode = product.LearningLanguageCode,
                    SchemaVersion = configuredPackage.SchemaVersion,
                    IsActive = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                };

                dbContext.ContentStreams.Add(stream);
                product.ContentStreams.Add(stream);
            }
            else
            {
                stream.SchemaVersion = configuredPackage.SchemaVersion;
                stream.IsActive = true;
                stream.LearningLanguageCode = product.LearningLanguageCode;
                stream.UpdatedAtUtc = now;
            }

            PublishedPackageEntity? package = stream.PublishedPackages.FirstOrDefault(existingPackage =>
                existingPackage.PackageId.Equals(configuredPackage.PackageId, StringComparison.OrdinalIgnoreCase));

            if (package is null)
            {
                package = new PublishedPackageEntity
                {
                    Id = Guid.NewGuid(),
                    PackageId = configuredPackage.PackageId.Trim(),
                    ContentStreamId = stream.Id,
                    ContentStream = stream,
                    CreatedAtUtc = configuredPackage.CreatedAtUtc == default ? now : configuredPackage.CreatedAtUtc,
                };

                dbContext.PublishedPackages.Add(package);
                stream.PublishedPackages.Add(package);
            }

            package.PackageType = configuredPackage.PackageType.Trim();
            package.Version = configuredPackage.Version.Trim();
            package.PublicationBatchId = string.IsNullOrWhiteSpace(package.PublicationBatchId)
                ? package.Version
                : package.PublicationBatchId;
            package.PublicationStatus = PackagePublicationStatus.Published;
            package.SchemaVersion = configuredPackage.SchemaVersion;
            package.MinimumAppSchemaVersion = configuredPackage.MinimumAppSchemaVersion;
            package.Checksum = configuredPackage.Checksum.Trim();
            package.EntryCount = configuredPackage.EntryCount;
            package.WordCount = configuredPackage.WordCount;
            package.RelativeDownloadPath = configuredPackage.RelativeDownloadPath.Trim();
            package.PublishedAtUtc ??= package.CreatedAtUtc;
            package.SupersededAtUtc = null;
            package.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyPublishedPackageCompatibilityUpdatesAsync(CancellationToken cancellationToken)
    {
        if (!await ColumnExistsAsync("PublishedPackages", "PublicationBatchId", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteColumnAddAsync(
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "PublicationBatchId" TEXT NOT NULL DEFAULT '';""",
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "PublicationBatchId" text NOT NULL DEFAULT '';""",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await ColumnExistsAsync("PublishedPackages", "PublicationStatus", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteColumnAddAsync(
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "PublicationStatus" TEXT NOT NULL DEFAULT 'Published';""",
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "PublicationStatus" text NOT NULL DEFAULT 'Published';""",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await ColumnExistsAsync("PublishedPackages", "PublishedAtUtc", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteColumnAddAsync(
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "PublishedAtUtc" TEXT NULL;""",
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "PublishedAtUtc" timestamp with time zone NULL;""",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await ColumnExistsAsync("PublishedPackages", "SupersededAtUtc", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteColumnAddAsync(
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "SupersededAtUtc" TEXT NULL;""",
                    """ALTER TABLE "PublishedPackages" ADD COLUMN "SupersededAtUtc" timestamp with time zone NULL;""",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await TableExistsAsync("ContentPublicationEvents", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteCreateTableAsync(
                    """
                    CREATE TABLE "ContentPublicationEvents" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_ContentPublicationEvents" PRIMARY KEY,
                        "ClientProductKey" TEXT NOT NULL,
                        "PublicationBatchId" TEXT NOT NULL,
                        "EventType" TEXT NOT NULL,
                        "PackageIds" TEXT NOT NULL,
                        "RelatedBatchIds" TEXT NOT NULL,
                        "Notes" TEXT NOT NULL,
                        "OccurredAtUtc" TEXT NOT NULL
                    );
                    CREATE INDEX "IX_ContentPublicationEvents_ClientProductKey_OccurredAtUtc"
                    ON "ContentPublicationEvents" ("ClientProductKey", "OccurredAtUtc");
                    """,
                    """
                    CREATE TABLE "ContentPublicationEvents" (
                        "Id" uuid NOT NULL,
                        "ClientProductKey" character varying(128) NOT NULL,
                        "PublicationBatchId" character varying(128) NOT NULL,
                        "EventType" character varying(32) NOT NULL,
                        "PackageIds" character varying(4000) NOT NULL,
                        "RelatedBatchIds" character varying(4000) NOT NULL,
                        "Notes" character varying(2000) NOT NULL,
                        "OccurredAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_ContentPublicationEvents" PRIMARY KEY ("Id")
                    );
                    CREATE INDEX "IX_ContentPublicationEvents_ClientProductKey_OccurredAtUtc"
                    ON "ContentPublicationEvents" ("ClientProductKey", "OccurredAtUtc");
                    """,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        await dbContext.Database.ExecuteSqlRawAsync(
                """UPDATE "PublishedPackages" SET "PublicationBatchId" = COALESCE(NULLIF("PublicationBatchId", ''), "Version", "PackageId");""",
                cancellationToken)
            .ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
                """UPDATE "PublishedPackages" SET "PublicationStatus" = COALESCE(NULLIF("PublicationStatus", ''), 'Published');""",
                cancellationToken)
            .ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync(
                """UPDATE "PublishedPackages" SET "PublishedAtUtc" = COALESCE("PublishedAtUtc", "CreatedAtUtc") WHERE "PublicationStatus" = 'Published';""",
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<bool> ColumnExistsAsync(string tableName, string columnName, CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        await using DbCommand command = connection.CreateCommand();
        if (dbContext.Database.IsSqlite())
        {
            command.CommandText = $"PRAGMA table_info(\"{tableName}\");";
            await using DbDataReader reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        command.CommandText =
            """
            SELECT 1
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = @tableName
              AND column_name = @columnName;
            """;

        DbParameter tableParameter = command.CreateParameter();
        tableParameter.ParameterName = "@tableName";
        tableParameter.Value = tableName;
        command.Parameters.Add(tableParameter);

        DbParameter columnParameter = command.CreateParameter();
        columnParameter.ParameterName = "@columnName";
        columnParameter.Value = columnName;
        command.Parameters.Add(columnParameter);

        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null;
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }

        await using DbCommand command = connection.CreateCommand();
        if (dbContext.Database.IsSqlite())
        {
            command.CommandText =
                """
                SELECT 1
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $tableName;
                """;
        }
        else
        {
            command.CommandText =
                """
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = @tableName;
                """;
        }

        DbParameter tableParameter = command.CreateParameter();
        tableParameter.ParameterName = dbContext.Database.IsSqlite() ? "$tableName" : "@tableName";
        tableParameter.Value = tableName;
        command.Parameters.Add(tableParameter);

        object? result = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        return result is not null;
    }

    private async Task ExecuteColumnAddAsync(string sqliteSql, string postgresSql, CancellationToken cancellationToken)
    {
        if (dbContext.Database.IsSqlite())
        {
            await dbContext.Database.ExecuteSqlRawAsync(sqliteSql, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await dbContext.Database.ExecuteSqlRawAsync(postgresSql, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ExecuteCreateTableAsync(string sqliteSql, string postgresSql, CancellationToken cancellationToken)
    {
        if (dbContext.Database.IsSqlite())
        {
            await dbContext.Database.ExecuteSqlRawAsync(sqliteSql, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (dbContext.Database.IsNpgsql())
        {
            await dbContext.Database.ExecuteSqlRawAsync(postgresSql, cancellationToken).ConfigureAwait(false);
        }
    }
}
