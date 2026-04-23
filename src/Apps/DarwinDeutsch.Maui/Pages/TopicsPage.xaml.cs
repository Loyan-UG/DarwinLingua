using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using DarwinLingua.Catalog.Application.Models;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the seeded browse topics in the currently active UI language.
/// </summary>
public partial class TopicsPage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly ITopicCatalogCacheService _topicCatalogCacheService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private CancellationTokenSource? _topicsRefreshCancellationTokenSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="TopicsPage"/> class.
    /// </summary>
    public TopicsPage(
        IAppLocalizationService appLocalizationService,
        ICefrBrowseStateService cefrBrowseStateService,
        ITopicCatalogCacheService topicCatalogCacheService,
        IPerformanceTelemetryService performanceTelemetryService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);
        ArgumentNullException.ThrowIfNull(topicCatalogCacheService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _cefrBrowseStateService = cefrBrowseStateService;
        _topicCatalogCacheService = topicCatalogCacheService;
        _performanceTelemetryService = performanceTelemetryService;

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
    }

    /// <summary>
    /// Refreshes the localized topic list whenever the page becomes visible.
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();
        try
        {
            await RefreshTopicsAsync(showLoadingState: TopicsCollectionView.ItemsSource is null).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    protected override void OnDisappearing()
    {
        CancelTopicsRefreshRequest();
        base.OnDisappearing();
    }

    /// <summary>
    /// Releases event subscriptions when the page handler is detached.
    /// </summary>
    /// <param name="args">The handler-changing event arguments.</param>
    protected override void OnHandlerChanging(HandlerChangingEventArgs args)
    {
        if (args.NewHandler is null)
        {
            _appLocalizationService.CultureChanged -= OnCultureChanged;
        }

        base.OnHandlerChanging(args);
    }

    /// <summary>
    /// Handles UI culture changes raised by the localization service.
    /// </summary>
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedText();

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                await RefreshTopicsAsync(showLoadingState: false).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
            }
        });
    }

    /// <summary>
    /// Applies localized text to the page.
    /// </summary>
    private void ApplyLocalizedText()
    {
        Title = AppStrings.BrowseTabTitle;
        StatusBadgeLabel.Text = AppStrings.TopicsPageStatusBadge;
        HeadlineLabel.Text = AppStrings.TopicsPageHeadline;
        DescriptionLabel.Text = AppStrings.TopicsPageDescription;
        ShortcutsSectionLabel.Text = AppStrings.TopicsPageShortcutsLabel;
        TopicsSectionLabel.Text = AppStrings.TopicsPageTopicListLabel;
        CefrQuickFilterView.Caption = AppStrings.HomeCefrBrowseLabel;
        SearchActionBlockView.Caption = AppStrings.HomeSearchLabel;
        SearchActionBlockView.ButtonText = AppStrings.HomeSearchButton;
        CollectionsActionBlockView.Caption = AppStrings.HomeCollectionsLabel;
        CollectionsActionBlockView.ButtonText = AppStrings.HomeCollectionsButton;
        FavoritesActionBlockView.Caption = AppStrings.HomeFavoritesLabel;
        FavoritesActionBlockView.ButtonText = AppStrings.HomeFavoritesButton;
        EmptyStateLabel.Text = AppStrings.TopicsPageEmpty;
        LoadingStateView.Message = AppStrings.CommonStateLoading;
        ErrorStateLabel.Text = AppStrings.CommonStateError;
    }

    /// <summary>
    /// Loads the localized topic list for the current UI language.
    /// </summary>
    private async Task RefreshTopicsAsync(bool showLoadingState)
    {
        ResetTopicsRefreshRequest();
        CancellationToken cancellationToken = _topicsRefreshCancellationTokenSource!.Token;
        Stopwatch stopwatch = Stopwatch.StartNew();

        if (showLoadingState)
        {
            ShowLoadingState(TopicsCollectionView.ItemsSource is not null);
        }

        try
        {
            await Task.Yield();

            IReadOnlyList<TopicListItemModel> topics = await _topicCatalogCacheService
                .GetTopicsAsync(_appLocalizationService.CurrentCulture.TwoLetterISOLanguageName, cancellationToken)
                .ConfigureAwait(true);

            TopicsCollectionView.ItemsSource = topics;
            EmptyStateLabel.IsVisible = topics.Count == 0;
            TopicsCollectionView.IsVisible = topics.Count > 0;
            ErrorStateLabel.IsVisible = false;
            _performanceTelemetryService.Record("topics.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success, topics.Count);
        }
        catch (OperationCanceledException)
        {
            _performanceTelemetryService.Record("topics.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
            return;
        }
        catch
        {
            TopicsCollectionView.ItemsSource = Array.Empty<TopicListItemModel>();
            TopicsCollectionView.IsVisible = false;
            EmptyStateLabel.IsVisible = false;
            ErrorStateLabel.IsVisible = true;
            _performanceTelemetryService.Record("topics.refresh", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Shows the loading state while topic data is being fetched.
    /// </summary>
    private void ShowLoadingState(bool hasExistingTopics)
    {
        SetLoadingState(true);
        ErrorStateLabel.IsVisible = false;

        if (hasExistingTopics)
        {
            EmptyStateLabel.IsVisible = false;
            TopicsCollectionView.IsVisible = true;
            return;
        }

        EmptyStateLabel.IsVisible = false;
        TopicsCollectionView.IsVisible = false;
    }

    private void SetLoadingState(bool isLoading)
    {
        LoadingStateView.IsLoading = isLoading;
    }

    private void ResetTopicsRefreshRequest()
    {
        CancelTopicsRefreshRequest();
        _topicsRefreshCancellationTokenSource = new CancellationTokenSource();
    }

    private void CancelTopicsRefreshRequest()
    {
        if (_topicsRefreshCancellationTokenSource is null)
        {
            return;
        }

        _topicsRefreshCancellationTokenSource.Cancel();
        _topicsRefreshCancellationTokenSource.Dispose();
        _topicsRefreshCancellationTokenSource = null;
    }

    /// <summary>
    /// Navigates directly to the selected CEFR browse page from the browse hub.
    /// </summary>
    private async void OnCefrLevelSelected(object? sender, EventArgs e)
    {
        string cefrLevel = CefrQuickFilterView.SelectedLevel;
        if (string.IsNullOrWhiteSpace(cefrLevel))
        {
            return;
        }

        string escapedCefrLevel = Uri.EscapeDataString(cefrLevel);
        string navigationRoute = $"{nameof(CefrWordsPage)}?cefrLevel={escapedCefrLevel}";
        try
        {
            try
            {
                Guid? startingWordPublicId = await _cefrBrowseStateService
                    .GetStartingWordPublicIdAsync(cefrLevel, CancellationToken.None)
                    .ConfigureAwait(true);

                if (startingWordPublicId is not null)
                {
                    string escapedWordPublicId = Uri.EscapeDataString(startingWordPublicId.Value.ToString("D"));
                    navigationRoute =
                        $"{nameof(WordDetailPage)}?wordPublicId={escapedWordPublicId}&cefrLevel={escapedCefrLevel}";
                }
            }
            catch
            {
                // Fall back to the CEFR list when the preferred starting word cannot be resolved.
            }

            await Shell.Current.GoToAsync(navigationRoute).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Navigates to search from the browse hub.
    /// </summary>
    private async void OnSearchActionInvoked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(SearchWordsPage)).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Navigates to favorites from the browse hub.
    /// </summary>
    private async void OnFavoritesActionInvoked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("//favorites").ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnCollectionsActionInvoked(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(CollectionsPage)).ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>
    /// Navigates to the selected topic browse page.
    /// </summary>
    private async void OnTopicsSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not TopicListItemModel selectedTopic)
        {
            return;
        }

        TopicsCollectionView.SelectedItem = null;

        string topicKey = Uri.EscapeDataString(selectedTopic.Key);
        string topicTitle = Uri.EscapeDataString(selectedTopic.DisplayName);

        try
        {
            await Shell.Current
                .GoToAsync($"{nameof(TopicWordsPage)}?topicKey={topicKey}&topicTitle={topicTitle}")
                .ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
