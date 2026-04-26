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

    private sealed class FakeRepository(bool packageExists = false) : IContentImportRepository
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
            return Task.FromResult(false);
        }

        public Task<IReadOnlyList<WordEntry>> GetActiveWordsByNormalizedLemmasAsync(
            IReadOnlyCollection<string> normalizedLemmas,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<WordEntry> empty = [];
            return Task.FromResult(empty);
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
