using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using System.Collections.Concurrent;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Caches topic browse pages for the current meaning-language preference.
/// </summary>
internal sealed class TopicBrowseStateService : ITopicBrowseStateService
{
    private const int InitialSliceSize = 24;
    private readonly ConcurrentDictionary<string, IReadOnlyList<WordListItemModel>> _pageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Task<IReadOnlyList<WordListItemModel>>> _inFlightPageRequests = new(StringComparer.OrdinalIgnoreCase);
    private readonly IWordQueryService _wordQueryService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicBrowseStateService"/> class.
    /// </summary>
    public TopicBrowseStateService(
        IWordQueryService wordQueryService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService)
    {
        ArgumentNullException.ThrowIfNull(wordQueryService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);

        _wordQueryService = wordQueryService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetWordsPageAsync(
        string topicKey,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topicKey);

        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildPageCacheKey(topicKey, profile.PreferredMeaningLanguage1, skip, take);
        if (_pageCache.TryGetValue(cacheKey, out IReadOnlyList<WordListItemModel>? cachedWords))
        {
            return cachedWords;
        }

        Task<IReadOnlyList<WordListItemModel>> loadTask = _inFlightPageRequests.GetOrAdd(
            cacheKey,
            _ => LoadWordsPageAsync(topicKey, profile.PreferredMeaningLanguage1, skip, take, cacheKey, CancellationToken.None));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await loadTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (loadTask.IsCompleted)
            {
                _inFlightPageRequests.TryRemove(cacheKey, out _);
            }
        }
    }

    /// <inheritdoc />
    public async Task<Guid?> GetStartingWordPublicIdAsync(string topicKey, CancellationToken cancellationToken)
    {
        IReadOnlyList<WordListItemModel> words = await GetWordsPageAsync(topicKey, 0, InitialSliceSize, cancellationToken)
            .ConfigureAwait(false);
        return words.FirstOrDefault()?.PublicId;
    }

    /// <inheritdoc />
    public async Task PrefetchInitialSliceAsync(string topicKey, CancellationToken cancellationToken)
    {
        _ = await GetWordsPageAsync(topicKey, 0, InitialSliceSize, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _pageCache.Clear();
        _inFlightPageRequests.Clear();
    }

    private static string BuildPageCacheKey(string topicKey, string meaningLanguageCode, int skip, int take)
    {
        return $"{topicKey.Trim().ToLowerInvariant()}::{meaningLanguageCode.Trim().ToLowerInvariant()}::{skip}::{take}";
    }

    private async Task<IReadOnlyList<WordListItemModel>> LoadWordsPageAsync(
        string topicKey,
        string meaningLanguageCode,
        int skip,
        int take,
        string cacheKey,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<WordListItemModel> words = await _wordQueryService
                .GetWordsByTopicPageAsync(topicKey, meaningLanguageCode, skip, take, cancellationToken)
                .ConfigureAwait(false);

            _pageCache[cacheKey] = words;
            return words;
        }
        finally
        {
            _inFlightPageRequests.TryRemove(cacheKey, out _);
        }
    }
}
