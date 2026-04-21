namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Verifies that the admin area uses a dedicated layout and operator navigation.
/// </summary>
public sealed class WebAdminShellStructureTests
{
    [Fact]
    public void AdminShell_ShouldUseDedicatedLayoutAndOperatorRoutes()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string adminLayoutPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Shared/_AdminLayout.cshtml");
        string adminViewStartPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/_ViewStart.cshtml");
        string dashboardViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Dashboard/Index.cshtml");

        string adminLayoutSource = File.ReadAllText(adminLayoutPath);
        string adminViewStartSource = File.ReadAllText(adminViewStartPath);
        string dashboardViewSource = File.ReadAllText(dashboardViewPath);

        Assert.Contains("Darwin Lingua Admin", adminLayoutSource, StringComparison.Ordinal);
        Assert.Contains(">Overview<", adminLayoutSource, StringComparison.Ordinal);
        Assert.Contains(">Publishing<", adminLayoutSource, StringComparison.Ordinal);
        Assert.Contains(">Diagnostics<", adminLayoutSource, StringComparison.Ordinal);
        Assert.Contains("_AdminLayout.cshtml", adminViewStartSource, StringComparison.Ordinal);
        Assert.Contains("live catalog metrics", dashboardViewSource, StringComparison.Ordinal);
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
