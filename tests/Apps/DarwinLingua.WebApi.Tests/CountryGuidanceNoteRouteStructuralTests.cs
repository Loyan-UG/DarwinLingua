namespace DarwinLingua.WebApi.Tests;

using DarwinLingua.SharedKernel.Globalization;
using Xunit;

public sealed class CountryGuidanceNoteRouteStructuralTests
{
    [Fact]
    public void WebApiProgram_ShouldExposeInternalCountryGuidanceNoteApiWithLocalizedProjection()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));

        Assert.Contains("\"/api/catalog/country-guidance/{countryContextCode}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/country-guidance/{countryContextCode}/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.DoesNotContain("\"/api/catalog/cultural-notes\"", programSource, StringComparison.Ordinal);
        Assert.Contains("ICountryGuidanceNoteQueryService", programSource, StringComparison.Ordinal);
        Assert.Contains("CountryGuidanceNoteListFilterModel(cefrLevel, category, context, q)", programSource, StringComparison.Ordinal);
        Assert.Contains("ResolveCountryContextCode(countryContextCode, targetLearningLanguageCode)", programSource, StringComparison.Ordinal);
        Assert.Contains("primaryMeaningLanguageCode", programSource, StringComparison.Ordinal);
        Assert.Contains("\"country-guidance\"", File.ReadAllText(ResolveRepositoryPath("src", "Modules", "Catalog", "DarwinLingua.Catalog.Application", "Services", "UnifiedLearningSearchService.cs")), StringComparison.Ordinal);
    }

    [Fact]
    public void WebLifeInGermanyRoutesAndViews_ShouldUsePublicRouteAndLearnerLanguageHelpers()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "CountryGuidanceController.cs"));
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "CountryGuidance", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "CountryGuidance", "Detail.cshtml"));
        string slugListSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "WritingTemplates", "_SlugList.cshtml"));
        string viewModelSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Models", "CountryGuidanceNotePageViewModels.cs"));
        string searchRepositorySource = File.ReadAllText(ResolveRepositoryPath("src", "Modules", "Catalog", "DarwinLingua.Catalog.Infrastructure", "Repositories", "UnifiedLearningSearchRepository.cs"));

        Assert.Contains("LearningRouteConventions.CountryGuidance", controllerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LearningRouteConventions.LifeInGermany", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Life in Germany", indexSource, StringComparison.Ordinal);
        Assert.Contains("GetCountryGuidanceAsync(filter, targetLearningLanguageCode, normalizedCountryContextCode", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetCountryGuidanceBySlugAsync(slug, targetLearningLanguageCode, normalizedCountryContextCode", controllerSource, StringComparison.Ordinal);
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
        Assert.Contains("BuildLearnUrl(targetLanguageCode, \"country-guidance/\" + item.CountryContextCode.ToLower(), item.Slug)", searchRepositorySource, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildLearnUrl(targetLanguageCode, \"country-guidance/de\", item.Slug)", searchRepositorySource, StringComparison.Ordinal);
    }

    [Fact]
    public void CountryContextCatalog_ShouldExposePlannedContextsWithoutActivatingThemPrematurely()
    {
        Assert.Contains(CountryContextCatalog.All, context => context.Code == "DE" && context.IsActive);
        Assert.Contains(CountryContextCatalog.All, context => context.Code == "AT" && !context.IsActive && context.TargetLearningLanguageCodes.Contains("de"));
        Assert.Contains(CountryContextCatalog.All, context => context.Code == "CH" && !context.IsActive && context.TargetLearningLanguageCodes.Contains("de") && context.TargetLearningLanguageCodes.Contains("fr"));
        Assert.Contains(CountryContextCatalog.All, context => context.Code == "US" && !context.IsActive && context.TargetLearningLanguageCodes.Contains("en"));
        Assert.Contains(CountryContextCatalog.All, context => context.Code == "GB" && !context.IsActive && context.TargetLearningLanguageCodes.Contains("en"));
        Assert.Contains(CountryContextCatalog.All, context => context.Code == "AU" && !context.IsActive && context.TargetLearningLanguageCodes.Contains("en"));

        Assert.True(CountryContextCatalog.TryFindActive("de", "de", out CountryContextDefinition germany));
        Assert.Equal("DE", germany.Code);
        Assert.False(CountryContextCatalog.TryFindActive("at", "de", out _));
        Assert.False(CountryContextCatalog.TryFindActive("us", "en", out _));
        Assert.Equal("DE", CountryContextCatalog.ResolveDefaultActiveCode("de"));
    }

    [Fact]
    public void CountryGuidanceRepository_ShouldRejectInvalidCountryContextInsteadOfFallingBack()
    {
        string repositorySource = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Modules",
            "Catalog",
            "DarwinLingua.Catalog.Infrastructure",
            "Repositories",
            "CountryGuidanceNoteRepository.cs"));

        Assert.Contains("CountryContextCatalog.TryFindActive(normalized, targetLearningLanguageCode", repositorySource, StringComparison.Ordinal);
        Assert.Contains("throw new DomainRuleException($\"Country context", repositorySource, StringComparison.Ordinal);
        Assert.DoesNotContain(": ResolveDefaultCountryContextCode(targetLearningLanguageCode);", repositorySource, StringComparison.Ordinal);
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
