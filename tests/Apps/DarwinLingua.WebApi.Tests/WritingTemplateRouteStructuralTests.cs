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
        string slugListSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "WritingTemplates", "_SlugList.cshtml"));
        string viewModelSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Models", "WritingTemplatePageViewModels.cs"));
        string siteScript = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "js", "site.js"));
        string englishResources = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", "SharedResource.en.resx"));
        string germanResources = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", "SharedResource.de.resx"));

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
        Assert.Contains("LearningContentLinkResolver.ResolveHref", slugListSource, StringComparison.Ordinal);
        Assert.Contains("\"grammar-topic\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"word\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"expression\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"exercise\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("\"course-lesson\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("<a href=\"@href\">@slug</a>", slugListSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<code>@slug</code>", slugListSource, StringComparison.Ordinal);
        Assert.Contains("data-writing-template-editor", detailSource, StringComparison.Ordinal);
        Assert.Contains("data-template-text=\"@Model.Template.TemplateText\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("data-template-variable=\"@variable\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("data-writing-template-output", detailSource, StringComparison.Ordinal);
        Assert.Contains("data-writing-template-reset", detailSource, StringComparison.Ordinal);
        Assert.Contains("Try this template", detailSource, StringComparison.Ordinal);
        Assert.Contains("Fill the variables below to preview your own version. Nothing is saved.", detailSource, StringComparison.Ordinal);

        Assert.Contains("function configureWritingTemplateEditors", siteScript, StringComparison.Ordinal);
        Assert.Contains("renderedText.split(placeholder).join(replacement || placeholder)", siteScript, StringComparison.Ordinal);
        Assert.Contains("output.textContent = renderedText", siteScript, StringComparison.Ordinal);
        Assert.DoesNotContain("output.innerHTML", siteScript, StringComparison.Ordinal);
        Assert.Contains("configureWritingTemplateEditors(document)", siteScript, StringComparison.Ordinal);

        Assert.Contains("<value>Try this template</value>", englishResources, StringComparison.Ordinal);
        Assert.Contains("<value>Diese Vorlage ausprobieren</value>", germanResources, StringComparison.Ordinal);
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
