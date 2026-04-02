using DarwinDeutsch.Maui.Services.Browse.Models;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using System.Collections.Concurrent;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Caches CEFR word lists for the current meaning-language preference and persists the last viewed word per level.
/// </summary>
internal sealed class CefrBrowseStateService : ICefrBrowseStateService
{
    private const string LastViewedWordPreferencePrefix = "cefr-last-viewed-word";
    private const int InitialSliceSize = 12;
    private const int NavigationChunkSize = 12;
    private const int NavigationPrefetchTargetSize = 24;
    private const int MaxNavigationCacheSize = 96;
    private readonly ConcurrentDictionary<string, IReadOnlyList<WordListItemModel>> _pageCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Task<IReadOnlyList<WordListItemModel>>> _inFlightPageRequests = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, NavigationSliceCacheEntry> _navigationCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _navigationGates = new(StringComparer.OrdinalIgnoreCase);
    private readonly IWordQueryService _wordQueryService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CefrBrowseStateService"/> class.
    /// </summary>
    public CefrBrowseStateService(
        IWordQueryService wordQueryService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService)
    {
        ArgumentNullException.ThrowIfNull(wordQueryService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);

        _wordQueryService = wordQueryService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetWordsAsync(string cefrLevel, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildCacheKey(cefrLevel, profile.PreferredMeaningLanguage1);
        if (_navigationCache.TryGetValue(cacheKey, out NavigationSliceCacheEntry? navigationCacheEntry)
            && !navigationCacheEntry.HasMore)
        {
            return navigationCacheEntry.Words;
        }

        string pageCacheKey = BuildPageCacheKey(cefrLevel, profile.PreferredMeaningLanguage1, 0, int.MaxValue);
        if (_pageCache.TryGetValue(pageCacheKey, out IReadOnlyList<WordListItemModel>? cachedWords))
        {
            return cachedWords;
        }

        IReadOnlyList<WordListItemModel> words = await _wordQueryService
            .GetWordsByCefrAsync(cefrLevel, profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        _pageCache[pageCacheKey] = words;
        _navigationCache[cacheKey] = new NavigationSliceCacheEntry(words, false);
        return words;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetWordsPageAsync(
        string cefrLevel,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

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

        string pageCacheKey = BuildPageCacheKey(cefrLevel, profile.PreferredMeaningLanguage1, skip, take);
        if (_pageCache.TryGetValue(pageCacheKey, out IReadOnlyList<WordListItemModel>? cachedWords))
        {
            return cachedWords;
        }

        Task<IReadOnlyList<WordListItemModel>> loadTask = _inFlightPageRequests.GetOrAdd(
            pageCacheKey,
            _ => LoadWordsPageAsync(cefrLevel, profile.PreferredMeaningLanguage1, skip, take, pageCacheKey, CancellationToken.None));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await loadTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (loadTask.IsCompleted)
            {
                _inFlightPageRequests.TryRemove(pageCacheKey, out _);
            }
        }
    }

    /// <inheritdoc />
    public async Task<Guid?> GetStartingWordPublicIdAsync(string cefrLevel, CancellationToken cancellationToken)
    {
        string preferenceKey = BuildLastViewedWordPreferenceKey(cefrLevel);
        string? persistedWordPublicId = Preferences.Default.Get<string?>(preferenceKey, null);

        await PrefetchInitialSliceAsync(cefrLevel, cancellationToken).ConfigureAwait(false);

        if (Guid.TryParse(persistedWordPublicId, out Guid rememberedWordPublicId))
        {
            return rememberedWordPublicId;
        }

        IReadOnlyList<WordListItemModel> words = await GetWordsPageAsync(cefrLevel, 0, InitialSliceSize, cancellationToken)
            .ConfigureAwait(false);
        return words.FirstOrDefault()?.PublicId;
    }

    /// <inheritdoc />
    public async Task<CefrBrowseNavigationState> GetNavigationStateAsync(
        string cefrLevel,
        Guid currentWordPublicId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildCacheKey(cefrLevel, profile.PreferredMeaningLanguage1);
        NavigationSliceCacheEntry navigationCacheEntry = await EnsureNavigationSliceForCurrentWordAsync(
                cefrLevel,
                profile.PreferredMeaningLanguage1,
                cacheKey,
                currentWordPublicId,
                cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<WordListItemModel> words = navigationCacheEntry.Words;
        int currentIndex = words
            .Select((word, index) => new { word.PublicId, Index = index })
            .Where(item => item.PublicId == currentWordPublicId)
            .Select(item => item.Index)
            .DefaultIfEmpty(-1)
            .First();

        if (currentIndex < 0)
        {
            return new CefrBrowseNavigationState(null, null, 0, words.Count);
        }

        Guid? previousWordPublicId = currentIndex > 0
            ? words[currentIndex - 1].PublicId
            : null;
        Guid? nextWordPublicId = currentIndex < words.Count - 1
            ? words[currentIndex + 1].PublicId
            : null;

        return new CefrBrowseNavigationState(previousWordPublicId, nextWordPublicId, currentIndex + 1, words.Count);
    }

    /// <inheritdoc />
    public async Task PrefetchInitialSliceAsync(string cefrLevel, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        await EnsureNavigationSliceSizeAsync(
                cefrLevel,
                profile.PreferredMeaningLanguage1,
                BuildCacheKey(cefrLevel, profile.PreferredMeaningLanguage1),
                InitialSliceSize,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task PrefetchNavigationAsync(string cefrLevel, Guid currentWordPublicId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildCacheKey(cefrLevel, profile.PreferredMeaningLanguage1);
        NavigationSliceCacheEntry currentEntry = await EnsureNavigationSliceForCurrentWordAsync(
                cefrLevel,
                profile.PreferredMeaningLanguage1,
                cacheKey,
                currentWordPublicId,
                cancellationToken)
            .ConfigureAwait(false);

        if (currentEntry.HasMore && currentEntry.Words.Count < NavigationPrefetchTargetSize)
        {
            await EnsureNavigationSliceSizeAsync(
                    cefrLevel,
                    profile.PreferredMeaningLanguage1,
                    cacheKey,
                    Math.Min(NavigationPrefetchTargetSize, MaxNavigationCacheSize),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void RememberLastViewedWord(string cefrLevel, Guid wordPublicId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        Preferences.Default.Set(BuildLastViewedWordPreferenceKey(cefrLevel), wordPublicId.ToString("D"));
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _pageCache.Clear();
        _inFlightPageRequests.Clear();
        _navigationCache.Clear();
    }

    private static string BuildCacheKey(string cefrLevel, string meaningLanguageCode)
    {
        return $"{cefrLevel.Trim().ToUpperInvariant()}::{meaningLanguageCode.Trim().ToLowerInvariant()}";
    }

    private static string BuildPageCacheKey(string cefrLevel, string meaningLanguageCode, int skip, int take)
    {
        return $"{BuildCacheKey(cefrLevel, meaningLanguageCode)}::{skip}::{take}";
    }

    private static string BuildLastViewedWordPreferenceKey(string cefrLevel)
    {
        return $"{LastViewedWordPreferencePrefix}:{cefrLevel.Trim().ToUpperInvariant()}";
    }

    private async Task<NavigationSliceCacheEntry> EnsureNavigationSliceForCurrentWordAsync(
        string cefrLevel,
        string meaningLanguageCode,
        string cacheKey,
        Guid currentWordPublicId,
        CancellationToken cancellationToken)
    {
        NavigationSliceCacheEntry entry = await EnsureNavigationSliceSizeAsync(
                cefrLevel,
                meaningLanguageCode,
                cacheKey,
                InitialSliceSize,
                cancellationToken)
            .ConfigureAwait(false);

        while (!entry.Words.Any(word => word.PublicId == currentWordPublicId) &&
               entry.HasMore &&
               entry.Words.Count < MaxNavigationCacheSize)
        {
            entry = await EnsureNavigationSliceSizeAsync(
                    cefrLevel,
                    meaningLanguageCode,
                    cacheKey,
                    Math.Min(entry.Words.Count + NavigationChunkSize, MaxNavigationCacheSize),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        if (!entry.Words.Any(word => word.PublicId == currentWordPublicId))
        {
            IReadOnlyList<WordListItemModel> allWords = await _wordQueryService
                .GetWordsByCefrAsync(cefrLevel, meaningLanguageCode, cancellationToken)
                .ConfigureAwait(false);
            entry = new NavigationSliceCacheEntry(allWords, false);
            _navigationCache[cacheKey] = entry;
        }

        int currentIndex = entry.Words
            .Select((word, index) => new { word.PublicId, Index = index })
            .Where(item => item.PublicId == currentWordPublicId)
            .Select(item => item.Index)
            .DefaultIfEmpty(-1)
            .First();

        if (currentIndex >= 0 &&
            entry.HasMore &&
            currentIndex >= entry.Words.Count - 3 &&
            entry.Words.Count < MaxNavigationCacheSize)
        {
            entry = await EnsureNavigationSliceSizeAsync(
                    cefrLevel,
                    meaningLanguageCode,
                    cacheKey,
                    Math.Min(entry.Words.Count + NavigationChunkSize, MaxNavigationCacheSize),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        return entry;
    }

    private async Task<NavigationSliceCacheEntry> EnsureNavigationSliceSizeAsync(
        string cefrLevel,
        string meaningLanguageCode,
        string cacheKey,
        int targetCount,
        CancellationToken cancellationToken)
    {
        SemaphoreSlim gate = _navigationGates.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_navigationCache.TryGetValue(cacheKey, out NavigationSliceCacheEntry? existingEntry) &&
                (existingEntry.Words.Count >= targetCount || !existingEntry.HasMore))
            {
                return existingEntry;
            }

            NavigationSliceCacheEntry currentEntry = existingEntry ?? new NavigationSliceCacheEntry([], true);
            List<WordListItemModel> words = currentEntry.Words.ToList();
            bool hasMore = currentEntry.HasMore;

            while (hasMore && words.Count < targetCount && words.Count < MaxNavigationCacheSize)
            {
                int remaining = Math.Min(NavigationChunkSize, targetCount - words.Count);
                if (remaining <= 0)
                {
                    break;
                }

                IReadOnlyList<WordListItemModel> nextPage = await _wordQueryService
                    .GetWordsByCefrPageAsync(cefrLevel, meaningLanguageCode, words.Count, remaining, cancellationToken)
                    .ConfigureAwait(false);

                _pageCache[BuildPageCacheKey(cefrLevel, meaningLanguageCode, words.Count, remaining)] = nextPage;
                words.AddRange(nextPage);
                hasMore = nextPage.Count == remaining;
            }

            NavigationSliceCacheEntry updatedEntry = new(words, hasMore);
            _navigationCache[cacheKey] = updatedEntry;
            return updatedEntry;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task<IReadOnlyList<WordListItemModel>> LoadWordsPageAsync(
        string cefrLevel,
        string meaningLanguageCode,
        int skip,
        int take,
        string pageCacheKey,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<WordListItemModel> words = await _wordQueryService
                .GetWordsByCefrPageAsync(cefrLevel, meaningLanguageCode, skip, take, cancellationToken)
                .ConfigureAwait(false);

            _pageCache[pageCacheKey] = words;
            return words;
        }
        finally
        {
            _inFlightPageRequests.TryRemove(pageCacheKey, out _);
        }
    }

    private sealed record NavigationSliceCacheEntry(IReadOnlyList<WordListItemModel> Words, bool HasMore);
}
