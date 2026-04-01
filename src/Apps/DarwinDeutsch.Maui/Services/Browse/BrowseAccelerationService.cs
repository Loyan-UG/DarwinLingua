using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Catalog.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace DarwinDeutsch.Maui.Services.Browse;

/// <summary>
/// Warms browse caches after startup and invalidates them after content updates.
/// </summary>
internal sealed class BrowseAccelerationService : IBrowseAccelerationService
{
    private static readonly string[] CefrLevels = ["A1", "A2", "B1", "B2", "C1", "C2"];
    private const int TopicWarmupLimit = 12;
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly ITopicBrowseStateService _topicBrowseStateService;
    private readonly IWordDetailCacheService _wordDetailCacheService;
    private readonly ITopicQueryService _topicQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ILogger<BrowseAccelerationService> _logger;
    private readonly SemaphoreSlim _warmupGate = new(1, 1);
    private int _warmupScheduled;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseAccelerationService"/> class.
    /// </summary>
    public BrowseAccelerationService(
        ICefrBrowseStateService cefrBrowseStateService,
        ITopicBrowseStateService topicBrowseStateService,
        IWordDetailCacheService wordDetailCacheService,
        ITopicQueryService topicQueryService,
        IUserLearningProfileService userLearningProfileService,
        IAppLocalizationService appLocalizationService,
        ILogger<BrowseAccelerationService> logger)
    {
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);
        ArgumentNullException.ThrowIfNull(topicBrowseStateService);
        ArgumentNullException.ThrowIfNull(wordDetailCacheService);
        ArgumentNullException.ThrowIfNull(topicQueryService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(logger);

        _cefrBrowseStateService = cefrBrowseStateService;
        _topicBrowseStateService = topicBrowseStateService;
        _wordDetailCacheService = wordDetailCacheService;
        _topicQueryService = topicQueryService;
        _userLearningProfileService = userLearningProfileService;
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

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                await RunWarmupAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Browse warm-up failed.");
            }
        });
    }

    /// <inheritdoc />
    public void ResetCaches()
    {
        _cefrBrowseStateService.ResetCache();
        _topicBrowseStateService.ResetCache();
        _wordDetailCacheService.ResetCache();
        Interlocked.Exchange(ref _warmupScheduled, 0);
    }

    private async Task RunWarmupAsync(CancellationToken cancellationToken)
    {
        if (!await _warmupGate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            DarwinLingua.Learning.Application.Models.UserLearningProfileModel profile = await _userLearningProfileService
                .GetCurrentProfileAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (string cefrLevel in CefrLevels)
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

            IReadOnlyList<DarwinLingua.Catalog.Application.Models.TopicListItemModel> topics = await _topicQueryService
                .GetTopicsAsync(_appLocalizationService.CurrentCulture.TwoLetterISOLanguageName, cancellationToken)
                .ConfigureAwait(false);

            foreach (string topicKey in topics.Take(TopicWarmupLimit).Select(topic => topic.Key))
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
                "Browse warm-up completed for {CefrCount} CEFR levels and {TopicCount} topics.",
                CefrLevels.Length,
                Math.Min(TopicWarmupLimit, topics.Count));
        }
        finally
        {
            _warmupGate.Release();
        }
    }
}
