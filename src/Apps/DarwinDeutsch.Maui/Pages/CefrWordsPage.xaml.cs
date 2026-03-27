using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays lexical entries filtered by CEFR level.
/// </summary>
[QueryProperty(nameof(CefrLevel), "cefrLevel")]
public partial class CefrWordsPage : ContentPage
{
    private readonly IWordQueryService _wordQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private string _cefrLevel = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CefrWordsPage"/> class.
    /// </summary>
    public CefrWordsPage(IWordQueryService wordQueryService, IUserLearningProfileService userLearningProfileService)
    {
        ArgumentNullException.ThrowIfNull(wordQueryService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);

        InitializeComponent();

        _wordQueryService = wordQueryService;
        _userLearningProfileService = userLearningProfileService;
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
        Title = string.IsNullOrWhiteSpace(CefrLevel) ? AppStrings.CefrWordsPageTitle : CefrLevel;
        HeadlineLabel.Text = string.Format(AppStrings.CefrWordsPageHeadlineFormat, CefrLevel);
        DescriptionLabel.Text = AppStrings.CefrWordsPageDescription;
        CefrQuickFilterView.Caption = AppStrings.HomeCefrBrowseLabel;
        CefrQuickFilterView.SelectedLevel = CefrLevel;
        EmptyStateLabel.Text = AppStrings.CefrWordsPageEmpty;
        LoadingStateLabel.Text = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CommonStateError;

        if (string.IsNullOrWhiteSpace(CefrLevel))
        {
            ShowEmptyState(Array.Empty<CefrWordItemViewModel>());
            return;
        }

        ShowLoadingState();

        try
        {
            UserLearningProfileModel profile = await _userLearningProfileService
                .GetCurrentProfileAsync(CancellationToken.None)
                .ConfigureAwait(true);

            IReadOnlyList<WordListItemModel> words = await _wordQueryService
                .GetWordsByCefrAsync(CefrLevel, profile.PreferredMeaningLanguage1, CancellationToken.None)
                .ConfigureAwait(true);

            ShowEmptyState(words
                .Select(word => new CefrWordItemViewModel(
                    word.PublicId,
                    string.IsNullOrWhiteSpace(word.Article) ? word.Lemma : $"{word.Article} {word.Lemma}",
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    $"{word.PartOfSpeech} · {word.CefrLevel}"))
                .ToArray());
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
        await RefreshAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Applies the current result set to the page.
    /// </summary>
    private void ShowEmptyState(IReadOnlyList<CefrWordItemViewModel> words)
    {
        WordsCollectionView.ItemsSource = words;
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
        WordsCollectionView.ItemsSource = Array.Empty<CefrWordItemViewModel>();
        WordsCollectionView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        ErrorStateLabel.IsVisible = true;
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
        await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}")
            .ConfigureAwait(true);
    }

    /// <summary>
    /// Represents the UI model used by the CEFR browse collection view.
    /// </summary>
    private sealed record CefrWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}
