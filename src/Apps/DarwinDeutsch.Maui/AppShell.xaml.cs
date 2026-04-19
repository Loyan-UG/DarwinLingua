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
    private readonly IServiceProvider _serviceProvider;
    private bool _isNormalizingHomeSelection;
    private readonly ShellContent _homeContent;
    private readonly ShellContent _practiceContent;
    private readonly ShellContent _browseContent;
    private readonly ShellContent _favoritesContent;
    private readonly ShellContent _settingsContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppShell"/> class.
    /// </summary>
    /// <param name="appLocalizationService">The service that manages UI localization.</param>
    /// <param name="serviceProvider">The service provider used to resolve tab pages lazily.</param>
    public AppShell(
        IAppLocalizationService appLocalizationService,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(appLocalizationService);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        InitializeComponent();

        _appLocalizationService = appLocalizationService;
        _serviceProvider = serviceProvider;
        Navigating += OnShellNavigating;

        _homeContent = new ShellContent
        {
            Route = "home",
            ContentTemplate = CreatePageTemplate<HomePage>(),
        };

        _practiceContent = new ShellContent
        {
            Route = "practice",
            ContentTemplate = CreatePageTemplate<PracticePage>(),
        };

        _browseContent = new ShellContent
        {
            Route = "browse",
            ContentTemplate = CreatePageTemplate<TopicsPage>(),
        };

        _favoritesContent = new ShellContent
        {
            Route = "favorites",
            ContentTemplate = CreatePageTemplate<FavoritesPage>(),
        };

        _settingsContent = new ShellContent
        {
            Route = "settings",
            ContentTemplate = CreatePageTemplate<SettingsPage>(),
        };

        Routing.RegisterRoute(nameof(TopicWordsPage), typeof(TopicWordsPage));
        Routing.RegisterRoute(nameof(CefrWordsPage), typeof(CefrWordsPage));
        Routing.RegisterRoute(nameof(SearchWordsPage), typeof(SearchWordsPage));
        Routing.RegisterRoute(nameof(WordDetailPage), typeof(WordDetailPage));
        Routing.RegisterRoute(nameof(PracticeSessionPage), typeof(PracticeSessionPage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));

        Items.Add(new TabBar
        {
            Items =
            {
                _homeContent,
                _practiceContent,
                _browseContent,
                _favoritesContent,
                _settingsContent,
            },
        });

        _appLocalizationService.CultureChanged += OnCultureChanged;

        ApplyLocalizedShellText();
    }

    private DataTemplate CreatePageTemplate<TPage>()
        where TPage : Page
        => new(() => _serviceProvider.GetRequiredService<TPage>());

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
            Navigating -= OnShellNavigating;
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
        _practiceContent.Title = AppStrings.PracticeTabTitle;
        _browseContent.Title = AppStrings.BrowseTabTitle;
        _favoritesContent.Title = AppStrings.FavoritesTabTitle;
        _settingsContent.Title = AppStrings.SettingsTabTitle;
    }

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (_isNormalizingHomeSelection)
        {
            return;
        }

        if (e.Source is not (ShellNavigationSource.ShellItemChanged or ShellNavigationSource.ShellSectionChanged))
        {
            return;
        }

        string targetLocation = e.Target.Location.OriginalString;
        if (!targetLocation.StartsWith("//home", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(targetLocation, "//home", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        e.Cancel();
        _isNormalizingHomeSelection = true;

        try
        {
            await GoToAsync("//home", false);
        }
        finally
        {
            _isNormalizingHomeSelection = false;
        }
    }
}
