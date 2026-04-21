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
        string settingsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/SettingsController.cs");
        string wordDetailViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Words/Detail.cshtml");
        string settingsViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Settings/Index.cshtml");
        string adminControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/DashboardController.cs");
        string adminViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Dashboard/Index.cshtml");
        string manifestPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/manifest.webmanifest");
        string serviceWorkerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/sw.js");
        string siteCssPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/css/site.css");
        string siteJsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/wwwroot/js/site.js");
        string packageJsonPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/package.json");
        string tailwindConfigPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/tailwind.config.js");
        string tailwindInputPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Styles/tailwind.css");

        Assert.True(File.Exists(webProjectPath), $"Web project file not found: {webProjectPath}");
        Assert.True(File.Exists(programPath), $"Web Program.cs file not found: {programPath}");
        Assert.True(File.Exists(layoutPath), $"Web layout file not found: {layoutPath}");
        Assert.True(File.Exists(homeViewPath), $"Web home view not found: {homeViewPath}");
        Assert.True(File.Exists(installViewPath), $"Web install view not found: {installViewPath}");
        Assert.True(File.Exists(browseControllerPath), $"Browse controller not found: {browseControllerPath}");
        Assert.True(File.Exists(searchControllerPath), $"Search controller not found: {searchControllerPath}");
        Assert.True(File.Exists(wordsControllerPath), $"Words controller not found: {wordsControllerPath}");
        Assert.True(File.Exists(favoritesControllerPath), $"Favorites controller not found: {favoritesControllerPath}");
        Assert.True(File.Exists(settingsControllerPath), $"Settings controller not found: {settingsControllerPath}");
        Assert.True(File.Exists(wordDetailViewPath), $"Word detail view not found: {wordDetailViewPath}");
        Assert.True(File.Exists(settingsViewPath), $"Settings view not found: {settingsViewPath}");
        Assert.True(File.Exists(adminControllerPath), $"Admin dashboard controller not found: {adminControllerPath}");
        Assert.True(File.Exists(adminViewPath), $"Admin dashboard view not found: {adminViewPath}");
        Assert.True(File.Exists(manifestPath), $"PWA manifest not found: {manifestPath}");
        Assert.True(File.Exists(serviceWorkerPath), $"Service worker file not found: {serviceWorkerPath}");
        Assert.True(File.Exists(siteCssPath), $"Site CSS file not found: {siteCssPath}");
        Assert.True(File.Exists(siteJsPath), $"Site JS file not found: {siteJsPath}");
        Assert.True(File.Exists(packageJsonPath), $"package.json file not found: {packageJsonPath}");
        Assert.True(File.Exists(tailwindConfigPath), $"tailwind.config.js file not found: {tailwindConfigPath}");
        Assert.True(File.Exists(tailwindInputPath), $"Tailwind input file not found: {tailwindInputPath}");

        string programSource = File.ReadAllText(programPath);
        string layoutSource = File.ReadAllText(layoutPath);
        string homeViewSource = File.ReadAllText(homeViewPath);
        string installViewSource = File.ReadAllText(installViewPath);
        string browseControllerSource = File.ReadAllText(browseControllerPath);
        string searchControllerSource = File.ReadAllText(searchControllerPath);
        string wordsControllerSource = File.ReadAllText(wordsControllerPath);
        string favoritesControllerSource = File.ReadAllText(favoritesControllerPath);
        string settingsControllerSource = File.ReadAllText(settingsControllerPath);
        string wordDetailViewSource = File.ReadAllText(wordDetailViewPath);
        string settingsViewSource = File.ReadAllText(settingsViewPath);
        string adminControllerSource = File.ReadAllText(adminControllerPath);
        string adminViewSource = File.ReadAllText(adminViewPath);
        string manifestSource = File.ReadAllText(manifestPath);
        string serviceWorkerSource = File.ReadAllText(serviceWorkerPath);
        string siteCssSource = File.ReadAllText(siteCssPath);
        string siteJsSource = File.ReadAllText(siteJsPath);
        string packageJsonSource = File.ReadAllText(packageJsonPath);

        Assert.Contains("AddControllersWithViews", programSource, StringComparison.Ordinal);
        Assert.Contains("AddOutputCache", programSource, StringComparison.Ordinal);
        Assert.Contains("AddResponseCompression", programSource, StringComparison.Ordinal);
        Assert.Contains("Content-Security-Policy", programSource, StringComparison.Ordinal);
        Assert.Contains("CatalogBrowse", programSource, StringComparison.Ordinal);
        Assert.Contains("{area:exists}", programSource, StringComparison.Ordinal);
        Assert.Contains("public partial class Program", programSource, StringComparison.Ordinal);
        Assert.Contains("manifest.webmanifest", layoutSource, StringComparison.Ordinal);
        Assert.Contains("htmx.org", layoutSource, StringComparison.Ordinal);
        Assert.Contains("install-banner", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Darwin Lingua now has a real MVC learner surface", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("ConnectionStrings__SharedCatalog", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("Installable Web App", installViewSource, StringComparison.Ordinal);
        Assert.Contains("GetWordsByTopicPageAsync", browseControllerSource, StringComparison.Ordinal);
        Assert.Contains("SearchWordsAsync", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("TrackWordViewedAsync", wordsControllerSource, StringComparison.Ordinal);
        Assert.Contains("ToggleKnown", wordsControllerSource, StringComparison.Ordinal);
        Assert.Contains("GetFavoriteWordsAsync", favoritesControllerSource, StringComparison.Ordinal);
        Assert.Contains("UpdateMeaningLanguagePreferencesAsync", settingsControllerSource, StringComparison.Ordinal);
        Assert.Contains("Word forms", wordDetailViewSource, StringComparison.Ordinal);
        Assert.Contains("Mark difficult", wordDetailViewSource, StringComparison.Ordinal);
        Assert.Contains("Save preferences", settingsViewSource, StringComparison.Ordinal);
        Assert.Contains("[Area(\"Admin\")]", adminControllerSource, StringComparison.Ordinal);
        Assert.Contains("Operator shell baseline", adminViewSource, StringComparison.Ordinal);
        Assert.Contains("\"display\": \"standalone\"", manifestSource, StringComparison.Ordinal);
        Assert.Contains("shellCacheName", serviceWorkerSource, StringComparison.Ordinal);
        Assert.Contains("--accent", siteCssSource, StringComparison.Ordinal);
        Assert.Contains("beforeinstallprompt", siteJsSource, StringComparison.Ordinal);
        Assert.Contains("\"tailwindcss\"", packageJsonSource, StringComparison.Ordinal);
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
