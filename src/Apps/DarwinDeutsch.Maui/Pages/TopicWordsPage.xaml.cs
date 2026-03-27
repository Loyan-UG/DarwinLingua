using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current lexical entries linked to a selected topic.
/// </summary>
[QueryProperty(nameof(TopicKey), "topicKey")]
[QueryProperty(nameof(TopicTitle), "topicTitle")]
public partial class TopicWordsPage : ContentPage
{
    private readonly IWordQueryService _wordQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private string _topicKey = string.Empty;
    private string _topicTitle = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicWordsPage"/> class.
    /// </summary>
    public TopicWordsPage(
        IWordQueryService wordQueryService,
        IUserLearningProfileService userLearningProfileService)
    {
        ArgumentNullException.ThrowIfNull(wordQueryService);
        ArgumentNullException.ThrowIfNull(userLearningProfileService);

        InitializeComponent();

        _wordQueryService = wordQueryService;
        _userLearningProfileService = userLearningProfileService;
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

        await RefreshAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Loads the current topic words using the user's preferred meaning language.
    /// </summary>
    private async Task RefreshAsync()
    {
        Title = string.IsNullOrWhiteSpace(TopicTitle) ? AppStrings.TopicWordsPageTitle : TopicTitle;
        HeadlineLabel.Text = string.Format(AppStrings.TopicWordsPageHeadlineFormat, TopicTitle);
        DescriptionLabel.Text = AppStrings.TopicWordsPageDescription;
        EmptyStateLabel.Text = AppStrings.TopicWordsPageEmpty;
        LoadingStateLabel.Text = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CommonStateError;

        if (string.IsNullOrWhiteSpace(TopicKey))
        {
            ShowEmptyState(Array.Empty<TopicWordItemViewModel>());
            return;
        }

        ShowLoadingState();

        try
        {
            UserLearningProfileModel profile = await _userLearningProfileService
                .GetCurrentProfileAsync(CancellationToken.None)
                .ConfigureAwait(true);

            IReadOnlyList<WordListItemModel> words = await _wordQueryService
                .GetWordsByTopicAsync(TopicKey, profile.PreferredMeaningLanguage1, CancellationToken.None)
                .ConfigureAwait(true);

            ShowEmptyState(words
                .Select(word => new TopicWordItemViewModel(
                    word.PublicId,
                    BuildLemmaLine(word),
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
    /// Applies the current topic-word results and empty state visibility.
    /// </summary>
    private void ShowEmptyState(IReadOnlyList<TopicWordItemViewModel> words)
    {
        WordsCollectionView.ItemsSource = words;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = words.Count == 0;
        WordsCollectionView.IsVisible = words.Count > 0;
    }

    /// <summary>
    /// Shows the loading state while topic words are being loaded.
    /// </summary>
    private void ShowLoadingState()
    {
        LoadingStateLabel.IsVisible = true;
        ErrorStateLabel.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        WordsCollectionView.IsVisible = false;
    }

    /// <summary>
    /// Shows the generic error state when topic-word loading fails.
    /// </summary>
    private void ShowErrorState()
    {
        WordsCollectionView.ItemsSource = Array.Empty<TopicWordItemViewModel>();
        WordsCollectionView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        ErrorStateLabel.IsVisible = true;
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
