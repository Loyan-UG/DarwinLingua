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

            CREATE TABLE IF NOT EXISTS "WebEmailDeliveryLogs" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebEmailDeliveryLogs" PRIMARY KEY,
                "ScenarioKey" TEXT NOT NULL,
                "RecipientEmailHash" TEXT NOT NULL,
                "RecipientUserId" TEXT NULL,
                "TemplateKey" TEXT NOT NULL,
                "Culture" TEXT NOT NULL,
                "Subject" TEXT NOT NULL,
                "ProviderName" TEXT NOT NULL,
                "ProviderMessageId" TEXT NULL,
                "ProviderLastEvent" TEXT NULL,
                "ProviderLastEventAtUtc" TEXT NULL,
                "ProviderLastEventReason" TEXT NULL,
                "Status" TEXT NOT NULL,
                "FailureCode" TEXT NULL,
                "FailureMessageSummary" TEXT NULL,
                "RetryCount" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "SentAtUtc" TEXT NULL,
                "LastAttemptAtUtc" TEXT NULL,
                "CorrelationId" TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebEmailSuppressions" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebEmailSuppressions" PRIMARY KEY,
                "RecipientEmailHash" TEXT NOT NULL,
                "Reason" TEXT NOT NULL,
                "ProviderName" TEXT NOT NULL,
                "ProviderMessageId" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "LastSeenAtUtc" TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebBillingProfiles" (
                "UserId" TEXT NOT NULL CONSTRAINT "PK_WebBillingProfiles" PRIMARY KEY,
                "ProviderName" TEXT NOT NULL,
                "ProviderCustomerId" TEXT NULL,
                "ProviderSubscriptionId" TEXT NULL,
                "PlanKey" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "CurrentPeriodEndsAtUtc" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebBillingEvents" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebBillingEvents" PRIMARY KEY,
                "ProviderName" TEXT NOT NULL,
                "ProviderEventId" TEXT NOT NULL,
                "EventType" TEXT NOT NULL,
                "Status" TEXT NOT NULL,
                "UserId" TEXT NULL,
                "ProviderCustomerId" TEXT NULL,
                "ProviderSubscriptionId" TEXT NULL,
                "ErrorSummary" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "ProcessedAtUtc" TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebBillingNotifications" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WebBillingNotifications" PRIMARY KEY,
                "NotificationKey" TEXT NOT NULL,
                "ScenarioKey" TEXT NOT NULL,
                "UserId" TEXT NULL,
                "ProviderSubscriptionId" TEXT NULL,
                "BillingStatus" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await TryAddSqliteColumnAsync(
            """ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN "ProviderLastEvent" TEXT NULL;""",
            cancellationToken)
            .ConfigureAwait(false);
        await TryAddSqliteColumnAsync(
            """ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN "ProviderLastEventAtUtc" TEXT NULL;""",
            cancellationToken)
            .ConfigureAwait(false);
        await TryAddSqliteColumnAsync(
            """ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN "ProviderLastEventReason" TEXT NULL;""",
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
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_CreatedAtUtc_Status"
            ON "WebEmailDeliveryLogs" ("CreatedAtUtc", "Status");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_Status_CreatedAtUtc"
            ON "WebEmailDeliveryLogs" ("Status", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_ScenarioKey_CreatedAtUtc"
            ON "WebEmailDeliveryLogs" ("ScenarioKey", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_ProviderMessageId"
            ON "WebEmailDeliveryLogs" ("ProviderMessageId");
            DROP INDEX IF EXISTS "IX_WebEmailDeliveryLogs_ProviderLastEvent_ProviderLastEventAtUtc";
            DROP INDEX IF EXISTS "IX_WebEmailDeliveryLogs_ProviderLastEventAtUtc";
            CREATE INDEX IF NOT EXISTS "IX_WebEmailLogs_Event_EventAtUtc"
            ON "WebEmailDeliveryLogs" ("ProviderLastEvent", "ProviderLastEventAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailLogs_EventAtUtc"
            ON "WebEmailDeliveryLogs" ("ProviderLastEventAtUtc");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebEmailSuppressions_RecipientEmailHash"
            ON "WebEmailSuppressions" ("RecipientEmailHash");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailSuppressions_CreatedAtUtc"
            ON "WebEmailSuppressions" ("CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingProfiles_Provider_Customer"
            ON "WebBillingProfiles" ("ProviderName", "ProviderCustomerId");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingProfiles_Provider_Subscription"
            ON "WebBillingProfiles" ("ProviderName", "ProviderSubscriptionId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebBillingEvents_Provider_EventId"
            ON "WebBillingEvents" ("ProviderName", "ProviderEventId");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingEvents_Status_CreatedAtUtc"
            ON "WebBillingEvents" ("Status", "CreatedAtUtc");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebBillingNotifications_NotificationKey"
            ON "WebBillingNotifications" ("NotificationKey");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingNotifications_CreatedAtUtc"
            ON "WebBillingNotifications" ("CreatedAtUtc");
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

            CREATE TABLE IF NOT EXISTS "WebEmailDeliveryLogs" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebEmailDeliveryLogs" PRIMARY KEY,
                "ScenarioKey" character varying(128) NOT NULL,
                "RecipientEmailHash" character varying(128) NOT NULL,
                "RecipientUserId" character varying(450) NULL,
                "TemplateKey" character varying(128) NOT NULL,
                "Culture" character varying(16) NOT NULL,
                "Subject" character varying(256) NOT NULL,
                "ProviderName" character varying(64) NOT NULL,
                "ProviderMessageId" character varying(256) NULL,
                "ProviderLastEvent" character varying(64) NULL,
                "ProviderLastEventAtUtc" timestamp with time zone NULL,
                "ProviderLastEventReason" character varying(512) NULL,
                "Status" character varying(32) NOT NULL,
                "FailureCode" character varying(128) NULL,
                "FailureMessageSummary" character varying(512) NULL,
                "RetryCount" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "SentAtUtc" timestamp with time zone NULL,
                "LastAttemptAtUtc" timestamp with time zone NULL,
                "CorrelationId" character varying(128) NULL
            );

            CREATE TABLE IF NOT EXISTS "WebEmailSuppressions" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebEmailSuppressions" PRIMARY KEY,
                "RecipientEmailHash" character varying(128) NOT NULL,
                "Reason" character varying(128) NOT NULL,
                "ProviderName" character varying(64) NOT NULL,
                "ProviderMessageId" character varying(256) NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "LastSeenAtUtc" timestamp with time zone NULL
            );

            CREATE TABLE IF NOT EXISTS "WebBillingProfiles" (
                "UserId" character varying(450) NOT NULL CONSTRAINT "PK_WebBillingProfiles" PRIMARY KEY,
                "ProviderName" character varying(64) NOT NULL,
                "ProviderCustomerId" character varying(128) NULL,
                "ProviderSubscriptionId" character varying(128) NULL,
                "PlanKey" character varying(128) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "CurrentPeriodEndsAtUtc" timestamp with time zone NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "WebBillingEvents" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebBillingEvents" PRIMARY KEY,
                "ProviderName" character varying(64) NOT NULL,
                "ProviderEventId" character varying(128) NOT NULL,
                "EventType" character varying(128) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "UserId" character varying(450) NULL,
                "ProviderCustomerId" character varying(128) NULL,
                "ProviderSubscriptionId" character varying(128) NULL,
                "ErrorSummary" character varying(512) NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "ProcessedAtUtc" timestamp with time zone NULL
            );

            CREATE TABLE IF NOT EXISTS "WebBillingNotifications" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebBillingNotifications" PRIMARY KEY,
                "NotificationKey" character varying(256) NOT NULL,
                "ScenarioKey" character varying(128) NOT NULL,
                "UserId" character varying(450) NULL,
                "ProviderSubscriptionId" character varying(128) NULL,
                "BillingStatus" character varying(64) NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN IF NOT EXISTS "ProviderLastEvent" character varying(64) NULL;
            ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN IF NOT EXISTS "ProviderLastEventAtUtc" timestamp with time zone NULL;
            ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN IF NOT EXISTS "ProviderLastEventReason" character varying(512) NULL;
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
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_CreatedAtUtc_Status"
            ON "WebEmailDeliveryLogs" ("CreatedAtUtc", "Status");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_Status_CreatedAtUtc"
            ON "WebEmailDeliveryLogs" ("Status", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_ScenarioKey_CreatedAtUtc"
            ON "WebEmailDeliveryLogs" ("ScenarioKey", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_ProviderMessageId"
            ON "WebEmailDeliveryLogs" ("ProviderMessageId");
            DROP INDEX IF EXISTS "IX_WebEmailDeliveryLogs_ProviderLastEvent_ProviderLastEventAtUtc";
            DROP INDEX IF EXISTS "IX_WebEmailDeliveryLogs_ProviderLastEventAtUtc";
            CREATE INDEX IF NOT EXISTS "IX_WebEmailLogs_Event_EventAtUtc"
            ON "WebEmailDeliveryLogs" ("ProviderLastEvent", "ProviderLastEventAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailLogs_EventAtUtc"
            ON "WebEmailDeliveryLogs" ("ProviderLastEventAtUtc");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebEmailSuppressions_RecipientEmailHash"
            ON "WebEmailSuppressions" ("RecipientEmailHash");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailSuppressions_CreatedAtUtc"
            ON "WebEmailSuppressions" ("CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingProfiles_Provider_Customer"
            ON "WebBillingProfiles" ("ProviderName", "ProviderCustomerId");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingProfiles_Provider_Subscription"
            ON "WebBillingProfiles" ("ProviderName", "ProviderSubscriptionId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebBillingEvents_Provider_EventId"
            ON "WebBillingEvents" ("ProviderName", "ProviderEventId");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingEvents_Status_CreatedAtUtc"
            ON "WebBillingEvents" ("Status", "CreatedAtUtc");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebBillingNotifications_NotificationKey"
            ON "WebBillingNotifications" ("NotificationKey");
            CREATE INDEX IF NOT EXISTS "IX_WebBillingNotifications_CreatedAtUtc"
            ON "WebBillingNotifications" ("CreatedAtUtc");
            """,
            cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task TryAddSqliteColumnAsync(string sql, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (
            exception.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase))
        {
        }
    }
}
