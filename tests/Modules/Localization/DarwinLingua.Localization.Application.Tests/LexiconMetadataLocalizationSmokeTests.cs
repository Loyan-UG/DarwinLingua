namespace DarwinLingua.Localization.Application.Tests;

/// <summary>
/// Guards localized lexical metadata rendering in MAUI browse and detail surfaces.
/// </summary>
public sealed class LexiconMetadataLocalizationSmokeTests
{
    /// <summary>
    /// Verifies that metadata-bearing pages route part-of-speech text through the localization helper.
    /// </summary>
    [Fact]
    public void MetadataScreens_ShouldUseLocalizedLexiconDisplayTextHelper()
    {
        string repositoryRoot = ResolveRepositoryRoot();

        string[] pagePaths =
        [
            "src/Apps/DarwinDeutsch.Maui/Pages/SearchWordsPage.xaml.cs",
            "src/Apps/DarwinDeutsch.Maui/Pages/FavoritesPage.xaml.cs",
            "src/Apps/DarwinDeutsch.Maui/Pages/TopicWordsPage.xaml.cs",
            "src/Apps/DarwinDeutsch.Maui/Pages/CefrWordsPage.xaml.cs",
            "src/Apps/DarwinDeutsch.Maui/Pages/WordDetailPage.xaml.cs",
        ];

        foreach (string relativePath in pagePaths)
        {
            string fullPath = Path.Combine(repositoryRoot, relativePath);
            Assert.True(File.Exists(fullPath), $"MAUI page source file not found: {fullPath}");

            string sourceCode = File.ReadAllText(fullPath);

            Assert.Contains("LexiconDisplayText.FormatMetadata", sourceCode, StringComparison.Ordinal);

            if (relativePath.EndsWith("WordDetailPage.xaml.cs", StringComparison.Ordinal))
            {
                Assert.Contains("LexiconTagDisplayText", sourceCode, StringComparison.Ordinal);
            }

            Assert.DoesNotContain("PartOfSpeech} · {word.CefrLevel}", sourceCode, StringComparison.Ordinal);
        }
    }

    /// <summary>
    /// Verifies that the helper maps part-of-speech labels to AppStrings resource entries.
    /// </summary>
    [Fact]
    public void LexiconDisplayText_ShouldResolvePartOfSpeechLabelsFromAppStrings()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string helperPath = Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinDeutsch.Maui/Services/Localization/LexiconDisplayText.cs");

        Assert.True(File.Exists(helperPath), $"Localization helper source file not found: {helperPath}");

        string sourceCode = File.ReadAllText(helperPath);

        Assert.Contains("AppStrings.LexiconMetadataFormat", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppStrings.PartOfSpeechNoun", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppStrings.PartOfSpeechVerb", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppStrings.PartOfSpeechOther", sourceCode, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that the lexical-tag helper maps starter label keys to AppStrings resource entries.
    /// </summary>
    [Fact]
    public void LexiconTagDisplayText_ShouldResolveKnownTagsFromAppStrings()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string helperPath = Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinDeutsch.Maui/Services/Localization/LexiconTagDisplayText.cs");

        Assert.True(File.Exists(helperPath), $"Lexical-tag helper source file not found: {helperPath}");

        string sourceCode = File.ReadAllText(helperPath);

        Assert.Contains("AppStrings.WordLabelFormal", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppStrings.WordLabelInformal", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppStrings.WordLabelShopping", sourceCode, StringComparison.Ordinal);
        Assert.Contains("AppStrings.WordLabelDoctor", sourceCode, StringComparison.Ordinal);
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
