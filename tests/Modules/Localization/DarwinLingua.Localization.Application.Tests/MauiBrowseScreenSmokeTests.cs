namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Provides structural smoke coverage for the MAUI home and browse surfaces.
/// </summary>
public sealed class MauiBrowseScreenSmokeTests
{
    /// <summary>
    /// Verifies that the home screen keeps the main dashboard browse entry points wired in XAML.
    /// </summary>
    [Fact]
    public void HomePage_ShouldExposeDashboardBrowseSections()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string homePagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/HomePage.xaml");

        Assert.True(File.Exists(homePagePath), $"Home page XAML file not found: {homePagePath}");

        string sourceCode = File.ReadAllText(homePagePath);

        Assert.Contains("CefrQuickFilterView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("SearchActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("BrowseTopicsActionBlockView", sourceCode, StringComparison.Ordinal);
        Assert.Contains("FavoritesActionBlockView", sourceCode, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the browse screen combines CEFR shortcuts, topic browsing, and navigation actions.
    /// </summary>
    [Fact]
    public void TopicsPage_ShouldActAsBrowseHub()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string topicsPagePath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/TopicsPage.xaml");
        string topicsCodeBehindPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/Pages/TopicsPage.xaml.cs");

        Assert.True(File.Exists(topicsPagePath), $"Topics page XAML file not found: {topicsPagePath}");
        Assert.True(File.Exists(topicsCodeBehindPath), $"Topics page code-behind file not found: {topicsCodeBehindPath}");

        string xamlSource = File.ReadAllText(topicsPagePath);
        string codeBehindSource = File.ReadAllText(topicsCodeBehindPath);

        Assert.Contains("CefrQuickFilterView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("SearchActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("FavoritesActionBlockView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("TopicListItemView", xamlSource, StringComparison.Ordinal);
        Assert.Contains("nameof(CefrWordsPage)", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("nameof(SearchWordsPage)", codeBehindSource, StringComparison.Ordinal);
        Assert.Contains("\"//favorites\"", codeBehindSource, StringComparison.Ordinal);
    }

    /// <summary>
    /// Resolves the repository root path by walking parent directories.
    /// </summary>
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
