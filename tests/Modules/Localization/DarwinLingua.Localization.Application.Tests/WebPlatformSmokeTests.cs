namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Provides structural smoke coverage for the initial DarwinLingua.Web host baseline.
/// </summary>
public sealed class WebPlatformSmokeTests
{
    /// <summary>
    /// Verifies that the approved MVC + Tailwind + htmx + PWA shell exists in the repository.
    /// </summary>
    [Fact]
    public void WebHost_ShouldExposeLearnerShellAdminAreaAndPwaAssets()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string webProjectPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj");
        string programPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Program.cs");
        string layoutPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_Layout.cshtml");
        string homeViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Home/Index.cshtml");
        string installViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Home/Install.cshtml");
        string browseControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/BrowseController.cs");
        string searchControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/SearchController.cs");
        string wordsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/WordsController.cs");
        string favoritesControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/FavoritesController.cs");
        string recentControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/RecentController.cs");
        string settingsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/SettingsController.cs");
        string accountControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/AccountController.cs");
        string telemetryControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/TelemetryController.cs");
        string browseResultsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Browse/_BrowseResults.cshtml");
        string wordDetailViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Words/Detail.cshtml");
        string interactionPanelPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Words/_InteractionPanel.cshtml");
        string favoriteTogglePath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_FavoriteToggle.cshtml");
        string settingsViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Settings/Index.cshtml");
        string accountViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Account/Index.cshtml");
        string recentViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Recent/Index.cshtml");
        string recentPanelPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Recent/_RecentActivityPanel.cshtml");
        string adminControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/DashboardController.cs");
        string adminPublishingControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/PublishingController.cs");
        string adminDiagnosticsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/DiagnosticsController.cs");
        string adminImportsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/ImportsController.cs");
        string adminDraftsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/DraftsController.cs");
        string adminHistoryControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/HistoryController.cs");
        string adminRollbackControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/RollbackController.cs");
        string adminViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Dashboard/Index.cshtml");
        string adminImportsViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Imports/Index.cshtml");
        string adminDraftsViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Drafts/Index.cshtml");
        string adminHistoryViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/History/Index.cshtml");
        string adminRollbackViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Rollback/Index.cshtml");
        string adminLayoutPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Shared/_AdminLayout.cshtml");
        string appHeaderPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_AppHeader.cshtml");
        string mobileNavPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_MobileNav.cshtml");
        string sectionHeadingPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_SectionHeading.cshtml");
        string metricCardPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_MetricCard.cshtml");
        string cefrBadgePath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_CefrBadge.cshtml");
        string asyncStatePanelPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_AsyncStatePanel.cshtml");
        string confirmActionModalPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_ConfirmActionModal.cshtml");
        string manifestPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/manifest.webmanifest");
        string serviceWorkerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/sw.js");
        string siteCssPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/css/site.css");
        string generatedCssPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/css/tailwind.generated.css");
        string siteJsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/js/site.js");
        string faviconPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/icons/favicon.svg");
        string maskableIconPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/icons/icon-maskable.svg");
        string packageJsonPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/package.json");
        string tailwindConfigPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/tailwind.config.js");
        string tailwindInputPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Styles/tailwind.css");
        string accessibilityChecklistPath = Path.Combine(repositoryRoot, "docs/62-Web-Accessibility-Checklist.md");

        Assert.True(File.Exists(webProjectPath), $"Web project file not found: {webProjectPath}");
        Assert.True(File.Exists(programPath), $"Web Program.cs file not found: {programPath}");
        Assert.True(File.Exists(layoutPath), $"Web layout file not found: {layoutPath}");
        Assert.True(File.Exists(homeViewPath), $"Web home view not found: {homeViewPath}");
        Assert.True(File.Exists(installViewPath), $"Web install view not found: {installViewPath}");
        Assert.True(File.Exists(browseControllerPath), $"Browse controller not found: {browseControllerPath}");
        Assert.True(File.Exists(searchControllerPath), $"Search controller not found: {searchControllerPath}");
        Assert.True(File.Exists(wordsControllerPath), $"Words controller not found: {wordsControllerPath}");
        Assert.True(File.Exists(favoritesControllerPath), $"Favorites controller not found: {favoritesControllerPath}");
        Assert.True(File.Exists(recentControllerPath), $"Recent controller not found: {recentControllerPath}");
        Assert.True(File.Exists(settingsControllerPath), $"Settings controller not found: {settingsControllerPath}");
        Assert.True(File.Exists(accountControllerPath), $"Account controller not found: {accountControllerPath}");
        Assert.True(File.Exists(telemetryControllerPath), $"Telemetry controller not found: {telemetryControllerPath}");
        Assert.True(File.Exists(browseResultsPath), $"Browse results partial not found: {browseResultsPath}");
        Assert.True(File.Exists(wordDetailViewPath), $"Word detail view not found: {wordDetailViewPath}");
        Assert.True(File.Exists(interactionPanelPath), $"Interaction panel partial not found: {interactionPanelPath}");
        Assert.True(File.Exists(favoriteTogglePath), $"Favorite toggle partial not found: {favoriteTogglePath}");
        Assert.True(File.Exists(settingsViewPath), $"Settings view not found: {settingsViewPath}");
        Assert.True(File.Exists(accountViewPath), $"Account view not found: {accountViewPath}");
        Assert.True(File.Exists(recentViewPath), $"Recent view not found: {recentViewPath}");
        Assert.True(File.Exists(recentPanelPath), $"Recent panel partial not found: {recentPanelPath}");
        Assert.True(File.Exists(adminControllerPath), $"Admin dashboard controller not found: {adminControllerPath}");
        Assert.True(File.Exists(adminPublishingControllerPath), $"Admin publishing controller not found: {adminPublishingControllerPath}");
        Assert.True(File.Exists(adminDiagnosticsControllerPath), $"Admin diagnostics controller not found: {adminDiagnosticsControllerPath}");
        Assert.True(File.Exists(adminImportsControllerPath), $"Admin imports controller not found: {adminImportsControllerPath}");
        Assert.True(File.Exists(adminDraftsControllerPath), $"Admin drafts controller not found: {adminDraftsControllerPath}");
        Assert.True(File.Exists(adminHistoryControllerPath), $"Admin history controller not found: {adminHistoryControllerPath}");
        Assert.True(File.Exists(adminRollbackControllerPath), $"Admin rollback controller not found: {adminRollbackControllerPath}");
        Assert.True(File.Exists(adminViewPath), $"Admin dashboard view not found: {adminViewPath}");
        Assert.True(File.Exists(adminImportsViewPath), $"Admin imports view not found: {adminImportsViewPath}");
        Assert.True(File.Exists(adminDraftsViewPath), $"Admin drafts view not found: {adminDraftsViewPath}");
        Assert.True(File.Exists(adminHistoryViewPath), $"Admin history view not found: {adminHistoryViewPath}");
        Assert.True(File.Exists(adminRollbackViewPath), $"Admin rollback view not found: {adminRollbackViewPath}");
        Assert.True(File.Exists(adminLayoutPath), $"Admin layout not found: {adminLayoutPath}");
        Assert.True(File.Exists(appHeaderPath), $"App header partial not found: {appHeaderPath}");
        Assert.True(File.Exists(mobileNavPath), $"Mobile nav partial not found: {mobileNavPath}");
        Assert.True(File.Exists(sectionHeadingPath), $"Section heading partial not found: {sectionHeadingPath}");
        Assert.True(File.Exists(metricCardPath), $"Metric card partial not found: {metricCardPath}");
        Assert.True(File.Exists(cefrBadgePath), $"CEFR badge partial not found: {cefrBadgePath}");
        Assert.True(File.Exists(asyncStatePanelPath), $"Async state panel partial not found: {asyncStatePanelPath}");
        Assert.True(File.Exists(confirmActionModalPath), $"Confirm action modal partial not found: {confirmActionModalPath}");
        Assert.True(File.Exists(manifestPath), $"PWA manifest not found: {manifestPath}");
        Assert.True(File.Exists(serviceWorkerPath), $"Service worker file not found: {serviceWorkerPath}");
        Assert.True(File.Exists(siteCssPath), $"Site CSS file not found: {siteCssPath}");
        Assert.True(File.Exists(generatedCssPath), $"Generated Tailwind CSS file not found: {generatedCssPath}");
        Assert.True(File.Exists(siteJsPath), $"Site JS file not found: {siteJsPath}");
        Assert.True(File.Exists(faviconPath), $"Favicon not found: {faviconPath}");
        Assert.True(File.Exists(maskableIconPath), $"Maskable icon not found: {maskableIconPath}");
        Assert.True(File.Exists(packageJsonPath), $"package.json file not found: {packageJsonPath}");
        Assert.True(File.Exists(tailwindConfigPath), $"tailwind.config.js file not found: {tailwindConfigPath}");
        Assert.True(File.Exists(tailwindInputPath), $"Tailwind input file not found: {tailwindInputPath}");
        Assert.True(File.Exists(accessibilityChecklistPath), $"Accessibility checklist not found: {accessibilityChecklistPath}");

        string programSource = File.ReadAllText(programPath);
        string layoutSource = File.ReadAllText(layoutPath);
        string homeViewSource = File.ReadAllText(homeViewPath);
        string installViewSource = File.ReadAllText(installViewPath);
        string browseControllerSource = File.ReadAllText(browseControllerPath);
        string browseViewSource = File.ReadAllText(Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Browse/Index.cshtml"));
        string searchControllerSource = File.ReadAllText(searchControllerPath);
        string wordsControllerSource = File.ReadAllText(wordsControllerPath);
        string favoritesControllerSource = File.ReadAllText(favoritesControllerPath);
        string recentControllerSource = File.ReadAllText(recentControllerPath);
        string settingsControllerSource = File.ReadAllText(settingsControllerPath);
        string accountControllerSource = File.ReadAllText(accountControllerPath);
        string telemetryControllerSource = File.ReadAllText(telemetryControllerPath);
        string browseResultsSource = File.ReadAllText(browseResultsPath);
        string wordDetailViewSource = File.ReadAllText(wordDetailViewPath);
        string interactionPanelSource = File.ReadAllText(interactionPanelPath);
        string favoriteToggleSource = File.ReadAllText(favoriteTogglePath);
        string settingsViewSource = File.ReadAllText(settingsViewPath);
        string accountViewSource = File.ReadAllText(accountViewPath);
        string recentViewSource = File.ReadAllText(recentViewPath);
        string recentPanelSource = File.ReadAllText(recentPanelPath);
        string adminControllerSource = File.ReadAllText(adminControllerPath);
        string adminPublishingControllerSource = File.ReadAllText(adminPublishingControllerPath);
        string adminDiagnosticsControllerSource = File.ReadAllText(adminDiagnosticsControllerPath);
        string adminImportsControllerSource = File.ReadAllText(adminImportsControllerPath);
        string adminDraftsControllerSource = File.ReadAllText(adminDraftsControllerPath);
        string adminHistoryControllerSource = File.ReadAllText(adminHistoryControllerPath);
        string adminRollbackControllerSource = File.ReadAllText(adminRollbackControllerPath);
        string adminViewSource = File.ReadAllText(adminViewPath);
        string adminImportsViewSource = File.ReadAllText(adminImportsViewPath);
        string adminDraftsViewSource = File.ReadAllText(adminDraftsViewPath);
        string adminHistoryViewSource = File.ReadAllText(adminHistoryViewPath);
        string adminRollbackViewSource = File.ReadAllText(adminRollbackViewPath);
        string adminLayoutSource = File.ReadAllText(adminLayoutPath);
        string appHeaderSource = File.ReadAllText(appHeaderPath);
        string mobileNavSource = File.ReadAllText(mobileNavPath);
        string sectionHeadingSource = File.ReadAllText(sectionHeadingPath);
        string metricCardSource = File.ReadAllText(metricCardPath);
        string cefrBadgeSource = File.ReadAllText(cefrBadgePath);
        string asyncStatePanelSource = File.ReadAllText(asyncStatePanelPath);
        string confirmActionModalSource = File.ReadAllText(confirmActionModalPath);
        string manifestSource = File.ReadAllText(manifestPath);
        string serviceWorkerSource = File.ReadAllText(serviceWorkerPath);
        string siteCssSource = File.ReadAllText(siteCssPath);
        string generatedCssSource = File.ReadAllText(generatedCssPath);
        string siteJsSource = File.ReadAllText(siteJsPath);
        string packageJsonSource = File.ReadAllText(packageJsonPath);
        string accessibilityChecklistSource = File.ReadAllText(accessibilityChecklistPath);

        Assert.Contains("AddControllersWithViews", programSource, StringComparison.Ordinal);
        Assert.Contains("AddOutputCache", programSource, StringComparison.Ordinal);
        Assert.Contains("AddResponseCompression", programSource, StringComparison.Ordinal);
        Assert.Contains("AddDefaultIdentity", programSource, StringComparison.Ordinal);
        Assert.Contains("AddRazorPages", programSource, StringComparison.Ordinal);
        Assert.Contains("AddAuthorization", programSource, StringComparison.Ordinal);
        Assert.Contains("Content-Security-Policy", programSource, StringComparison.Ordinal);
        Assert.Contains("CatalogBrowse", programSource, StringComparison.Ordinal);
        Assert.Contains("{area:exists}", programSource, StringComparison.Ordinal);
        Assert.Contains("public partial class Program", programSource, StringComparison.Ordinal);
        Assert.Contains("manifest.webmanifest", layoutSource, StringComparison.Ordinal);
        Assert.Contains("tailwind.generated.css", layoutSource, StringComparison.Ordinal);
        Assert.Contains("htmx.org", layoutSource, StringComparison.Ordinal);
        Assert.Contains("install-banner", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Darwin Lingua now has a real MVC learner surface", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__SharedCatalog", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("Installable Web App", installViewSource, StringComparison.Ordinal);
        Assert.Contains("GetWordsByTopicPageAsync", browseControllerSource, StringComparison.Ordinal);
        Assert.Contains("Browse_Index", browseControllerSource, StringComparison.Ordinal);
        Assert.Contains("hx-target=\"#browse-results\"", browseViewSource, StringComparison.Ordinal);
        Assert.Contains("SearchWordsAsync", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("Search_Results", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("TrackWordViewedAsync", wordsControllerSource, StringComparison.Ordinal);
        Assert.Contains("ToggleKnown", wordsControllerSource, StringComparison.Ordinal);
        Assert.Contains("RenderInteractionPanelAsync", wordsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Words_ToggleFavorite", wordsControllerSource, StringComparison.Ordinal);
        Assert.Contains("GetFavoriteWordsAsync", favoritesControllerSource, StringComparison.Ordinal);
        Assert.Contains("Favorites_Index", favoritesControllerSource, StringComparison.Ordinal);
        Assert.Contains("GetRecentWordActivityAsync", recentControllerSource, StringComparison.Ordinal);
        Assert.Contains("Recent_Panel", recentControllerSource, StringComparison.Ordinal);
        Assert.Contains("UpdatePreferencesAsync", settingsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Settings_Update", settingsControllerSource, StringComparison.Ordinal);
        Assert.Contains("[Authorize]", accountControllerSource, StringComparison.Ordinal);
        Assert.Contains("Account_Index", accountControllerSource, StringComparison.Ordinal);
        Assert.Contains("Telemetry_ClientEvent", telemetryControllerSource, StringComparison.Ordinal);
        Assert.Contains("Word forms", wordDetailViewSource, StringComparison.Ordinal);
        Assert.Contains("_InteractionPanel", wordDetailViewSource, StringComparison.Ordinal);
        Assert.Contains("hx-post", interactionPanelSource, StringComparison.Ordinal);
        Assert.Contains("Add to favorites", favoriteToggleSource, StringComparison.Ordinal);
        Assert.Contains("Save preferences", settingsViewSource, StringComparison.Ordinal);
        Assert.Contains("Manage account", accountViewSource, StringComparison.Ordinal);
        Assert.Contains("Recently viewed words", recentViewSource, StringComparison.Ordinal);
        Assert.Contains("Open word", recentPanelSource, StringComparison.Ordinal);
        Assert.Contains("[Area(\"Admin\")]", adminControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Dashboard", adminControllerSource, StringComparison.Ordinal);
        Assert.Contains("GetDashboardAsync", adminControllerSource, StringComparison.Ordinal);
        Assert.Contains("[Area(\"Admin\")]", adminPublishingControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Publishing", adminPublishingControllerSource, StringComparison.Ordinal);
        Assert.Contains("[Area(\"Admin\")]", adminDiagnosticsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Diagnostics", adminDiagnosticsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Imports", adminImportsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Drafts", adminDraftsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_HistoryPanel", adminHistoryControllerSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Rollback", adminRollbackControllerSource, StringComparison.Ordinal);
        Assert.Contains("Operator shell is now backed by live catalog metrics", adminViewSource, StringComparison.Ordinal);
        Assert.Contains("Content import dashboard", adminImportsViewSource, StringComparison.Ordinal);
        Assert.Contains("Draft batch inspection", adminDraftsViewSource, StringComparison.Ordinal);
        Assert.Contains("Publication and import history", adminHistoryViewSource, StringComparison.Ordinal);
        Assert.Contains("Open confirmation modal", adminRollbackViewSource, StringComparison.Ordinal);
        Assert.Contains("Darwin Lingua Admin", adminLayoutSource, StringComparison.Ordinal);
        Assert.Contains("Learner Web", appHeaderSource, StringComparison.Ordinal);
        Assert.Contains("aria-label=\"Mobile\"", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("headline-md", sectionHeadingSource, StringComparison.Ordinal);
        Assert.Contains("MetricCardViewModel", metricCardSource, StringComparison.Ordinal);
        Assert.Contains("chip-link--static", cefrBadgeSource, StringComparison.Ordinal);
        Assert.Contains("state-panel--", asyncStatePanelSource, StringComparison.Ordinal);
        Assert.Contains("rollback-confirmation", confirmActionModalSource, StringComparison.Ordinal);
        Assert.Contains("\"display\": \"standalone\"", manifestSource, StringComparison.Ordinal);
        Assert.Contains("\"purpose\": \"maskable\"", manifestSource, StringComparison.Ordinal);
        Assert.Contains("shellCacheName", serviceWorkerSource, StringComparison.Ordinal);
        Assert.Contains("icon-maskable.svg", serviceWorkerSource, StringComparison.Ordinal);
        Assert.Contains("--accent", siteCssSource, StringComparison.Ordinal);
        Assert.Contains(".button", generatedCssSource, StringComparison.Ordinal);
        Assert.Contains("beforeinstallprompt", siteJsSource, StringComparison.Ordinal);
        Assert.Contains("htmx:responseError", siteJsSource, StringComparison.Ordinal);
        Assert.Contains("page.load.slow", siteJsSource, StringComparison.Ordinal);
        Assert.Contains("\"tailwindcss\"", packageJsonSource, StringComparison.Ordinal);
        Assert.Contains("Keyboard Navigation", accessibilityChecklistSource, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }
}
