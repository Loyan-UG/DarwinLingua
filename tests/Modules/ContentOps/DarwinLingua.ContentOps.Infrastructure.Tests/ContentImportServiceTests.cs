using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
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

            Assert.True(result.IsSuccess);
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

            Assert.True(result.IsSuccess);
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

            Assert.True(result.IsSuccess);
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

            Assert.True(result.IsSuccess);
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

            Assert.True(result.IsSuccess);
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

            Assert.True(result.IsSuccess);
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
            Assert.Single(lesson.DialogueTurns);
            Assert.Single(lesson.UsefulPhrases);
            DarwinLingua.Catalog.Domain.Entities.DialogueQuestion question = Assert.Single(lesson.Questions);
            Assert.Equal(2, question.Answers.Count);
            Assert.Contains(question.Answers, answer => answer.IsCorrect);

            IDialogueLessonQueryService DialogueLessonQueryService = serviceProvider.GetRequiredService<IDialogueLessonQueryService>();
            IReadOnlyList<DarwinLingua.Catalog.Application.Models.DialogueLessonListItemModel> Dialogues =
                await DialogueLessonQueryService.GetPublishedDialoguesAsync(CancellationToken.None);
            DarwinLingua.Catalog.Application.Models.DialogueLessonListItemModel DialogueListItem = Assert.Single(Dialogues);
            Assert.Equal("doctor-appointment-a1", DialogueListItem.Slug);

            DarwinLingua.Catalog.Application.Models.DialogueLessonDetailModel? DialogueDetail =
                await DialogueLessonQueryService.GetPublishedDialogueBySlugAsync(
                    "doctor-appointment-a1",
                    "fa",
                    "en",
                    CancellationToken.None);

            Assert.NotNull(DialogueDetail);
            Assert.Equal("I need an appointment.", Assert.Single(DialogueDetail!.DialogueTurns).PrimaryMeaning);
            Assert.Equal("I need an appointment.", Assert.Single(DialogueDetail.DialogueTurns).SecondaryMeaning);
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

            Assert.True(result.IsSuccess);
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
            Assert.Equal(2, phrase.Translations.Count);

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

            Assert.True(result.IsSuccess);
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

            Assert.True(result.IsSuccess);
            Assert.Equal("CompletedWithWarnings", result.Status);
            Assert.Equal(12, result.TotalEntries);
            Assert.Equal(12, result.ImportedEntries);
            Assert.Equal(0, result.SkippedDuplicateEntries);
            Assert.Equal(0, result.InvalidEntries);
            Assert.Equal(12, result.WarningCount);
            Assert.Contains(result.Issues, issue =>
                issue.Severity == "Warning" &&
                issue.Message.Contains("example", StringComparison.Ordinal) &&
                issue.Message.Contains("fa", StringComparison.Ordinal));
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

    private static string CreateValidPackageJson(string packageId)
    {
        return $$"""
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
            """;
    }

    private static string CreatePackageWithInvalidEntryJson(string packageId)
    {
        return $$"""
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
            """;
    }

    private static string CreatePackageWithMultipleLexicalFormsJson(string packageId)
    {
        return $$"""
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
            """;
    }

    private static string CreatePackageWithCollectionWordKeysJson(string packageId)
    {
        return $$"""
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
            """;
    }

    private static string CreatePackageWithDialogueJson(string packageId)
    {
        return $$"""
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
                  "sortOrder": 1,
                  "dialogueTurns": [
                    {
                      "speakerRole": "learner",
                      "baseText": "Ich brauche einen Termin.",
                      "translations": [
                        { "language": "en", "text": "I need an appointment." }
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
            """;
    }

    private static string CreatePackageWithConversationStarterJson(string packageId)
    {
        return $$"""
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
            """;
    }

    private static string CreatePackageWithEventPreparationJson(string packageId)
    {
        return $$"""
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
            """;
    }

    private static string CreatePackageWithDuplicateEntriesJson(string packageId)
    {
        return $$"""
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
            """;
    }
}
