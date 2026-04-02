using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Warms browse caches after startup and invalidates them after content updates.
/// </summary>
internal sealed class BrowseAccelerationService : IBrowseAccelerationService
{
    private static readonly string[] CefrLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
    private static readonly string[] PriorityCefrLevels = ["A1", "A2"];
    private const int PriorityTopicWarmupLimit = 4;
    private const int TopicWarmupLimit = 12;
    private static readonly TimeSpan InitialWarmupDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ExtendedWarmupDelay = TimeSpan.FromSeconds(6);
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly ITopicCatalogCacheService _topicCatalogCacheService;
    private readonly ITopicBrowseStateService _topicBrowseStateService;
    private readonly IWordDetailCacheService _wordDetailCacheService;
    private readonly IWordSearchCacheService _wordSearchCacheService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ILogger<BrowseAccelerationService> _logger;
    private readonly SemaphoreSlim _warmupGate = new(1, 1);
    private readonly object _warmupStateLock = new();
    private CancellationTokenSource? _warmupCancellationSource;
    private int _warmupScheduled;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseAccelerationService"/> class.
    /// </summary>
    public BrowseAccelerationService(
        ICefrBrowseStateService cefrBrowseStateService,
        ITopicCatalogCacheService topicCatalogCacheService,
        ITopicBrowseStateService topicBrowseStateService,
        IWordDetailCacheService wordDetailCacheService,
        IWordSearchCacheService wordSearchCacheService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IAppLocalizationService appLocalizationService,
        ILogger<BrowseAccelerationService> logger)
    {
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);
        ArgumentNullException.ThrowIfNull(topicCatalogCacheService);
        ArgumentNullException.ThrowIfNull(topicBrowseStateService);
        ArgumentNullException.ThrowIfNull(wordDetailCacheService);
        ArgumentNullException.ThrowIfNull(wordSearchCacheService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(logger);

        _cefrBrowseStateService = cefrBrowseStateService;
        _topicCatalogCacheService = topicCatalogCacheService;
        _topicBrowseStateService = topicBrowseStateService;
        _wordDetailCacheService = wordDetailCacheService;
        _wordSearchCacheService = wordSearchCacheService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _appLocalizationService = appLocalizationService;
        _logger = logger;
    }

    /// <inheritdoc />
    public void ScheduleInitialWarmup()
    {
        if (Interlocked.Exchange(ref _warmupScheduled, 1) == 1)
        {
            return;
        }

        CancellationTokenSource warmupCancellationSource;
        lock (_warmupStateLock)
        {
            _warmupCancellationSource?.Dispose();
            _warmupCancellationSource = new CancellationTokenSource();
            warmupCancellationSource = _warmupCancellationSource;
        }

        _ = RunScheduledWarmupAsync(warmupCancellationSource);
    }

    /// <inheritdoc />
    public void ResetCaches()
    {
        lock (_warmupStateLock)
        {
            _warmupCancellationSource?.Cancel();
        }

        _cefrBrowseStateService.ResetCache();
        _topicCatalogCacheService.ResetCache();
        _topicBrowseStateService.ResetCache();
        _wordDetailCacheService.ResetCache();
        _wordSearchCacheService.ResetCache();
        Interlocked.Exchange(ref _warmupScheduled, 0);
    }

    private async Task RunScheduledWarmupAsync(CancellationTokenSource warmupCancellationSource)
    {
        try
        {
            await Task.Delay(InitialWarmupDelay, warmupCancellationSource.Token).ConfigureAwait(false);
            await RunWarmupAsync(prioritizeEssentialSlicesOnly: true, warmupCancellationSource.Token).ConfigureAwait(false);
            await Task.Delay(ExtendedWarmupDelay, warmupCancellationSource.Token).ConfigureAwait(false);
            await RunWarmupAsync(prioritizeEssentialSlicesOnly: false, warmupCancellationSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (warmupCancellationSource.IsCancellationRequested)
        {
            _logger.LogDebug("Browse warm-up was cancelled.");
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Browse warm-up failed.");
        }
        finally
        {
            lock (_warmupStateLock)
            {
                if (ReferenceEquals(_warmupCancellationSource, warmupCancellationSource))
                {
                    _warmupCancellationSource.Dispose();
                    _warmupCancellationSource = null;
                }
            }
        }
    }

    private async Task RunWarmupAsync(bool prioritizeEssentialSlicesOnly, CancellationToken cancellationToken)
    {
        if (!await _warmupGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            DarwinLingua.Learning.Application.Models.UserLearningProfileModel profile = await _activeLearningProfileCacheService
                .GetCurrentProfileAsync(cancellationToken)
                .ConfigureAwait(false);

            IEnumerable<string> cefrLevelsToWarm = prioritizeEssentialSlicesOnly
                ? PriorityCefrLevels
                : CefrLevels.Except(PriorityCefrLevels, StringComparer.OrdinalIgnoreCase);

            foreach (string cefrLevel in cefrLevelsToWarm)
            {
                await _cefrBrowseStateService.PrefetchInitialSliceAsync(cefrLevel, cancellationToken).ConfigureAwait(false);
                IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> initialSlice = await _cefrBrowseStateService
                    .GetWordsPageAsync(cefrLevel, 0, 1, cancellationToken)
                    .ConfigureAwait(false);

                if (initialSlice.Count > 0)
                {
                    await _wordDetailCacheService
                        .PrefetchWordDetailsAsync(
                            initialSlice[0].PublicId,
                            profile.PreferredMeaningLanguage1,
                            profile.PreferredMeaningLanguage2,
                            profile.UiLanguageCode,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.TopicListItemModel> topics = await _topicCatalogCacheService
                .GetTopicsAsync(_appLocalizationService.CurrentCulture.TwoLetterISOLanguageName, cancellationToken)
                .ConfigureAwait(false);

            int topicLimit = prioritizeEssentialSlicesOnly ? PriorityTopicWarmupLimit : TopicWarmupLimit;
            IEnumerable<string> topicKeysToWarm = prioritizeEssentialSlicesOnly
                ? topics.Take(topicLimit).Select(topic => topic.Key)
                : topics.Skip(PriorityTopicWarmupLimit).Take(Math.Max(0, topicLimit - PriorityTopicWarmupLimit)).Select(topic => topic.Key);

            foreach (string topicKey in topicKeysToWarm)
            {
                await _topicBrowseStateService.PrefetchInitialSliceAsync(topicKey, cancellationToken).ConfigureAwait(false);
                IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordListItemModel> initialSlice = await _topicBrowseStateService
                    .GetWordsPageAsync(topicKey, 0, 1, cancellationToken)
                    .ConfigureAwait(false);

                if (initialSlice.Count > 0)
                {
                    await _wordDetailCacheService
                        .PrefetchWordDetailsAsync(
                            initialSlice[0].PublicId,
                            profile.PreferredMeaningLanguage1,
                            profile.PreferredMeaningLanguage2,
                            profile.UiLanguageCode,
                            cancellationToken)
                        .ConfigureAwait(false);
                }
            }

            _logger.LogInformation(
                "Browse warm-up stage '{WarmupStage}' completed for {CefrCount} CEFR levels and {TopicCount} topics.",
                prioritizeEssentialSlicesOnly ? "priority" : "extended",
                cefrLevelsToWarm.Count(),
                prioritizeEssentialSlicesOnly
                    ? Math.Min(PriorityTopicWarmupLimit, topics.Count)
                    : Math.Max(0, Math.Min(TopicWarmupLimit, topics.Count) - Math.Min(PriorityTopicWarmupLimit, topics.Count)));
        }
        finally
        {
            _warmupGate.Release();
        }
    }
}
