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
        IReadOnlyList<WordEntry>? existingWords = null) : IContentImportRepository
    {
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
            IReadOnlySet<LanguageCode> languages = new HashSet<LanguageCode> { LanguageCode.From("en") };
            return Task.FromResult(languages);
        }

        public Task<bool> PackageExistsAsync(string packageId, CancellationToken cancellationToken)
        {
            return Task.FromResult(packageExists);
        }

        public Task<bool> WordExistsAsync(
            string normalizedLemma,
            PartOfSpeech partOfSpeech,
            CefrLevel cefrLevel,
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
            IReadOnlyList<WordEntry> importedWords,
            IReadOnlyList<WordCollection> importedCollections,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
