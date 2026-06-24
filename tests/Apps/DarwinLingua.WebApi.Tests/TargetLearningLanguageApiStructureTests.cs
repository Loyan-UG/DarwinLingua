namespace DarwinLingua.WebApi.Tests;

using DarwinLingua.SharedKernel.Globalization;
using Xunit;

public sealed class TargetLearningLanguageApiStructureTests
{
    [Fact]
    public void CoreCatalogEndpoints_ShouldAcceptAndValidateTargetLearningLanguage()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("string? targetLearningLanguageCode", programSource, StringComparison.Ordinal);
        Assert.Contains("static string ResolveTargetLearningLanguageCode(string? requested)", programSource, StringComparison.Ordinal);
        Assert.Contains("ContentLanguageRequirements.DefaultTargetLearningLanguageCode", programSource, StringComparison.Ordinal);
        Assert.Contains("TargetLearningLanguageCatalog.TryFindActive", programSource, StringComparison.Ordinal);
        Assert.Contains("Unsupported target learning language", programSource, StringComparison.Ordinal);
        Assert.Contains("throw new DomainRuleException($\"Unsupported target learning language", programSource, StringComparison.Ordinal);
        Assert.Contains("GetPublishedGrammarTopicsAsync(", programSource, StringComparison.Ordinal);
        Assert.Contains("GetPublishedExpressionsAsync(", programSource, StringComparison.Ordinal);
        Assert.Contains("GetPublishedCoursePathsAsync(", programSource, StringComparison.Ordinal);
        Assert.Contains("ResolveTargetLearningLanguageCode(targetLearningLanguageCode)", programSource, StringComparison.Ordinal);
        Assert.Contains("new UnifiedLearningSearchFilterModel(", programSource, StringComparison.Ordinal);
        Assert.Contains("ResolveTargetLearningLanguageCode(targetLearningLanguageCode))", programSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TargetLearningLanguageResolver_ShouldRejectInactiveAndUnsupportedTargets()
    {
        Assert.True(TargetLearningLanguageCatalog.TryFindActive("de", out TargetLearningLanguageDefinition german));
        Assert.Equal("de", german.Code);
        Assert.False(TargetLearningLanguageCatalog.TryFindActive("en", out _));
        Assert.False(TargetLearningLanguageCatalog.TryFindActive("xx", out _));

        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string resolverSource = ExtractMethod(programSource, "static string ResolveTargetLearningLanguageCode(string? requested)");

        Assert.Contains("if (string.IsNullOrWhiteSpace(requested))", resolverSource, StringComparison.Ordinal);
        Assert.Contains("return ContentLanguageRequirements.DefaultTargetLearningLanguageCode;", resolverSource, StringComparison.Ordinal);
        Assert.Contains("TargetLearningLanguageCatalog.TryFindActive(requested", resolverSource, StringComparison.Ordinal);
        Assert.Contains("return language.Code;", resolverSource, StringComparison.Ordinal);
        Assert.Contains("throw new DomainRuleException", resolverSource, StringComparison.Ordinal);
        Assert.DoesNotContain("NormalizeOrDefault", resolverSource, StringComparison.Ordinal);
        Assert.DoesNotContain("return ContentLanguageRequirements.DefaultTargetLearningLanguageCode;\r\n}", resolverSource, StringComparison.Ordinal);
    }

    [Fact]
    public void CountryContextResolver_ShouldValidateAgainstResolvedActiveTargetLanguage()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string resolverSource = ExtractMethod(programSource, "static string ResolveCountryContextCode(string requested, string? requestedTargetLearningLanguageCode)");

        Assert.Contains("string targetLearningLanguageCode = ResolveTargetLearningLanguageCode(requestedTargetLearningLanguageCode);", resolverSource, StringComparison.Ordinal);
        Assert.Contains("CountryContextCatalog.TryFindActive", resolverSource, StringComparison.Ordinal);
        Assert.Contains("normalized, targetLearningLanguageCode", resolverSource, StringComparison.Ordinal);
        Assert.Contains("throw new DomainRuleException", resolverSource, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GrammarTopicRepository.cs")]
    [InlineData("ExpressionRepository.cs")]
    [InlineData("CourseRepository.cs")]
    [InlineData("DialogueLessonRepository.cs")]
    [InlineData("TalkTopicRepository.cs")]
    [InlineData("RoleplayScenarioRepository.cs")]
    [InlineData("ExerciseRepository.cs")]
    [InlineData("WritingTemplateRepository.cs")]
    [InlineData("CountryGuidanceNoteRepository.cs")]
    [InlineData("ExamPrepRepository.cs")]
    public void CoreCatalogRepositories_ShouldFilterPublishedRowsByTargetLearningLanguage(string repositoryFileName)
    {
        string repositorySource = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Modules",
            "Catalog",
            "DarwinLingua.Catalog.Infrastructure",
            "Repositories",
            repositoryFileName));

        Assert.Contains("NormalizeRequiredLanguageCode(targetLearningLanguageCode)", repositorySource, StringComparison.Ordinal);
        Assert.Contains(".TargetLearningLanguageCode == targetLanguageCode", repositorySource, StringComparison.Ordinal);
        Assert.Contains("ContentLanguageRequirements.DefaultTargetLearningLanguageCode", repositorySource, StringComparison.Ordinal);
    }

    [Fact]
    public void UnifiedLearningSearchRepository_ShouldFilterTargetScopedResultTypes()
    {
        string repositorySource = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Modules",
            "Catalog",
            "DarwinLingua.Catalog.Infrastructure",
            "Repositories",
            "UnifiedLearningSearchRepository.cs"));

        Assert.Contains("TargetLearningLanguageScope.NormalizeOrDefault(filter.TargetLearningLanguageCode)", repositorySource, StringComparison.Ordinal);
        Assert.Contains("word.LanguageCode == languageCode", repositorySource, StringComparison.Ordinal);
        Assert.Contains("item.TargetLearningLanguageCode == targetLanguageCode", repositorySource, StringComparison.Ordinal);
        Assert.Contains("BuildLearnUrl(targetLanguageCode", repositorySource, StringComparison.Ordinal);
        Assert.Contains("$\"/learn/{targetLanguageCode}/courses/{item.CoursePathSlug}/{item.Slug}\"", repositorySource, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine(new[] { directory }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository file '{string.Join(Path.DirectorySeparatorChar, segments)}'.");
    }

    private static string ExtractMethod(string source, string signature)
    {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"Could not find method signature '{signature}'.");
        }

        int bodyStart = source.IndexOf('{', start);
        if (bodyStart < 0)
        {
            throw new InvalidOperationException($"Could not find method body for '{signature}'.");
        }

        int depth = 0;
        for (int i = bodyStart; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source[start..(i + 1)];
                }
            }
        }

        throw new InvalidOperationException($"Could not read method body for '{signature}'.");
    }
}
