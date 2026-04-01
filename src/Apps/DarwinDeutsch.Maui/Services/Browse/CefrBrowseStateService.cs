using DarwinDeutsch.Maui.Services.Browse.Models;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using System.Collections.Concurrent;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Caches CEFR word lists for the current meaning-language preference and persists the last viewed word per level.
/// </summary>
internal sealed class CefrBrowseStateService : ICefrBrowseStateService
{
    private const string LastViewedWordPreferencePrefix = "cefr-last-viewed-word";
    private readonly ConcurrentDictionary<string, IReadOnlyList<WordListItemModel>> _wordCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly IWordQueryService _wordQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CefrBrowseStateService"/> class.
    /// </summary>
    public CefrBrowseStateService(
        IWordQueryService wordQueryService,
        IUserLearningProfileService userLearningProfileService)
    {
        ArgumentNullException.ThrowIfNull(wordQueryService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);

        _wordQueryService = wordQueryService;
        _userLearningProfileService = userLearningProfileService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WordListItemModel>> GetWordsAsync(string cefrLevel, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        UserLearningProfileModel profile = await _userLearningProfileService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        string cacheKey = BuildCacheKey(cefrLevel, profile.PreferredMeaningLanguage1);
        if (_wordCache.TryGetValue(cacheKey, out IReadOnlyList<WordListItemModel>? cachedWords))
        {
            return cachedWords;
        }

        IReadOnlyList<WordListItemModel> words = await _wordQueryService
            .GetWordsByCefrAsync(cefrLevel, profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        _wordCache[cacheKey] = words;
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

        UserLearningProfileModel profile = await _userLearningProfileService
            .GetCurrentProfileAsync(cancellationToken)
            .ConfigureAwait(false);

        return await _wordQueryService
            .GetWordsByCefrPageAsync(cefrLevel, profile.PreferredMeaningLanguage1, skip, take, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Guid?> GetStartingWordPublicIdAsync(string cefrLevel, CancellationToken cancellationToken)
    {
        IReadOnlyList<WordListItemModel> words = await GetWordsAsync(cefrLevel, cancellationToken).ConfigureAwait(false);
        if (words.Count == 0)
        {
            return null;
        }

        string preferenceKey = BuildLastViewedWordPreferenceKey(cefrLevel);
        string? persistedWordPublicId = Preferences.Default.Get<string?>(preferenceKey, null);

        if (Guid.TryParse(persistedWordPublicId, out Guid rememberedWordPublicId)
            && words.Any(word => word.PublicId == rememberedWordPublicId))
        {
            return rememberedWordPublicId;
        }

        return words[0].PublicId;
    }

    /// <inheritdoc />
    public async Task<CefrBrowseNavigationState> GetNavigationStateAsync(
        string cefrLevel,
        Guid currentWordPublicId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<WordListItemModel> words = await GetWordsAsync(cefrLevel, cancellationToken).ConfigureAwait(false);
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
    public void RememberLastViewedWord(string cefrLevel, Guid wordPublicId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cefrLevel);

        Preferences.Default.Set(BuildLastViewedWordPreferenceKey(cefrLevel), wordPublicId.ToString("D"));
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _wordCache.Clear();
    }

    private static string BuildCacheKey(string cefrLevel, string meaningLanguageCode)
    {
        return $"{cefrLevel.Trim().ToUpperInvariant()}::{meaningLanguageCode.Trim().ToLowerInvariant()}";
    }

    private static string BuildLastViewedWordPreferenceKey(string cefrLevel)
    {
        return $"{LastViewedWordPreferencePrefix}:{cefrLevel.Trim().ToUpperInvariant()}";
    }
}
