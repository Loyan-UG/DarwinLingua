using DarwinDeutsch.Maui.Pages;
using DarwinDeutsch.Maui.Resources.Strings;
using DarwinDeutsch.Maui.Services.Localization;

namespace DarwinDeutsch.Maui;

/// <summary>
/// Defines the root shell and navigation structure of the MAUI application.
/// </summary>
public partial class AppShell : Shell
{
    private readonly IAppLocalizationService _appLocalizationService;
    private readonly ShellContent _homeContent;
    private readonly ShellContent _browseContent;
    private readonly ShellContent _favoritesContent;
    private readonly ShellContent _settingsContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    /// <param name="homePage">The home page instance.</param>
    /// <param name="settingsPage">The settings page instance.</param>
    public AppShell(
        IAppLocalizationService appLocalizationService,
        HomePage homePage,
        TopicsPage topicsPage,
        FavoritesPage favoritesPage,
        SettingsPage settingsPage)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(homePage);
        ArgumentNullException.ThrowIfNull(topicsPage);
        ArgumentNullException.ThrowIfNull(favoritesPage);
        ArgumentNullException.ThrowIfNull(settingsPage);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;

        _homeContent = new ShellContent
        {
            Route = "home",
            Content = homePage,
        };

        _browseContent = new ShellContent
        {
            Route = "browse",
            Content = topicsPage,
        };

        _favoritesContent = new ShellContent
        {
            Route = "favorites",
            Content = favoritesPage,
        };

        _settingsContent = new ShellContent
        {
            Route = "settings",
            Content = settingsPage,
        };

        Routing.RegisterRoute(nameof(TopicWordsPage), typeof(TopicWordsPage));
        Routing.RegisterRoute(nameof(CefrWordsPage), typeof(CefrWordsPage));
        Routing.RegisterRoute(nameof(SearchWordsPage), typeof(SearchWordsPage));
        Routing.RegisterRoute(nameof(WordDetailPage), typeof(WordDetailPage));

        Items.Add(new TabBar
        {
            Items =
            {
                _homeContent,
                _browseContent,
                _favoritesContent,
                _settingsContent,
            },
        });

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedShellText();
    }

    /// <summary>
    /// Updates shell titles after the active culture changes.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnCultureChanged(object? sender, EventArgs e)
    {
        ApplyLocalizedShellText();
    }

    /// <summary>
    /// Releases event subscriptions when the shell handler is detached.
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
    /// Applies localized titles to the shell and its navigation items.
    /// </summary>
    private void ApplyLocalizedShellText()
    {
        Title = AppStrings.AppTitle;
        _homeContent.Title = AppStrings.HomeTabTitle;
        _browseContent.Title = AppStrings.BrowseTabTitle;
        _favoritesContent.Title = AppStrings.FavoritesTabTitle;
        _settingsContent.Title = AppStrings.SettingsTabTitle;
    }
}
