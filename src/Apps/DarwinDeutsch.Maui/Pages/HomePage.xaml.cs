using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Browse;
using DarwinDeutsch.Maui.Services.Diagnostics;
using DarwinDeutsch.Maui.Services.Localization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current foundation state backed by localized strings, seeded reference data, and persisted user preferences.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ICefrBrowseStateService _cefrBrowseStateService;
    private readonly IPerformanceTelemetryService _performanceTelemetryService;
    private readonly ILogger<HomePage> _logger;
    private bool _isNavigatingToCefr;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    public HomePage(
        IAppLocalizationService appLocalizationService,
        ICefrBrowseStateService cefrBrowseStateService,
        IPerformanceTelemetryService performanceTelemetryService,
        ILogger<HomePage> logger)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(cefrBrowseStateService);
        ArgumentNullException.ThrowIfNull(performanceTelemetryService);
        ArgumentNullException.ThrowIfNull(logger);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _cefrBrowseStateService = cefrBrowseStateService;
        _performanceTelemetryService = performanceTelemetryService;
        _logger = logger;

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedText();
    }

    /// <summary>
    /// Refreshes localized copy and seeded data whenever the page becomes visible.
    /// </summary>
    protected override void OnAppearing()
    {
        base.OnAppearing();

        ApplyLocalizedText();
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
    /// Handles culture changes raised by the localization service.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedText();
    }

    /// <summary>
    /// Applies localized text to the current visual elements.
    /// </summary>
    private void ApplyLocalizedText()
    {
        Title = AppStrings.HomeTabTitle;
        LogoPlaceholderLabel.Text = AppStrings.HomeLogoPlaceholder;
        AppNameLabel.Text = AppStrings.AppTitle;
        AppSubtitleLabel.Text = AppStrings.HomeHeaderSubtitle;
        ExploreSectionLabel.Text = AppStrings.HomePageExploreSectionLabel;
        CefrQuickFilterView.Caption = AppStrings.HomeCefrBrowseLabel;
        PracticeActionBlockView.Caption = AppStrings.HomePracticeLabel;
        PracticeActionBlockView.ButtonText = AppStrings.HomePracticeButton;
        SearchActionBlockView.Caption = AppStrings.HomeSearchLabel;
        SearchActionBlockView.ButtonText = AppStrings.HomeSearchButton;
        BrowseTopicsActionBlockView.Caption = AppStrings.HomeBrowseTopicsLabel;
        BrowseTopicsActionBlockView.ButtonText = AppStrings.HomeBrowseTopicsButton;
        CollectionsActionBlockView.Caption = AppStrings.HomeCollectionsLabel;
        CollectionsActionBlockView.ButtonText = AppStrings.HomeCollectionsButton;
        FavoritesActionBlockView.Caption = AppStrings.HomeFavoritesLabel;
        FavoritesActionBlockView.ButtonText = AppStrings.HomeFavoritesButton;
        CefrNavigationStatusLabel.Text = AppStrings.CommonStateLoading;
    }

    /// <summary>
    /// Navigates to the selected CEFR browse page.
    /// </summary>
    private async void OnCefrLevelSelected(object? sender, EventArgs e)
    {
        if (_isNavigatingToCefr)
        {
            return;
        }

        string cefrLevel = CefrQuickFilterView.SelectedLevel;
        if (string.IsNullOrWhiteSpace(cefrLevel))
        {
            return;
        }

        _isNavigatingToCefr = true;
        SetCefrNavigationBusyState(true);
        Stopwatch stopwatch = Stopwatch.StartNew();
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
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Home CEFR starting-word resolution failed for level '{CefrLevel}'. Falling back to list view.", cefrLevel);
            }

            await Shell.Current.GoToAsync(navigationRoute).ConfigureAwait(true);

            _performanceTelemetryService.Record("home.cefr-navigation", stopwatch.Elapsed, PerformanceTelemetryOutcome.Success);
        }
        catch (OperationCanceledException)
        {
            _performanceTelemetryService.Record("home.cefr-navigation", stopwatch.Elapsed, PerformanceTelemetryOutcome.Cancelled);
        }
        catch (Exception exception)
        {
            _performanceTelemetryService.Record("home.cefr-navigation", stopwatch.Elapsed, PerformanceTelemetryOutcome.Failed);
            _logger.LogWarning(exception, "Home CEFR navigation failed for level '{CefrLevel}' using route '{Route}'.", cefrLevel, navigationRoute);
            CefrNavigationStatusLabel.Text = AppStrings.CommonStateError;
            CefrNavigationStatusLayout.IsVisible = true;
        }
        finally
        {
            _isNavigatingToCefr = false;
            if (CefrNavigationStatusLabel.Text == AppStrings.CommonStateError)
            {
                CefrQuickFilterView.IsEnabled = true;
                CefrNavigationActivityIndicator.IsRunning = false;
            }
            else
            {
                SetCefrNavigationBusyState(false);
            }
        }
    }

    /// <summary>
    /// Navigates to the lexical search page.
    /// </summary>
    private async void OnSearchActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SearchWordsPage)).ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the practice tab from the home dashboard.
    /// </summary>
    private async void OnPracticeActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//practice").ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the topics browse tab from the home dashboard.
    /// </summary>
    private async void OnBrowseTopicsActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//browse").ConfigureAwait(true);
    }

    private async void OnCollectionsActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CollectionsPage)).ConfigureAwait(true);
    }

    /// <summary>
    /// Navigates to the favorites tab from the home dashboard.
    /// </summary>
    private async void OnFavoritesActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//favorites").ConfigureAwait(true);
    }

    private void SetCefrNavigationBusyState(bool isBusy)
    {
        CefrQuickFilterView.IsEnabled = !isBusy;
        if (isBusy)
        {
            CefrNavigationStatusLabel.Text = AppStrings.CommonStateLoading;
        }

        CefrNavigationStatusLayout.IsVisible = isBusy;
        CefrNavigationActivityIndicator.IsRunning = isBusy;
    }
}
