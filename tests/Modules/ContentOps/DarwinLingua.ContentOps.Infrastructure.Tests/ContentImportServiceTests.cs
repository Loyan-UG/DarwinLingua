using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Infrastructure.DependencyInjection;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.DependencyInjection;
using DarwinLingua.Infrastructure.Persistence.Abstractions;
using DarwinLingua.Localization.Application.DependencyInjection;
using DarwinLingua.Localization.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DarwinLingua.ContentOps.Infrastructure.Tests;

/// <summary>
/// Verifies the end-to-end Phase 1 content import workflow against a temporary SQLite database.
/// </summary>
public sealed class ContentImportServiceTests
{
    /// <summary>
    /// Verifies that a valid package imports one new word and that the imported word becomes queryable.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportValidPackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateValidPackageJson("a1-shopping-import-test"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("Completed", result.Status);
            Assert.Equal(1, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(["Brot"], result.ImportedLemmas);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> words = await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.WordListItemModel importedWord = Assert.Single(words);
            Assert.Equal("Brot", importedWord.Lemma);
            Assert.Equal("bread", importedWord.PrimaryMeaning);

            IWordDetailQueryService detailQueryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();
            DarwinLingua.Catalog.Application.Models.WordDetailModel? detail = await detailQueryService
                .GetWordDetailsAsync(importedWord.PublicId, "en", null, "en", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Contains("informal", detail!.UsageLabels);
            Assert.Contains("shopping", detail.ContextLabels);
            Assert.Contains("Plural form is mostly used when talking about different bread types.", detail.GrammarNotes);
            Assert.Contains(detail.Collocations, collocation => collocation.Text == "frisches Brot kaufen" && collocation.Meaning == "to buy fresh bread");
            Assert.Contains(detail.WordFamilies, member => member.Lemma == "Bäcker" && member.RelationLabel == "Profession");
            Assert.Contains(detail.Synonyms, relation => relation.Lemma == "Laib");
            Assert.Contains(detail.Antonyms, relation => relation.Lemma == "Fasten");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that the same package identifier cannot be imported twice.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldRejectDuplicatePackageId()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreateValidPackageJson("a1-shopping-import-duplicate-package"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();

            ImportContentPackageResult firstResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);
            ImportContentPackageResult secondResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(firstResult.IsSuccess);
            Assert.False(secondResult.IsSuccess);
            Assert.Equal("Failed", secondResult.Status);
            Assert.Contains(secondResult.Issues, issue => issue.Message.Contains("already exists", StringComparison.Ordinal));
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that invalid entries are reported while valid entries in the same package are still imported.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldReportInvalidEntriesAndImportValidEntries()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithInvalidEntryJson("a1-shopping-import-invalid-mixed"));
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("CompletedWithWarnings", result.Status);
            Assert.Equal(2, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(1, result.InvalidEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Contains(result.Issues, issue => issue.EntryIndex == 2 && issue.Severity == "Error");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that duplicate entries inside one package are skipped with warning accounting.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldSkipDuplicateEntriesWithinSinglePackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithDuplicateEntriesJson("a1-shopping-import-duplicate-entry"));
            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("CompletedWithWarnings", result.Status);
            Assert.Equal(2, result.TotalEntries);
            Assert.Equal(1, result.ImportedEntries);
            Assert.Equal(1, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(1, result.WarningCount);
            Assert.Contains(result.Issues, issue => issue.EntryIndex == 2 && issue.Severity == "Warning");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that lexicalForms import populates additional lexical roles while preserving the primary role.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportMultipleLexicalForms()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithMultipleLexicalFormsJson("a1-shopping-multi-lexical-forms"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();
            DarwinLingua.Catalog.Application.Models.WordListItemModel importedWord = Assert.Single(await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None));

            IWordDetailQueryService detailQueryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();
            DarwinLingua.Catalog.Application.Models.WordDetailModel? detail = await detailQueryService
                .GetWordDetailsAsync(importedWord.PublicId, "en", null, "en", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal(2, detail!.LexicalForms.Count);
            Assert.Contains(detail.LexicalForms, form => form.PartOfSpeech == "Noun" && form.IsPrimary && form.Article == "die");
            Assert.Contains(detail.LexicalForms, form => form.PartOfSpeech == "Verb" && !form.IsPrimary && form.InfinitiveForm == "Kasse machen");
            Assert.Equal("/ˈkasə/", detail.PronunciationIpa);
            Assert.Equal("Kas-se", detail.SyllableBreak);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that the compact collection wordKeys format is accepted for AI-generated content packages.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollectionWordKeys()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithCollectionWordKeysJson("a1-shopping-collection-word-keys"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(2, result.ImportedEntries);

            IWordCollectionQueryService collectionQueryService = serviceProvider.GetRequiredService<IWordCollectionQueryService>();
            DarwinLingua.Catalog.Application.Models.WordCollectionDetailModel? collection = await collectionQueryService
                .GetPublishedCollectionBySlugAsync("a1-shopping-word-keys", "en", CancellationToken.None);

            Assert.NotNull(collection);
            Assert.Equal("collections/a1-shopping-word-keys.png", collection!.ImageUrl);
            Assert.Equal(2, collection.Words.Count);
            Assert.Contains(collection.Words, word => word.Lemma == "Brot");
            Assert.Contains(collection.Words, word => word.Lemma == "Milch");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that collection-only packages can attach existing imported words without carrying a duplicate anchor entry.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollectionOnlyPackage()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string wordsPackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-words-{Guid.NewGuid():N}.json");
        string collectionPackagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-collection-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(wordsPackagePath, CreatePackageWithCollectionWordKeysJson("a1-shopping-collection-only-words"));
            await File.WriteAllTextAsync(collectionPackagePath, CreateCollectionOnlyPackageJson("a1-shopping-collection-only-package"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult wordsResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(wordsPackagePath), CancellationToken.None);
            ImportContentPackageResult collectionResult = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(collectionPackagePath), CancellationToken.None);

            Assert.True(wordsResult.IsSuccess, string.Join(Environment.NewLine, wordsResult.Issues.Select(issue => issue.Message)));
            Assert.True(collectionResult.IsSuccess, string.Join(Environment.NewLine, collectionResult.Issues.Select(issue => issue.Message)));
            Assert.Equal(0, collectionResult.TotalEntries);
            Assert.Equal(0, collectionResult.ImportedEntries);

            IWordCollectionQueryService collectionQueryService = serviceProvider.GetRequiredService<IWordCollectionQueryService>();
            DarwinLingua.Catalog.Application.Models.WordCollectionDetailModel? collection = await collectionQueryService
                .GetPublishedCollectionBySlugAsync("a1-shopping-collection-only", "en", CancellationToken.None);

            Assert.NotNull(collection);
            Assert.Equal(2, collection!.Words.Count);
            Assert.Contains(collection.Words, word => word.Lemma == "Brot");
            Assert.Contains(collection.Words, word => word.Lemma == "Milch");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(wordsPackagePath))
            {
                File.Delete(wordsPackagePath);
            }

            if (File.Exists(collectionPackagePath))
            {
                File.Delete(collectionPackagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that valid Dialogue lessons from a content package are persisted with their nested content.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistDialogueLessons()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithDialogueJson("a1-Dialogue-import-test"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            DarwinLingua.Catalog.Domain.Entities.DialogueLesson lesson = Assert.Single(dbContext.DialogueLessons
                .Include(Dialogue => Dialogue.Topics)
                .Include(Dialogue => Dialogue.DialogueTurns).ThenInclude(turn => turn.Translations)
                .Include(Dialogue => Dialogue.UsefulPhrases).ThenInclude(phrase => phrase.Translations)
                .Include(Dialogue => Dialogue.Questions).ThenInclude(question => question.Translations)
                .Include(Dialogue => Dialogue.Questions).ThenInclude(question => question.Answers).ThenInclude(answer => answer.Translations));

            Assert.Equal("doctor-appointment-a1", lesson.Slug);
            Assert.Single(lesson.Topics);
            Assert.Equal(10, lesson.DialogueTurns.Count);
            Assert.Single(lesson.UsefulPhrases);
            DarwinLingua.Catalog.Domain.Entities.DialogueQuestion question = Assert.Single(lesson.Questions);
            Assert.Equal(2, question.Answers.Count);
            Assert.Contains(question.Answers, answer => answer.IsCorrect);

            IDialogueLessonQueryService DialogueLessonQueryService = serviceProvider.GetRequiredService<IDialogueLessonQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.DialogueLessonListItemModel> Dialogues =
                await DialogueLessonQueryService.GetPublishedDialoguesAsync(new DialogueLessonListFilterModel(null, null, null, null, null, null, null, null, null), CancellationToken.None);
            DarwinLingua.Catalog.Application.Models.DialogueLessonListItemModel DialogueListItem = Assert.Single(Dialogues);
            Assert.Equal("doctor-appointment-a1", DialogueListItem.Slug);

            DarwinLingua.Catalog.Application.Models.DialogueLessonDetailModel? DialogueDetail =
                await DialogueLessonQueryService.GetPublishedDialogueBySlugAsync(
                    "doctor-appointment-a1",
                    "fa",
                    "en",
                    CancellationToken.None);

            Assert.NotNull(DialogueDetail);
            DarwinLingua.Catalog.Application.Models.DialogueTurnModel firstLearnerTurn = DialogueDetail!.DialogueTurns
                .First(turn => turn.SpeakerRole == "learner");
            Assert.Equal("I need an appointment.", firstLearnerTurn.PrimaryMeaning);
            Assert.Equal("I need an appointment.", firstLearnerTurn.SecondaryMeaning);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that valid conversation starter packs are persisted and queryable with dual meaning-language fallback.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistConversationStarterPacks()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithConversationStarterJson("a1-starter-import-test"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            DarwinLingua.Catalog.Domain.Entities.ConversationStarterPack pack = Assert.Single(dbContext.ConversationStarterPacks
                .Include(starter => starter.Topics)
                .Include(starter => starter.LinkedDialogues)
                .Include(starter => starter.LinkedEventPreparationPacks)
                .Include(starter => starter.Phrases).ThenInclude(phrase => phrase.Translations)
                .Include(starter => starter.Phrases).ThenInclude(phrase => phrase.AlternativeBaseTexts));

            Assert.Equal("a1-cafe-first-meeting", pack.Slug);
            Assert.Single(pack.Topics);
            Assert.Equal("doctor-appointment-a1", Assert.Single(pack.LinkedDialogues).DialogueSlug);
            DarwinLingua.Catalog.Domain.Entities.ConversationStarterPhrase phrase = Assert.Single(pack.Phrases);
            Assert.Equal("opening", phrase.Function);
            Assert.Equal("Hallo, ich heisse Sara.", Assert.Single(phrase.AlternativeBaseTexts).BaseText);
            Assert.Equal(10, phrase.Translations.Count);

            IConversationStarterQueryService queryService = serviceProvider.GetRequiredService<IConversationStarterQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.ConversationStarterPackListItemModel> starterPacks =
                await queryService.GetPublishedStarterPacksAsync(
                    new DarwinLingua.Catalog.Application.Models.ConversationStarterListFilterModel("A1", "cafe", "friendly", "introduction", "everyday-life"),
                    CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.ConversationStarterPackListItemModel listItem = Assert.Single(starterPacks);
            Assert.Equal("a1-cafe-first-meeting", listItem.Slug);
            Assert.Equal(["doctor-appointment-a1"], listItem.LinkedDialogueSlugs);

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.ConversationStarterPackListItemModel> DialogueStarterPacks =
                await queryService.GetPublishedStarterPacksForDialogueAsync("doctor-appointment-a1", CancellationToken.None);
            Assert.Equal("a1-cafe-first-meeting", Assert.Single(DialogueStarterPacks).Slug);

            DarwinLingua.Catalog.Application.Models.ConversationStarterPackDetailModel? detail =
                await queryService.GetPublishedStarterPackBySlugAsync(
                    "a1-cafe-first-meeting",
                    "fa",
                    "en",
                    CancellationToken.None);

            Assert.NotNull(detail);
            DarwinLingua.Catalog.Application.Models.ConversationStarterPhraseModel detailPhrase = Assert.Single(detail!.Phrases);
            Assert.Equal("سلام، اسم من سارا است.", detailPhrase.PrimaryMeaning);
            Assert.Equal("Hello, my name is Sara.", detailPhrase.SecondaryMeaning);
            Assert.Equal(["Hallo, ich heisse Sara."], detailPhrase.AlternativeBaseTexts);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that valid event preparation packs are persisted with links and prompt groups.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistEventPreparationPacks()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            await File.WriteAllTextAsync(packagePath, CreatePackageWithEventPreparationJson("a1-event-preparation-import-test"));

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal(1, result.ImportedEntries);

            await using DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext dbContext = serviceProvider
                .GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<DarwinLingua.Infrastructure.Persistence.DarwinLinguaDbContext>>()
                .CreateDbContext();

            DarwinLingua.Catalog.Domain.Entities.EventPreparationPack pack = Assert.Single(dbContext.EventPreparationPacks
                .Include(item => item.Topics)
                .Include(item => item.LinkedDialogues)
                .Include(item => item.LinkedConversationStarterPacks)
                .Include(item => item.LinkedVocabulary)
                .Include(item => item.Prompts));

            Assert.Equal("a1-first-cafe-event", pack.Slug);
            Assert.Single(pack.Topics);
            Assert.Equal("cafe-first-meeting-a1", Assert.Single(pack.LinkedDialogues).DialogueSlug);
            Assert.Equal("a1-cafe-first-meeting", Assert.Single(pack.LinkedConversationStarterPacks).ConversationStarterPackSlug);
            Assert.Equal("Name", Assert.Single(pack.LinkedVocabulary).Word);
            Assert.Equal(3, pack.Prompts.Count);
            Assert.Contains(pack.Prompts, prompt => prompt.PromptType == "opening" && prompt.Text == "Say your name and ask for the other person's name.");
            Assert.Contains(pack.Prompts, prompt => prompt.PromptType == "roleplay" && prompt.Text == "Start a two-minute cafe introduction.");
            Assert.Contains(pack.Prompts, prompt => prompt.PromptType == "review" && prompt.Text == "Write one phrase you want to reuse.");

            IEventPreparationQueryService queryService = serviceProvider.GetRequiredService<IEventPreparationQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.EventPreparationPackListItemModel> eventPreparationPacks =
                await queryService.GetPublishedEventPreparationPacksAsync(
                    new DarwinLingua.Catalog.Application.Models.EventPreparationListFilterModel("A1", "social-event", "conversation-cafe", "everyday-life"),
                    CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.EventPreparationPackListItemModel listItem = Assert.Single(eventPreparationPacks);
            Assert.Equal("a1-first-cafe-event", listItem.Slug);
            Assert.Equal(["cafe-first-meeting-a1"], listItem.LinkedDialogueSlugs);
            Assert.Equal(["a1-cafe-first-meeting"], listItem.LinkedConversationStarterPackSlugs);

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.EventPreparationPackListItemModel> DialoguePreparationPacks =
                await queryService.GetPublishedEventPreparationPacksForDialogueAsync("cafe-first-meeting-a1", CancellationToken.None);
            Assert.Equal("a1-first-cafe-event", Assert.Single(DialoguePreparationPacks).Slug);

            DarwinLingua.Catalog.Application.Models.EventPreparationPackDetailModel? detail =
                await queryService.GetPublishedEventPreparationPackBySlugAsync("a1-first-cafe-event", CancellationToken.None);

            Assert.NotNull(detail);
            Assert.Equal("Name", Assert.Single(detail!.LinkedVocabulary).Word);
            Assert.Equal(3, detail.Prompts.Count);
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    /// <summary>
    /// Verifies that the Phase 1 sample package imports successfully into a freshly initialized database.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportPhase1SampleContentPackageIntoFreshDatabase()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-import-{Guid.NewGuid():N}.db");
        string packagePath = Path.Combine(Path.GetTempPath(), $"darwin-lingua-package-{Guid.NewGuid():N}.json");
        ServiceProvider? serviceProvider = null;

        try
        {
            File.Copy(GetSamplePackagePath(), packagePath, overwrite: true);

            serviceProvider = BuildServiceProvider(databasePath);

            IDatabaseInitializer databaseInitializer = serviceProvider.GetRequiredService<IDatabaseInitializer>();
            await databaseInitializer.InitializeAsync(CancellationToken.None);

            IContentImportService contentImportService = serviceProvider.GetRequiredService<IContentImportService>();
            ImportContentPackageResult result = await contentImportService
                .ImportAsync(new ImportContentPackageRequest(packagePath), CancellationToken.None);

            Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
            Assert.Equal("Completed", result.Status);
            Assert.Equal(12, result.TotalEntries);
            Assert.Equal(12, result.ImportedEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(0, result.WarningCount);
            Assert.Equal(12, result.ImportedLemmas.Count);
            Assert.Contains("Brot", result.ImportedLemmas);
            Assert.Contains("Unabdingbarkeit", result.ImportedLemmas);

            IWordQueryService wordQueryService = serviceProvider.GetRequiredService<IWordQueryService>();

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> shoppingWords = await wordQueryService
                .GetWordsByTopicAsync("shopping", "en", CancellationToken.None);
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> workWords = await wordQueryService
                .GetWordsByTopicAsync("work-and-jobs", "en", CancellationToken.None);
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> c2Words = await wordQueryService
                .GetWordsByCefrAsync("C2", "en", CancellationToken.None);

            DarwinLingua.Catalog.Application.Models.WordListItemModel breadWord = Assert.Single(shoppingWords);

            Assert.Equal("Brot", breadWord.Lemma);
            Assert.Equal("bread", breadWord.PrimaryMeaning);
            Assert.Contains(workWords, word => word.Lemma == "Bewerbung" && word.PrimaryMeaning == "job application");
            Assert.Equal(2, c2Words.Count);
            Assert.Contains(c2Words, word => word.Lemma == "Unabdingbarkeit");
            Assert.Contains(c2Words, word => word.Lemma == "niederschmettern");
        }
        finally
        {
            if (serviceProvider is not null)
            {
                await serviceProvider.DisposeAsync();
            }

            if (File.Exists(packagePath))
            {
                File.Delete(packagePath);
            }
        }
    }

    private static ServiceProvider BuildServiceProvider(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        ServiceCollection services = new();
        services
            .AddDarwinLinguaInfrastructure(options => options.DatabasePath = databasePath)
            .AddCatalogApplication()
            .AddCatalogInfrastructure()
            .AddContentOpsApplication()
            .AddContentOpsInfrastructure()
            .AddLocalizationApplication()
            .AddLocalizationInfrastructure();

        return services.BuildServiceProvider();
    }

    private static string GetSamplePackagePath()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string samplePackagePath = Path.Combine(
            repositoryRoot,
            "tests/Modules/ContentOps/DarwinLingua.ContentOps.Infrastructure.Tests/Fixtures/phase1-sample-content-package.json");

        Assert.True(File.Exists(samplePackagePath), $"Sample package fixture was not found: {samplePackagePath}");
        return samplePackagePath;
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }

    private static string NormalizePackageJson(string json)
    {
        string[] requiredMeaningLanguages = ["ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr"];
        string[] requiredLocalizationLanguages = ["de", .. requiredMeaningLanguages];
        JsonObject document = JsonNode.Parse(json)!.AsObject();

        document["defaultMeaningLanguages"] = new JsonArray(requiredMeaningLanguages.Select(language => JsonValue.Create(language)).ToArray<JsonNode?>());

        HashSet<string> labelKeys = new(StringComparer.OrdinalIgnoreCase);
        foreach (JsonObject entry in ReadObjects(document["entries"]))
        {
            foreach (string key in ReadStrings(entry["usageLabels"]))
            {
                labelKeys.Add(key);
            }

            foreach (string key in ReadStrings(entry["contextLabels"]))
            {
                labelKeys.Add(key);
            }

            FillMeaningTranslations(entry["meanings"], requiredMeaningLanguages, "text");
            foreach (JsonObject example in ReadObjects(entry["examples"]))
            {
                FillMeaningTranslations(example["translations"], requiredMeaningLanguages, "text");
            }
        }

        if (labelKeys.Count == 0)
        {
            labelKeys.Add("general");
        }

        document["labels"] = new JsonArray(labelKeys
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select((key, index) => CreateLabelDefinition(key, index, requiredLocalizationLanguages))
            .ToArray<JsonNode?>());

        foreach (JsonObject collection in ReadObjects(document["collections"]))
        {
            string name = collection["name"]?.GetValue<string>() ?? string.Empty;
            string? description = collection["description"]?.GetValue<string>();
            collection["localizations"] = new JsonArray(requiredLocalizationLanguages
                .Select(language => CreateLocalization(language, name, description))
                .ToArray<JsonNode?>());
        }

        foreach (JsonObject dialogue in ReadObjects(document["dialogues"]))
        {
            foreach (JsonObject turn in ReadObjects(dialogue["dialogueTurns"]))
            {
                FillMeaningTranslations(turn["translations"], requiredMeaningLanguages, "text");
            }

            foreach (JsonObject phrase in ReadObjects(dialogue["usefulPhrases"]))
            {
                FillMeaningTranslations(phrase["translations"], requiredMeaningLanguages, "text");
            }

            foreach (JsonObject question in ReadObjects(dialogue["questions"]))
            {
                FillMeaningTranslations(question["translations"], requiredMeaningLanguages, "text");
                foreach (JsonObject answer in ReadObjects(question["answers"]))
                {
                    FillMeaningTranslations(answer["translations"], requiredMeaningLanguages, "text");
                }
            }
        }

        foreach (JsonObject pack in ReadObjects(document["conversationStarterPacks"]))
        {
            foreach (JsonObject phrase in ReadObjects(pack["phrases"]))
            {
                FillMeaningTranslations(phrase["translations"], requiredMeaningLanguages, "text");
            }
        }

        return document.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonObject CreateLabelDefinition(string key, int index, IReadOnlyList<string> requiredLocalizationLanguages)
    {
        string displayName = string.Join(' ', key.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(part => string.Concat(char.ToUpperInvariant(part[0]), part[1..])));
        string kind = key is "formal" or "informal" or "written" ? "usage" : "context";

        return new JsonObject
        {
            ["kind"] = kind,
            ["key"] = key,
            ["displayName"] = displayName,
            ["localizations"] = new JsonArray(requiredLocalizationLanguages
                .Select(language => CreateLocalization(language, displayName, $"{displayName} label"))
                .ToArray<JsonNode?>()),
            ["sortOrder"] = (index + 1) * 10,
        };
    }

    private static JsonObject CreateLocalization(string language, string name, string? description) =>
        new()
        {
            ["language"] = language,
            ["name"] = name,
            ["description"] = description,
        };

    private static IEnumerable<JsonObject> ReadObjects(JsonNode? node) =>
        node is JsonArray array
            ? array.OfType<JsonObject>()
            : [];

    private static IEnumerable<string> ReadStrings(JsonNode? node) =>
        node is JsonArray array
            ? array.Select(item => item?.GetValue<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value!)
            : [];

    private static void FillMeaningTranslations(JsonNode? node, IReadOnlyList<string> requiredMeaningLanguages, string valuePropertyName)
    {
        if (node is not JsonArray translations || translations.Count == 0)
        {
            return;
        }

        Dictionary<string, string> existingTranslations = translations
            .OfType<JsonObject>()
            .Where(item => item["language"] is not null)
            .ToDictionary(
                item => item["language"]!.GetValue<string>().Trim().ToLowerInvariant(),
                item => item[valuePropertyName]?.GetValue<string>() ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);

        if (!existingTranslations.TryGetValue("en", out string? fallback) || string.IsNullOrWhiteSpace(fallback))
        {
            fallback = existingTranslations.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
        }

        if (string.IsNullOrWhiteSpace(fallback))
        {
            return;
        }

        translations.Clear();
        foreach (string language in requiredMeaningLanguages)
        {
            translations.Add(new JsonObject
            {
                ["language"] = language,
                [valuePropertyName] = existingTranslations.GetValueOrDefault(language, fallback),
            });
        }
    }

    private static string CreateValidPackageJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Brot",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "das",
                  "plural": "Brote",
                  "topics": ["shopping"],
                  "usageLabels": ["informal"],
                  "contextLabels": ["shopping"],
                  "grammarNotes": ["Plural form is mostly used when talking about different bread types."],
                  "collocations": [
                    {
                      "text": "frisches Brot kaufen",
                      "meaning": "to buy fresh bread"
                    }
                  ],
                  "wordFamilies": [
                    {
                      "lemma": "Bäcker",
                      "relationLabel": "Profession",
                      "note": "person who bakes or sells bread"
                    }
                  ],
                  "relations": [
                    {
                      "kind": "synonym",
                      "lemma": "Laib",
                      "note": "used for a loaf of bread"
                    },
                    {
                      "kind": "antonym",
                      "lemma": "Fasten",
                      "note": "going without food"
                    }
                  ],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "bread"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich kaufe Brot.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I buy bread."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithInvalidEntryJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Import Mixed Validity Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Milch",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "die",
                  "plural": "Milch",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "milk"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich brauche Milch.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I need milk."
                        }
                      ]
                    }
                  ]
                },
                {
                  "word": "Falscheintrag",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "topics": ["missing-topic"],
                  "meanings": [],
                  "examples": []
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithMultipleLexicalFormsJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Multi Lexical Forms Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Kasse",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "die",
                  "plural": "Kassen",
                  "pronunciationIpa": "/ˈkasə/",
                  "syllableBreak": "Kas-se",
                  "lexicalForms": [
                    {
                      "partOfSpeech": "Noun",
                      "article": "die",
                      "plural": "Kassen",
                      "isPrimary": true
                    },
                    {
                      "partOfSpeech": "Verb",
                      "infinitive": "Kasse machen"
                    }
                  ],
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "checkout"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich gehe zur Kasse.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I am going to the checkout."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithCollectionWordKeysJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Collection Word Keys Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Brot",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "das",
                  "plural": "Brote",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "bread"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich kaufe Brot.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I buy bread."
                        }
                      ]
                    }
                  ]
                },
                {
                  "word": "Milch",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "die",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "milk"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich trinke Milch.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I drink milk."
                        }
                      ]
                    }
                  ]
                }
              ],
              "collections": [
                {
                  "slug": "a1-shopping-word-keys",
                  "name": "A1 Shopping Word Keys",
                  "description": "Compact collection reference test.",
                  "image": "collections/a1-shopping-word-keys.png",
                  "wordKeys": ["Brot", "Milch"]
                }
              ]
            }
            """);
    }

    private static string CreateCollectionOnlyPackageJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Collection Only Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "labels": [],
              "entries": [],
              "collections": [
                {
                  "slug": "a1-shopping-collection-only",
                  "name": "A1 Shopping Collection Only",
                  "description": "Collection-only package reference test.",
                  "image": "collections/a1-shopping-collection-only.png",
                  "wordKeys": ["Brot", "Milch"]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithDialogueJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Dialogue Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Termin",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "topics": ["appointments-and-health"],
                  "meanings": [
                    { "language": "en", "text": "appointment" }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich brauche einen Termin.",
                      "translations": [
                        { "language": "en", "text": "I need an appointment." }
                      ]
                    }
                  ]
                }
              ],
              "dialogues": [
                {
                  "slug": "doctor-appointment-a1",
                  "title": "Doctor Appointment",
                  "description": "Prepare for a simple appointment conversation.",
                  "learnerGoal": "Ask for an appointment.",
                  "cefrLevel": "A1",
                  "category": "doctor-and-healthcare",
                  "topics": ["appointments-and-health"],
                  "examProfiles": ["goethe-a1", "telc-a1"],
                  "skillFocus": ["speaking", "phone-call", "exam-speaking"],
                  "taskType": "make-appointment",
                  "interactionMode": "phone",
                  "register": "formal",
                  "speakingFunctions": ["greet", "request", "confirm", "close-conversation"],
                  "estimatedPracticeMinutes": 10,
                  "difficultyNote": "Short A1 phone dialogue with polite appointment phrases.",
                  "examRelevance": "Useful for A1 appointment roleplay tasks.",
                  "sortOrder": 1,
                  "dialogueTurns": [
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Guten Morgen, Praxis Müller.",
                      "translations": [
                        { "language": "en", "text": "Good morning, Müller practice." }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Ich brauche einen Termin.",
                      "translations": [
                        { "language": "en", "text": "I need an appointment." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Haben Sie heute Schmerzen?",
                      "translations": [
                        { "language": "en", "text": "Do you have pain today?" }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Ja, mein Kopf tut weh.",
                      "translations": [
                        { "language": "en", "text": "Yes, my head hurts." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Morgen um neun Uhr ist ein Termin frei.",
                      "translations": [
                        { "language": "en", "text": "Tomorrow at nine o'clock an appointment is available." }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Morgen um neun Uhr passt gut.",
                      "translations": [
                        { "language": "en", "text": "Tomorrow at nine o'clock works well." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Wie ist Ihr Name?",
                      "translations": [
                        { "language": "en", "text": "What is your name?" }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Mein Name ist Sara Ali.",
                      "translations": [
                        { "language": "en", "text": "My name is Sara Ali." }
                      ]
                    },
                    {
                      "speakerRole": "receptionist",
                      "baseText": "Bitte bringen Sie Ihre Karte mit.",
                      "translations": [
                        { "language": "en", "text": "Please bring your card with you." }
                      ]
                    },
                    {
                      "speakerRole": "learner",
                      "baseText": "Danke, auf Wiederhören.",
                      "translations": [
                        { "language": "en", "text": "Thank you, goodbye on the phone." }
                      ]
                    }
                  ],
                  "usefulWords": [
                    { "lemma": "der Termin", "wordSlug": "der-termin", "cefrLevel": "A1", "sortOrder": 10 },
                    { "lemma": "die Praxis", "wordSlug": "die-praxis", "cefrLevel": "A1", "sortOrder": 20 },
                    { "lemma": "der Schmerz", "wordSlug": "der-schmerz", "cefrLevel": "A1", "sortOrder": 30 },
                    { "lemma": "morgen", "wordSlug": "morgen", "cefrLevel": "A1", "sortOrder": 40 },
                    { "lemma": "passen", "wordSlug": "passen", "cefrLevel": "A1", "sortOrder": 50 },
                    { "lemma": "der Name", "wordSlug": "der-name", "cefrLevel": "A1", "sortOrder": 60 },
                    { "lemma": "die Karte", "wordSlug": "die-karte", "cefrLevel": "A1", "sortOrder": 70 },
                    { "lemma": "mitbringen", "wordSlug": "mitbringen", "cefrLevel": "A1", "sortOrder": 80 },
                    { "lemma": "danke", "wordSlug": "danke", "cefrLevel": "A1", "sortOrder": 90 },
                    { "lemma": "auf Wiederhören", "wordSlug": "auf-wiederhoeren", "cefrLevel": "A1", "sortOrder": 100 }
                  ],
                  "speakingPrompts": [
                    {
                      "promptType": "speaking-prompt",
                      "prompt": "Rufen Sie in einer Praxis an und fragen Sie nach einem Termin.",
                      "sortOrder": 10,
                      "translations": [
                        { "language": "ar", "text": "Call a practice and ask for an appointment." },
                        { "language": "ckb", "text": "Call a practice and ask for an appointment." },
                        { "language": "en", "text": "Call a practice and ask for an appointment." },
                        { "language": "fa", "text": "با یک مطب تماس بگیرید و درخواست وقت ملاقات کنید." },
                        { "language": "kmr", "text": "Call a practice and ask for an appointment." },
                        { "language": "pl", "text": "Call a practice and ask for an appointment." },
                        { "language": "ro", "text": "Call a practice and ask for an appointment." },
                        { "language": "ru", "text": "Call a practice and ask for an appointment." },
                        { "language": "sq", "text": "Call a practice and ask for an appointment." },
                        { "language": "tr", "text": "Call a practice and ask for an appointment." }
                      ]
                    },
                    {
                      "promptType": "roleplay-task",
                      "prompt": "Sagen Sie Ihren Namen und bestätigen Sie die Uhrzeit.",
                      "sortOrder": 20,
                      "translations": [
                        { "language": "ar", "text": "Say your name and confirm the time." },
                        { "language": "ckb", "text": "Say your name and confirm the time." },
                        { "language": "en", "text": "Say your name and confirm the time." },
                        { "language": "fa", "text": "نام خود را بگویید و ساعت را تأیید کنید." },
                        { "language": "kmr", "text": "Say your name and confirm the time." },
                        { "language": "pl", "text": "Say your name and confirm the time." },
                        { "language": "ro", "text": "Say your name and confirm the time." },
                        { "language": "ru", "text": "Say your name and confirm the time." },
                        { "language": "sq", "text": "Say your name and confirm the time." },
                        { "language": "tr", "text": "Say your name and confirm the time." }
                      ]
                    }
                  ],
                  "usefulPhrases": [
                    {
                      "baseText": "Können Sie das bitte wiederholen?",
                      "usageNote": "Use when you did not understand.",
                      "translations": [
                        { "language": "en", "text": "Could you please repeat that?" }
                      ]
                    }
                  ],
                  "questions": [
                    {
                      "prompt": "Was braucht die Person?",
                      "translations": [
                        { "language": "en", "text": "What does the person need?" }
                      ],
                      "answers": [
                        {
                          "text": "Einen Termin.",
                          "isCorrect": true,
                          "feedback": "Correct.",
                          "translations": [
                            { "language": "en", "text": "An appointment." }
                          ]
                        },
                        {
                          "text": "Ein Brot.",
                          "isCorrect": false,
                          "feedback": "This belongs to shopping.",
                          "translations": [
                            { "language": "en", "text": "A bread." }
                          ]
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithConversationStarterJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Conversation Starter Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en", "fa"],
              "entries": [
                {
                  "word": "Name",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "topics": ["everyday-life"],
                  "meanings": [
                    { "language": "en", "text": "name" },
                    { "language": "fa", "text": "اسم" }
                  ],
                  "examples": [
                    {
                      "baseText": "Mein Name ist Sara.",
                      "translations": [
                        { "language": "en", "text": "My name is Sara." },
                        { "language": "fa", "text": "اسم من سارا است." }
                      ]
                    }
                  ]
                }
              ],
              "conversationStarterPacks": [
                {
                  "slug": "a1-cafe-first-meeting",
                  "title": "Cafe First Meeting",
                  "description": "Simple phrases for starting a friendly first conversation in a cafe.",
                  "cefrLevel": "A1",
                  "category": "first-meetings",
                  "situation": "cafe",
                  "tone": "friendly",
                  "conversationGoal": "introduction",
                  "topics": ["everyday-life"],
                  "sortOrder": 1,
                  "LinkedDialogueSlugs": ["doctor-appointment-a1"],
                  "linkedEventPreparationPackSlugs": ["a1-first-cafe-event"],
                  "phrases": [
                    {
                      "baseText": "Hallo, ich heisse Sara.",
                      "function": "opening",
                      "register": "neutral",
                      "usageNote": "Use this as a simple first introduction.",
                      "sortOrder": 1,
                      "alternativeBaseTexts": ["Hallo, ich heisse Sara."],
                      "commonMistake": "Do not omit ich in a full sentence.",
                      "translations": [
                        { "language": "en", "text": "Hello, my name is Sara." },
                        { "language": "fa", "text": "سلام، اسم من سارا است." }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithEventPreparationJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Event Preparation Import Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Name",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "topics": ["everyday-life"],
                  "meanings": [
                    { "language": "en", "text": "name" }
                  ],
                  "examples": [
                    {
                      "baseText": "Mein Name ist Sara.",
                      "translations": [
                        { "language": "en", "text": "My name is Sara." }
                      ]
                    }
                  ]
                }
              ],
              "eventPreparationPacks": [
                {
                  "slug": "a1-first-cafe-event",
                  "title": "First Cafe Event",
                  "description": "Prepare for a short first conversation at a cafe event.",
                  "cefrLevel": "A1",
                  "category": "social-event",
                  "eventType": "conversation-cafe",
                  "topics": ["everyday-life"],
                  "sortOrder": 1,
                  "LinkedDialogueSlugs": ["cafe-first-meeting-a1"],
                  "linkedVocabulary": [
                    { "word": "Name", "partOfSpeech": "Noun", "cefrLevel": "A1" }
                  ],
                  "linkedConversationStarterPackSlugs": ["a1-cafe-first-meeting"],
                  "openingPrompts": ["Say your name and ask for the other person's name."],
                  "roleplayPrompts": ["Start a two-minute cafe introduction."],
                  "reviewPrompts": ["Write one phrase you want to reuse."]
                }
              ]
            }
            """);
    }

    private static string CreatePackageWithDuplicateEntriesJson(string packageId)
    {
        return NormalizePackageJson($$"""
            {
              "packageVersion": "1.0",
              "packageId": "{{packageId}}",
              "packageName": "A1 Shopping Duplicate Entry Test",
              "source": "Hybrid",
              "defaultMeaningLanguages": ["en"],
              "entries": [
                {
                  "word": "Apfel",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "plural": "Äpfel",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "apple"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Der Apfel ist frisch.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "The apple is fresh."
                        }
                      ]
                    }
                  ]
                },
                {
                  "word": "Apfel",
                  "language": "de",
                  "cefrLevel": "A1",
                  "partOfSpeech": "Noun",
                  "article": "der",
                  "plural": "Äpfel",
                  "topics": ["shopping"],
                  "meanings": [
                    {
                      "language": "en",
                      "text": "apple"
                    }
                  ],
                  "examples": [
                    {
                      "baseText": "Ich esse einen Apfel.",
                      "translations": [
                        {
                          "language": "en",
                          "text": "I eat an apple."
                        }
                      ]
                    }
                  ]
                }
              ]
            }
            """);
    }
}



