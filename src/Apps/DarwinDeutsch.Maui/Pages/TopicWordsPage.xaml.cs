using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current lexical entries linked to a selected topic.
/// </summary>
[QueryProperty(nameof(TopicKey), "topicKey")]
[QueryProperty(nameof(TopicTitle), "topicTitle")]
public partial class TopicWordsPage : ContentPage
{
    private const int PageSize = 24;
    private readonly IWordQueryService _wordQueryService;
    private readonly IUserLearningProfileService _userLearningProfileService;
    private readonly ObservableCollection<TopicWordItemViewModel> _visibleWords = [];
    private IReadOnlyList<TopicWordItemViewModel> _allWords = [];
    private int _loadedWordCount;
    private bool _isLoadingMore;
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

        await RefreshAsync().ConfigureAwait(true);
    }

    /// <summary>
    /// Loads the current topic words using the user's preferred meaning language.
    /// </summary>
    private async Task RefreshAsync()
    {
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

            ShowWords(words
                .Select(word => new TopicWordItemViewModel(
                    word.PublicId,
                    BuildLemmaLine(word),
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray());
        }
        catch (OperationCanceledException)
        {
            ShowLoadingState();
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
    private void ShowWords(IReadOnlyList<TopicWordItemViewModel> words)
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
        _allWords = [];
        _visibleWords.Clear();
        WordsCollectionView.IsVisible = false;
        EmptyStateLabel.IsVisible = false;
        ErrorStateLabel.IsVisible = true;
    }

    /// <summary>
    /// Loads the next visual page of topic words.
    /// </summary>
    private void LoadNextPage()
    {
        if (_isLoadingMore || _loadedWordCount >= _allWords.Count)
        {
            return;
        }

        _isLoadingMore = true;
        int nextCount = Math.Min(PageSize, _allWords.Count - _loadedWordCount);

        foreach (TopicWordItemViewModel word in _allWords.Skip(_loadedWordCount).Take(nextCount))
        {
            _visibleWords.Add(word);
        }

        _loadedWordCount += nextCount;
        _isLoadingMore = false;
    }

    /// <summary>
    /// Loads the next chunk when the learner scrolls near the end of the visible topic words.
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
