using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays lexical entries filtered by CEFR level.
/// </summary>
[QueryProperty(nameof(CefrLevel), "cefrLevel")]
public partial class CefrWordsPage : ContentPage
{
    private const int PageSize = 24;
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ObservableCollection<CefrWordItemViewModel> _visibleWords = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;
    private int _loadedWordCount;
    private bool _hasMoreWords;
    private bool _isLoadingMore;
    private string _loadedCefrLevel = string.Empty;
    private string _cefrLevel = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CefrWordsPage"/> class.
    /// </summary>
    public CefrWordsPage(
        ICefrBrowseStateService cefrBrowseStateService,
        IPerformanceTelemetryService performanceTelemetryService)
    {
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);

        InitializeComponent();

        _cefrBrowseStateService = cefrBrowseStateService;
        _performanceTelemetryService = performanceTelemetryService;
        WordsCollectionView.ItemsSource = _visibleWords;
    }

    /// <summary>
    /// Gets or sets the selected CEFR level.
    /// </summary>
    public string CefrLevel
    {
        get => _cefrLevel;
        set => _cefrLevel = Uri.UnescapeDataString(value ?? string.Empty);
    }

    /// <summary>
    /// Refreshes the page whenever it becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshAsync(showLoadingState: _visibleWords.Count == 0, force: false).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override void OnDisappearing()
    {
        CancelRefreshRequest();
        base.OnDisappearing();
    }

    /// <summary>
    /// Loads the lexical entries for the selected CEFR level.
    /// </summary>
    private async Task RefreshAsync(bool showLoadingState, bool force)
    {
        if (!force &&
            !string.IsNullOrWhiteSpace(CefrLevel) &&
            string.Equals(_loadedCefrLevel, CefrLevel, StringComparison.OrdinalIgnoreCase) &&
            _visibleWords.Count > 0)
        {
            return;
        }

        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;
        Stopwatch stopwatch = Stopwatch.StartNew();
        string resolvedCefrLevel = string.IsNullOrWhiteSpace(CefrLevel) ? AppStrings.CefrWordsPageTitle : CefrLevel;
        Title = resolvedCefrLevel;
        HeadlineLabel.Text = string.IsNullOrWhiteSpace(CefrLevel)
            ? AppStrings.CefrWordsPageTitle
            : string.Format(AppStrings.CefrWordsPageHeadlineFormat, resolvedCefrLevel);
        DescriptionLabel.Text = AppStrings.CefrWordsPageDescription;
        CefrQuickFilterView.Caption = AppStrings.HomeCefrBrowseLabel;
        CefrQuickFilterView.SelectedLevel = CefrLevel;
        EmptyStateLabel.Text = AppStrings.CefrWordsPageEmpty;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CefrWordsPageLoadError;

        if (string.IsNullOrWhiteSpace(CefrLevel))
        {
            ShowWords([]);
            _loadedCefrLevel = string.Empty;
            return;
        }

        if (showLoadingState)
        {
            ShowLoadingState(_visibleWords.Count > 0);
        }

        try
        {
            await Task.Yield();

            IReadOnlyList<WordListItemModel> firstPage = await _cefrBrowseStateService
                .GetWordsPageAsync(CefrLevel, skip: 0, take: PageSize, cancellationToken)
                .ConfigureAwait(true);

            ShowWords(firstPage
                .Select(word => new CefrWordItemViewModel(
                    word.PublicId,
                    string.IsNullOrWhiteSpace(word.Article) ? word.Lemma : $"{word.Article} {word.Lemma}",
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray());
            _loadedCefrLevel = CefrLevel;

            ScheduleNextPagePrefetch();
            _performanceTelemetryService.Record("cefr.list.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, firstPage.Count);
        }
        catch (OperationCanceledException)
        {
            _performanceTelemetryService.Record("cefr.list.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
        }
        catch
        {
            ShowErrorState();
            _performanceTelemetryService.Record("cefr.list.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Refreshes the list using the selected quick-filter CEFR level.
    /// </summary>
    private async void OnCefrLevelSelected(object? sender, EventArgs e)
    {
        string cefrLevel = CefrQuickFilterView.SelectedLevel;
        if (string.IsNullOrWhiteSpace(cefrLevel))
        {
            return;
        }

        CefrLevel = cefrLevel;
        string escapedCefrLevel = Uri.EscapeDataString(cefrLevel);
        string navigationRoute = $"{nameof(CefrWordsPage)}?cefrLevel={escapedCefrLevel}";
        try
        {
            try
            {
                Guid? startingWordPublicId = await _cefrBrowseStateService
                    .GetStartingWordPublicIdAsync(cefrLevel, CancellationToken.None)
                    .ConfigureAwait(true);

                if (startingWordPublicId is null)
                {
                    await RefreshAsync(showLoadingState: _visibleWords.Count == 0, force: true).ConfigureAwait(true);
                    return;
                }

                string escapedWordPublicId = Uri.EscapeDataString(startingWordPublicId.Value.ToString("D"));
                navigationRoute =
                    $"{nameof(WordDetailPage)}?wordPublicId={escapedWordPublicId}&cefrLevel={escapedCefrLevel}";
            }
            catch
            {
                await RefreshAsync(showLoadingState: _visibleWords.Count == 0, force: true).ConfigureAwait(true);
                return;
            }

            await Shell.Current.GoToAsync(navigationRoute).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Applies the current result set to the page.
    /// </summary>
    private void ShowWords(IReadOnlyList<CefrWordItemViewModel> words)
    {
        _visibleWords.Clear();
        foreach (CefrWordItemViewModel word in words)
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
    /// Shows the loading state while CEFR words are being loaded.
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
    /// Shows the generic error state when CEFR word loading fails.
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
    /// Loads the next visual page of words into the collection view.
    /// </summary>
    private async Task LoadNextPageAsync()
    {
        if (_isLoadingMore || !_hasMoreWords || string.IsNullOrWhiteSpace(CefrLevel))
        {
            return;
        }

        _isLoadingMore = true;
        try
        {
            IReadOnlyList<WordListItemModel> nextPage = await _cefrBrowseStateService
                .GetWordsPageAsync(CefrLevel, _loadedWordCount, PageSize, CancellationToken.None)
                .ConfigureAwait(true);

            IReadOnlyList<CefrWordItemViewModel> viewModels = nextPage
                .Select(word => new CefrWordItemViewModel(
                    word.PublicId,
                    string.IsNullOrWhiteSpace(word.Article) ? word.Lemma : $"{word.Article} {word.Lemma}",
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray();

            foreach (CefrWordItemViewModel word in viewModels)
            {
                _visibleWords.Add(word);
            }

            _loadedWordCount += viewModels.Count;
            _hasMoreWords = viewModels.Count == PageSize;

            ScheduleNextPagePrefetch();
        }
        catch
        {
            LoadingStateView.Message = AppStrings.CefrWordsPageLoadError;
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
    /// Loads the next result chunk when the learner scrolls near the end of the current list.
    /// </summary>
    private async void OnWordsRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        try
        {
            await LoadNextPageAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Prefetches the next CEFR result page into the browse cache without blocking the current UI flow.
    /// </summary>
    private void ScheduleNextPagePrefetch()
    {
        if (!_hasMoreWords || string.IsNullOrWhiteSpace(CefrLevel))
        {
            return;
        }

        _ = PrefetchNextPageAsync();
    }

    private async Task PrefetchNextPageAsync()
    {
        try
        {
            await _cefrBrowseStateService.GetWordsPageAsync(
            CefrLevel,
            _loadedWordCount,
            PageSize,
            CancellationToken.None).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingStateView.IsLoading = isLoading;
    }

    /// <summary>
    /// Navigates to the selected lexical-entry detail page.
    /// </summary>
    private async void OnWordsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CefrWordItemViewModel selectedWord)
        {
            return;
        }

        WordsCollectionView.SelectedItem = null;

        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString());
        string escapedCefrLevel = Uri.EscapeDataString(CefrLevel);
        try
        {
            await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}&cefrLevel={escapedCefrLevel}")
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Represents the UI model used by the CEFR browse collection view.
    /// </summary>
    private sealed record CefrWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}
