using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays local lexical search results for German lemma queries.
/// </summary>
public partial class SearchWordsPage : ContentPage
{
    private static readonly TimeSpan SearchDebounceDelay = TimeSpan.FromMilliseconds(280);
    private const int PrefetchResultCount = 4;
    private readonly IWordSearchCacheService _wordSearchCacheService;
    private readonly IWordDetailCacheService _wordDetailCacheService;
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ILogger<SearchWordsPage> _logger;
    private CancellationTokenSource? _searchCancellationTokenSource;
    private CancellationTokenSource? _debounceCancellationTokenSource;
    private string _lastCompletedQuery = string.Empty;
    private IReadOnlyList<SearchWordItemViewModel> _lastResults = Array.Empty<SearchWordItemViewModel>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchWordsPage"/> class.
    /// </summary>
    public SearchWordsPage(
        IWordSearchCacheService wordSearchCacheService,
        IWordDetailCacheService wordDetailCacheService,
        IActiveLearningProfileCacheService activeLearningProfileCacheService,
        IPerformanceTelemetryService performanceTelemetryService,
        ILogger<SearchWordsPage> logger)
    {
        ArgumentNullException.ThrowIfNull(wordSearchCacheService);
        ArgumentNullException.ThrowIfNull(wordDetailCacheService);
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);
        ArgumentNullException.ThrowIfNull(logger);

        InitializeComponent();

        _wordSearchCacheService = wordSearchCacheService;
        _wordDetailCacheService = wordDetailCacheService;
        _activeLearningProfileCacheService = activeLearningProfileCacheService;
        _performanceTelemetryService = performanceTelemetryService;
        _logger = logger;

        ApplyLocalizedText();
    }

    /// <summary>
    /// Re-applies localized text whenever the page becomes visible.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();
    }

    /// <summary>
    /// Cancels any in-flight search work when the page is no longer visible.
    /// </summary>
    protected override void OnDisappearing()
    {
        CancelDebounceRequest();
        CancelSearchRequest();

        base.OnDisappearing();
    }

    /// <summary>
    /// Starts a debounced search when the query text changes.
    /// </summary>
    private void OnSearchBarTextChanged(object? sender, TextChangedEventArgs e)
    {
        ScheduleDebouncedSearch(e.NewTextValue);
    }

    /// <summary>
    /// Executes the search when the user submits the search bar.
    /// </summary>
    private async void OnSearchButtonPressed(object? sender, EventArgs e)
    {
        CancelDebounceRequest();

        try
        {
            await SearchAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Navigates to the selected lexical-entry detail page.
    /// </summary>
    private async void OnWordsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not SearchWordItemViewModel selectedWord)
        {
            return;
        }

        WordsCollectionView.SelectedItem = null;

        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString());

        try
        {
            await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}")
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Applies the localized static text values to the page.
    /// </summary>
    private void ApplyLocalizedText()
    {
        Title = AppStrings.SearchWordsPageTitle;
        StatusBadgeLabel.Text = AppStrings.SearchWordsPageStatusBadge;
        HeadlineLabel.Text = AppStrings.SearchWordsPageHeadline;
        DescriptionLabel.Text = AppStrings.SearchWordsPageDescription;
        SearchSectionLabel.Text = AppStrings.SearchWordsPageSearchSectionLabel;
        SearchHintLabel.Text = AppStrings.SearchWordsPageSearchHint;
        ResultsSectionLabel.Text = AppStrings.SearchWordsPageResultsLabel;
        SearchBarControl.Placeholder = AppStrings.SearchWordsPagePlaceholder;
        EmptyStateLabel.Text = AppStrings.SearchWordsPageEmpty;
        LoadingStateLabel.Text = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.SearchWordsPageLoadError;
    }

    /// <summary>
    /// Executes the current search query against the local lexical store.
    /// </summary>
    private async Task SearchAsync()
    {
        ResetSearchRequest();
        CancellationToken cancellationToken = _searchCancellationTokenSource!.Token;
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            string query = SearchBarControl.Text ?? string.Empty;
            string normalizedQuery = query.Trim();

            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                _lastCompletedQuery = string.Empty;
                _lastResults = Array.Empty<SearchWordItemViewModel>();
                ShowResults(Array.Empty<SearchWordItemViewModel>());
                return;
            }

            if (string.Equals(_lastCompletedQuery, normalizedQuery, StringComparison.Ordinal))
            {
                _logger.LogDebug("Search skipped for unchanged query '{Query}'.", normalizedQuery);
                ShowResults(_lastResults);
                return;
            }

            UserLearningProfileModel profile = await _activeLearningProfileCacheService
                .GetCurrentProfileAsync(cancellationToken)
                .ConfigureAwait(true);

            ShowLoadingState(_lastResults.Count > 0);

            IReadOnlyList<WordListItemModel> words = await _wordSearchCacheService
                .SearchAsync(normalizedQuery, profile.PreferredMeaningLanguage1, cancellationToken)
                .ConfigureAwait(true);

            _logger.LogInformation(
                "Search completed for query '{Query}' with {ResultCount} results in {ElapsedMs} ms.",
                normalizedQuery,
                words.Count,
                stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("search.words", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, words.Count);
            _lastCompletedQuery = normalizedQuery;

            SearchWordItemViewModel[] results = words
                .Select(word => new SearchWordItemViewModel(
                    word.PublicId,
                    string.IsNullOrWhiteSpace(word.Article) ? word.Lemma : $"{word.Article} {word.Lemma}",
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray();

            _lastResults = results;
            ShowResults(results);

            ScheduleResultPrefetch(words, profile);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Search cancelled after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("search.words", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
            return;
        }
        catch
        {
            _logger.LogWarning("Search failed after {ElapsedMs} ms.", stopwatch.ElapsedMilliseconds);
            _performanceTelemetryService.Record("search.words", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            ShowErrorState();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void ScheduleDebouncedSearch(string? query)
    {
        CancelDebounceRequest();

        string normalizedQuery = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            _lastCompletedQuery = string.Empty;
            ShowResults(Array.Empty<SearchWordItemViewModel>());
            return;
        }

        _debounceCancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _debounceCancellationTokenSource.Token;
        _ = RunDebouncedSearchAsync(cancellationToken);
    }

    private async Task RunDebouncedSearchAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(SearchDebounceDelay, cancellationToken).ConfigureAwait(false);
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await SearchAsync().ConfigureAwait(true);
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Replaces any active search token with a fresh one.
    /// </summary>
    private void ResetSearchRequest()
    {
        CancelSearchRequest();
        _searchCancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Cancels and disposes the active search request token when one exists.
    /// </summary>
    private void CancelSearchRequest()
    {
        if (_searchCancellationTokenSource is null)
        {
            return;
        }

        _searchCancellationTokenSource.Cancel();
        _searchCancellationTokenSource.Dispose();
        _searchCancellationTokenSource = null;
    }

    private void CancelDebounceRequest()
    {
        if (_debounceCancellationTokenSource is null)
        {
            return;
        }

        _debounceCancellationTokenSource.Cancel();
        _debounceCancellationTokenSource.Dispose();
        _debounceCancellationTokenSource = null;
    }

    /// <summary>
    /// Applies the current search result set to the page.
    /// </summary>
    private void ShowResults(IReadOnlyList<SearchWordItemViewModel> words)
    {
        WordsCollectionView.ItemsSource = words;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = words.Count == 0;
        WordsCollectionView.IsVisible = words.Count > 0;
        SetLoadingState(false);
    }

    /// <summary>
    /// Shows the loading state while the query is being executed.
    /// </summary>
    private void ShowLoadingState(bool hasExistingResults)
    {
        SetLoadingState(true);
        ErrorStateLabel.IsVisible = false;

        if (hasExistingResults)
        {
            EmptyStateLabel.IsVisible = false;
            WordsCollectionView.IsVisible = true;
            return;
        }

        EmptyStateLabel.IsVisible = false;
        WordsCollectionView.IsVisible = false;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingStateLayout.IsVisible = isLoading;
        LoadingActivityIndicator.IsRunning = isLoading;
    }

    /// <summary>
    /// Shows the generic error state when the query cannot be completed.
    /// </summary>
    private void ShowErrorState()
    {
        WordsCollectionView.ItemsSource = Array.Empty<SearchWordItemViewModel>();
        WordsCollectionView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        ErrorStateLabel.IsVisible = true;
    }

    private void ScheduleResultPrefetch(IReadOnlyList<WordListItemModel> words, UserLearningProfileModel profile)
    {
        IReadOnlyList<WordListItemModel> prefetchCandidates = words
            .Take(PrefetchResultCount)
            .ToArray();

        if (prefetchCandidates.Count == 0)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            foreach (WordListItemModel candidate in prefetchCandidates)
            {
                try
                {
                    await _wordDetailCacheService
                        .PrefetchWordDetailsAsync(
                            candidate.PublicId,
                            profile.PreferredMeaningLanguage1,
                            profile.PreferredMeaningLanguage2,
                            profile.UiLanguageCode,
                            CancellationToken.None)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(exception, "Search result prefetch failed for word {WordPublicId}.", candidate.PublicId);
                }
            }
        });
    }

    /// <summary>
    /// Represents the UI model used by the search results collection view.
    /// </summary>
    private sealed record SearchWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}
