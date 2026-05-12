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
    private readonly ShellContent _speakContent;
    private readonly ShellContent _prepareContent;
    private readonly ShellContent _resourcesContent;

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
        Navigated += OnShellNavigated;

        _homeContent = new ShellContent
        {
            Route = "home",
            ContentTemplate = CreatePageTemplate<LearningPortalPage>(),
        };

        _practiceContent = new ShellContent
        {
            Route = "practice",
            ContentTemplate = CreatePageTemplate<PracticePage>(),
        };

        _speakContent = new ShellContent
        {
            Route = "speak",
            ContentTemplate = CreatePageTemplate<DialoguesPage>(),
        };

        _prepareContent = new ShellContent
        {
            Route = "prepare",
            ContentTemplate = CreatePageTemplate<ConversationStartersPage>(),
        };

        _resourcesContent = new ShellContent
        {
            Route = "resources",
            ContentTemplate = CreatePageTemplate<TopicsPage>(),
        };

        Routing.RegisterRoute(nameof(LearningPortalPage), typeof(LearningPortalPage));
        Routing.RegisterRoute(nameof(LearningPortalListPage), typeof(LearningPortalListPage));
        Routing.RegisterRoute(nameof(LearningPortalDetailPage), typeof(LearningPortalDetailPage));
        Routing.RegisterRoute(nameof(LearningPortalSearchPage), typeof(LearningPortalSearchPage));
        Routing.RegisterRoute(nameof(TopicsPage), typeof(TopicsPage));
        Routing.RegisterRoute(nameof(TopicWordsPage), typeof(TopicWordsPage));
        Routing.RegisterRoute(nameof(CollectionsPage), typeof(CollectionsPage));
        Routing.RegisterRoute(nameof(CollectionWordsPage), typeof(CollectionWordsPage));
        Routing.RegisterRoute(nameof(DialoguesPage), typeof(DialoguesPage));
        Routing.RegisterRoute(nameof(DialogueDetailPage), typeof(DialogueDetailPage));
        Routing.RegisterRoute(nameof(ConversationStartersPage), typeof(ConversationStartersPage));
        Routing.RegisterRoute(nameof(ConversationStarterDetailPage), typeof(ConversationStarterDetailPage));
        Routing.RegisterRoute(nameof(EventPreparationPackDetailPage), typeof(EventPreparationPackDetailPage));
        Routing.RegisterRoute(nameof(CefrWordsPage), typeof(CefrWordsPage));
        Routing.RegisterRoute(nameof(SearchWordsPage), typeof(SearchWordsPage));
        Routing.RegisterRoute(nameof(WordDetailPage), typeof(WordDetailPage));
        Routing.RegisterRoute(nameof(PracticeSessionPage), typeof(PracticeSessionPage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));
        Routing.RegisterRoute(nameof(AccountPage), typeof(AccountPage));
        Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
        Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

        Items.Add(new TabBar
        {
            Items =
            {
                _homeContent,
                _practiceContent,
                _speakContent,
                _prepareContent,
                _resourcesContent,
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
            Navigated -= OnShellNavigated;
        }

        base.OnHandlerChanging(args);
    }

    /// <summary>
    /// Applies localized titles to the shell and its navigation items.
    /// </summary>
    private void ApplyLocalizedShellText()
    {
        Title = AppStrings.AppTitle;
        _homeContent.Title = AppStrings.LearningPortalGroupLearn;
        _practiceContent.Title = AppStrings.PracticeTabTitle;
        _speakContent.Title = AppStrings.LearningPortalGroupSpeak;
        _prepareContent.Title = AppStrings.LearningPortalGroupPrepare;
        _resourcesContent.Title = AppStrings.LearningPortalGroupResources;
    }

    private async void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        if (_isNormalizingHomeSelection)
        {
            return;
        }

        if (e.Source is not (ShellNavigationSource.ShellItemChanged or ShellNavigationSource.ShellSectionChanged))
        {
            return;
        }

        if (!ShouldNormalizeHomeSelection())
        {
            return;
        }

        await NormalizeHomeSelectionAsync().ConfigureAwait(true);
    }

    private bool ShouldNormalizeHomeSelection()
    {
        if (!IsHomeContentSelected())
        {
            return false;
        }

        string location = CurrentState.Location.OriginalString;
        if (string.IsNullOrWhiteSpace(location))
        {
            return false;
        }

        if (string.Equals(location, "//home", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return location.StartsWith("//home/", StringComparison.OrdinalIgnoreCase)
            || location.Contains("/home/", StringComparison.OrdinalIgnoreCase)
            || location.EndsWith("/home", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsHomeContentSelected()
    {
        return string.Equals(
            CurrentItem?.CurrentItem?.CurrentItem?.Route,
            _homeContent.Route,
            StringComparison.OrdinalIgnoreCase);
    }

    private async Task NormalizeHomeSelectionAsync()
    {
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
