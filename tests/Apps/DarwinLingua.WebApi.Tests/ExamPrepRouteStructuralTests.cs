namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class ExamPrepRouteStructuralTests
{
    [Fact]
    public void WebApiProgram_ShouldExposeExamPrepLocalizedListDetailAndSearchRoutes()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/catalog/exam-profiles\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/exam-prep\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/exam-prep/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("IExamPrepQueryService", programSource, StringComparison.Ordinal);
        Assert.Contains("ExamPrepListFilterModel", programSource, StringComparison.Ordinal);
        Assert.Contains("primaryMeaningLanguageCode", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/search\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"exam-prep\"", File.ReadAllText(ResolveRepositoryPath("src", "Modules", "Catalog", "DarwinLingua.Catalog.Application", "Services", "UnifiedLearningSearchService.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void WebExamPrepRoutesAndViews_ShouldRenderLearnerLanguageHelpers()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "ExamPrepController.cs"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ExamPrep", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "ExamPrep", "Detail.cshtml"));
        string slugListSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "WritingTemplates", "_SlugList.cshtml"));
        string viewModelSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Models", "ExamPrepPageViewModels.cs"));

        Assert.Contains("IWebLearningProfileAccessor", controllerSource, StringComparison.Ordinal);
        Assert.Contains("PreferredMeaningLanguage1", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"profile/{examProfileKey}\"", controllerSource, StringComparison.Ordinal);
        Assert.Contains("RenderIndexAsync(filter", controllerSource, StringComparison.Ordinal);
        Assert.Contains("View(\"Index\"", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetExamProfilesAsync(primaryMeaningLanguageCode", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetExamPrepUnitsAsync(filter, primaryMeaningLanguageCode", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetExamPrepUnitBySlugAsync(slug, primaryMeaningLanguageCode", controllerSource, StringComparison.Ordinal);
        Assert.Contains("PrimaryMeaningLanguageCode", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("selectedProfile", indexSource, StringComparison.Ordinal);
        Assert.Contains("selectedProfile.DisplayName", indexSource, StringComparison.Ordinal);
        Assert.Contains("selectedProfile is null && Model.Profiles.Count > 0", indexSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageDisplayName", indexSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageShortDescription", indexSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageExplanation", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageStrategyNotes", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageChecklist", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearningContentLinkResolver.ResolveHref", slugListSource, StringComparison.Ordinal);
        Assert.Contains("\"dialogue\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"writing-template\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"grammar-topic\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"exercise\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"roleplay\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"talk-topic\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"expression\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"course-lesson\"", detailSource, StringComparison.Ordinal);
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
