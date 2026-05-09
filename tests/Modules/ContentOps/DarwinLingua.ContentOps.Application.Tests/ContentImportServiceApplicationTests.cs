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
        Assert.Contains(result.Issues, issue => issue.Message.Contains("entry", StringComparison.OrdinalIgnoreCase));
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
    /// Verifies that Talk Topic import validation rejects articles below the CEFR minimum length.
    /// </summary>
    [Fact]
    public async Task ImportAsync_ShouldFail_WhenTalkTopicArticleIsTooShort()
    {
        ParsedContentPackageModel parsedPackage = CreatePackageWithTalkTopic(
            CreateValidTalkTopic() with
            {
                Article = new ParsedTalkTopicArticleModel(
                    "Zu kurz.",
                    [new ParsedContentMeaningModel("en", "Too short.")]),
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
            issue.Message.Contains("at least 1000", StringComparison.Ordinal));
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
                        [new ParsedContentMeaningModel("en", "What do you think?")],
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
        Assert.NotEmpty(topic.ArticleTranslations);
        Assert.Equal(2, topic.WarmupQuestions.Count);
        Assert.Single(topic.DiscussionQuestions);
        Assert.Single(topic.VocabularyItems);
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

    private static ParsedTalkTopicModel CreateValidTalkTopic()
    {
        string article = string.Concat(Enumerable.Repeat(
            "Viele Menschen schauen am Abend in den Himmel. Sie sehen Sterne, den Mond und manchmal ein helles Licht. " +
            "Dann fragen sie sich: Gibt es Leben auf anderen Planeten? Diese Frage ist spannend, aber auch einfach. " +
            "Wir wissen, dass das Weltall sehr gross ist. Es gibt viele Sterne und viele Planeten. Vielleicht gibt es dort Wasser. " +
            "Vielleicht gibt es dort kleine Tiere, Pflanzen oder intelligente Wesen. Niemand in unserer Gruppe muss eine richtige Antwort haben. " +
            "Wir koennen nur denken, fragen und miteinander sprechen. Einige Menschen glauben, dass Ausserirdische freundlich sind. " +
            "Andere Menschen sind vorsichtig und sagen: Wir wissen zu wenig. In diesem Text geht es nicht um einen Test. " +
            "Es geht um Ideen, Meinungen und Gruende. Die Gruppe kann langsam sprechen, einfache Saetze benutzen und nachfragen. ",
            2));

        return new ParsedTalkTopicModel(
            "a1-aliens",
            "do-aliens-exist",
            "Gibt es Ausserirdische?",
            "A simple Talk Topic about aliens and life in space.",
            "A1",
            "science",
            ["shopping"],
            "article",
            new ParsedTalkTopicArticleModel(
                article,
                [new ParsedContentMeaningModel("en", "A simple article about aliens and life in space.")]),
            [
                new ParsedTalkTopicQuestionModel(
                    "Magst du Filme ueber Ausserirdische?",
                    [new ParsedContentMeaningModel("en", "Do you like films about aliens?")],
                    10),
                new ParsedTalkTopicQuestionModel(
                    "Schaust du gern in den Nachthimmel?",
                    [new ParsedContentMeaningModel("en", "Do you like looking at the night sky?")],
                    20),
            ],
            [
                new ParsedTalkTopicDiscussionQuestionModel(
                    "Glaubst du, dass es Ausserirdische gibt?",
                    "opinion",
                    [new ParsedContentMeaningModel("en", "Do you believe aliens exist?")],
                    10),
            ],
            [
                new ParsedTalkTopicVocabularyItemModel("das Weltall", "das-weltall", "A2", 10),
            ],
            ["express-opinion", "give-reasons"],
            5,
            20,
            false,
            null,
            false,
            10,
            true);
    }

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
            IReadOnlyList<ConversationStarterPack> importedConversationStarterPacks,
            IReadOnlyList<EventPreparationPack> importedEventPreparationPacks,
            CancellationToken cancellationToken)
        {
            ImportedTalkTopics = importedTalkTopics;
            return Task.CompletedTask;
        }
    }
}
