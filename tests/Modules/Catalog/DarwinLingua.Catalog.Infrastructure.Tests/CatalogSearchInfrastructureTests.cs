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
/// Verifies the legacy temporary SQLite catalog fixture and its operational indexes.
/// PostgreSQL-specific Web/API query behavior is covered by dedicated PostgreSQL integration tests.
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
    /// Verifies that browse/search summaries fall back to English when the requested primary meaning language is missing.
    /// </summary>
    [Fact]
    public async Task SearchActiveByLemmaAsync_ShouldFallbackPrimaryMeaningToEnglish()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-catalog-search-fallback-{Guid.NewGuid():N}.db");
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
                dbContext.WordEntries.Add(CreateWord("Termin", "appointment"));
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IWordEntryRepository repository = serviceProvider.GetRequiredService<IWordEntryRepository>();
            IReadOnlyList<WordListItemModel> words = await repository.SearchActiveByLemmaAsync("termin", "fa", CancellationToken.None);

            WordListItemModel word = Assert.Single(words);
            Assert.Equal("appointment", word.PrimaryMeaning);
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

    [Fact]
    public async Task GetPublishedExpressionsAsync_ShouldUseRequestedMeaningLanguageAndFilters()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-expression-list-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime nowUtc = DateTime.UtcNow;
                Topic topic = CreateTopic("shopping-and-services", nowUtc);
                ExpressionEntry expression = CreateExpression("ich-haette-gern", "Ich hätte gern ...", "I would like ...", "من ... می‌خواهم.", nowUtc);
                expression.AddTopic(Guid.NewGuid(), topic.Id, true, nowUtc);

                dbContext.Topics.Add(topic);
                dbContext.ExpressionEntries.Add(expression);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IExpressionRepository repository = serviceProvider.GetRequiredService<IExpressionRepository>();
            IReadOnlyList<ExpressionListItemModel> expressions = await repository.GetPublishedExpressionsAsync(
                new ExpressionListFilterModel("A2", "polite-formula", "polite", "shopping-and-service", "shopping-and-services", false, "gern", "fa"),
                CancellationToken.None);

            ExpressionListItemModel item = Assert.Single(expressions);
            Assert.Equal("ich-haette-gern", item.Slug);
            Assert.Equal("من ... می‌خواهم.", item.ActualMeaning);
            Assert.Contains("shopping-and-services", item.TopicKeys);
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

    [Fact]
    public async Task GetPublishedExpressionBySlugAsync_ShouldProjectWarningsAndLinksSafely()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-expression-detail-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime nowUtc = DateTime.UtcNow;
                ExpressionEntry expression = CreateExpression("na-ja", "Na ja.", "Well, not completely.", "خب، کاملاً نه.", nowUtc, isRisky: true);
                ExpressionExample example = expression.AddExample(Guid.NewGuid(), 10, "Na ja, die Wohnung ist sehr klein.", null, nowUtc);
                example.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "خب، خانه خیلی کوچک است.", nowUtc);
                ExpressionWarning warning = expression.AddWarning(Guid.NewGuid(), "tone", "Use carefully.", nowUtc);
                warning.AddTranslation(Guid.NewGuid(), LanguageCode.From("fa"), "با دقت استفاده کن.", nowUtc);
                expression.AddLinkedWord(Guid.NewGuid(), "die Wohnung", null, 10, nowUtc);
                expression.AddRelatedExpression(Guid.NewGuid(), "alles-klar", 10, nowUtc);
                expression.AddLinkedExercise(Guid.NewGuid(), "a2-expression-tone-practice", 10, nowUtc);

                dbContext.ExpressionEntries.Add(expression);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IExpressionRepository repository = serviceProvider.GetRequiredService<IExpressionRepository>();
            ExpressionDetailModel? detail = await repository.GetPublishedExpressionBySlugAsync("na-ja", "fa", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("خب، کاملاً نه.", detail.ActualMeaning);
            Assert.Equal("با دقت استفاده کن.", Assert.Single(detail.Warnings).Text);
            Assert.Equal("خب، خانه خیلی کوچک است.", Assert.Single(detail.Examples).Translation);
            Assert.Equal("die Wohnung", Assert.Single(detail.LinkedWords).Lemma);
            Assert.Equal("alles-klar", Assert.Single(detail.RelatedExpressionSlugs));
            Assert.Equal("a2-expression-tone-practice", Assert.Single(detail.LinkedExerciseSlugs));
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

    [Fact]
    public async Task UnifiedLearningSearchRepository_ShouldReturnExpressionResults()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-expression-search-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime nowUtc = DateTime.UtcNow;
                dbContext.ExpressionEntries.Add(CreateExpression("einen-moment-bitte", "Einen Moment bitte.", "Please wait briefly.", "لطفاً کمی صبر کنید.", nowUtc));
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IUnifiedLearningSearchRepository repository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> results = await repository.SearchAsync(
                new UnifiedLearningSearchFilterModel("Moment", "A1", "expression", null, null),
                CancellationToken.None);

            UnifiedLearningSearchResultModel result = Assert.Single(results);
            Assert.Equal("expression", result.ResultType);
            Assert.Equal("Einen Moment bitte.", result.Title);
            Assert.Equal("/expressions/einen-moment-bitte", result.Url);
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

    [Fact]
    public async Task ExpressionRepositories_ShouldHideAdultOnlyExpressionsByDefault()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-expression-adult-filter-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime nowUtc = DateTime.UtcNow;
                dbContext.ExpressionEntries.Add(CreateExpression("safe-formula", "Alles klar.", "Understood.", "متوجه شدم.", nowUtc));
                dbContext.ExpressionEntries.Add(CreateExpression(
                    "restricted-expression",
                    "Restricted.",
                    "Restricted.",
                    "محدود.",
                    nowUtc,
                    requiresAdultAccess: true,
                    safetyRating: "explicit-adult"));
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IExpressionRepository expressionRepository = serviceProvider.GetRequiredService<IExpressionRepository>();
            IReadOnlyList<ExpressionListItemModel> expressions = await expressionRepository.GetPublishedExpressionsAsync(
                new ExpressionListFilterModel(null, null, null, null, null, null, null, "fa"),
                CancellationToken.None);
            ExpressionDetailModel? restrictedDetail = await expressionRepository.GetPublishedExpressionBySlugAsync(
                "restricted-expression",
                "fa",
                CancellationToken.None);

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> searchResults = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("Restricted", null, "expression", null, null),
                CancellationToken.None);

            Assert.Contains(expressions, item => item.Slug == "safe-formula");
            Assert.DoesNotContain(expressions, item => item.Slug == "restricted-expression");
            Assert.Null(restrictedDetail);
            Assert.Empty(searchResults);
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

    [Fact]
    public async Task ExpressionRepositories_ShouldShowSensitiveEducationalLanguageOnlyWhenOptedIn()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-expression-sensitive-filter-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);
            await serviceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                DateTime nowUtc = DateTime.UtcNow;
                ExpressionEntry sensitive = CreateExpression(
                    "sensitive-formula",
                    "Mir reicht's.",
                    "I have had enough.",
                    "دیگر برایم کافی است.",
                    nowUtc,
                    isRisky: true,
                    safetyRating: "mild-rude",
                    minimumAge: 16,
                    sensitiveContentKind: "rude-colloquial",
                    requiresSensitiveOptIn: true,
                    usagePolicy: "use-with-care");
                sensitive.AddWarning(Guid.NewGuid(), "tone", "Use with care.", nowUtc);

                ExpressionEntry verifiedAdultOnly = CreateExpression(
                    "verified-adult-only",
                    "Verified only.",
                    "Restricted.",
                    "محدود.",
                    nowUtc,
                    safetyRating: "mild-rude",
                    minimumAge: 18,
                    sensitiveContentKind: "rude-colloquial",
                    requiresSensitiveOptIn: true,
                    requiresVerifiedAdult: true,
                    usagePolicy: "use-with-care");

                dbContext.ExpressionEntries.Add(CreateExpression("safe-formula", "Alles klar.", "Understood.", "متوجه شدم.", nowUtc));
                dbContext.ExpressionEntries.Add(sensitive);
                dbContext.ExpressionEntries.Add(verifiedAdultOnly);
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IExpressionRepository expressionRepository = serviceProvider.GetRequiredService<IExpressionRepository>();
            IReadOnlyList<ExpressionListItemModel> defaultExpressions = await expressionRepository.GetPublishedExpressionsAsync(
                new ExpressionListFilterModel(null, null, null, null, null, null, null, "fa"),
                CancellationToken.None);
            IReadOnlyList<ExpressionListItemModel> optedInExpressions = await expressionRepository.GetPublishedExpressionsAsync(
                new ExpressionListFilterModel(null, null, null, null, null, null, null, "fa", IncludeSensitiveEducationalLanguage: true),
                CancellationToken.None);
            ExpressionDetailModel? defaultDetail = await expressionRepository.GetPublishedExpressionBySlugAsync(
                "sensitive-formula",
                "fa",
                CancellationToken.None);
            ExpressionDetailModel? optedInDetail = await expressionRepository.GetPublishedExpressionBySlugAsync(
                "sensitive-formula",
                "fa",
                true,
                CancellationToken.None);
            ExpressionDetailModel? verifiedAdultDetail = await expressionRepository.GetPublishedExpressionBySlugAsync(
                "verified-adult-only",
                "fa",
                true,
                CancellationToken.None);

            IUnifiedLearningSearchRepository searchRepository = serviceProvider.GetRequiredService<IUnifiedLearningSearchRepository>();
            IReadOnlyList<UnifiedLearningSearchResultModel> defaultSearch = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("reicht", null, "expression", null, null),
                CancellationToken.None);
            IReadOnlyList<UnifiedLearningSearchResultModel> optedInSearch = await searchRepository.SearchAsync(
                new UnifiedLearningSearchFilterModel("reicht", null, "expression", null, null, IncludeSensitiveEducationalLanguage: true),
                CancellationToken.None);

            Assert.DoesNotContain(defaultExpressions, item => item.Slug == "sensitive-formula");
            Assert.Contains(optedInExpressions, item => item.Slug == "sensitive-formula");
            Assert.DoesNotContain(optedInExpressions, item => item.Slug == "verified-adult-only");
            Assert.Null(defaultDetail);
            Assert.NotNull(optedInDetail);
            Assert.Equal("use-with-care", optedInDetail.UsagePolicy);
            Assert.Null(verifiedAdultDetail);
            Assert.Empty(defaultSearch);
            Assert.Contains(optedInSearch, item => item.Url == "/expressions/sensitive-formula");
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
     /// Verifies that inactive lexical entries are not materialized through the detail repository path.
     /// </summary>
    [Fact]
    public async Task GetByPublicIdAsync_ShouldIgnoreInactiveWordEntries()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-catalog-detail-{Guid.NewGuid():N}.db");
        ServiceProvider? serviceProvider = null;

        try
        {
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IDbContextFactory<DarwinLinguaDbContext> dbContextFactory =
                serviceProvider.GetRequiredService<IDbContextFactory<DarwinLinguaDbContext>>();

            Guid publicId = Guid.NewGuid();

            await using (DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(CancellationToken.None))
            {
                dbContext.WordEntries.Add(CreateWord("Altwort", "legacy word", publicId, PublicationStatus.Draft));
                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            IWordEntryRepository repository = serviceProvider.GetRequiredService<IWordEntryRepository>();
            WordEntry? word = await repository.GetByPublicIdAsync(publicId, CancellationToken.None);

            Assert.Null(word);
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
    /// Verifies that Talk Topic vocabulary can resolve catalog words even when the item includes a German article.
    /// </summary>
    [Fact]
    public async Task GetPublishedTalkTopicBySlugAsync_ShouldResolveVocabularyByArticleStrippedLemma()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-talk-topic-vocabulary-{Guid.NewGuid():N}.db");
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
                DateTime nowUtc = DateTime.UtcNow;
                dbContext.WordEntries.Add(CreateWord("Bahnhof", "station"));

                TalkTopic talkTopic = new(
                    Guid.NewGuid(),
                    "a1-bahnhof",
                    "bahnhof",
                    "Bahnhof",
                    "Ein kurzer Talk Topic über den Bahnhof.",
                    CefrLevel.A1,
                    "travel",
                    TalkTopicContentType.Article,
                    "Der Bahnhof ist ein wichtiger Ort in der Stadt.",
                    3,
                    20,
                    false,
                    null,
                    false,
                    PublicationStatus.Active,
                    10,
                    nowUtc);
                talkTopic.AddVocabularyItem(Guid.NewGuid(), "der Bahnhof", "der-bahnhof", CefrLevel.A1, 10, nowUtc);
                dbContext.TalkTopics.Add(talkTopic);

                await dbContext.SaveChangesAsync(CancellationToken.None);
            }

            ITalkTopicRepository repository = serviceProvider.GetRequiredService<ITalkTopicRepository>();
            TalkTopicDetailModel? detail = await repository.GetPublishedTalkTopicBySlugAsync(
                "a1-bahnhof",
                "en",
                null,
                CancellationToken.None);

            Assert.NotNull(detail);
            TalkTopicVocabularyItemModel vocabularyItem = Assert.Single(detail.VocabularyItems);
            Assert.True(vocabularyItem.IsResolved);
            Assert.Equal("bahnhof", vocabularyItem.WordSlug);
            Assert.Equal("station", vocabularyItem.PrimaryMeaning);
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
    /// Builds the shared DI container used by the legacy temporary SQLite repository fixture.
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
    private static WordEntry CreateWord(
        string lemma,
        string translationText,
        Guid? publicId = null,
        PublicationStatus publicationStatus = PublicationStatus.Active)
    {
        DateTime nowUtc = DateTime.UtcNow;
        WordEntry word = new(
            Guid.NewGuid(),
            publicId ?? Guid.NewGuid(),
            lemma,
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            publicationStatus,
            ContentSourceType.Manual,
            nowUtc,
            article: "die");

        WordSense sense = word.AddSense(Guid.NewGuid(), 1, true, PublicationStatus.Active, nowUtc);
        sense.AddTranslation(Guid.NewGuid(), LanguageCode.From("en"), translationText, true, nowUtc);

        return word;
    }

    private static Topic CreateTopic(string key, DateTime nowUtc)
    {
        Topic topic = new(Guid.NewGuid(), key, 10, true, nowUtc);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), key, nowUtc);
        return topic;
    }

    private static ExpressionEntry CreateExpression(
        string slug,
        string expressionText,
        string englishMeaning,
        string persianMeaning,
        DateTime nowUtc,
        bool isRisky = false,
        bool requiresAdultAccess = false,
        string safetyRating = "general",
        int? minimumAge = null,
        string? sensitiveContentKind = null,
        bool requiresSensitiveOptIn = false,
        bool requiresVerifiedAdult = false,
        string? usagePolicy = null)
    {
        ExpressionEntry expression = new(
            Guid.NewGuid(),
            slug,
            expressionText,
            null,
            englishMeaning,
            "Use this expression in a specific everyday context.",
            slug == "einen-moment-bitte" ? CefrLevel.A1 : CefrLevel.A2,
            slug == "na-ja" ? "colloquial-phrase" : "polite-formula",
            slug == "na-ja" ? "colloquial" : "polite",
            slug == "ich-haette-gern" ? "shopping-and-service" : "daily-life",
            "de",
            isRisky,
            PublicationStatus.Active,
            10,
            nowUtc,
            "pragmatic-formula",
            "It has a conventional pragmatic use.",
            safetyRating,
            minimumAge ?? (requiresAdultAccess ? 18 : 0),
            requiresAdultAccess,
            null,
            sensitiveContentKind,
            requiresSensitiveOptIn,
            requiresVerifiedAdult,
            usagePolicy);

        expression.AddMeaning(Guid.NewGuid(), LanguageCode.From("en"), englishMeaning, null, "Use it in everyday German.", nowUtc);
        expression.AddMeaning(Guid.NewGuid(), LanguageCode.From("fa"), persianMeaning, null, "در موقعیت روزمره آلمانی استفاده می‌شود.", nowUtc);

        return expression;
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
