using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using System.Collections.ObjectModel;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays practical scenario lessons available on the device.
/// </summary>
public partial class ScenariosPage : ContentPage
{
    private readonly IScenarioLessonQueryService _scenarioLessonQueryService;
    private readonly ObservableCollection<ScenarioCardViewModel> _scenarios = [];
    private CancellationTokenSource? _refreshCancellationTokenSource;

    public ScenariosPage(IScenarioLessonQueryService scenarioLessonQueryService)
    {
        ArgumentNullException.ThrowIfNull(scenarioLessonQueryService);

        InitializeComponent();
        _scenarioLessonQueryService = scenarioLessonQueryService;
        ScenariosCollectionView.ItemsSource = _scenarios;
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

        Title = "Scenarios";
        HeadlineLabel.Text = "Practical German scenarios";
        DescriptionLabel.Text = "Practice short dialogues, useful phrases, and quick checks for real situations.";
        EmptyStateLabel.Text = "No scenarios are available yet.";
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        LoadingStateView.IsLoading = true;

        IReadOnlyList<ScenarioLessonListItemModel> scenarios = await _scenarioLessonQueryService
            .GetPublishedScenariosAsync(cancellationToken)
            .ConfigureAwait(true);

        _scenarios.Clear();
        foreach (ScenarioLessonListItemModel scenario in scenarios)
        {
            _scenarios.Add(new ScenarioCardViewModel(
                scenario.Slug,
                scenario.Title,
                scenario.Description,
                scenario.LearnerGoal,
                $"{scenario.CefrLevel} • {scenario.Category} • {string.Join(", ", scenario.TopicKeys)}"));
        }

        LoadingStateView.IsLoading = false;
        EmptyStateLabel.IsVisible = _scenarios.Count == 0;
        ScenariosCollectionView.IsVisible = _scenarios.Count > 0;
    }

    private async void OnScenariosSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not ScenarioCardViewModel selectedScenario)
        {
            return;
        }

        ScenariosCollectionView.SelectedItem = null;
        string escapedSlug = Uri.EscapeDataString(selectedScenario.Slug);

        try
        {
            await Shell.Current.GoToAsync($"{nameof(ScenarioDetailPage)}?scenarioSlug={escapedSlug}").ConfigureAwait(true);
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

    private sealed record ScenarioCardViewModel(
        string Slug,
        string Title,
        string Description,
        string Goal,
        string MetadataLine);
}
