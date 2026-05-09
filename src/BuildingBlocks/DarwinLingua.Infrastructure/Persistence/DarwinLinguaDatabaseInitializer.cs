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
            CREATE UNIQUE INDEX IF NOT EXISTS UX_WordEntries_NormalizedLemma
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

        await ExecuteSqliteIndexIfTableExistsAsync(
            dbContext,
            "WordLabels",
            """
            CREATE INDEX IF NOT EXISTS IX_WordLabels_Kind_Key_WordEntryId
            ON WordLabels (Kind, Key, WordEntryId);
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteSqliteIndexIfTableExistsAsync(
            dbContext,
            "WordTopics",
            """
            CREATE INDEX IF NOT EXISTS IX_WordTopics_TopicId
            ON WordTopics (TopicId);
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteSqliteIndexIfTableExistsAsync(
            dbContext,
            "DialogueLessonTopics",
            """
            CREATE INDEX IF NOT EXISTS IX_DialogueLessonTopics_TopicId
            ON DialogueLessonTopics (TopicId);
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteSqliteIndexIfTableExistsAsync(
            dbContext,
            "ConversationStarterPackTopics",
            """
            CREATE INDEX IF NOT EXISTS IX_ConversationStarterPackTopics_TopicId
            ON ConversationStarterPackTopics (TopicId);
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteSqliteIndexIfTableExistsAsync(
            dbContext,
            "EventPreparationPackTopics",
            """
            CREATE INDEX IF NOT EXISTS IX_EventPreparationPackTopics_TopicId
            ON EventPreparationPackTopics (TopicId);
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExecuteSqliteIndexIfTableExistsAsync(
        DarwinLinguaDbContext dbContext,
        string tableName,
        string sql,
        CancellationToken cancellationToken)
    {
        if (await TableExistsAsync(dbContext, tableName, cancellationToken).ConfigureAwait(false))
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken).ConfigureAwait(false);
        }
    }

    private static async Task EnsureRetrofitSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        await EnsureTalkTopicRetrofitSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);

        if (!await TableExistsAsync(dbContext, "WordLexicalForms", cancellationToken).ConfigureAwait(false))
        {
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
            }
            else
            {
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
        }

        if (!await TableExistsAsync(dbContext, "WordCollections", cancellationToken).ConfigureAwait(false))
        {
            if (dbContext.Database.IsSqlite())
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    """
                    CREATE TABLE IF NOT EXISTS WordCollections (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Slug TEXT NOT NULL,
                        Name TEXT NOT NULL,
                        Description TEXT NULL,
                        ImageUrl TEXT NULL,
                        PublicationStatus TEXT NOT NULL,
                        SortOrder INTEGER NOT NULL,
                        CreatedAtUtc TEXT NOT NULL,
                        UpdatedAtUtc TEXT NOT NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_WordCollections_Slug
                        ON WordCollections (Slug);

                    CREATE TABLE IF NOT EXISTS WordCollectionEntries (
                        Id TEXT NOT NULL PRIMARY KEY,
                        WordCollectionId TEXT NOT NULL,
                        WordEntryId TEXT NOT NULL,
                        SortOrder INTEGER NOT NULL,
                        CreatedAtUtc TEXT NOT NULL,
                        CONSTRAINT FK_WordCollectionEntries_WordCollections_WordCollectionId
                            FOREIGN KEY (WordCollectionId) REFERENCES WordCollections (Id) ON DELETE CASCADE,
                        CONSTRAINT FK_WordCollectionEntries_WordEntries_WordEntryId
                            FOREIGN KEY (WordEntryId) REFERENCES WordEntries (Id) ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_WordCollectionEntries_WordCollectionId_SortOrder
                        ON WordCollectionEntries (WordCollectionId, SortOrder);

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_WordCollectionEntries_WordCollectionId_WordEntryId
                        ON WordCollectionEntries (WordCollectionId, WordEntryId);
                    """,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    """
                    CREATE TABLE IF NOT EXISTS "WordCollections" (
                        "Id" uuid NOT NULL,
                        "Slug" character varying(128) NOT NULL,
                        "Name" character varying(256) NOT NULL,
                        "Description" character varying(4000),
                        "ImageUrl" character varying(1024),
                        "PublicationStatus" character varying(32) NOT NULL,
                        "SortOrder" integer NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        "UpdatedAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_WordCollections" PRIMARY KEY ("Id")
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordCollections_Slug"
                        ON "WordCollections" ("Slug");

                    CREATE TABLE IF NOT EXISTS "WordCollectionEntries" (
                        "Id" uuid NOT NULL,
                        "WordCollectionId" uuid NOT NULL,
                        "WordEntryId" uuid NOT NULL,
                        "SortOrder" integer NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_WordCollectionEntries" PRIMARY KEY ("Id"),
                        CONSTRAINT "FK_WordCollectionEntries_WordCollections_WordCollectionId"
                            FOREIGN KEY ("WordCollectionId") REFERENCES "WordCollections" ("Id") ON DELETE CASCADE,
                        CONSTRAINT "FK_WordCollectionEntries_WordEntries_WordEntryId"
                            FOREIGN KEY ("WordEntryId") REFERENCES "WordEntries" ("Id") ON DELETE CASCADE
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordCollectionEntries_WordCollectionId_SortOrder"
                        ON "WordCollectionEntries" ("WordCollectionId", "SortOrder");

                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_WordCollectionEntries_WordCollectionId_WordEntryId"
                        ON "WordCollectionEntries" ("WordCollectionId", "WordEntryId");
                    """,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        if (!await TableExistsAsync(dbContext, "LabelDefinitions", cancellationToken).ConfigureAwait(false))
        {
            if (dbContext.Database.IsSqlite())
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    """
                    CREATE TABLE IF NOT EXISTS LabelDefinitions (
                        Id TEXT NOT NULL PRIMARY KEY,
                        Kind TEXT NOT NULL,
                        Key TEXT NOT NULL,
                        DisplayName TEXT NOT NULL,
                        SortOrder INTEGER NOT NULL,
                        IsSystem INTEGER NOT NULL,
                        CreatedAtUtc TEXT NOT NULL,
                        UpdatedAtUtc TEXT NOT NULL
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS IX_LabelDefinitions_Kind_Key
                        ON LabelDefinitions (Kind, Key);
                    """,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await dbContext.Database.ExecuteSqlRawAsync(
                    """
                    CREATE TABLE IF NOT EXISTS "LabelDefinitions" (
                        "Id" uuid NOT NULL,
                        "Kind" character varying(16) NOT NULL,
                        "Key" character varying(64) NOT NULL,
                        "DisplayName" character varying(128) NOT NULL,
                        "SortOrder" integer NOT NULL,
                        "IsSystem" boolean NOT NULL,
                        "CreatedAtUtc" timestamp with time zone NOT NULL,
                        "UpdatedAtUtc" timestamp with time zone NOT NULL,
                        CONSTRAINT "PK_LabelDefinitions" PRIMARY KEY ("Id")
                    );

                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_LabelDefinitions_Kind_Key"
                        ON "LabelDefinitions" ("Kind", "Key");
                    """,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        await EnsurePhase6CatalogSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
        await EnsurePhase6OperationalSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureTalkTopicRetrofitSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (dbContext.Database.IsSqlite() ||
            await TableExistsAsync(dbContext, "TalkTopics", cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "TalkTopics" (
                "Id" uuid NOT NULL,
                "Slug" character varying(128) NOT NULL,
                "TopicGroupKey" character varying(128) NOT NULL,
                "Title" character varying(256) NOT NULL,
                "Description" character varying(4000) NOT NULL,
                "CefrLevel" character varying(8) NOT NULL,
                "Category" character varying(128) NOT NULL,
                "ContentType" character varying(64) NOT NULL,
                "ArticleBaseText" character varying(12000) NOT NULL,
                "EstimatedReadingMinutes" integer NOT NULL,
                "EstimatedDiscussionMinutes" integer NOT NULL,
                "IsSensitive" boolean NOT NULL,
                "SensitivityNote" character varying(1024),
                "RecommendedForModeratedGroupsOnly" boolean NOT NULL,
                "PublicationStatus" character varying(32) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopics" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "TalkTopicArticleTranslations" (
                "Id" uuid NOT NULL,
                "OwnerId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(12000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopicArticleTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TalkTopicArticleTranslations_TalkTopics_OwnerId"
                    FOREIGN KEY ("OwnerId") REFERENCES "TalkTopics" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "TalkTopicQuestions" (
                "Id" uuid NOT NULL,
                "TalkTopicId" uuid NOT NULL,
                "Kind" character varying(32) NOT NULL,
                "QuestionType" character varying(64),
                "SortOrder" integer NOT NULL,
                "Prompt" character varying(1024) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopicQuestions" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TalkTopicQuestions_TalkTopics_TalkTopicId"
                    FOREIGN KEY ("TalkTopicId") REFERENCES "TalkTopics" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "TalkTopicQuestionTranslations" (
                "Id" uuid NOT NULL,
                "OwnerId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopicQuestionTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TalkTopicQuestionTranslations_TalkTopicQuestions_OwnerId"
                    FOREIGN KEY ("OwnerId") REFERENCES "TalkTopicQuestions" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "TalkTopicSpeakingGoals" (
                "Id" uuid NOT NULL,
                "TalkTopicId" uuid NOT NULL,
                "SpeakingGoal" character varying(64) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopicSpeakingGoals" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TalkTopicSpeakingGoals_TalkTopics_TalkTopicId"
                    FOREIGN KEY ("TalkTopicId") REFERENCES "TalkTopics" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "TalkTopicTopics" (
                "Id" uuid NOT NULL,
                "TalkTopicId" uuid NOT NULL,
                "TopicId" uuid NOT NULL,
                "IsPrimary" boolean NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopicTopics" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TalkTopicTopics_TalkTopics_TalkTopicId"
                    FOREIGN KEY ("TalkTopicId") REFERENCES "TalkTopics" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_TalkTopicTopics_Topics_TopicId"
                    FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS "TalkTopicVocabularyItems" (
                "Id" uuid NOT NULL,
                "TalkTopicId" uuid NOT NULL,
                "Lemma" character varying(128) NOT NULL,
                "WordSlug" character varying(128),
                "CefrLevel" character varying(8),
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_TalkTopicVocabularyItems" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TalkTopicVocabularyItems_TalkTopics_TalkTopicId"
                    FOREIGN KEY ("TalkTopicId") REFERENCES "TalkTopics" ("Id") ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicArticleTranslations_OwnerId_LanguageCode" ON "TalkTopicArticleTranslations" ("OwnerId", "LanguageCode");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicQuestions_TalkTopicId_Kind_SortOrder" ON "TalkTopicQuestions" ("TalkTopicId", "Kind", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicQuestionTranslations_OwnerId_LanguageCode" ON "TalkTopicQuestionTranslations" ("OwnerId", "LanguageCode");
            CREATE INDEX IF NOT EXISTS "IX_TalkTopics_CefrLevel_ContentType_Category" ON "TalkTopics" ("CefrLevel", "ContentType", "Category");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopics_Slug" ON "TalkTopics" ("Slug");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicSpeakingGoals_TalkTopicId_SortOrder" ON "TalkTopicSpeakingGoals" ("TalkTopicId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicSpeakingGoals_TalkTopicId_SpeakingGoal" ON "TalkTopicSpeakingGoals" ("TalkTopicId", "SpeakingGoal");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicTopics_TalkTopicId" ON "TalkTopicTopics" ("TalkTopicId") WHERE "IsPrimary";
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicTopics_TalkTopicId_TopicId" ON "TalkTopicTopics" ("TalkTopicId", "TopicId");
            CREATE INDEX IF NOT EXISTS "IX_TalkTopicTopics_TopicId" ON "TalkTopicTopics" ("TopicId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_TalkTopicVocabularyItems_TalkTopicId_SortOrder" ON "TalkTopicVocabularyItems" ("TalkTopicId", "SortOrder");
            CREATE INDEX IF NOT EXISTS "IX_TalkTopicVocabularyItems_WordSlug" ON "TalkTopicVocabularyItems" ("WordSlug");
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePhase6CatalogSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (dbContext.Database.IsSqlite())
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "DialogueLessons" (
                "Id" uuid NOT NULL,
                "Slug" character varying(128) NOT NULL,
                "Title" character varying(256) NOT NULL,
                "Description" character varying(4000) NOT NULL,
                "LearnerGoal" character varying(1024) NOT NULL,
                "CefrLevel" character varying(8) NOT NULL,
                "Category" character varying(128) NOT NULL,
                "PublicationStatus" character varying(32) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueLessons" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "DialogueLessonTopics" (
                "Id" uuid NOT NULL,
                "DialogueLessonId" uuid NOT NULL,
                "TopicId" uuid NOT NULL,
                "IsPrimary" boolean NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueLessonTopics" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueLessonTopics_DialogueLessons_DialogueLessonId"
                    FOREIGN KEY ("DialogueLessonId") REFERENCES "DialogueLessons" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_DialogueLessonTopics_Topics_TopicId"
                    FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS "DialogueTurns" (
                "Id" uuid NOT NULL,
                "DialogueLessonId" uuid NOT NULL,
                "SortOrder" integer NOT NULL,
                "SpeakerRole" character varying(64) NOT NULL,
                "BaseText" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueTurns" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueTurns_DialogueLessons_DialogueLessonId"
                    FOREIGN KEY ("DialogueLessonId") REFERENCES "DialogueLessons" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialoguePhrases" (
                "Id" uuid NOT NULL,
                "DialogueLessonId" uuid NOT NULL,
                "SortOrder" integer NOT NULL,
                "BaseText" character varying(1024) NOT NULL,
                "UsageNote" character varying(1024),
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialoguePhrases" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialoguePhrases_DialogueLessons_DialogueLessonId"
                    FOREIGN KEY ("DialogueLessonId") REFERENCES "DialogueLessons" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialogueQuestions" (
                "Id" uuid NOT NULL,
                "DialogueLessonId" uuid NOT NULL,
                "SortOrder" integer NOT NULL,
                "Prompt" character varying(1024) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueQuestions" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueQuestions_DialogueLessons_DialogueLessonId"
                    FOREIGN KEY ("DialogueLessonId") REFERENCES "DialogueLessons" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialogueAnswers" (
                "Id" uuid NOT NULL,
                "DialogueQuestionId" uuid NOT NULL,
                "SortOrder" integer NOT NULL,
                "Text" character varying(1024) NOT NULL,
                "IsCorrect" boolean NOT NULL,
                "Feedback" character varying(1024),
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueAnswers" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueAnswers_DialogueQuestions_DialogueQuestionId"
                    FOREIGN KEY ("DialogueQuestionId") REFERENCES "DialogueQuestions" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialogueTurnTranslations" (
                "Id" uuid NOT NULL,
                "OwnerId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueTurnTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueTurnTranslations_DialogueTurns_OwnerId"
                    FOREIGN KEY ("OwnerId") REFERENCES "DialogueTurns" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialoguePhraseTranslations" (
                "Id" uuid NOT NULL,
                "OwnerId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialoguePhraseTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialoguePhraseTranslations_DialoguePhrases_OwnerId"
                    FOREIGN KEY ("OwnerId") REFERENCES "DialoguePhrases" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialogueQuestionTranslations" (
                "Id" uuid NOT NULL,
                "OwnerId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueQuestionTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueQuestionTranslations_DialogueQuestions_OwnerId"
                    FOREIGN KEY ("OwnerId") REFERENCES "DialogueQuestions" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "DialogueAnswerTranslations" (
                "Id" uuid NOT NULL,
                "OwnerId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_DialogueAnswerTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_DialogueAnswerTranslations_DialogueAnswers_OwnerId"
                    FOREIGN KEY ("OwnerId") REFERENCES "DialogueAnswers" ("Id") ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueLessons_Slug" ON "DialogueLessons" ("Slug");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueLessonTopics_PrimaryPerLesson" ON "DialogueLessonTopics" ("DialogueLessonId") WHERE "IsPrimary" = TRUE;
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueLessonTopics_DialogueLessonId_TopicId" ON "DialogueLessonTopics" ("DialogueLessonId", "TopicId");
            CREATE INDEX IF NOT EXISTS "IX_DialogueLessonTopics_TopicId" ON "DialogueLessonTopics" ("TopicId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueTurns_DialogueLessonId_SortOrder" ON "DialogueTurns" ("DialogueLessonId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueTurnTranslations_OwnerId_LanguageCode" ON "DialogueTurnTranslations" ("OwnerId", "LanguageCode");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialoguePhrases_DialogueLessonId_SortOrder" ON "DialoguePhrases" ("DialogueLessonId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialoguePhraseTranslations_OwnerId_LanguageCode" ON "DialoguePhraseTranslations" ("OwnerId", "LanguageCode");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueQuestions_DialogueLessonId_SortOrder" ON "DialogueQuestions" ("DialogueLessonId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueQuestionTranslations_OwnerId_LanguageCode" ON "DialogueQuestionTranslations" ("OwnerId", "LanguageCode");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueAnswers_DialogueQuestionId_SortOrder" ON "DialogueAnswers" ("DialogueQuestionId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DialogueAnswerTranslations_OwnerId_LanguageCode" ON "DialogueAnswerTranslations" ("OwnerId", "LanguageCode");
            """,
            cancellationToken).ConfigureAwait(false);

        await EnsurePhase6StarterSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
        await EnsurePhase6EventPreparationSchemaAsync(dbContext, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePhase6StarterSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ConversationStarterPacks" (
                "Id" uuid NOT NULL,
                "Slug" character varying(128) NOT NULL,
                "Title" character varying(256) NOT NULL,
                "Description" character varying(4000) NOT NULL,
                "CefrLevel" character varying(8) NOT NULL,
                "Category" character varying(128) NOT NULL,
                "Situation" character varying(128) NOT NULL,
                "Tone" character varying(128) NOT NULL,
                "ConversationGoal" character varying(128) NOT NULL,
                "PublicationStatus" character varying(32) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterPacks" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "ConversationStarterPackTopics" (
                "Id" uuid NOT NULL,
                "ConversationStarterPackId" uuid NOT NULL,
                "TopicId" uuid NOT NULL,
                "IsPrimary" boolean NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterPackTopics" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ConversationStarterPackTopics_ConversationStarterPacks_ConversationStarterPackId"
                    FOREIGN KEY ("ConversationStarterPackId") REFERENCES "ConversationStarterPacks" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_ConversationStarterPackTopics_Topics_TopicId"
                    FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS "ConversationStarterLinkedDialogues" (
                "Id" uuid NOT NULL,
                "ConversationStarterPackId" uuid NOT NULL,
                "DialogueSlug" character varying(128) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterLinkedDialogues" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ConversationStarterLinkedDialogues_ConversationStarterPacks_ConversationStarterPackId"
                    FOREIGN KEY ("ConversationStarterPackId") REFERENCES "ConversationStarterPacks" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "ConversationStarterLinkedEventPreparationPacks" (
                "Id" uuid NOT NULL,
                "ConversationStarterPackId" uuid NOT NULL,
                "EventPreparationPackSlug" character varying(128) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterLinkedEventPreparationPacks" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ConversationStarterLinkedEventPreparationPacks_ConversationStarterPacks_ConversationStarterPackId"
                    FOREIGN KEY ("ConversationStarterPackId") REFERENCES "ConversationStarterPacks" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "ConversationStarterPhrases" (
                "Id" uuid NOT NULL,
                "ConversationStarterPackId" uuid NOT NULL,
                "SortOrder" integer NOT NULL,
                "BaseText" character varying(1024) NOT NULL,
                "Function" character varying(128) NOT NULL,
                "UsageNote" character varying(1024),
                "Register" character varying(64),
                "CommonMistake" character varying(1024),
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterPhrases" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ConversationStarterPhrases_ConversationStarterPacks_ConversationStarterPackId"
                    FOREIGN KEY ("ConversationStarterPackId") REFERENCES "ConversationStarterPacks" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "ConversationStarterPhraseAlternatives" (
                "Id" uuid NOT NULL,
                "ConversationStarterPhraseId" uuid NOT NULL,
                "SortOrder" integer NOT NULL,
                "BaseText" character varying(1024) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterPhraseAlternatives" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ConversationStarterPhraseAlternatives_ConversationStarterPhrases_ConversationStarterPhraseId"
                    FOREIGN KEY ("ConversationStarterPhraseId") REFERENCES "ConversationStarterPhrases" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "ConversationStarterPhraseTranslations" (
                "Id" uuid NOT NULL,
                "ConversationStarterPhraseId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationStarterPhraseTranslations" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ConversationStarterPhraseTranslations_ConversationStarterPhrases_ConversationStarterPhraseId"
                    FOREIGN KEY ("ConversationStarterPhraseId") REFERENCES "ConversationStarterPhrases" ("Id") ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPacks_Slug" ON "ConversationStarterPacks" ("Slug");
            CREATE INDEX IF NOT EXISTS "IX_ConversationStarterPacks_CefrLevel_Situation_Tone_ConversationGoal" ON "ConversationStarterPacks" ("CefrLevel", "Situation", "Tone", "ConversationGoal");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPackTopics_PrimaryPerPack" ON "ConversationStarterPackTopics" ("ConversationStarterPackId") WHERE "IsPrimary" = TRUE;
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPackTopics_ConversationStarterPackId_TopicId" ON "ConversationStarterPackTopics" ("ConversationStarterPackId", "TopicId");
            CREATE INDEX IF NOT EXISTS "IX_ConversationStarterPackTopics_TopicId" ON "ConversationStarterPackTopics" ("TopicId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterLinkedDialogues_ConversationStarterPackId_DialogueSlug" ON "ConversationStarterLinkedDialogues" ("ConversationStarterPackId", "DialogueSlug");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterLinkedEventPreparationPacks_ConversationStarterPackId_EventPreparationPackSlug" ON "ConversationStarterLinkedEventPreparationPacks" ("ConversationStarterPackId", "EventPreparationPackSlug");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPhrases_ConversationStarterPackId_SortOrder" ON "ConversationStarterPhrases" ("ConversationStarterPackId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPhraseAlternatives_ConversationStarterPhraseId_SortOrder" ON "ConversationStarterPhraseAlternatives" ("ConversationStarterPhraseId", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationStarterPhraseTranslations_ConversationStarterPhraseId_LanguageCode" ON "ConversationStarterPhraseTranslations" ("ConversationStarterPhraseId", "LanguageCode");
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePhase6EventPreparationSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "EventPreparationPacks" (
                "Id" uuid NOT NULL,
                "Slug" character varying(128) NOT NULL,
                "Title" character varying(256) NOT NULL,
                "Description" character varying(4000) NOT NULL,
                "CefrLevel" character varying(8) NOT NULL,
                "Category" character varying(128) NOT NULL,
                "EventType" character varying(128) NOT NULL,
                "PublicationStatus" character varying(32) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventPreparationPacks" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "EventPreparationPackTopics" (
                "Id" uuid NOT NULL,
                "EventPreparationPackId" uuid NOT NULL,
                "TopicId" uuid NOT NULL,
                "IsPrimary" boolean NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventPreparationPackTopics" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_EventPreparationPackTopics_EventPreparationPacks_EventPreparationPackId"
                    FOREIGN KEY ("EventPreparationPackId") REFERENCES "EventPreparationPacks" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_EventPreparationPackTopics_Topics_TopicId"
                    FOREIGN KEY ("TopicId") REFERENCES "Topics" ("Id") ON DELETE RESTRICT
            );

            CREATE TABLE IF NOT EXISTS "EventPreparationLinkedDialogues" (
                "Id" uuid NOT NULL,
                "EventPreparationPackId" uuid NOT NULL,
                "DialogueSlug" character varying(128) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventPreparationLinkedDialogues" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_EventPreparationLinkedDialogues_EventPreparationPacks_EventPreparationPackId"
                    FOREIGN KEY ("EventPreparationPackId") REFERENCES "EventPreparationPacks" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "EventPreparationLinkedConversationStarterPacks" (
                "Id" uuid NOT NULL,
                "EventPreparationPackId" uuid NOT NULL,
                "ConversationStarterPackSlug" character varying(128) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventPreparationLinkedConversationStarterPacks" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_EventPreparationLinkedConversationStarterPacks_EventPreparationPacks_EventPreparationPackId"
                    FOREIGN KEY ("EventPreparationPackId") REFERENCES "EventPreparationPacks" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "EventPreparationPrompts" (
                "Id" uuid NOT NULL,
                "EventPreparationPackId" uuid NOT NULL,
                "PromptType" character varying(64) NOT NULL,
                "SortOrder" integer NOT NULL,
                "Text" character varying(2000) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventPreparationPrompts" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_EventPreparationPrompts_EventPreparationPacks_EventPreparationPackId"
                    FOREIGN KEY ("EventPreparationPackId") REFERENCES "EventPreparationPacks" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "EventPreparationVocabularyReferences" (
                "Id" uuid NOT NULL,
                "EventPreparationPackId" uuid NOT NULL,
                "Word" character varying(256) NOT NULL,
                "PartOfSpeech" character varying(32),
                "CefrLevel" character varying(8),
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventPreparationVocabularyReferences" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_EventPreparationVocabularyReferences_EventPreparationPacks_EventPreparationPackId"
                    FOREIGN KEY ("EventPreparationPackId") REFERENCES "EventPreparationPacks" ("Id") ON DELETE CASCADE
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationPacks_Slug" ON "EventPreparationPacks" ("Slug");
            CREATE INDEX IF NOT EXISTS "IX_EventPreparationPacks_CefrLevel_Category_EventType" ON "EventPreparationPacks" ("CefrLevel", "Category", "EventType");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationPackTopics_PrimaryPerPack" ON "EventPreparationPackTopics" ("EventPreparationPackId") WHERE "IsPrimary" = TRUE;
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationPackTopics_EventPreparationPackId_TopicId" ON "EventPreparationPackTopics" ("EventPreparationPackId", "TopicId");
            CREATE INDEX IF NOT EXISTS "IX_EventPreparationPackTopics_TopicId" ON "EventPreparationPackTopics" ("TopicId");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationLinkedDialogues_EventPreparationPackId_DialogueSlug" ON "EventPreparationLinkedDialogues" ("EventPreparationPackId", "DialogueSlug");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationLinkedConversationStarterPacks_EventPreparationPackId_ConversationStarterPackSlug" ON "EventPreparationLinkedConversationStarterPacks" ("EventPreparationPackId", "ConversationStarterPackSlug");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationPrompts_EventPreparationPackId_PromptType_SortOrder" ON "EventPreparationPrompts" ("EventPreparationPackId", "PromptType", "SortOrder");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventPreparationVocabularyReferences_EventPreparationPackId_SortOrder" ON "EventPreparationVocabularyReferences" ("EventPreparationPackId", "SortOrder");
            """,
            cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsurePhase6OperationalSchemaAsync(
        DarwinLinguaDbContext dbContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        if (dbContext.Database.IsSqlite())
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "ConversationEvents" (
                "Id" uuid NOT NULL,
                "Slug" character varying(128) NOT NULL,
                "Name" character varying(256) NOT NULL,
                "Description" character varying(4000) NOT NULL,
                "City" character varying(128),
                "CountryRegion" character varying(128) NOT NULL,
                "ApproximateLocation" character varying(512),
                "IsOnline" boolean NOT NULL,
                "Category" character varying(128) NOT NULL,
                "OrganizerName" character varying(256) NOT NULL,
                "OrganizerProfileSlug" character varying(128),
                "ExternalLink" character varying(1024),
                "ContactMethod" character varying(512),
                "ScheduleText" character varying(1000) NOT NULL,
                "RecurrenceRule" character varying(256),
                "Capacity" integer,
                "PriceType" character varying(64) NOT NULL,
                "VerificationStatus" character varying(64) NOT NULL,
                "SourceName" character varying(256),
                "SourceUrl" character varying(1024),
                "LastVerifiedAtUtc" timestamp with time zone,
                "PublicationStatus" character varying(32) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationEvents" PRIMARY KEY ("Id")
            );

            ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "OrganizerProfileSlug" character varying(128);
            ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "RecurrenceRule" character varying(256);
            ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "Capacity" integer;
            ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "SourceName" character varying(256);
            ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "SourceUrl" character varying(1024);
            ALTER TABLE "ConversationEvents" ADD COLUMN IF NOT EXISTS "LastVerifiedAtUtc" timestamp with time zone;

            CREATE TABLE IF NOT EXISTS "ConversationEventLevels" (
                "Id" uuid NOT NULL,
                "ConversationEventId" uuid NOT NULL,
                "CefrLevel" character varying(8) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationEventLevels" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "ConversationEventHelperLanguages" (
                "Id" uuid NOT NULL,
                "ConversationEventId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationEventHelperLanguages" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "ConversationEventPreparationPackLinks" (
                "Id" uuid NOT NULL,
                "ConversationEventId" uuid NOT NULL,
                "PreparationPackSlug" character varying(128) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ConversationEventPreparationPackLinks" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "OrganizerProfiles" (
                "Id" uuid NOT NULL,
                "Slug" character varying(128) NOT NULL,
                "DisplayName" character varying(256) NOT NULL,
                "OrganizerType" character varying(64) NOT NULL,
                "Description" character varying(4000) NOT NULL,
                "CityRegion" character varying(128),
                "IsOnlineAvailable" boolean NOT NULL,
                "WebsiteUrl" character varying(1024),
                "PublicContactMethod" character varying(512),
                "VerificationStatus" character varying(64) NOT NULL,
                "PlanKey" character varying(64) NOT NULL,
                "PublicationStatus" character varying(32) NOT NULL,
                "HistoricalEventCount" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_OrganizerProfiles" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "OrganizerProfileSupportedLevels" (
                "Id" uuid NOT NULL,
                "OrganizerProfileId" uuid NOT NULL,
                "CefrLevel" character varying(8) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_OrganizerProfileSupportedLevels" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "OrganizerProfileHelperLanguages" (
                "Id" uuid NOT NULL,
                "OrganizerProfileId" uuid NOT NULL,
                "LanguageCode" character varying(16) NOT NULL,
                "SortOrder" integer NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_OrganizerProfileHelperLanguages" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "OrganizerClaimRequests" (
                "Id" uuid NOT NULL,
                "OrganizerProfileSlug" character varying(128) NOT NULL,
                "RequesterName" character varying(256) NOT NULL,
                "RequesterEmail" character varying(320) NOT NULL,
                "RelationshipToOrganizer" character varying(256) NOT NULL,
                "EvidenceText" character varying(4000) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_OrganizerClaimRequests" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "OrganizerProfileOwners" (
                "Id" uuid NOT NULL,
                "OrganizerProfileSlug" character varying(128) NOT NULL,
                "OwnerEmail" character varying(320) NOT NULL,
                "AssignedBy" character varying(320) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_OrganizerProfileOwners" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "EventRsvps" (
                "Id" uuid NOT NULL,
                "ConversationEventSlug" character varying(128) NOT NULL,
                "ParticipantName" character varying(256) NOT NULL,
                "ParticipantEmail" character varying(320) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_EventRsvps" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "LearnerConversationProfiles" (
                "Id" uuid NOT NULL,
                "OwnerEmail" character varying(320) NOT NULL,
                "DisplayName" character varying(128) NOT NULL,
                "CityRegion" character varying(128),
                "InteractionPreference" character varying(32) NOT NULL,
                "GermanLevel" character varying(8) NOT NULL,
                "HelperLanguageCodes" character varying(256) NOT NULL,
                "ConversationGoals" character varying(1000) NOT NULL,
                "AvailabilityNotes" character varying(1000),
                "Visibility" character varying(32) NOT NULL,
                "HasConfirmedAdult" boolean NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_LearnerConversationProfiles" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "PartnerRequests" (
                "Id" uuid NOT NULL,
                "RequesterEmail" character varying(320) NOT NULL,
                "TargetLearnerProfileId" uuid NOT NULL,
                "OpenerTemplateKey" character varying(64) NOT NULL,
                "Note" character varying(500),
                "Status" character varying(64) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "ExpiresAtUtc" timestamp with time zone NOT NULL,
                "RespondedAtUtc" timestamp with time zone,
                CONSTRAINT "PK_PartnerRequests" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "UserReports" (
                "Id" uuid NOT NULL,
                "ReporterEmail" character varying(320) NOT NULL,
                "TargetType" character varying(64) NOT NULL,
                "TargetKey" character varying(256) NOT NULL,
                "ReportedUserEmail" character varying(320),
                "Reason" character varying(64) NOT NULL,
                "Details" character varying(2000) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "DecisionNote" character varying(1000),
                "DecidedBy" character varying(320),
                "DecidedAtUtc" timestamp with time zone,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_UserReports" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "UserBlocks" (
                "Id" uuid NOT NULL,
                "BlockerEmail" character varying(320) NOT NULL,
                "BlockedEmail" character varying(320) NOT NULL,
                "Reason" character varying(500),
                "SourcePartnerRequestId" uuid,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_UserBlocks" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "ModerationDecisionAudits" (
                "Id" uuid NOT NULL,
                "UserReportId" uuid NOT NULL,
                "DecisionStatus" character varying(64) NOT NULL,
                "DecidedBy" character varying(320) NOT NULL,
                "DecisionNote" character varying(1000),
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ModerationDecisionAudits" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "OrganizerVerifications" (
                "Id" uuid NOT NULL,
                "OrganizerProfileSlug" character varying(128) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "RequestedByEmail" character varying(320) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_OrganizerVerifications" PRIMARY KEY ("Id")
            );

            CREATE TABLE IF NOT EXISTS "ListingReviews" (
                "Id" uuid NOT NULL,
                "ListingType" character varying(64) NOT NULL,
                "ListingKey" character varying(256) NOT NULL,
                "Status" character varying(64) NOT NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                CONSTRAINT "PK_ListingReviews" PRIMARY KEY ("Id")
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationEvents_Slug" ON "ConversationEvents" ("Slug");
            CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_OrganizerProfileSlug" ON "ConversationEvents" ("OrganizerProfileSlug");
            CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_City_IsOnline_PriceType" ON "ConversationEvents" ("City", "IsOnline", "PriceType");
            CREATE INDEX IF NOT EXISTS "IX_ConversationEvents_Category_PublicationStatus" ON "ConversationEvents" ("Category", "PublicationStatus");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationEventLevels_ConversationEventId_CefrLevel" ON "ConversationEventLevels" ("ConversationEventId", "CefrLevel");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationEventHelperLanguages_ConversationEventId_LanguageCode" ON "ConversationEventHelperLanguages" ("ConversationEventId", "LanguageCode");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ConversationEventPreparationPackLinks_ConversationEventId_PreparationPackSlug" ON "ConversationEventPreparationPackLinks" ("ConversationEventId", "PreparationPackSlug");

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizerProfiles_Slug" ON "OrganizerProfiles" ("Slug");
            CREATE INDEX IF NOT EXISTS "IX_OrganizerProfiles_OrganizerType_PublicationStatus" ON "OrganizerProfiles" ("OrganizerType", "PublicationStatus");
            CREATE INDEX IF NOT EXISTS "IX_OrganizerProfiles_CityRegion_PublicationStatus" ON "OrganizerProfiles" ("CityRegion", "PublicationStatus");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizerProfileSupportedLevels_OrganizerProfileId_CefrLevel" ON "OrganizerProfileSupportedLevels" ("OrganizerProfileId", "CefrLevel");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizerProfileHelperLanguages_OrganizerProfileId_LanguageCode" ON "OrganizerProfileHelperLanguages" ("OrganizerProfileId", "LanguageCode");
            CREATE INDEX IF NOT EXISTS "IX_OrganizerClaimRequests_OrganizerProfileSlug_Status" ON "OrganizerClaimRequests" ("OrganizerProfileSlug", "Status");
            CREATE INDEX IF NOT EXISTS "IX_OrganizerClaimRequests_RequesterEmail_OrganizerProfileSlug_Status" ON "OrganizerClaimRequests" ("RequesterEmail", "OrganizerProfileSlug", "Status");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizerProfileOwners_OrganizerProfileSlug_OwnerEmail" ON "OrganizerProfileOwners" ("OrganizerProfileSlug", "OwnerEmail");
            CREATE INDEX IF NOT EXISTS "IX_OrganizerProfileOwners_OwnerEmail" ON "OrganizerProfileOwners" ("OwnerEmail");

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EventRsvps_ConversationEventSlug_ParticipantEmail" ON "EventRsvps" ("ConversationEventSlug", "ParticipantEmail");
            CREATE INDEX IF NOT EXISTS "IX_EventRsvps_ConversationEventSlug_Status" ON "EventRsvps" ("ConversationEventSlug", "Status");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_LearnerConversationProfiles_OwnerEmail" ON "LearnerConversationProfiles" ("OwnerEmail");
            CREATE INDEX IF NOT EXISTS "IX_LearnerConversationProfiles_Visibility_CityRegion_GermanLevel" ON "LearnerConversationProfiles" ("Visibility", "CityRegion", "GermanLevel");
            CREATE INDEX IF NOT EXISTS "IX_PartnerRequests_RequesterEmail_TargetLearnerProfileId_Status" ON "PartnerRequests" ("RequesterEmail", "TargetLearnerProfileId", "Status");
            CREATE INDEX IF NOT EXISTS "IX_PartnerRequests_TargetLearnerProfileId_Status" ON "PartnerRequests" ("TargetLearnerProfileId", "Status");
            CREATE INDEX IF NOT EXISTS "IX_PartnerRequests_RequesterEmail_CreatedAtUtc" ON "PartnerRequests" ("RequesterEmail", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_UserReports_Status_CreatedAtUtc" ON "UserReports" ("Status", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_UserReports_TargetType_TargetKey" ON "UserReports" ("TargetType", "TargetKey");
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_UserBlocks_BlockerEmail_BlockedEmail" ON "UserBlocks" ("BlockerEmail", "BlockedEmail");
            CREATE INDEX IF NOT EXISTS "IX_UserBlocks_BlockedEmail" ON "UserBlocks" ("BlockedEmail");
            CREATE INDEX IF NOT EXISTS "IX_ModerationDecisionAudits_UserReportId_CreatedAtUtc" ON "ModerationDecisionAudits" ("UserReportId", "CreatedAtUtc");
            CREATE INDEX IF NOT EXISTS "IX_OrganizerVerifications_OrganizerProfileSlug_Status" ON "OrganizerVerifications" ("OrganizerProfileSlug", "Status");
            CREATE INDEX IF NOT EXISTS "IX_ListingReviews_ListingType_ListingKey_Status" ON "ListingReviews" ("ListingType", "ListingKey", "Status");
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
