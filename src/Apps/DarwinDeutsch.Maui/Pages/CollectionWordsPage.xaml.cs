using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the words belonging to a curated collection.
/// </summary>
[QueryProperty(nameof(CollectionSlug), "collectionSlug")]
public partial class CollectionWordsPage : ContentPage
{
    private readonly IWordCollectionBrowseStateService _wordCollectionBrowseStateService;
    private readonly ObservableCollection<CollectionWordItemViewModel> _visibleWords = [];
    private string _collectionSlug = string.Empty;
    private string _loadedCollectionSlug = string.Empty;
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public CollectionWordsPage(IWordCollectionBrowseStateService wordCollectionBrowseStateService)
    {
        ArgumentNullException.ThrowIfNull(wordCollectionBrowseStateService);

        InitializeComponent();
        _wordCollectionBrowseStateService = wordCollectionBrowseStateService;
        WordsCollectionView.ItemsSource = _visibleWords;
    }

    public string CollectionSlug
    {
        get => _collectionSlug;
        set => _collectionSlug = Uri.UnescapeDataString(value ?? string.Empty);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshAsync(force: false).ConfigureAwait(true);
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

    private async Task RefreshAsync(bool force)
    {
        if (!force &&
            !string.IsNullOrWhiteSpace(CollectionSlug) &&
            string.Equals(_loadedCollectionSlug, CollectionSlug, StringComparison.OrdinalIgnoreCase) &&
            _visibleWords.Count > 0)
        {
            return;
        }

        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CollectionsWordsPageLoadError;
        EmptyStateLabel.Text = AppStrings.CollectionsWordsPageEmpty;
        LoadingStateView.IsLoading = true;
        ErrorStateLabel.IsVisible = false;

        if (string.IsNullOrWhiteSpace(CollectionSlug))
        {
            ShowWords(null);
            return;
        }

        try
        {
            WordCollectionDetailModel? collection = await _wordCollectionBrowseStateService
                .GetCollectionAsync(CollectionSlug, cancellationToken)
                .ConfigureAwait(true);

            Title = collection?.Name ?? AppStrings.CollectionsWordsPageTitle;
            HeadlineLabel.Text = collection?.Name ?? AppStrings.CollectionsWordsPageTitle;
            DescriptionLabel.Text = string.IsNullOrWhiteSpace(collection?.Description)
                ? AppStrings.CollectionsWordsPageDescription
                : collection.Description;

            if (collection is null)
            {
                ShowWords([]);
                return;
            }

            ShowWords(collection.Words
                .Select(word => new CollectionWordItemViewModel(
                    word.PublicId,
                    BuildLemmaLine(word),
                    word.PrimaryMeaning ?? AppStrings.TopicWordsPageMeaningUnavailable,
                    LexiconDisplayText.FormatMetadata(word.PartOfSpeech, word.CefrLevel)))
                .ToArray());
            _loadedCollectionSlug = CollectionSlug;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            _visibleWords.Clear();
            WordsCollectionView.IsVisible = false;
            EmptyStateLabel.IsVisible = false;
            ErrorStateLabel.IsVisible = true;
        }
        finally
        {
            LoadingStateView.IsLoading = false;
        }
    }

    private void ShowWords(IReadOnlyList<CollectionWordItemViewModel>? words)
    {
        _visibleWords.Clear();
        foreach (CollectionWordItemViewModel word in words ?? [])
        {
            _visibleWords.Add(word);
        }

        WordsCollectionView.IsVisible = _visibleWords.Count > 0;
        EmptyStateLabel.IsVisible = _visibleWords.Count == 0;
        ErrorStateLabel.IsVisible = false;
    }

    private async void OnWordsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CollectionWordItemViewModel selectedWord)
        {
            return;
        }

        WordsCollectionView.SelectedItem = null;
        string wordPublicId = Uri.EscapeDataString(selectedWord.PublicId.ToString("D"));
        string collectionSlug = Uri.EscapeDataString(CollectionSlug);

        try
        {
            await Shell.Current.GoToAsync($"{nameof(WordDetailPage)}?wordPublicId={wordPublicId}&collectionSlug={collectionSlug}")
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
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

    private static string BuildLemmaLine(WordListItemModel word)
    {
        ArgumentNullException.ThrowIfNull(word);

        return string.IsNullOrWhiteSpace(word.Article)
            ? word.Lemma
            : $"{word.Article} {word.Lemma}";
    }

    private sealed record CollectionWordItemViewModel(Guid PublicId, string Lemma, string PrimaryMeaning, string MetadataLine);
}
