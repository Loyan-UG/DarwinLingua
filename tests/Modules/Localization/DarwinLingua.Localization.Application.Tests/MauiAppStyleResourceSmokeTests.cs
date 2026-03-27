namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Guards app-wide MAUI text-style resource usage against parse-time startup crashes.
/// </summary>
public sealed class MauiAppStyleResourceSmokeTests
{
    /// <summary>
    /// Verifies that MAUI XAML files use DynamicResource for shared application text styles.
    /// </summary>
    [Fact]
    public void MauiXaml_ShouldUseDynamicResourceForSharedTextStyles()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string mauiRoot = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui");
        string[] xamlPaths = Directory.GetFiles(mauiRoot, "*.xaml", SearchOption.AllDirectories);

        Assert.NotEmpty(xamlPaths);

        string[] forbiddenUsages =
        [
            "{StaticResource Headline}",
            "{StaticResource Title2}",
            "{StaticResource Body}",
        ];

        foreach (string xamlPath in xamlPaths)
        {
            string sourceCode = File.ReadAllText(xamlPath);

            foreach (string forbiddenUsage in forbiddenUsages)
            {
                Assert.DoesNotContain(forbiddenUsage, sourceCode, StringComparison.Ordinal);
            }
        }
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
