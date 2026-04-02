using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current lexical entries linked to a selected topic.
/// </summary>
[QueryProperty(nameof(TopicKey), "topicKey")]
[QueryProperty(nameof(TopicTitle), "topicTitle")]
public partial class TopicWordsPage : ContentPage
{
    private const int PageSize = 24;
    private readonly ITopicBrowseStateService _topicBrowseStateService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ObservableCollection<TopicWordItemViewModel> _visibleWords = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private int _loadedWordCount;
    private bool _hasMoreWords;
    private bool _isLoadingMore;
    private string _loadedTopicKey = string.Empty;
    private string _topicKey = string.Empty;
    private string _topicTitle = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicWordsPage"/> class.
    /// </summary>
    public TopicWordsPage(
        ITopicBrowseStateService topicBrowseStateService,
        IPerformanceTelemetryService performanceTelemetryService)
    {
        ArgumentNullException.ThrowIfNull(topicBrowseStateService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);

        InitializeComponent();

        _topicBrowseStateService = topicBrowseStateService;
        _performanceTelemetryService = performanceTelemetryService;
        WordsCollectionView.ItemsSource = _visibleWords;
    }

    /// <summary>
    /// Gets or sets the selected topic key passed by shell navigation.
    /// </summary>
    public string TopicKey
    {
        get => _topicKey;
        set => _topicKey = Uri.UnescapeDataString(value ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the localized selected topic title passed by shell navigation.
    /// </summary>
    public string TopicTitle
    {
        get => _topicTitle;
        set => _topicTitle = Uri.UnescapeDataString(value ?? string.Empty);
    }

    /// <summary>
    /// Refreshes the page when it becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await RefreshAsync(showLoadingState: _visibleWords.Count == 0, force: false).ConfigureAwait(true);
    }

    protected override void OnDisappearing()
    {
        CancelRefreshRequest();
        base.OnDisappearing();
    }

    /// <summary>
    /// Loads the current topic words using the user's preferred meaning language.
    /// </summary>
    private async Task RefreshAsync(bool showLoadingState, bool force)
    {
        if (!force &&
            !string.IsNullOrWhiteSpace(TopicKey) &&
            string.Equals(_loadedTopicKey, TopicKey, StringComparison.OrdinalIgnoreCase) &&
            _visibleWords.Count > 0)
        {
            return;
        }

        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;
        Stopwatch stopwatch = Stopwatch.StartNew();
        string resolvedTopicTitle = string.IsNullOrWhiteSpace(TopicTitle) ? AppStrings.TopicWordsPageTitle : TopicTitle;
        Title = resolvedTopicTitle;
        HeadlineLabel.Text = string.Format(AppStrings.TopicWordsPageHeadlineFormat, resolvedTopicTitle);
        DescriptionLabel.Text = AppStrings.TopicWordsPageDescription;
        EmptyStateLabel.Text = AppStrings.TopicWordsPageEmpty;
        LoadingStateLabel.Text = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.TopicWordsPageLoadError;

        if (string.IsNullOrWhiteSpace(TopicKey))
        {
            ShowWords(Array.Empty<TopicWordItemViewModel>());
            _loadedTopicKey = string.Empty;
            return;
        }

        if (showLoadingState)
        {
            ShowLoadingState(_visibleWords.Count > 0);
        }

        try
        {
            await Task.Yield();

            IReadOnlyList<WordListItemModel> firstPage = await _topicBrowseStateService
                .GetWordsPageAsync(TopicKey, skip: 0, take: PageSize, cancellationToken)
                .ConfigureAwait(true);

            ShowWords(firstPage
                .Select(word => new TopicWordItemViewModel(
                    word.PublicId,
                    BuildLemmaLine(word),
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray());
            _loadedTopicKey = TopicKey;

            ScheduleNextPagePrefetch();
            _performanceTelemetryService.Record("topic.list.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, firstPage.Count);
        }
        catch (OperationCanceledException)
        {
            _performanceTelemetryService.Record("topic.list.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
        }
        catch
        {
            ShowErrorState();
            _performanceTelemetryService.Record("topic.list.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Applies the current topic-word results and empty state visibility.
    /// </summary>
    private void ShowWords(IReadOnlyList<TopicWordItemViewModel> words)
    {
        _visibleWords.Clear();
        foreach (TopicWordItemViewModel word in words)
        {
            _visibleWords.Add(word);
        }

        _loadedWordCount = words.Count;
        _hasMoreWords = words.Count == PageSize;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = words.Count == 0;
        WordsCollectionView.IsVisible = words.Count > 0;
        SetLoadingState(false);
    }

    /// <summary>
    /// Shows the loading state while topic words are being loaded.
    /// </summary>
    private void ShowLoadingState(bool hasExistingWords)
    {
        SetLoadingState(true);
        ErrorStateLabel.IsVisible = false;

        if (hasExistingWords)
        {
            EmptyStateLabel.IsVisible = false;
            WordsCollectionView.IsVisible = true;
            return;
        }

        EmptyStateLabel.IsVisible = false;
        WordsCollectionView.IsVisible = false;
    }

    /// <summary>
    /// Shows the generic error state when topic-word loading fails.
    /// </summary>
    private void ShowErrorState()
    {
        _visibleWords.Clear();
        _loadedWordCount = 0;
        _hasMoreWords = false;
        WordsCollectionView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        ErrorStateLabel.IsVisible = true;
        SetLoadingState(false);
    }

    /// <summary>
    /// Loads the next visual page of topic words.
    /// </summary>
    private async Task LoadNextPageAsync()
    {
        if (_isLoadingMore || !_hasMoreWords || string.IsNullOrWhiteSpace(TopicKey))
        {
            return;
        }

        _isLoadingMore = true;
        try
        {
            IReadOnlyList<WordListItemModel> nextPage = await _topicBrowseStateService
                .GetWordsPageAsync(TopicKey, _loadedWordCount, PageSize, CancellationToken.None)
                .ConfigureAwait(true);

            IReadOnlyList<TopicWordItemViewModel> viewModels = nextPage
                .Select(word => new TopicWordItemViewModel(
                    word.PublicId,
                    BuildLemmaLine(word),
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray();

            foreach (TopicWordItemViewModel word in viewModels)
            {
                _visibleWords.Add(word);
            }

            _loadedWordCount += viewModels.Count;
            _hasMoreWords = viewModels.Count == PageSize;

            ScheduleNextPagePrefetch();
        }
        catch
        {
            LoadingStateLabel.Text = AppStrings.TopicWordsPageLoadError;
            SetLoadingState(true);
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    private void ResetRefreshRequest()
    {
        CancelRefreshRequest();
        _refreshCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelRefreshRequest()
    {
        if (_refreshCancellationTokenSource is null)
        {
            return;
        }

        _refreshCancellationTokenSource.Cancel();
        _refreshCancellationTokenSource.Dispose();
        _refreshCancellationTokenSource = null;
    }

    /// <summary>
    /// Loads the next chunk when the learner scrolls near the end of the visible topic words.
    /// </summary>
    private async void OnWordsRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        await LoadNextPageAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Prefetches the next topic result page into the browse cache without blocking the current UI flow.
    /// </summary>
    private void ScheduleNextPagePrefetch()
    {
        if (!_hasMoreWords || string.IsNullOrWhiteSpace(TopicKey))
        {
            return;
        }

        _ = Task.Run(() => _topicBrowseStateService.GetWordsPageAsync(
            TopicKey,
            _loadedWordCount,
            PageSize,
            CancellationToken.None));
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingStateLayout.IsVisible = isLoading;
        LoadingActivityIndicator.IsRunning = isLoading;
    }

    /// <summary>
    /// Navigates to the selected lexical-entry detail page.
    /// </summary>
    private async void OnWordsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TopicWordItemViewModel selectedWord)
        {
            return;
        }

        WordsCollectionView.SelectedItem = null;

        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString());
        await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}")
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Builds the browse headline shown for a lexical entry.
    /// </summary>
    private static string BuildLemmaLine(WordListItemModel word)
    {
        ArgumentNullException.ThrowIfNull(word);

        return string.IsNullOrWhiteSpace(word.Article)
            ? word.Lemma
            : $"{word.Article} {word.Lemma}";
    }

    /// <summary>
    /// Represents the UI model used by the topic-words collection view.
    /// </summary>
    private sealed record TopicWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}
