namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Verifies the structural learner-shell composition for the web host.
/// </summary>
public sealed class WebLearnerShellStructureTests
{
    [Fact]
    public void LearnerShell_ShouldExposePrimaryNavigationAndRecentActivityEntryPoints()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string layoutPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_Layout.cshtml");
        string homeViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Home/Index.cshtml");
        string recentViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Recent/Index.cshtml");

        string layoutSource = File.ReadAllText(layoutPath);
        string homeViewSource = File.ReadAllText(homeViewPath);
        string recentViewSource = File.ReadAllText(recentViewPath);

        Assert.Contains(">Browse<", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">Search<", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">Favorites<", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">Recent<", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">Settings<", layoutSource, StringComparison.Ordinal);
        Assert.Contains("hx-get", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("Recently viewed words", recentViewSource, StringComparison.Ordinal);
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
