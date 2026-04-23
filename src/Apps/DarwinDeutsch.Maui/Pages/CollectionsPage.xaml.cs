using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays curated word collections available on the device.
/// </summary>
public partial class CollectionsPage : ContentPage
{
    private readonly IWordCollectionBrowseStateService _wordCollectionBrowseStateService;
    private readonly ObservableCollection<WordCollectionCardViewModel> _collections = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public CollectionsPage(IWordCollectionBrowseStateService wordCollectionBrowseStateService)
    {
        ArgumentNullException.ThrowIfNull(wordCollectionBrowseStateService);

        InitializeComponent();
        _wordCollectionBrowseStateService = wordCollectionBrowseStateService;
        CollectionsCollectionView.ItemsSource = _collections;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await RefreshAsync().ConfigureAwait(true);
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

    private async Task RefreshAsync()
    {
        ResetRefreshRequest();
        CancellationToken cancellationToken = _refreshCancellationTokenSource!.Token;

        Title = AppStrings.CollectionsPageTitle;
        HeadlineLabel.Text = AppStrings.CollectionsPageHeadline;
        DescriptionLabel.Text = AppStrings.CollectionsPageDescription;
        EmptyStateLabel.Text = AppStrings.CollectionsPageEmpty;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;

        IReadOnlyList<WordCollectionListItemModel> collections = await _wordCollectionBrowseStateService
            .GetCollectionsAsync(cancellationToken)
            .ConfigureAwait(true);

        _collections.Clear();
        foreach (WordCollectionListItemModel collection in collections)
        {
            _collections.Add(new WordCollectionCardViewModel(
                collection.Slug,
                collection.Name,
                collection.Description ?? string.Empty,
                !string.IsNullOrWhiteSpace(collection.Description),
                string.Format(AppStrings.CollectionsPageCardSummaryFormat, collection.WordCount, string.Join(", ", collection.CefrLevels)),
                string.Join(" • ", collection.PreviewWords)));
        }

        LoadingStateView.IsLoading = false;
        EmptyStateLabel.IsVisible = _collections.Count == 0;
        CollectionsCollectionView.IsVisible = _collections.Count > 0;
    }

    private async void OnCollectionsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not WordCollectionCardViewModel selectedCollection)
        {
            return;
        }

        CollectionsCollectionView.SelectedItem = null;
        string escapedSlug = Uri.EscapeDataString(selectedCollection.Slug);

        try
        {
            await Shell.Current.GoToAsync($"{nameof(CollectionWordsPage)}?collectionSlug={escapedSlug}").ConfigureAwait(true);
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

    private sealed record WordCollectionCardViewModel(
        string Slug,
        string Name,
        string Description,
        bool HasDescription,
        string Summary,
        string Preview);
}
