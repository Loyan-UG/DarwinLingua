using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Application.Tests;

/// <summary>
/// Verifies the lexical browse query behavior.
/// </summary>
public sealed class WordQueryServiceTests
{
    /// <summary>
    /// Verifies that the query service selects the requested meaning language when available.
    /// </summary>
    [Fact]
    public async Task GetWordsByTopicAsync_ShouldReturnRequestedMeaningLanguage()
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(new FakeWordEntryRepository(
            topicWords:
            [
                new WordListItemModel(Guid.NewGuid(), "Bahnhof", "der", null, "Noun", "A1", "ایستگاه"),
            ],
            cefrWords: [],
            cefrPageWords: [],
            searchWords: []));

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordQueryService queryService = serviceProvider.GetRequiredService<IWordQueryService>();

        IReadOnlyList<WordListItemModel> words = await queryService
            .GetWordsByTopicAsync("travel", "fa", CancellationToken.None);

        WordListItemModel result = Assert.Single(words);
        Assert.Equal("Bahnhof", result.Lemma);
        Assert.Equal("ایستگاه", result.PrimaryMeaning);
    }

    /// <summary>
    /// Verifies that the CEFR query returns only matching words.
    /// </summary>
    [Fact]
    public async Task GetWordsByCefrAsync_ShouldReturnMatchingWords()
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(new FakeWordEntryRepository(
            topicWords: [],
            cefrWords:
            [
                new WordListItemModel(Guid.NewGuid(), "Bahnhof", "der", null, "Noun", "A1", "station"),
            ],
            cefrPageWords: [],
            searchWords: []));

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordQueryService queryService = serviceProvider.GetRequiredService<IWordQueryService>();

        IReadOnlyList<WordListItemModel> words = await queryService
            .GetWordsByCefrAsync("A1", "en", CancellationToken.None);

        WordListItemModel result = Assert.Single(words);
        Assert.Equal("Bahnhof", result.Lemma);
    }

    /// <summary>
    /// Verifies that the paged CEFR query delegates to the repository and returns only the requested page.
    /// </summary>
    [Fact]
    public async Task GetWordsByCefrPageAsync_ShouldReturnRequestedPage()
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(new FakeWordEntryRepository(
            topicWords: [],
            cefrWords:
            [
                new WordListItemModel(Guid.NewGuid(), "Bahnhof", "der", null, "Noun", "A1", "station"),
            ],
            cefrPageWords:
            [
                new WordListItemModel(Guid.NewGuid(), "Brot", "das", null, "Noun", "A1", "bread"),
            ],
            searchWords: []));

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordQueryService queryService = serviceProvider.GetRequiredService<IWordQueryService>();

        IReadOnlyList<WordListItemModel> words = await queryService
            .GetWordsByCefrPageAsync("A1", "en", 24, 24, CancellationToken.None);

        WordListItemModel result = Assert.Single(words);
        Assert.Equal("Brot", result.Lemma);
    }

    /// <summary>
    /// Verifies that search normalizes whitespace and casing.
    /// </summary>
    [Fact]
    public async Task SearchWordsAsync_ShouldNormalizeSearchQuery()
    {
        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(new FakeWordEntryRepository(
            topicWords: [],
            cefrWords: [],
            cefrPageWords: [],
            searchWords:
            [
                new WordListItemModel(Guid.NewGuid(), "Bahnhof", "der", null, "Noun", "A1", "station"),
            ]));

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordQueryService queryService = serviceProvider.GetRequiredService<IWordQueryService>();

        IReadOnlyList<WordListItemModel> words = await queryService
            .SearchWordsAsync("  BAHNHOF  ", "en", CancellationToken.None);

        WordListItemModel result = Assert.Single(words);
        Assert.Equal("Bahnhof", result.Lemma);
    }

    private sealed class FakeWordEntryRepository(
        IReadOnlyList<WordListItemModel> topicWords,
        IReadOnlyList<WordListItemModel> cefrWords,
        IReadOnlyList<WordListItemModel> cefrPageWords,
        IReadOnlyList<WordListItemModel> searchWords) : IWordEntryRepository
    {
        public Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicKeyAsync(
            string topicKey,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(topicWords);
        }

        public Task<WordEntry?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
        {
            return Task.FromResult<WordEntry?>(null);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrAsync(
            CefrLevel cefrLevel,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(cefrWords);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrPageAsync(
            CefrLevel cefrLevel,
            string meaningLanguageCode,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(cefrPageWords);
        }

        public Task<IReadOnlyList<WordListItemModel>> SearchActiveByLemmaAsync(
            string normalizedLemmaQuery,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(searchWords);
        }
    }
}
