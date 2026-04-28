using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

/// <summary>
/// Creates web-specific user state tables without relying on EnsureCreated for shared databases.
/// </summary>
public sealed class WebUserStateDatabaseBootstrapper(WebIdentityDbContext dbContext)
    : IWebUserStateDatabaseBootstrapper
{
    /// <summary>
    /// Initializes provider-specific tables used by the web host for preferences, favorites, and recent state.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string providerName = dbContext.Database.ProviderName ?? string.Empty;

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureSqliteTablesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            await EnsurePostgresTablesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureSqliteTablesAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "WebUserPreferences" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebUserPreferences" PRIMARY KEY,
                "ActorId" TEXT NOT NULL,
                "UiLanguageCode" TEXT NOT NULL,
                "PrimaryMeaningLanguageCode" TEXT NOT NULL,
                "SecondaryMeaningLanguageCode" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebUserFavoriteWords" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebUserFavoriteWords" PRIMARY KEY,
                "ActorId" TEXT NOT NULL,
                "WordPublicId" TEXT NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebUserWordStates" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebUserWordStates" PRIMARY KEY,
                "ActorId" TEXT NOT NULL,
                "WordPublicId" TEXT NOT NULL,
                "IsKnown" INTEGER NOT NULL,
                "IsDifficult" INTEGER NOT NULL,
                "FirstViewedAtUtc" TEXT NULL,
                "LastViewedAtUtc" TEXT NULL,
                "ViewCount" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserPreferences_ActorId"
            ON "WebUserPreferences" ("ActorId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserFavoriteWords_ActorId_WordPublicId"
            ON "WebUserFavoriteWords" ("ActorId", "WordPublicId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserWordStates_ActorId_WordPublicId"
            ON "WebUserWordStates" ("ActorId", "WordPublicId");
            CREATE INDEX IF NOT EXISTS "IX_WebUserWordStates_ActorId_LastViewedAtUtc"
            ON "WebUserWordStates" ("ActorId", "LastViewedAtUtc");
            """,
            cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task EnsurePostgresTablesAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "WebUserPreferences" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebUserPreferences" PRIMARY KEY,
                "ActorId" character varying(128) NOT NULL,
                "UiLanguageCode" character varying(16) NOT NULL,
                "PrimaryMeaningLanguageCode" character varying(16) NOT NULL,
                "SecondaryMeaningLanguageCode" character varying(16) NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebUserFavoriteWords" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebUserFavoriteWords" PRIMARY KEY,
                "ActorId" character varying(128) NOT NULL,
                "WordPublicId" uuid NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebUserWordStates" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebUserWordStates" PRIMARY KEY,
                "ActorId" character varying(128) NOT NULL,
                "WordPublicId" uuid NOT NULL,
                "IsKnown" boolean NOT NULL,
                "IsDifficult" boolean NOT NULL,
                "FirstViewedAtUtc" timestamp with time zone NULL,
                "LastViewedAtUtc" timestamp with time zone NULL,
                "ViewCount" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserPreferences_ActorId"
            ON "WebUserPreferences" ("ActorId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserFavoriteWords_ActorId_WordPublicId"
            ON "WebUserFavoriteWords" ("ActorId", "WordPublicId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserWordStates_ActorId_WordPublicId"
            ON "WebUserWordStates" ("ActorId", "WordPublicId");
            CREATE INDEX IF NOT EXISTS "IX_WebUserWordStates_ActorId_LastViewedAtUtc"
            ON "WebUserWordStates" ("ActorId", "LastViewedAtUtc");
            """,
            cancellationToken)
            .ConfigureAwait(false);
    }
}
