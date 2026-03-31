using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays lexical entries filtered by CEFR level.
/// </summary>
[QueryProperty(nameof(CefrLevel), "cefrLevel")]
public partial class CefrWordsPage : ContentPage
{
    private const int PageSize = 24;
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly ObservableCollection<CefrWordItemViewModel> _visibleWords = [];
    private IReadOnlyList<CefrWordItemViewModel> _allWords = [];
    private int _loadedWordCount;
    private bool _isLoadingMore;
    private string _cefrLevel = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CefrWordsPage"/> class.
    /// </summary>
    public CefrWordsPage(
        ICefrBrowseStateService cefrBrowseStateService)
    {
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);

        InitializeComponent();

        _cefrBrowseStateService = cefrBrowseStateService;
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

        await RefreshAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Loads the lexical entries for the selected CEFR level.
    /// </summary>
    private async Task RefreshAsync()
    {
        string resolvedCefrLevel = string.IsNullOrWhiteSpace(CefrLevel) ? AppStrings.CefrWordsPageTitle : CefrLevel;
        Title = resolvedCefrLevel;
        HeadlineLabel.Text = string.IsNullOrWhiteSpace(CefrLevel)
            ? AppStrings.CefrWordsPageTitle
            : string.Format(AppStrings.CefrWordsPageHeadlineFormat, resolvedCefrLevel);
        DescriptionLabel.Text = AppStrings.CefrWordsPageDescription;
        CefrQuickFilterView.Caption = AppStrings.HomeCefrBrowseLabel;
        CefrQuickFilterView.SelectedLevel = CefrLevel;
        EmptyStateLabel.Text = AppStrings.CefrWordsPageEmpty;
        LoadingStateLabel.Text = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CefrWordsPageLoadError;

        if (string.IsNullOrWhiteSpace(CefrLevel))
        {
            ShowWords(Array.Empty<CefrWordItemViewModel>());
            return;
        }

        ShowLoadingState();

        try
        {
            IReadOnlyList<WordListItemModel> words = await _cefrBrowseStateService
                .GetWordsAsync(CefrLevel, CancellationToken.None)
                .ConfigureAwait(true);

            _allWords = words
                .Select(word => new CefrWordItemViewModel(
                    word.PublicId,
                    string.IsNullOrWhiteSpace(word.Article) ? word.Lemma : $"{word.Article} {word.Lemma}",
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray();
            ShowWords(_allWords);
        }
        catch (OperationCanceledException)
        {
            ShowWords(Array.Empty<CefrWordItemViewModel>());
        }
        catch
        {
            ShowErrorState();
        }
        finally
        {
            LoadingStateLabel.IsVisible = false;
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

        Guid? startingWordPublicId = await _cefrBrowseStateService
            .GetStartingWordPublicIdAsync(cefrLevel, CancellationToken.None)
            .ConfigureAwait(true);

        if (startingWordPublicId is null)
        {
            await RefreshAsync().ConfigureAwait(true);
            return;
        }

        string escapedCefrLevel = Uri.EscapeDataString(cefrLevel);
        string escapedWordPublicId = Uri.EscapeDataString(startingWordPublicId.Value.ToString("D"));
        await Shell.Current.GoToAsync(
                $"{nameof(WordDetailPage)}?wordPublicId={escapedWordPublicId}&cefrLevel={escapedCefrLevel}")
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Applies the current result set to the page.
    /// </summary>
    private void ShowWords(IReadOnlyList<CefrWordItemViewModel> words)
    {
        _allWords = words;
        _visibleWords.Clear();
        _loadedWordCount = 0;
        LoadNextPage();

        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = words.Count == 0;
        WordsCollectionView.IsVisible = words.Count > 0;
    }

    /// <summary>
    /// Shows the loading state while CEFR words are being loaded.
    /// </summary>
    private void ShowLoadingState()
    {
        LoadingStateLabel.IsVisible = true;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        WordsCollectionView.IsVisible = false;
    }

    /// <summary>
    /// Shows the generic error state when CEFR word loading fails.
    /// </summary>
    private void ShowErrorState()
    {
        _allWords = [];
        _visibleWords.Clear();
        WordsCollectionView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        ErrorStateLabel.IsVisible = true;
    }

    /// <summary>
    /// Loads the next visual page of words into the collection view.
    /// </summary>
    private void LoadNextPage()
    {
        if (_isLoadingMore || _loadedWordCount >= _allWords.Count)
        {
            return;
        }

        _isLoadingMore = true;

        int nextCount = Math.Min(PageSize, _allWords.Count - _loadedWordCount);
        foreach (CefrWordItemViewModel word in _allWords.Skip(_loadedWordCount).Take(nextCount))
        {
            _visibleWords.Add(word);
        }

        _loadedWordCount += nextCount;
        _isLoadingMore = false;
    }

    /// <summary>
    /// Loads the next result chunk when the learner scrolls near the end of the current list.
    /// </summary>
    private void OnWordsRemainingItemsThresholdReached(object? sender, EventArgs e)
    {
        LoadNextPage();
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
        await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}&cefrLevel={escapedCefrLevel}")
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Represents the UI model used by the CEFR browse collection view.
    /// </summary>
    private sealed record CefrWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}
