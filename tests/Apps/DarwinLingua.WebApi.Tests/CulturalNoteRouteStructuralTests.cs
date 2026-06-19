namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class CulturalNoteRouteStructuralTests
{
    [Fact]
    public void WebApiProgram_ShouldExposeInternalCulturalNoteApiWithLocalizedProjection()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/catalog/cultural-notes\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/cultural-notes/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("ICulturalNoteQueryService", programSource, StringComparison.Ordinal);
        Assert.Contains("CulturalNoteListFilterModel(cefrLevel, category, context, q)", programSource, StringComparison.Ordinal);
        Assert.Contains("primaryMeaningLanguageCode", programSource, StringComparison.Ordinal);
        Assert.Contains("\"cultural-note\"", File.ReadAllText(ResolveRepositoryPath("src", "Modules", "Catalog", "DarwinLingua.Catalog.Application", "Services", "UnifiedLearningSearchService.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void WebLifeInGermanyRoutesAndViews_ShouldUsePublicRouteAndLearnerLanguageHelpers()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "CulturalNotesController.cs"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "CulturalNotes", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "CulturalNotes", "Detail.cshtml"));
        string slugListSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "WritingTemplates", "_SlugList.cshtml"));
        string viewModelSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Models", "CulturalNotePageViewModels.cs"));
        string searchRepositorySource = File.ReadAllText(ResolveRepositoryPath("src", "Modules", "Catalog", "DarwinLingua.Catalog.Infrastructure", "Repositories", "UnifiedLearningSearchRepository.cs"));

        Assert.Contains("[Route(\"life-in-germany\")", controllerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("[Route(\"cultural-notes\")", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Life in Germany", indexSource, StringComparison.Ordinal);
        Assert.Contains("GetCulturalNotesAsync(filter, primaryMeaningLanguageCode", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetCulturalNoteBySlugAsync(slug, primaryMeaningLanguageCode", controllerSource, StringComparison.Ordinal);
        Assert.Contains("ResolveSecondaryMeaningLanguageAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("PrimaryMeaningLanguageCode", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("SecondaryMeaningLanguageCode", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.PrimaryMeaningLanguageCode)", indexSource, StringComparison.Ordinal);
        Assert.Contains("TextDirection.FromLanguageCode(Model.PrimaryMeaningLanguageCode)", detailSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", indexSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageTitle", indexSource, StringComparison.Ordinal);
        Assert.Contains("LearnerLanguageSections", detailSource, StringComparison.Ordinal);
        Assert.Contains("LinkedWritingTemplateSlugs", detailSource, StringComparison.Ordinal);
        Assert.Contains("LinkedCourseLessonSlugs", detailSource, StringComparison.Ordinal);
        Assert.Contains("LearningContentLinkResolver.ResolveHref", slugListSource, StringComparison.Ordinal);
        Assert.Contains("\"dialogue\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"expression\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"writing-template\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"talk-topic\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"course-lesson\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("$\"/life-in-germany/{item.Slug}", searchRepositorySource, StringComparison.Ordinal);
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
