namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class WritingTemplateRouteStructuralTests
{
    [Fact]
    public void WebApiProgram_ShouldExposeWritingTemplateLocalizedListDetailAndSearchRoutes()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/catalog/writing-templates\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/writing-templates/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("IWritingTemplateQueryService", programSource, StringComparison.Ordinal);
        Assert.Contains("WritingTemplateListFilterModel", programSource, StringComparison.Ordinal);
        Assert.Contains("primaryMeaningLanguageCode", programSource, StringComparison.Ordinal);
        Assert.Contains("\"writing-template\"", File.ReadAllText(ResolveRepositoryPath("src", "Modules", "Catalog", "DarwinLingua.Catalog.Application", "Services", "UnifiedLearningSearchService.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void WebWritingTemplateRoutesAndViews_ShouldRenderLearnerLanguageHelpers()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "WritingTemplatesController.cs"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "WritingTemplates", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "WritingTemplates", "Detail.cshtml"));
        string viewModelSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Models", "WritingTemplatePageViewModels.cs"));

        Assert.Contains("IWebLearningProfileAccessor", controllerSource, StringComparison.Ordinal);
        Assert.Contains("IWebEntitledFeatureAccessService", controllerSource, StringComparison.Ordinal);
        Assert.Contains("PreferredMeaningLanguage1", controllerSource, StringComparison.Ordinal);
        Assert.Contains("PreferredMeaningLanguage2", controllerSource, StringComparison.Ordinal);
        Assert.Contains("ResolveSecondaryMeaningLanguageAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetWritingTemplatesAsync(filter", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetWritingTemplateBySlugAsync(slug", controllerSource, StringComparison.Ordinal);
        Assert.Contains("PrimaryMeaningLanguageCode", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("SecondaryMeaningLanguageCode", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("SecondaryLanguageTemplate", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.PrimaryMeaningLanguageCode)", indexSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.PrimaryMeaningLanguageCode)", detailSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.SecondaryMeaningLanguageCode)", indexSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.SecondaryMeaningLanguageCode)", detailSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@secondaryMeaningDirection\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@secondaryMeaningDirection\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageTitle", indexSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageShortDescription", indexSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageTemplateText", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageExplanation", detailSource, StringComparison.Ordinal);
        Assert.Contains("LinkedCourseLessonSlugs", detailSource, StringComparison.Ordinal);
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
}
