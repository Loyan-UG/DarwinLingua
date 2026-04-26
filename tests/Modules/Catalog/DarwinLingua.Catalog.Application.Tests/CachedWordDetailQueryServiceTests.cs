using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Application.Tests;

/// <summary>
/// Verifies the caching behavior of the word-detail query service.
/// </summary>
public sealed class CachedWordDetailQueryServiceTests
{
    /// <summary>
    /// Verifies that calling the service twice with the same parameters hits the cache on the second call,
    /// returning the same model instance without calling the repository a second time.
    /// </summary>
    [Fact]
    public async Task GetWordDetailsAsync_ShouldReturnCachedResultOnSecondCallWithSameParameters()
    {
        CallCountingWordEntryRepository repository = new(CreateWordEntry());

        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(repository);
        services.AddSingleton<ITopicRepository>(new EmptyTopicRepository());

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordDetailQueryService queryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();

        WordDetailModel? firstResult = await queryService.GetWordDetailsAsync(
            repository.Word.PublicId,
            "en",
            null,
            "en",
            CancellationToken.None);

        WordDetailModel? secondResult = await queryService.GetWordDetailsAsync(
            repository.Word.PublicId,
            "en",
            null,
            "en",
            CancellationToken.None);

        Assert.Equal(1, repository.GetByPublicIdCallCount);
        Assert.Same(firstResult, secondResult);
    }

    /// <summary>
    /// Verifies that different parameter combinations produce separate cache entries.
    /// </summary>
    [Fact]
    public async Task GetWordDetailsAsync_ShouldUseSeparateCacheEntriesForDifferentLanguageParameters()
    {
        CallCountingWordEntryRepository repository = new(CreateWordEntry());

        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(repository);
        services.AddSingleton<ITopicRepository>(new EmptyTopicRepository());

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordDetailQueryService queryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();

        await queryService.GetWordDetailsAsync(
            repository.Word.PublicId,
            "en",
            null,
            "en",
            CancellationToken.None);

        await queryService.GetWordDetailsAsync(
            repository.Word.PublicId,
            "tr",
            null,
            "en",
            CancellationToken.None);

        Assert.Equal(2, repository.GetByPublicIdCallCount);
    }

    /// <summary>
    /// Verifies that a null result from the inner service is also cached so the repository is not called again.
    /// </summary>
    [Fact]
    public async Task GetWordDetailsAsync_ShouldCacheNullResultWhenWordIsNotFound()
    {
        WordEntry word = CreateWordEntry();
        CallCountingWordEntryRepository repository = new(word);

        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<IWordEntryRepository>(repository);
        services.AddSingleton<ITopicRepository>(new EmptyTopicRepository());

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();
        IWordDetailQueryService queryService = serviceProvider.GetRequiredService<IWordDetailQueryService>();

        Guid unknownPublicId = Guid.NewGuid();

        WordDetailModel? firstResult = await queryService.GetWordDetailsAsync(
            unknownPublicId,
            "en",
            null,
            "en",
            CancellationToken.None);

        WordDetailModel? secondResult = await queryService.GetWordDetailsAsync(
            unknownPublicId,
            "en",
            null,
            "en",
            CancellationToken.None);

        Assert.Null(firstResult);
        Assert.Null(secondResult);
        Assert.Equal(1, repository.GetByPublicIdCallCount);
    }

    private static WordEntry CreateWordEntry()
    {
        return new WordEntry(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Haus",
            LanguageCode.From("de"),
            CefrLevel.A1,
            PartOfSpeech.Noun,
            PublicationStatus.Active,
            ContentSourceType.Manual,
            DateTime.UtcNow,
            article: "das");
    }

    private sealed class CallCountingWordEntryRepository(WordEntry word) : IWordEntryRepository
    {
        public WordEntry Word { get; } = word;

        public int GetByPublicIdCallCount { get; private set; }

        public Task<WordEntry?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
        {
            GetByPublicIdCallCount++;
            return Task.FromResult(Word.PublicId == publicId ? Word : null);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicKeyAsync(
            string topicKey,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByTopicPageAsync(
            string topicKey,
            string meaningLanguageCode,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrAsync(
            CefrLevel cefrLevel,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> GetActiveByCefrPageAsync(
            CefrLevel cefrLevel,
            string meaningLanguageCode,
            int skip,
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }

        public Task<IReadOnlyList<WordListItemModel>> SearchActiveByLemmaAsync(
            string normalizedLemmaQuery,
            string meaningLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<WordListItemModel>>([]);
        }
    }

    private sealed class EmptyTopicRepository : ITopicRepository
    {
        public Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<Topic>>([]);
        }

        public Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
            IReadOnlyCollection<Guid> topicIds,
            LanguageCode preferredLanguageCode,
            LanguageCode fallbackLanguageCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<Guid, string>>(
                new Dictionary<Guid, string>());
        }
    }
}
