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
        await EnsureServerContentBaseSchemaAsync(cancellationToken).ConfigureAwait(false);
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

    private async Task EnsureServerContentBaseSchemaAsync(CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync("ClientProducts", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteCreateTableAsync(
                    """
                    CREATE TABLE "ClientProducts" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_ClientProducts" PRIMARY KEY,
                        "Key" TEXT NOT NULL,
                        "DisplayName" TEXT NOT NULL,
                        "LearningLanguageCode" TEXT NOT NULL,
                        "DefaultUiLanguageCode" TEXT NOT NULL,
                        "IsActive" INTEGER NOT NULL,
                        "CreatedAtUtc" TEXT NOT NULL,
                        "UpdatedAtUtc" TEXT NOT NULL
                    );
                    CREATE UNIQUE INDEX "IX_ClientProducts_Key" ON "ClientProducts" ("Key");
                    """,
                    """
                    CREATE TABLE "ClientProducts" (
                        "Id" uuid NOT NULL,
                        "Key" character varying(128) NOT NULL,
                        "DisplayName" character varying(256) NOT NULL,
                        "LearningLanguageCode" character varying(16) NOT NULL,
                        "DefaultUiLanguageCode" character varying(16) NOT NULL,
                        "IsActive" boolean NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        "UpdatedAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_ClientProducts" PRIMARY KEY ("Id")
                    );
                    CREATE UNIQUE INDEX "IX_ClientProducts_Key" ON "ClientProducts" ("Key");
                    """,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await TableExistsAsync("ContentStreams", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteCreateTableAsync(
                    """
                    CREATE TABLE "ContentStreams" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_ContentStreams" PRIMARY KEY,
                        "ClientProductId" TEXT NOT NULL,
                        "ContentAreaKey" TEXT NOT NULL,
                        "SliceKey" TEXT NOT NULL,
                        "LearningLanguageCode" TEXT NOT NULL,
                        "SchemaVersion" INTEGER NOT NULL,
                        "IsActive" INTEGER NOT NULL,
                        "CreatedAtUtc" TEXT NOT NULL,
                        "UpdatedAtUtc" TEXT NOT NULL,
                        CONSTRAINT "FK_ContentStreams_ClientProducts_ClientProductId"
                            FOREIGN KEY ("ClientProductId") REFERENCES "ClientProducts" ("Id") ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX "IX_ContentStreams_ClientProductId_ContentAreaKey_SliceKey"
                    ON "ContentStreams" ("ClientProductId", "ContentAreaKey", "SliceKey");
                    """,
                    """
                    CREATE TABLE "ContentStreams" (
                        "Id" uuid NOT NULL,
                        "ClientProductId" uuid NOT NULL,
                        "ContentAreaKey" character varying(128) NOT NULL,
                        "SliceKey" character varying(128) NOT NULL,
                        "LearningLanguageCode" character varying(16) NOT NULL,
                        "SchemaVersion" integer NOT NULL,
                        "IsActive" boolean NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        "UpdatedAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_ContentStreams" PRIMARY KEY ("Id"),
                        CONSTRAINT "FK_ContentStreams_ClientProducts_ClientProductId"
                            FOREIGN KEY ("ClientProductId") REFERENCES "ClientProducts" ("Id") ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX "IX_ContentStreams_ClientProductId_ContentAreaKey_SliceKey"
                    ON "ContentStreams" ("ClientProductId", "ContentAreaKey", "SliceKey");
                    """,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await TableExistsAsync("PublishedPackages", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteCreateTableAsync(
                    """
                    CREATE TABLE "PublishedPackages" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_PublishedPackages" PRIMARY KEY,
                        "PackageId" TEXT NOT NULL,
                        "ContentStreamId" TEXT NOT NULL,
                        "PackageType" TEXT NOT NULL,
                        "Version" TEXT NOT NULL,
                        "PublicationBatchId" TEXT NOT NULL,
                        "PublicationStatus" TEXT NOT NULL,
                        "SchemaVersion" INTEGER NOT NULL,
                        "MinimumAppSchemaVersion" INTEGER NOT NULL,
                        "Checksum" TEXT NOT NULL,
                        "EntryCount" INTEGER NOT NULL,
                        "WordCount" INTEGER NOT NULL,
                        "RelativeDownloadPath" TEXT NOT NULL,
                        "CreatedAtUtc" TEXT NOT NULL,
                        "UpdatedAtUtc" TEXT NOT NULL,
                        "PublishedAtUtc" TEXT NULL,
                        "SupersededAtUtc" TEXT NULL,
                        CONSTRAINT "FK_PublishedPackages_ContentStreams_ContentStreamId"
                            FOREIGN KEY ("ContentStreamId") REFERENCES "ContentStreams" ("Id") ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX "IX_PublishedPackages_PackageId" ON "PublishedPackages" ("PackageId");
                    CREATE INDEX "IX_PublishedPackages_ContentStreamId_PublicationStatus_CreatedAtUtc"
                    ON "PublishedPackages" ("ContentStreamId", "PublicationStatus", "CreatedAtUtc");
                    """,
                    """
                    CREATE TABLE "PublishedPackages" (
                        "Id" uuid NOT NULL,
                        "PackageId" character varying(256) NOT NULL,
                        "ContentStreamId" uuid NOT NULL,
                        "PackageType" character varying(128) NOT NULL,
                        "Version" character varying(64) NOT NULL,
                        "PublicationBatchId" character varying(128) NOT NULL,
                        "PublicationStatus" character varying(32) NOT NULL,
                        "SchemaVersion" integer NOT NULL,
                        "MinimumAppSchemaVersion" integer NOT NULL,
                        "Checksum" character varying(256) NOT NULL,
                        "EntryCount" integer NOT NULL,
                        "WordCount" integer NOT NULL,
                        "RelativeDownloadPath" character varying(512) NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        "UpdatedAtUtc" timestamp with time zone NOT NULL,
                        "PublishedAtUtc" timestamp with time zone NULL,
                        "SupersededAtUtc" timestamp with time zone NULL,
                        CONSTRAINT "PK_PublishedPackages" PRIMARY KEY ("Id"),
                        CONSTRAINT "FK_PublishedPackages_ContentStreams_ContentStreamId"
                            FOREIGN KEY ("ContentStreamId") REFERENCES "ContentStreams" ("Id") ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX "IX_PublishedPackages_PackageId" ON "PublishedPackages" ("PackageId");
                    CREATE INDEX "IX_PublishedPackages_ContentStreamId_PublicationStatus_CreatedAtUtc"
                    ON "PublishedPackages" ("ContentStreamId", "PublicationStatus", "CreatedAtUtc");
                    """,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await TableExistsAsync("ContentImportReceipts", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteCreateTableAsync(
                    """
                    CREATE TABLE "ContentImportReceipts" (
                        "Id" TEXT NOT NULL CONSTRAINT "PK_ContentImportReceipts" PRIMARY KEY,
                        "ClientProductKey" TEXT NOT NULL,
                        "SourceFilePath" TEXT NOT NULL,
                        "SourceFileName" TEXT NOT NULL,
                        "ImportedPackageId" TEXT NULL,
                        "ImportedPackageName" TEXT NULL,
                        "ImportStatus" TEXT NOT NULL,
                        "TotalEntries" INTEGER NOT NULL,
                        "ImportedEntries" INTEGER NOT NULL,
                        "SkippedDuplicateEntries" INTEGER NOT NULL,
                        "InvalidEntries" INTEGER NOT NULL,
                        "WarningCount" INTEGER NOT NULL,
                        "IssueSummary" TEXT NOT NULL,
                        "DraftPublicationBatchId" TEXT NULL,
                        "PublishedPackageCount" INTEGER NOT NULL,
                        "PublishedPackageIds" TEXT NOT NULL,
                        "CreatedAtUtc" TEXT NOT NULL,
                        "UpdatedAtUtc" TEXT NOT NULL
                    );
                    CREATE INDEX "IX_ContentImportReceipts_CreatedAtUtc" ON "ContentImportReceipts" ("CreatedAtUtc");
                    """,
                    """
                    CREATE TABLE "ContentImportReceipts" (
                        "Id" uuid NOT NULL,
                        "ClientProductKey" character varying(128) NOT NULL,
                        "SourceFilePath" character varying(1024) NOT NULL,
                        "SourceFileName" character varying(256) NOT NULL,
                        "ImportedPackageId" character varying(256) NULL,
                        "ImportedPackageName" character varying(256) NULL,
                        "ImportStatus" character varying(64) NOT NULL,
                        "TotalEntries" integer NOT NULL,
                        "ImportedEntries" integer NOT NULL,
                        "SkippedDuplicateEntries" integer NOT NULL,
                        "InvalidEntries" integer NOT NULL,
                        "WarningCount" integer NOT NULL,
                        "IssueSummary" character varying(4000) NOT NULL,
                        "DraftPublicationBatchId" character varying(128) NULL,
                        "PublishedPackageCount" integer NOT NULL,
                        "PublishedPackageIds" character varying(4000) NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        "UpdatedAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_ContentImportReceipts" PRIMARY KEY ("Id")
                    );
                    CREATE INDEX "IX_ContentImportReceipts_CreatedAtUtc" ON "ContentImportReceipts" ("CreatedAtUtc");
                    """,
                    cancellationToken)
                .ConfigureAwait(false);
        }
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

        if (!await ColumnExistsAsync("ContentImportReceipts", "PublishedPackageCount", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteColumnAddAsync(
                    """ALTER TABLE "ContentImportReceipts" ADD COLUMN "PublishedPackageCount" INTEGER NOT NULL DEFAULT 0;""",
                    """ALTER TABLE "ContentImportReceipts" ADD COLUMN "PublishedPackageCount" integer NOT NULL DEFAULT 0;""",
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!await ColumnExistsAsync("ContentImportReceipts", "UpdatedAtUtc", cancellationToken).ConfigureAwait(false))
        {
            await ExecuteColumnAddAsync(
                    """ALTER TABLE "ContentImportReceipts" ADD COLUMN "UpdatedAtUtc" TEXT NULL;""",
                    """ALTER TABLE "ContentImportReceipts" ADD COLUMN "UpdatedAtUtc" timestamp with time zone NULL;""",
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
        await dbContext.Database.ExecuteSqlRawAsync(
                """UPDATE "ContentImportReceipts" SET "UpdatedAtUtc" = COALESCE("UpdatedAtUtc", "CreatedAtUtc");""",
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
