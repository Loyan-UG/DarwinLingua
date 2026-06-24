using DarwinLingua.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Services;

/// <summary>
/// Creates web-specific user state tables in the PostgreSQL-backed web identity database.
/// </summary>
public sealed class WebUserStateDatabaseBootstrapper(WebIdentityDbContext dbContext)
    : IWebUserStateDatabaseBootstrapper
{
    /// <summary>
    /// Initializes tables used by the web host for preferences, favorites, billing, email, and recent state.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string providerName = dbContext.Database.ProviderName ?? string.Empty;

        if (!providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("DarwinLingua.Web requires the PostgreSQL Npgsql provider for web identity and user state.");
        }

        await EnsurePostgresTablesAsync(cancellationToken).ConfigureAwait(false);
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
                "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de',
                "AllowsRudeSlangContent" boolean NOT NULL DEFAULT FALSE,
                "AdultContentAccessState" character varying(64) NOT NULL DEFAULT 'not-requested',
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
                "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de',
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

            CREATE TABLE IF NOT EXISTS "WebWordSuggestions" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebWordSuggestions" PRIMARY KEY,
                "SuggestedWord" character varying(128) NOT NULL,
                "NormalizedSuggestedWord" character varying(128) NOT NULL,
                "Note" character varying(1000) NULL,
                "SourceQuery" character varying(128) NULL,
                "ActorId" character varying(128) NOT NULL,
                "UserId" character varying(450) NULL,
                "Email" character varying(256) NULL,
                "Status" character varying(32) NOT NULL,
                "AdminNote" character varying(1000) NULL,
                "DecidedBy" character varying(256) NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "DecidedAtUtc" timestamp with time zone NULL
            );

            CREATE TABLE IF NOT EXISTS "WebPolicyAcceptances" (
                "Id" uuid NOT NULL CONSTRAINT "PK_WebPolicyAcceptances" PRIMARY KEY,
                "UserId" character varying(450) NOT NULL,
                "PolicyKey" character varying(128) NOT NULL,
                "PolicyVersion" character varying(64) NOT NULL,
                "AcceptedAtUtc" timestamp with time zone NOT NULL,
                "Source" character varying(64) NOT NULL,
                "Culture" character varying(16) NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN IF NOT EXISTS "ProviderLastEvent" character varying(64) NULL;
            ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN IF NOT EXISTS "ProviderLastEventAtUtc" timestamp with time zone NULL;
            ALTER TABLE "WebEmailDeliveryLogs" ADD COLUMN IF NOT EXISTS "ProviderLastEventReason" character varying(512) NULL;
            ALTER TABLE "WebUserPreferences" ADD COLUMN IF NOT EXISTS "AllowsRudeSlangContent" boolean NOT NULL DEFAULT FALSE;
            ALTER TABLE "WebUserPreferences" ADD COLUMN IF NOT EXISTS "AdultContentAccessState" character varying(64) NOT NULL DEFAULT 'not-requested';
            ALTER TABLE "WebUserPreferences" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';
            ALTER TABLE "WebUserWordStates" ADD COLUMN IF NOT EXISTS "TargetLearningLanguageCode" character varying(16) NOT NULL DEFAULT 'de';
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserPreferences_ActorId"
            ON "WebUserPreferences" ("ActorId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserFavoriteWords_ActorId_WordPublicId"
            ON "WebUserFavoriteWords" ("ActorId", "WordPublicId");
            DROP INDEX IF EXISTS "IX_WebUserWordStates_ActorId_WordPublicId";
            DROP INDEX IF EXISTS "IX_WebUserWordStates_ActorId_LastViewedAtUtc";
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebUserWordStates_Actor_Target_Word"
            ON "WebUserWordStates" ("ActorId", "TargetLearningLanguageCode", "WordPublicId");
            CREATE INDEX IF NOT EXISTS "IX_WebUserWordStates_Actor_Target_LastViewed"
            ON "WebUserWordStates" ("ActorId", "TargetLearningLanguageCode", "LastViewedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_CreatedAtUtc_Status"
            ON "WebEmailDeliveryLogs" ("CreatedAtUtc", "Status");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_Status_CreatedAtUtc"
            ON "WebEmailDeliveryLogs" ("Status", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_ScenarioKey_CreatedAtUtc"
            ON "WebEmailDeliveryLogs" ("ScenarioKey", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebEmailDeliveryLogs_ProviderMessageId"
            ON "WebEmailDeliveryLogs" ("ProviderMessageId");
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
            CREATE INDEX IF NOT EXISTS "IX_WebWordSuggestions_Status_CreatedAtUtc"
            ON "WebWordSuggestions" ("Status", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_WebWordSuggestions_NormalizedWord"
            ON "WebWordSuggestions" ("NormalizedSuggestedWord");
            CREATE INDEX IF NOT EXISTS "IX_WebWordSuggestions_Actor_Word"
            ON "WebWordSuggestions" ("ActorId", "NormalizedSuggestedWord");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WebPolicyAcceptances_User_Policy_Version"
            ON "WebPolicyAcceptances" ("UserId", "PolicyKey", "PolicyVersion");
            CREATE INDEX IF NOT EXISTS "IX_WebPolicyAcceptances_Policy_AcceptedAtUtc"
            ON "WebPolicyAcceptances" ("PolicyKey", "PolicyVersion", "AcceptedAtUtc");
            """,
            cancellationToken)
            .ConfigureAwait(false);
    }
}
