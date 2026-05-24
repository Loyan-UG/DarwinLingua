using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.ContentOps.Application.Abstractions;
using DarwinLingua.ContentOps.Application.DependencyInjection;
using DarwinLingua.ContentOps.Application.Models;
using DarwinLingua.ContentOps.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.ContentOps.Application.Tests;

/// <summary>
/// Verifies main application-layer import use-case behavior with fake dependencies.
/// </summary>
public sealed class ContentImportServiceApplicationTests
{
    /// <summary>
    /// Verifies that file-read failures return a fatal failed result.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenFileReadThrows()
    {
        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new ThrowingFileReader(),
            new StubParser(_ => throw new InvalidOperationException("Parser should not be called.")),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("missing.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error");
    }

    /// <summary>
    /// Verifies that parser failures return a fatal failed result.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenParserThrowsInvalidData()
    {
        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("{ }"),
            new ThrowingParser(),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("broken.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("Invalid JSON package format", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that duplicate package identifiers are rejected before import processing.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenPackageIdAlreadyExists()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "duplicate-package",
            "Duplicate Package",
            "Hybrid",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository(packageExists: true));

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("duplicate.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("already exists", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a valid entry is imported and the result reflects one inserted entry.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldSucceed_WhenPackageHasValidEntry()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(1, result.TotalEntries);
        Assert.Equal(1, result.ImportedEntries);
        Assert.Equal(0, result.SkippedDuplicateEntries);
        Assert.Equal(0, result.InvalidEntries);
        Assert.Contains("Brot", result.ImportedLemmas);
    }

    /// <summary>
    /// Verifies that imports reject entries missing required meaning-language coverage.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldMarkEntryInvalid_WhenRequiredMeaningCoverageIsIncomplete()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ContentLanguageRequirements.RequiredMeaningLanguageCodes,
            [CreateEntryWithEnglishOnly("Brot", "shopping")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("CompletedWithWarnings", result.Status);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("Missing required meaning languages", StringComparison.Ordinal) &&
            issue.Message.Contains("fa", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that an entry is skipped when the word already exists in the repository.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldSkipEntry_WhenWordAlreadyExistsInRepository()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository(wordExists: true));

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.ImportedEntries);
        Assert.Equal(1, result.SkippedDuplicateEntries);
        Assert.Contains(result.Issues, issue => issue.Severity == "Warning");
    }

    /// <summary>
    /// Verifies that the second of two identical entries in the same package is skipped as a duplicate.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldSkipSecondEntry_WhenDuplicateLemmaInSamePackage()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping"), CreateValidEntry("Brot", "shopping")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.ImportedEntries);
        Assert.Equal(1, result.SkippedDuplicateEntries);
    }

    /// <summary>
    /// Verifies that an entry with an unrecognized CEFR level is marked as invalid.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldMarkEntryInvalid_WhenCefrLevelIsUnrecognized()
    {
        ParsedContentEntryModel badEntry = new ParsedContentEntryModel(
            "Brot",
            "de",
            "Z9",
            "Noun",
            [],
            ["shopping"],
            [],
            [],
            [],
            [],
            [],
            [],
            [new ParsedContentMeaningModel("en", "bread")],
            [new ParsedContentExampleModel("Ich kaufe Brot.", [new ParsedContentMeaningModel("en", "I buy bread.")])],
            "der",
            "Brote",
            null,
            null,
            null);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [badEntry],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.ImportedEntries);
        Assert.Equal(1, result.InvalidEntries);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("CEFR", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that an entry whose language is not German is marked as invalid.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldMarkEntryInvalid_WhenEntryLanguageIsNotGerman()
    {
        ParsedContentEntryModel badEntry = new ParsedContentEntryModel(
            "bread",
            "en",
            "A1",
            "Noun",
            [],
            ["shopping"],
            [],
            [],
            [],
            [],
            [],
            [],
            [new ParsedContentMeaningModel("en", "bread")],
            [new ParsedContentExampleModel("I buy bread.", [new ParsedContentMeaningModel("en", "I buy bread.")])],
            null,
            null,
            null,
            null,
            null);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [badEntry],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.InvalidEntries);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("language", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that an entry that references an unknown topic key is marked as invalid.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldMarkEntryInvalid_WhenTopicKeyIsUnknown()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "nonexistent-topic")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.InvalidEntries);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("topic", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that an entry with no meanings is marked as invalid.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldMarkEntryInvalid_WhenMeaningsAreEmpty()
    {
        ParsedContentEntryModel badEntry = new ParsedContentEntryModel(
            "Brot",
            "de",
            "A1",
            "Noun",
            [],
            ["shopping"],
            [],
            [],
            [],
            [],
            [],
            [],
            [],
            [new ParsedContentExampleModel("Ich kaufe Brot.", [new ParsedContentMeaningModel("en", "I buy bread.")])],
            "der",
            "Brote",
            null,
            null,
            null);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [badEntry],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.InvalidEntries);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("meanings", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that an entry with no examples is marked as invalid.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldMarkEntryInvalid_WhenExamplesAreEmpty()
    {
        ParsedContentEntryModel badEntry = new ParsedContentEntryModel(
            "Brot",
            "de",
            "A1",
            "Noun",
            [],
            ["shopping"],
            [],
            [],
            [],
            [],
            [],
            [],
            [new ParsedContentMeaningModel("en", "bread")],
            [],
            "der",
            "Brote",
            null,
            null,
            null);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [badEntry],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.InvalidEntries);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("examples", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that an unsupported package version causes a fatal failure.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenPackageVersionIsUnsupported()
    {
        ParsedContentPackageModel parsedPackage = new(
            "9.9",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("version", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that an empty entries list causes a fatal failure at the package level.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenPackageContainsNoEntries()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("content item", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that dialogue lessons are validated at import boundaries before persistence support is added.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenDialogueContractIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "dialogue-invalid-package",
            "Dialogue Invalid Package",
            "Hybrid",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            [])
        {
            Dialogues =
            [
                new ParsedDialogueLessonModel(
                    "doctor dialogue",
                    "Doctor Dialogue",
                    "Prepare for a doctor appointment.",
                    "Ask for an appointment.",
                    "A1",
                    "doctor-and-healthcare",
                    ["missing-topic"],
                    [],
                    [],
                    "ask-for-information",
                    "doctor-office",
                    "formal",
                    ["ask-question", "answer-question"],
                    15,
                    null,
                    null,
                    [],
                    [],
                    1,
                    [],
                    [],
                    [
                        new ParsedDialogueQuestionModel(
                            "Was braucht die Person?",
                            [new ParsedContentMeaningModel("en", "What does the person need?")],
                            [new ParsedDialogueAnswerModel("Einen Termin.", [new ParsedContentMeaningModel("en", "An appointment.")], true, null)])
                    ])
            ],
        };

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("dialogue-invalid.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("Dialogue", StringComparison.Ordinal) &&
            issue.Message.Contains("kebab-case", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("missing-topic", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that conversation starter packs are validated at import boundaries before persistence support is added.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenConversationStarterContractIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "starter-invalid-package",
            "Starter Invalid Package",
            "Hybrid",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            [])
        {
            ConversationStarterPacks =
            [
                new ParsedConversationStarterPackModel(
                    "bad starter",
                    "Bad Starter",
                    "Invalid starter content.",
                    "A1",
                    "first-meetings",
                    "cafe",
                    "friendly",
                    "introduction",
                    ["missing-topic"],
                    1,
                    ["bad-dialogue"],
                    [],
                    [
                        new ParsedConversationStarterPhraseModel(
                            "Hallo.",
                            "opening phrase",
                            [new ParsedContentMeaningModel("en", "Hello.")],
                            null,
                            "neutral",
                            1,
                            [],
                            null)
                    ])
            ],
        };

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("starter-invalid.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("Conversation starter pack", StringComparison.Ordinal) &&
            issue.Message.Contains("kebab-case", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("missing-topic", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that event preparation packs are validated at import boundaries before persistence support is added.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenEventPreparationContractIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "event-preparation-invalid-package",
            "Event Preparation Invalid Package",
            "Hybrid",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            [])
        {
            EventPreparationPacks =
            [
                new ParsedEventPreparationPackModel(
                    "bad event",
                    "",
                    "",
                    "Z9",
                    "social event",
                    "",
                    ["missing-topic"],
                    1,
                    ["bad-dialogue"],
                    [new ParsedEventPreparationVocabularyReferenceModel("", "UnknownPart", "Z9")],
                    ["bad starter"],
                    [""],
                    [""],
                    [""])
            ],
        };

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("event-preparation-invalid.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("Event preparation pack", StringComparison.Ordinal) &&
            issue.Message.Contains("kebab-case", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("missing-topic", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("linkedVocabulary", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects A1 articles shorter than the character target range.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenA1TalkTopicArticleIsShorterThanRange()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with
            {
                Article = new ParsedTalkTopicArticleModel(
                    CreateGermanArticle(899),
                    []),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-too-short.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("article.baseText", StringComparison.Ordinal) &&
            issue.Message.Contains("900-1100", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects A1 articles longer than the character target range.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenA1TalkTopicArticleIsLongerThanRange()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with
            {
                Article = new ParsedTalkTopicArticleModel(CreateGermanArticle(1101), []),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-too-long.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("900-1100", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation accepts an A1 article near the target length.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldAccept_WhenA1TalkTopicArticleIsInRange()
    {
        FakeRepository repository = new();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with
            {
                Article = new ParsedTalkTopicArticleModel(CreateGermanArticle(1000), []),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-a1-valid.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(repository.ImportedTalkTopics);
    }

    /// <summary>
    /// Verifies that Talk Topic import validation accepts a B2 article near the target length.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldAccept_WhenB2TalkTopicArticleIsInRange()
    {
        FakeRepository repository = new();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic("B2") with
            {
                Article = new ParsedTalkTopicArticleModel(CreateGermanArticle(2500), []),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-b2-valid.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(repository.ImportedTalkTopics);
    }

    /// <summary>
    /// Verifies that a B2 Talk Topic cannot use an A1-sized article.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenB2TalkTopicArticleUsesA1Length()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic("B2") with
            {
                Article = new ParsedTalkTopicArticleModel(CreateGermanArticle(1000), []),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-b2-too-short.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("2400-2600", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects unknown content types.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenTalkTopicContentTypeIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with { ContentType = "worksheet" });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-invalid-content-type.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("contentType 'worksheet'", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects article translations.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenTalkTopicArticleHasTranslations()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with
            {
                Article = new ParsedTalkTopicArticleModel(
                    CreateGermanArticle(1000),
                    [new ParsedContentMeaningModel("en", "Translation is not allowed.")]),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-article-translation.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("article.translations is not supported", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects warm-up and discussion question translations.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenTalkTopicQuestionsHaveTranslations()
    {
        ParsedTalkTopicModel validTopic = CreateValidTalkTopic();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            validTopic with
            {
                WarmupQuestions =
                [
                    new ParsedTalkTopicQuestionModel(
                        "Schaust du gern in den Nachthimmel?",
                        [new ParsedContentMeaningModel("en", "Do you like looking at the night sky?")],
                        10),
                    .. validTopic.WarmupQuestions.Skip(1),
                ],
                DiscussionQuestions =
                [
                    new ParsedTalkTopicDiscussionQuestionModel(
                        "Was denkst du über dieses Thema?",
                        "opinion",
                        [new ParsedContentMeaningModel("en", "What do you think about this topic?")],
                        10),
                    .. validTopic.DiscussionQuestions.Skip(1),
                ],
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-question-translations.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("warmupQuestions[1].translations is not supported", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("discussionQuestions[1].translations is not supported", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects unknown discussion question types.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenTalkTopicQuestionTypeIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with
            {
                DiscussionQuestions =
                [
                    new ParsedTalkTopicDiscussionQuestionModel(
                        "Was denkst du?",
                        "grammar-drill",
                        [],
                        10),
                ],
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-invalid-question-type.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("questionType is invalid", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation requires the configured discussion question count per type.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenDiscussionQuestionTypeCountIsTooLow()
    {
        ParsedTalkTopicModel validTopic = CreateValidTalkTopic();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            validTopic with
            {
                DiscussionQuestions = validTopic.DiscussionQuestions
                    .Where(question => question.QuestionType != "comparison")
                    .ToArray(),
            });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-missing-question-type.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("at least 2 'comparison' questions", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that B2 and higher Talk Topics require three discussion questions per configured type.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenB2DiscussionQuestionTypeCountIsTooLow()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(CreateValidTalkTopic("B2", questionsPerType: 2));

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-b2-question-count.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("at least 3 'opinion' questions", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation requires enough warm-up questions.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenWarmupQuestionCountIsTooLow()
    {
        ParsedTalkTopicModel validTopic = CreateValidTalkTopic();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            validTopic with { WarmupQuestions = validTopic.WarmupQuestions.Take(2).ToArray() });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-warmup-count.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("warmupQuestions must contain at least 3", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation enforces vocabulary count ranges by CEFR level.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenVocabularyCountIsOutsideCefrRange()
    {
        ParsedTalkTopicModel validTopic = CreateValidTalkTopic();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            validTopic with { VocabularyItems = validTopic.VocabularyItems.Take(11).ToArray() });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-vocabulary-count.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("vocabularyItems must contain 12-18", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that Talk Topic import validation rejects unknown speaking goals.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenTalkTopicSpeakingGoalIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with { SpeakingGoals = ["memorize-grammar"] });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-invalid-speaking-goal.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == "Error" &&
            issue.Message.Contains("speakingGoal 'memorize-grammar' is invalid", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that valid Talk Topics are mapped to domain aggregates for persistence.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldPersistTalkTopicFields()
    {
        FakeRepository repository = new();
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(CreateValidTalkTopic());

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("talk-topic-valid.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        TalkTopic topic = Assert.Single(repository.ImportedTalkTopics);
        Assert.Equal("a1-aliens", topic.Slug);
        Assert.Equal("do-aliens-exist", topic.TopicGroupKey);
        Assert.Equal(TalkTopicContentType.Article, topic.ContentType);
        Assert.Equal(CefrLevel.A1, topic.CefrLevel);
        Assert.Equal("science", topic.Category);
        Assert.False(topic.IsSensitive);
        Assert.Equal(5, topic.EstimatedReadingMinutes);
        Assert.Equal(20, topic.EstimatedDiscussionMinutes);
        Assert.Empty(topic.ArticleTranslations);
        Assert.Equal(3, topic.WarmupQuestions.Count);
        Assert.Equal(8, topic.DiscussionQuestions.Count);
        Assert.Equal(12, topic.VocabularyItems.Count);
        Assert.Contains(topic.SpeakingGoals, goal => goal.SpeakingGoal == TalkTopicSpeakingGoal.ExpressOpinion);
    }

    /// <summary>
    /// Verifies that a collection whose words all resolve from the newly imported entries is created successfully.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollection_WhenWordsResolveFromImportedEntries()
    {
        ParsedContentCollectionWordReferenceModel wordRef = new("Brot", null, null);
        ParsedContentCollectionModel collection = new("food-basics", "Food Basics", null, null, 1, [wordRef]);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            [collection]);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.ImportedEntries);
        Assert.DoesNotContain(result.Issues, issue => issue.Severity == "Error");
    }

    /// <summary>
    /// Verifies that a collection referencing a word that cannot be resolved records an error issue.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldRecordError_WhenCollectionWordCannotBeResolved()
    {
        ParsedContentCollectionWordReferenceModel wordRef = new("UnknownWort", null, null);
        ParsedContentCollectionModel collection = new("unknown-words", "Unknown Words", null, null, 1, [wordRef]);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            [collection]);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("UnknownWort", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that a package with a collection that has a missing slug fails at the package-validation stage.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenPackageCollectionHasMissingSlug()
    {
        ParsedContentCollectionWordReferenceModel wordRef = new("Brot", null, null);
        ParsedContentCollectionModel collection = new("   ", "Food Basics", null, null, 1, [wordRef]);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Brot", "shopping")],
            [collection]);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Issues, issue => issue.Severity == "Error" && issue.Message.Contains("slug", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that multiple valid entries in one package are all imported and all lemmas are present in the result.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportAllEntries_WhenPackageHasMultipleValidEntries()
    {
        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [
                CreateValidEntry("Brot", "shopping"),
                CreateValidEntry("Wasser", "shopping"),
            ],
            []);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Completed", result.Status);
        Assert.Equal(2, result.ImportedEntries);
        Assert.Contains("Brot", result.ImportedLemmas);
        Assert.Contains("Wasser", result.ImportedLemmas);
    }

    /// <summary>
    /// Verifies that a collection referencing an existing word resolved from the repository is imported successfully.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollection_WhenWordResolvesFromExistingRepositoryEntry()
    {
        WordEntry existingWord = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Brot",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            DateTime.UtcNow,
            article: "der");

        ParsedContentCollectionWordReferenceModel wordRef = new("Brot", null, null);
        ParsedContentCollectionModel collection = new("food-basics", "Food Basics", null, null, 1, [wordRef]);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-package",
            "Test Package",
            "Manual",
            ["en"],
            [CreateValidEntry("Wasser", "shopping")],
            [collection]);

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository(existingWords: [existingWord]));

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(result.Issues, issue => issue.Severity == "Error");
    }

    /// <summary>
    /// Verifies that a package can update collections without carrying a duplicate vocabulary anchor entry.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldImportCollection_WhenPackageHasNoEntries()
    {
        WordEntry existingWord = new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Brot",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            DateTime.UtcNow,
            article: "der");

        ParsedContentCollectionWordReferenceModel wordRef = new("Brot", "Noun", "A1");
        ParsedContentCollectionModel collection = new("food-basics", "Food Basics", null, null, 1, [wordRef]);

        ParsedContentPackageModel parsedPackage = new(
            "1.0",
            "test-collection-package",
            "Test Collection Package",
            "Manual",
            ["en"],
            [],
            [],
            [collection]);

        FakeRepository repository = new(existingWords: [existingWord]);
        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("test.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.TotalEntries);
        Assert.Single(repository.ImportedCollections);
        Assert.DoesNotContain(result.Issues, issue => issue.Severity == "Error");
    }

    [Fact]
    public async Task ImportAsync_ShouldImportExpressionEntry_WhenContractIsValid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithExpression(CreateValidExpression());
        FakeRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("expressions.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        ExpressionEntry expression = Assert.Single(repository.ImportedExpressions);
        Assert.Equal("a2-alles-klar", expression.Slug);
    }

    [Fact]
    public async Task ImportAsync_ShouldImportExpressionEntry_WhenAllPilotLanguagesArePresent()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithExpression(CreatePilotStyleExpression());
        FakeRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("expressions-pilot.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        ExpressionEntry expression = Assert.Single(repository.ImportedExpressions);
        Assert.Equal("koennten-sie-mir-bitte-helfen", expression.Slug);
        Assert.Equal(10, expression.Meanings.Count);
        Assert.Equal(10, Assert.Single(expression.Examples).Translations.Count);
        Assert.Equal(10, Assert.Single(expression.Warnings).Translations.Count);
    }

    [Theory]
    [InlineData("bad-type", "neutral", "Expression expressionType 'bad-type' is not supported.")]
    [InlineData("fixed-expression", "bad-register", "Expression register 'bad-register' is not supported.")]
    public async Task ImportAsync_ShouldRejectExpressionEntry_WhenControlledValueIsInvalid(
        string expressionType,
        string register,
        string expectedMessage)
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithExpression(
            CreateValidExpression() with { ExpressionType = expressionType, Register = register });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("expressions.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue => issue.Message.Contains(expectedMessage, StringComparison.Ordinal));
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectExpressionEntry_WhenRiskyExpressionMissingWarning()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithExpression(
            CreateValidExpression() with { Register = "rude", IsRisky = true, Warnings = [] });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("expressions.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("Risky expressions require at least one warning with text.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ImportAsync_ShouldImportCulturalNote_WhenContractIsValid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithCulturalNote(CreateValidCulturalNote());
        FakeRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("cultural-notes.json"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        CulturalNote note = Assert.Single(repository.ImportedCulturalNotes);
        Assert.Equal("a2-du-vs-sie-at-work", note.Slug);
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectCulturalNote_WhenCategoryIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithCulturalNote(
            CreateValidCulturalNote() with { Category = "bad-category" });

        await using ServiceProvider serviceProvider = BuildServiceProvider(
            new StubFileReader("ignored"),
            new StubParser(_ => parsedPackage),
            new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(
            new ImportContentPackageRequest("cultural-notes.json"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("Cultural note category 'bad-category' is not supported.", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ImportAsync_ShouldImportExamPrep_WhenContractIsValid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithExamPrep(CreateValidExamProfile(), CreateValidExamPrepUnit());
        FakeRepository repository = new();
        await using ServiceProvider serviceProvider = BuildServiceProvider(new StubFileReader("ignored"), new StubParser(_ => parsedPackage), repository);

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(new ImportContentPackageRequest("exam-prep.json"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("goethe-a2", Assert.Single(repository.ImportedExamProfiles).Key);
        Assert.Equal("a2-goethe-speaking-roleplay", Assert.Single(repository.ImportedExamPrepUnits).Slug);
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectExamPrep_WhenProfileIsInvalid()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithExamPrep(
            CreateValidExamProfile() with { Key = "bad-profile" },
            CreateValidExamPrepUnit() with { ExamProfileKey = "bad-profile" });
        await using ServiceProvider serviceProvider = BuildServiceProvider(new StubFileReader("ignored"), new StubParser(_ => parsedPackage), new FakeRepository());

        IContentImportService service = serviceProvider.GetRequiredService<IContentImportService>();

        ImportContentPackageResult result = await service.ImportAsync(new ImportContentPackageRequest("exam-prep.json"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Issues, issue => issue.Message.Contains("Exam profile key 'bad-profile' is not supported.", StringComparison.Ordinal));
    }

    private static ServiceProvider BuildServiceProvider(
        IContentImportFileReader fileReader,
        IContentImportParser parser,
        IContentImportRepository repository)
    {
        ServiceCollection services = new();
        services.AddContentOpsApplication();
        services.AddSingleton(fileReader);
        services.AddSingleton(parser);
        services.AddSingleton(repository);

        return services.BuildServiceProvider();
    }

    private static ParsedContentPackageModel CreatePackageWithExpression(ParsedExpressionEntryModel expression)
    {
        return new ParsedContentPackageModel(
            "1.0",
            "expression-test-package",
            "Expression Test Package",
            "Manual",
            ["en"],
            [],
            [])
        {
            ExpressionEntries = [expression],
        };
    }

    private static ParsedContentPackageModel CreatePackageWithCulturalNote(ParsedCulturalNoteModel note)
    {
        return new ParsedContentPackageModel(
            "1.0",
            "cultural-note-test-package",
            "Cultural Note Test Package",
            "Manual",
            ["en"],
            [],
            [])
        {
            CulturalNotes = [note],
        };
    }

    private static ParsedContentPackageModel CreatePackageWithExamPrep(ParsedExamProfileModel profile, ParsedExamPrepUnitModel unit)
    {
        return new ParsedContentPackageModel("1.0", "exam-prep-test-package", "Exam Prep Test Package", "Manual", ["en"], [], [])
        {
            ExamProfiles = [profile],
            ExamPrepUnits = [unit],
        };
    }

    private static ParsedExpressionEntryModel CreateValidExpression()
    {
        return new ParsedExpressionEntryModel(
            "a2-alles-klar",
            "Alles klar.",
            "Everything clear.",
            "All good or understood.",
            "Used to confirm understanding.",
            "A2",
            "fixed-expression",
            "neutral",
            "daily-life",
            "de",
            false,
            ["shopping"],
            true,
            10,
            [new ParsedExpressionMeaningModel("en", "All good or understood.", "Everything clear.", "A neutral confirmation phrase.")],
            [new ParsedExpressionExampleModel("Alles klar, wir treffen uns um acht.", null, [new ParsedContentMeaningModel("en", "All good, we meet at eight.")], 10)],
            [new ParsedExpressionWarningModel("tone", "Avoid in very formal letters.", [new ParsedContentMeaningModel("en", "Avoid in very formal letters.")])],
            [new ParsedExpressionLinkedWordModel("klar", "klar", 10)],
            ["verstanden"],
            ["a2-confirmation-phrases"]);
    }

    private static ParsedExpressionEntryModel CreatePilotStyleExpression()
    {
        string[] languages = ["en", "fa", "ar", "tr", "ru", "ckb", "kmr", "pl", "ro", "sq"];
        Dictionary<string, string> meanings = new(StringComparer.Ordinal)
        {
            ["en"] = "A polite request for help.",
            ["fa"] = "درخواست کمک به شکل مؤدبانه است.",
            ["ar"] = "طلب مساعدة بصيغة مهذبة.",
            ["tr"] = "Kibar bir yardım isteme cümlesidir.",
            ["ru"] = "Вежливая просьба о помощи.",
            ["ckb"] = "داواکارییەکی ڕێزدارانەی یارمەتییە.",
            ["kmr"] = "Ev daxwazeke bi rêz ji bo alîkariyê ye.",
            ["pl"] = "To uprzejma prośba o pomoc.",
            ["ro"] = "Este o cerere politicoasă de ajutor.",
            ["sq"] = "Është një kërkesë e sjellshme për ndihmë."
        };
        Dictionary<string, string> usages = new(StringComparer.Ordinal)
        {
            ["en"] = "Use it in simple formal situations.",
            ["fa"] = "در موقعیت‌های رسمی ساده استفاده می‌شود.",
            ["ar"] = "تُستخدم في مواقف رسمية بسيطة.",
            ["tr"] = "Basit resmi durumlarda kullanılır.",
            ["ru"] = "Используется в простых формальных ситуациях.",
            ["ckb"] = "لە دۆخی فەرمی سادەدا بەکاری دەهێنرێت.",
            ["kmr"] = "Di rewşên fermî yên hêsan de tê bikaranîn.",
            ["pl"] = "Używa się w prostych sytuacjach formalnych.",
            ["ro"] = "Se folosește în situații formale simple.",
            ["sq"] = "Përdoret në situata të thjeshta formale."
        };
        Dictionary<string, string> examples = new(StringComparer.Ordinal)
        {
            ["en"] = "Excuse me, could you please help me?",
            ["fa"] = "ببخشید، ممکن است لطفاً به من کمک کنید؟",
            ["ar"] = "عذرًا، هل يمكنكم مساعدتي من فضلكم؟",
            ["tr"] = "Affedersiniz, bana lütfen yardım edebilir misiniz?",
            ["ru"] = "Извините, не могли бы вы мне помочь?",
            ["ckb"] = "ببورە، دەتوانن تکایە یارمەتیم بدەن؟",
            ["kmr"] = "Bibore, hûn dikarin ji kerema xwe alîkariya min bikin?",
            ["pl"] = "Przepraszam, czy mogliby mi państwo pomóc?",
            ["ro"] = "Scuzați-mă, m-ați putea ajuta, vă rog?",
            ["sq"] = "Më falni, a mund të më ndihmoni ju lutem?"
        };
        Dictionary<string, string> warnings = new(StringComparer.Ordinal)
        {
            ["en"] = "Use formal address with Sie.",
            ["fa"] = "در این ساختار از خطاب رسمی Sie استفاده می‌شود.",
            ["ar"] = "تُستخدم هنا صيغة المخاطبة الرسمية Sie.",
            ["tr"] = "Bu yapıda resmi Sie hitabı kullanılır.",
            ["ru"] = "В этой конструкции используется формальное обращение Sie.",
            ["ckb"] = "لەم پێکهاتەیەدا بانگکردنی فەرمی Sie بەکار دێت.",
            ["kmr"] = "Di vê avahiyê de bangkirina fermî Sie tê bikaranîn.",
            ["pl"] = "W tej strukturze używa się formalnego zwrotu Sie.",
            ["ro"] = "În această structură se folosește adresarea formală Sie.",
            ["sq"] = "Në këtë strukturë përdoret forma zyrtare Sie."
        };

        return new ParsedExpressionEntryModel(
            "koennten-sie-mir-bitte-helfen",
            "Könnten Sie mir bitte helfen?",
            null,
            "A polite request for help.",
            "Use it with staff, offices, doctors, teachers, or strangers.",
            "A2",
            "polite-formula",
            "formal",
            "asking-for-help",
            "de",
            false,
            ["shopping"],
            true,
            10,
            languages.Select(language => new ParsedExpressionMeaningModel(
                language,
                meanings[language],
                null,
                usages[language])).ToArray(),
            [
                new ParsedExpressionExampleModel(
                    "Entschuldigung, könnten Sie mir bitte helfen?",
                    null,
                    languages.Select(language => new ParsedContentMeaningModel(
                        language,
                        examples[language])).ToArray(),
                    10)
            ],
            [
                new ParsedExpressionWarningModel(
                    "tone",
                    "Use formal address with Sie.",
                    languages.Select(language => new ParsedContentMeaningModel(
                        language,
                        warnings[language])).ToArray())
            ],
            [new ParsedExpressionLinkedWordModel("helfen", null, 10)],
            [],
            []);
    }

    private static ParsedCulturalNoteModel CreateValidCulturalNote()
    {
        return new ParsedCulturalNoteModel(
            "a2-du-vs-sie-at-work",
            "Du vs. Sie at work",
            "A practical note about address forms.",
            "A2",
            "du-vs-sie",
            "Workplace introductions",
            ["Use Sie until a colleague offers du."],
            [new ParsedCulturalNoteExampleModel("Sollen wir uns duzen?", "A polite question about switching to du.")],
            ["Start with Sie in formal settings."],
            ["Do not switch to du automatically."],
            "Address forms can feel personal in hierarchical contexts.",
            ["a2-workplace-introduction"],
            ["sollen-wir-uns-duzen"],
            ["a2-formal-work-email"],
            ["a2-workplace-small-talk"],
            ["a2-workplace-communication"],
            true,
            10);
    }

    private static ParsedExamProfileModel CreateValidExamProfile()
    {
        return new ParsedExamProfileModel("goethe-a2", "Goethe A2", "A2", "Goethe A2 preparation.", true, 10);
    }

    private static ParsedExamPrepUnitModel CreateValidExamPrepUnit()
    {
        return new ParsedExamPrepUnitModel(
            "a2-goethe-speaking-roleplay",
            "goethe-a2",
            "Speaking roleplay strategy",
            "Prepare a short A2 roleplay.",
            "A2",
            "speaking",
            "roleplay",
            "exam-preparation",
            "Use short, clear questions and answers.",
            ["Ask for clarification when needed."],
            ["Answer the prompt directly."],
            ["a2-appointment-roleplay"],
            ["a2-appointments"],
            ["a2-question-word-order"],
            ["koennten-sie-bitte"],
            ["a2-appointment-reschedule"],
            ["a2-speaking-roleplay-practice"],
            ["a2-appointments-lesson"],
            true,
            20);
    }

    private static ParsedContentEntryModel CreateValidEntry(string word, string topicKey)
    {
        ParsedContentMeaningModel[] meanings = ContentLanguageRequirements.RequiredMeaningLanguageCodes
            .Select(languageCode => new ParsedContentMeaningModel(languageCode, $"meaning {languageCode}"))
            .ToArray();

        ParsedContentMeaningModel[] exampleTranslations = ContentLanguageRequirements.RequiredMeaningLanguageCodes
            .Select(languageCode => new ParsedContentMeaningModel(languageCode, $"example {languageCode}"))
            .ToArray();

        return new ParsedContentEntryModel(
            word,
            "de",
            "A1",
            "Noun",
            [],
            [topicKey],
            [],
            [],
            [],
            [],
            [],
            [],
            meanings,
            [new ParsedContentExampleModel("Ich kaufe Brot.", exampleTranslations)],
            "der",
            "Brote",
            null,
            null,
            null);
    }

    private static ParsedContentEntryModel CreateEntryWithEnglishOnly(string word, string topicKey)
    {
        return new ParsedContentEntryModel(
            word,
            "de",
            "A1",
            "Noun",
            [],
            [topicKey],
            [],
            [],
            [],
            [],
            [],
            [],
            [new ParsedContentMeaningModel("en", "bread")],
            [new ParsedContentExampleModel("Ich kaufe Brot.", [new ParsedContentMeaningModel("en", "I buy bread.")])],
            "der",
            "Brote",
            null,
            null,
            null);
    }

    private static ParsedContentPackageModel CreatePackageWithTalkTopic(ParsedTalkTopicModel talkTopic)
    {
        return new ParsedContentPackageModel(
            "1.0",
            "talk-topic-test-package",
            "Talk Topic Test Package",
            "Manual",
            ContentLanguageRequirements.RequiredMeaningLanguageCodes,
            [CreateStrictValidEntry("Welt", "shopping")],
            [])
        {
            TalkTopics = [talkTopic],
        };
    }

    private static ParsedContentEntryModel CreateStrictValidEntry(string word, string topicKey)
    {
        ParsedContentMeaningModel[] meanings = ContentLanguageRequirements.RequiredMeaningLanguageCodes
            .Select(languageCode => new ParsedContentMeaningModel(languageCode, $"meaning {languageCode}"))
            .ToArray();

        ParsedContentMeaningModel[] exampleTranslations = ContentLanguageRequirements.RequiredMeaningLanguageCodes
            .Select(languageCode => new ParsedContentMeaningModel(languageCode, $"example {languageCode}"))
            .ToArray();

        return new ParsedContentEntryModel(
            word,
            "de",
            "A1",
            "Noun",
            [],
            [topicKey],
            [],
            [],
            [],
            [],
            [],
            [],
            meanings,
            [new ParsedContentExampleModel("Ich sehe die Welt.", exampleTranslations)],
            "die",
            null,
            null,
            null,
            null);
    }

    private static ParsedTalkTopicModel CreateValidTalkTopic(string cefrLevel = "A1", int? questionsPerType = null)
    {
        bool upperIntermediateOrHigher = cefrLevel is "B2" or "C1" or "C2";
        int articleLength = cefrLevel switch
        {
            "A2" => 1500,
            "B1" => 2000,
            "B2" => 2500,
            "C1" => 3000,
            "C2" => 3500,
            _ => 1000,
        };
        int vocabularyCount = cefrLevel switch
        {
            "A2" => 15,
            "B1" => 18,
            "B2" => 22,
            "C1" => 26,
            "C2" => 30,
            _ => 12,
        };
        int effectiveQuestionsPerType = questionsPerType ?? (upperIntermediateOrHigher ? 3 : 2);

        return new ParsedTalkTopicModel(
            "a1-aliens",
            "do-aliens-exist",
            "Gibt es Ausserirdische?",
            "A simple Talk Topic about aliens and life in space.",
            cefrLevel,
            "science",
            ["shopping"],
            "article",
            new ParsedTalkTopicArticleModel(
                CreateGermanArticle(articleLength),
                []),
            CreateTalkTopicWarmupQuestions(upperIntermediateOrHigher ? 4 : 3),
            CreateTalkTopicDiscussionQuestions(effectiveQuestionsPerType),
            CreateTalkTopicVocabularyItems(vocabularyCount, cefrLevel),
            ["express-opinion", "give-reasons", "imagine-possibilities"],
            5,
            20,
            false,
            null,
            false,
            10,
            true);
    }

    private static string CreateGermanArticle(int targetLength)
    {
        const string Seed = "Viele Menschen sprechen gern ueber das Weltall. Sie sehen Sterne, Planeten und den Mond. Sie fragen nach Leben, Forschung, Kontakt und Zukunft. In der Gruppe sagt jede Person ihre Meinung, nennt Gruende und stellt freundliche Fragen. ";
        string text = string.Concat(Enumerable.Repeat(Seed, (targetLength / Seed.Length) + 2));
        return text[..targetLength];
    }

    private static ParsedTalkTopicQuestionModel[] CreateTalkTopicWarmupQuestions(int count) =>
        Enumerable.Range(1, count)
            .Select(index => new ParsedTalkTopicQuestionModel($"Was denkst du am Anfang ueber Frage {index}?", [], index * 10))
            .ToArray();

    private static ParsedTalkTopicDiscussionQuestionModel[] CreateTalkTopicDiscussionQuestions(int countPerType)
    {
        string[] questionTypes = ["opinion", "imagination", "prediction", "comparison"];
        List<ParsedTalkTopicDiscussionQuestionModel> questions = [];
        int sortOrder = 10;
        foreach (string questionType in questionTypes)
        {
            for (int index = 1; index <= countPerType; index++)
            {
                questions.Add(new ParsedTalkTopicDiscussionQuestionModel(
                    $"Welche Idee hast du zu {questionType} Frage {index}?",
                    questionType,
                    [],
                    sortOrder));
                sortOrder += 10;
            }
        }

        return questions.ToArray();
    }

    private static ParsedTalkTopicVocabularyItemModel[] CreateTalkTopicVocabularyItems(int count, string cefrLevel) =>
        Enumerable.Range(1, count)
            .Select(index => new ParsedTalkTopicVocabularyItemModel(
                $"Wort {index}",
                null,
                cefrLevel,
                index * 10))
            .ToArray();

    private sealed class ThrowingFileReader : IContentImportFileReader
    {
        public Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken)
        {
            throw new FileNotFoundException("File not found.", filePath);
        }
    }

    private sealed class StubFileReader(string content) : IContentImportFileReader
    {
        public Task<string> ReadAllTextAsync(string filePath, CancellationToken cancellationToken)
        {
            return Task.FromResult(content);
        }
    }

    private sealed class ThrowingParser : IContentImportParser
    {
        public Task<ParsedContentPackageModel> ParseAsync(string content, CancellationToken cancellationToken)
        {
            throw new InvalidDataException("Invalid JSON package format.");
        }
    }

    private sealed class StubParser(Func<string, ParsedContentPackageModel> parseFunc) : IContentImportParser
    {
        public Task<ParsedContentPackageModel> ParseAsync(string content, CancellationToken cancellationToken)
        {
            return Task.FromResult(parseFunc(content));
        }
    }

    private sealed class FakeRepository(
        bool packageExists = false,
        bool wordExists = false,
        IReadOnlyList<WordEntry>? existingWords = null,
        IReadOnlySet<LanguageCode>? meaningLanguages = null) : IContentImportRepository
    {
        public IReadOnlyList<TalkTopic> ImportedTalkTopics { get; private set; } = [];

        public IReadOnlyList<WordCollection> ImportedCollections { get; private set; } = [];

        public IReadOnlyList<ExpressionEntry> ImportedExpressions { get; private set; } = [];

        public IReadOnlyList<CulturalNote> ImportedCulturalNotes { get; private set; } = [];

        public IReadOnlyList<ExamProfile> ImportedExamProfiles { get; private set; } = [];

        public IReadOnlyList<ExamPrepUnit> ImportedExamPrepUnits { get; private set; } = [];

        public Task<IReadOnlyDictionary<string, Topic>> GetActiveTopicsByKeyAsync(CancellationToken cancellationToken)
        {
            Topic topic = new(Guid.NewGuid(), "shopping", 10, true, DateTime.UtcNow);
            topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), "Shopping", DateTime.UtcNow);

            IReadOnlyDictionary<string, Topic> topicsByKey = new Dictionary<string, Topic>(StringComparer.Ordinal)
            {
                ["shopping"] = topic,
            };

            return Task.FromResult(topicsByKey);
        }

        public Task<IReadOnlySet<LanguageCode>> GetActiveMeaningLanguagesAsync(CancellationToken cancellationToken)
        {
            IReadOnlySet<LanguageCode> languages = meaningLanguages ??
                ContentLanguageRequirements.RequiredMeaningLanguageCodes
                    .Select(LanguageCode.From)
                    .ToHashSet();
            return Task.FromResult(languages);
        }

        public Task<bool> PackageExistsAsync(string packageId, CancellationToken cancellationToken)
        {
            return Task.FromResult(packageExists);
        }

        public Task<bool> WordExistsAsync(
            string normalizedLemma,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(wordExists);
        }

        public Task<IReadOnlyList<WordEntry>> GetActiveWordsByNormalizedLemmasAsync(
            IReadOnlyCollection<string> normalizedLemmas,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<WordEntry> result = existingWords ?? [];
            return Task.FromResult(result);
        }

        public Task PersistImportAsync(
            ContentPackage contentPackage,
            IReadOnlyList<LabelDefinition> importedLabelDefinitions,
            IReadOnlyList<WordEntry> importedWords,
            IReadOnlyList<WordCollection> importedCollections,
            IReadOnlyList<DialogueLesson> importedDialogues,
            IReadOnlyList<TalkTopic> importedTalkTopics,
            IReadOnlyList<GrammarTopic> importedGrammarTopics,
            IReadOnlyList<ExpressionEntry> importedExpressions,
            IReadOnlyList<Exercise> importedExercises,
            IReadOnlyList<ExerciseSet> importedExerciseSets,
            IReadOnlyList<CoursePath> importedCoursePaths,
            IReadOnlyList<CourseModule> importedCourseModules,
            IReadOnlyList<CourseLesson> importedCourseLessons,
            IReadOnlyList<WritingTemplate> importedWritingTemplates,
            IReadOnlyList<CulturalNote> importedCulturalNotes,
            IReadOnlyList<ExamProfile> importedExamProfiles,
            IReadOnlyList<ExamPrepUnit> importedExamPrepUnits,
            IReadOnlyList<ConversationStarterPack> importedConversationStarterPacks,
            IReadOnlyList<EventPreparationPack> importedEventPreparationPacks,
            CancellationToken cancellationToken)
        {
            ImportedTalkTopics = importedTalkTopics;
            ImportedCollections = importedCollections;
            ImportedExpressions = importedExpressions;
            ImportedCulturalNotes = importedCulturalNotes;
            ImportedExamProfiles = importedExamProfiles;
            ImportedExamPrepUnits = importedExamPrepUnits;
            return Task.CompletedTask;
        }
    }
}
