using System.Xml.Linq;

namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Verifies localization resource coverage for the MAUI user-facing string catalogs.
/// </summary>
public sealed class AppStringsLocalizationCoverageTests
{
    /// <summary>
    /// Verifies that English and German AppStrings catalogs expose the same resource keys.
    /// </summary>
    [Fact]
    public void AppStrings_EnglishAndGerman_ShouldHaveMatchingResourceKeys()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string englishResourcePath = Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinDeutsch.Maui/Resources/Strings/AppStrings.resx");
        string germanResourcePath = Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinDeutsch.Maui/Resources/Strings/AppStrings.de.resx");

        IReadOnlySet<string> englishKeys = ReadResourceKeys(englishResourcePath);
        IReadOnlySet<string> germanKeys = ReadResourceKeys(germanResourcePath);

        string[] missingInGerman = englishKeys.Except(germanKeys).OrderBy(key => key).ToArray();
        string[] missingInEnglish = germanKeys.Except(englishKeys).OrderBy(key => key).ToArray();

        Assert.True(
            missingInGerman.Length == 0,
            $"German resource file is missing keys: {string.Join(", ", missingInGerman)}");
        Assert.True(
            missingInEnglish.Length == 0,
            $"English resource file is missing keys: {string.Join(", ", missingInEnglish)}");
    }

    /// <summary>
    /// Reads all user-facing resource keys from a .resx file.
    /// </summary>
    private static IReadOnlySet<string> ReadResourceKeys(string resourcePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourcePath);
        Assert.True(File.Exists(resourcePath), $"Resource file not found: {resourcePath}");

        XDocument document = XDocument.Load(resourcePath);

        HashSet<string> keys = document
            .Root?
            .Elements("data")
            .Select(element => element.Attribute("name")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToHashSet(StringComparer.Ordinal)
            ?? [];

        Assert.NotEmpty(keys);
        return keys;
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
