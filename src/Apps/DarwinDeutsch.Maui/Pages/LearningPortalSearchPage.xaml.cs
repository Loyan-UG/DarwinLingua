using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Provides mobile unified search over locally available Learning Portal content.
/// </summary>
public partial class LearningPortalSearchPage : ContentPage
{
    private readonly IUnifiedLearningSearchService _searchService;
    private readonly ObservableCollection<SearchResultViewModel> _results = [];
    private CancellationTokenSource? _searchCancellationTokenSource;

    public LearningPortalSearchPage(IUnifiedLearningSearchService searchService)
    {
        InitializeComponent();
        _searchService = searchService;
        ResultsCollectionView.ItemsSource = _results;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Title = AppStrings.LearningPortalItemSearch;
        HeadlineLabel.Text = AppStrings.LearningPortalItemSearch;
        QuerySearchBar.Placeholder = AppStrings.LearningPortalSearchPlaceholder;
        EmptyStateLabel.Text = AppStrings.LearningPortalSearchEmptyState;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        EmptyStateLabel.IsVisible = _results.Count == 0;
    }

    protected override void OnDisappearing()
    {
        CancelSearchRequest();
        base.OnDisappearing();
    }

    private async void OnSearchRequested(object? sender, EventArgs e)
    {
        await SearchAsync().ConfigureAwait(true);
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.NewTextValue) || e.NewTextValue.Trim().Length >= 2)
        {
            await SearchAsync().ConfigureAwait(true);
        }
    }

    private async Task SearchAsync()
    {
        ResetSearchRequest();
        CancellationToken cancellationToken = _searchCancellationTokenSource!.Token;
        string query = QuerySearchBar.Text?.Trim() ?? string.Empty;

        LoadingStateView.IsLoading = true;
        IReadOnlyList<UnifiedLearningSearchResultModel> results = await _searchService
            .SearchAsync(new UnifiedLearningSearchFilterModel(query, null, null, null, null), cancellationToken)
            .ConfigureAwait(true);

        _results.Clear();
        foreach (UnifiedLearningSearchResultModel result in results)
        {
            _results.Add(new SearchResultViewModel(
                result.ResultType,
                result.Url,
                result.Title,
                result.ShortSnippet,
                $"{result.ResultType} - {result.CefrLevel ?? AppStrings.LearningPortalAnyLevel} - {result.Category ?? AppStrings.LearningPortalAnyCategory}"));
        }

        LoadingStateView.IsLoading = false;
        EmptyStateLabel.IsVisible = _results.Count == 0;
        ResultsCollectionView.IsVisible = _results.Count > 0;
    }

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not SearchResultViewModel selected)
        {
            return;
        }

        ResultsCollectionView.SelectedItem = null;
        if (TryMapResultToMobileRoute(selected, out string route))
        {
            await Shell.Current.GoToAsync(route).ConfigureAwait(true);
        }
    }

    private static bool TryMapResultToMobileRoute(SearchResultViewModel result, out string route)
    {
        string slug = result.Url.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
        string? module = result.ResultType.ToLowerInvariant() switch
        {
            "grammar" or "grammar-topic" => "grammar",
            "expression" => "expressions",
            "exercise" or "exercise-set" => "exercises",
            "course" or "course-lesson" => "courses",
            "exam-prep" => "exam-prep",
            "writing-template" => "writing-templates",
            "cultural-note" => "cultural-notes",
            "talk-topic" => "talk-topics",
            _ => null,
        };

        if (string.IsNullOrWhiteSpace(module) || string.IsNullOrWhiteSpace(slug))
        {
            route = string.Empty;
            return false;
        }

        route = $"{nameof(LearningPortalDetailPage)}?module={Uri.EscapeDataString(module)}&slug={Uri.EscapeDataString(slug)}";
        return true;
    }

    private void ResetSearchRequest()
    {
        CancelSearchRequest();
        _searchCancellationTokenSource = new CancellationTokenSource();
    }

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

    private sealed record SearchResultViewModel(string ResultType, string Url, string Title, string ShortSnippet, string MetadataLine);
}
