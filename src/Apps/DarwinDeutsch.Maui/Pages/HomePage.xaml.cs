using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;

namespace DarwinDeutsch.Maui.Pages;

/// <summary>
/// Displays the current foundation state backed by localized strings, seeded reference data, and persisted user preferences.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly IAppLocalizationService _appLocalizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePage"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    public HomePage(IAppLocalizationService appLocalizationService)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;

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
        FavoritesActionBlockView.Caption = AppStrings.HomeFavoritesLabel;
        FavoritesActionBlockView.ButtonText = AppStrings.HomeFavoritesButton;
    }

    /// <summary>
    /// Navigates to the selected CEFR browse page.
    /// </summary>
    private async void OnCefrLevelSelected(object? sender, EventArgs e)
    {
        string cefrLevel = CefrQuickFilterView.SelectedLevel;
        if (string.IsNullOrWhiteSpace(cefrLevel))
        {
            return;
        }

        string escapedCefrLevel = Uri.EscapeDataString(cefrLevel);
        await Shell.Current.GoToAsync($"{nameof(CefrWordsPage)}?cefrLevel={escapedCefrLevel}")
            .ConfigureAwait(true);
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

    /// <summary>
    /// Navigates to the favorites tab from the home dashboard.
    /// </summary>
    private async void OnFavoritesActionInvoked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//favorites").ConfigureAwait(true);
    }
}
