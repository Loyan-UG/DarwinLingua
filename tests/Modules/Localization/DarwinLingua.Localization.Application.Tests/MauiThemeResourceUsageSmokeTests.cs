using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Guards parse-time MAUI theme-resource usage outside the shared style dictionaries.
/// </summary>
public sealed class MauiThemeResourceUsageSmokeTests
{
    /// <summary>
    /// Verifies that non-style MAUI XAML only references application-scoped theme keys that exist in App.xaml.
    /// </summary>
    [Fact]
    public void NonStyleXaml_ShouldOnlyUseApplicationScopedStaticThemeKeys()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string appXamlPath = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui/App.xaml");
        HashSet<string> appKeys = ReadApplicationResourceKeys(appXamlPath);
        string mauiRoot = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui");
        string[] xamlPaths = Directory.GetFiles(mauiRoot, "*.xaml", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}Styles{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(xamlPaths);

        Regex staticResourcePattern = new(@"\{StaticResource\s+([A-Za-z0-9]+)\}", RegexOptions.Compiled);

        foreach (string xamlPath in xamlPaths)
        {
            string sourceCode = File.ReadAllText(xamlPath);
            MatchCollection matches = staticResourcePattern.Matches(sourceCode);

            foreach (Match match in matches)
            {
                string resourceKey = match.Groups[1].Value;
                Assert.Contains(
                    resourceKey,
                    appKeys);
            }
        }
    }

    /// <summary>
    /// Verifies that non-style MAUI XAML does not use nested StaticResource lookups inside AppThemeBinding.
    /// </summary>
    [Fact]
    public void NonStyleXaml_ShouldNotUseStaticResourceInsideAppThemeBinding()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string mauiRoot = Path.Combine(repositoryRoot, "src/Apps/DarwinDeutsch.Maui");
        string[] xamlPaths = Directory.GetFiles(mauiRoot, "*.xaml", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}Resources{Path.DirectorySeparatorChar}Styles{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(xamlPaths);

        foreach (string xamlPath in xamlPaths)
        {
            string sourceCode = File.ReadAllText(xamlPath);

            Assert.DoesNotContain(
                "AppThemeBinding Light={StaticResource",
                sourceCode,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "Dark={StaticResource",
                sourceCode,
                StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Resolves the application-level resource keys declared in App.xaml.
    /// </summary>
    private static HashSet<string> ReadApplicationResourceKeys(string appXamlPath)
    {
        Assert.True(File.Exists(appXamlPath), $"MAUI App.xaml file not found: {appXamlPath}");

        XDocument document = XDocument.Load(appXamlPath);
        XNamespace x = "http://schemas.microsoft.com/winfx/2009/xaml";

        return document
            .Descendants()
            .Select(element => element.Attribute(x + "Key")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal);
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
