namespace DarwinLingua.Localization.Application.Tests;

using System.Text.RegularExpressions;
using System.Xml.Linq;

/// <summary>
/// Verifies the structural learner-shell composition for the web host.
/// </summary>
public sealed class WebLearnerShellStructureTests
{
    [Fact]
    public void LearnerShell_ShouldExposePrimaryNavigationAndRecentActivityEntryPoints()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string layoutPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_Layout.cshtml");
        string mobileNavPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_MobileNav.cshtml");
        string targetLanguageSwitcherPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Shared/_TargetLearningLanguageSwitcher.cshtml");
        string filterConventionsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/LearningPortalFilterConventions.cs");
        string homeViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Home/Index.cshtml");
        string recentViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Recent/Index.cshtml");

        string layoutSource = File.ReadAllText(layoutPath);
        string mobileNavSource = File.ReadAllText(mobileNavPath);
        string targetLanguageSwitcherSource = File.ReadAllText(targetLanguageSwitcherPath);
        string filterConventionsSource = File.ReadAllText(filterConventionsPath);
        string homeViewSource = File.ReadAllText(homeViewPath);
        string recentViewSource = File.ReadAllText(recentViewPath);

        Assert.Contains("@T[\"Learn\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Practice\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Speak\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Prepare\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Resources\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">@T[\"Browse\"]</a>", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">@T[\"Search\"]</a>", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">@T[\"Favorites\"]</a>", layoutSource, StringComparison.Ordinal);
        Assert.Contains(">@T[\"Recent\"]</a>", layoutSource, StringComparison.Ordinal);
        Assert.Contains("ConversationStarters", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Grammar", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Expressions", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Expressions", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("Exercises", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Exercises", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("Courses", layoutSource, StringComparison.Ordinal);
        Assert.Contains("Courses", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("asp-route-targetLearningLanguageCode", layoutSource, StringComparison.Ordinal);
        Assert.Contains("asp-route-targetLearningLanguageCode", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("WritingTemplates", layoutSource, StringComparison.Ordinal);
        Assert.Contains("WritingTemplates", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("ExamPrep", layoutSource, StringComparison.Ordinal);
        Assert.Contains("ExamPrep", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("CountryGuidance", layoutSource, StringComparison.Ordinal);
        Assert.Contains("CountryGuidance", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("_TargetLearningLanguageSwitcher", layoutSource, StringComparison.Ordinal);
        Assert.Contains("_TargetLearningLanguageSwitcher", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("TargetLearningLanguageCatalog.All", targetLanguageSwitcherSource, StringComparison.Ordinal);
        Assert.Contains("target-language-switcher__option--disabled", targetLanguageSwitcherSource, StringComparison.Ordinal);
        Assert.Contains("language.IsPilot ? T[\"Pilot\"] : T[\"Planned\"]", targetLanguageSwitcherSource, StringComparison.Ordinal);
        Assert.Contains("Target learning language is separate from UI language and helper translations.", targetLanguageSwitcherSource, StringComparison.Ordinal);
        Assert.Contains("ConversationEvents", layoutSource, StringComparison.Ordinal);
        Assert.Contains("OrganizerProfiles", layoutSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Learn\"]", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Prepare\"]", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("LearningLevelSystemCatalog.GetCefrLevelsForTargetLanguage", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("CefrLevelDefinitions", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("GetCefrLevelDefinitions", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("FormatCefrLevelOption", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeCefrLevel", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Settings\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains("hx-get", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("Recently viewed words", recentViewSource, StringComparison.Ordinal);
    }

    [Fact]
    public void LearnerFacingLevelLabels_ShouldComeFromTargetLanguageMetadata()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string viewsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views");
        string filterConventionsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/LearningPortalFilterConventions.cs");
        string filterConventionsSource = File.ReadAllText(filterConventionsPath);

        Assert.Contains("LearningLevelSystemCatalog.GetCefrLevelsForTargetLanguage", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("FormatCefrLevelOption", filterConventionsSource, StringComparison.Ordinal);

        string[] viewSources = Directory.EnumerateFiles(viewsPath, "*.cshtml", SearchOption.AllDirectories)
            .Select(File.ReadAllText)
            .ToArray();

        Assert.DoesNotContain(viewSources, source => source.Contains("new[] { \"A1\", \"A2\", \"B1\", \"B2\", \"C1\", \"C2\" }", StringComparison.Ordinal));
        Assert.DoesNotContain(viewSources, source => source.Contains("LearningPortalFilterConventions.CefrLevels", StringComparison.Ordinal));
        Assert.DoesNotContain(viewSources, source => source.Contains(">@level</option>", StringComparison.Ordinal));
        Assert.DoesNotContain(viewSources, source => source.Contains(">@level</a>", StringComparison.Ordinal));
        Assert.DoesNotContain(viewSources, source => source.Contains(">@level</span>", StringComparison.Ordinal));
        Assert.Contains(viewSources, source => source.Contains("FormatCefrLevel(level)", StringComparison.Ordinal));
        Assert.Contains(viewSources, source => source.Contains("FormatCefrLevel(", StringComparison.Ordinal));
    }

    [Fact]
    public void LearningPortalReleaseRoutes_ShouldStayMappedForWebAndWebApi()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string webControllersPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers");
        string webApiProgramPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");

        string webApiProgramSource = File.ReadAllText(webApiProgramPath);

        AssertControllerRoute(webControllersPath, "GrammarController.cs", "LearningRouteConventions.Grammar");
        AssertControllerRoute(webControllersPath, "ExpressionsController.cs", "LearningRouteConventions.Expressions");
        AssertControllerRoute(webControllersPath, "ExercisesController.cs", "LearningRouteConventions.Exercises");
        AssertControllerRoute(webControllersPath, "CoursesController.cs", "LearningRouteConventions.Courses");
        AssertControllerRoute(webControllersPath, "ExamPrepController.cs", "LearningRouteConventions.ExamPrep");
        AssertControllerRoute(webControllersPath, "WritingTemplatesController.cs", "LearningRouteConventions.WritingTemplates");
        AssertControllerRoute(webControllersPath, "CountryGuidanceController.cs", "LearningRouteConventions.CountryGuidance");
        AssertControllerRoute(webControllersPath, "ConversationEventsController.cs", "LearningRouteConventions.ConversationEvents");
        AssertControllerRoute(webControllersPath, "OrganizerProfilesController.cs", "LearningRouteConventions.OrganizerProfiles");
        AssertControllerDoesNotContainRoute(webControllersPath, "CountryGuidanceController.cs", "[Route(\"life-in-germany\")");
        AssertControllerDoesNotContainRoute(webControllersPath, "ConversationEventsController.cs", "[Route(\"conversation-events\")");
        AssertControllerDoesNotContainRoute(webControllersPath, "OrganizerProfilesController.cs", "[Route(\"organizers\")");
        AssertControllerRoute(webControllersPath, "SearchController.cs", "LearningRouteConventions.Search");

        string[] expectedApiRoutes =
        [
            "\"/api/catalog/grammar\"",
            "\"/api/catalog/grammar/{slug}\"",
            "\"/api/catalog/expressions\"",
            "\"/api/catalog/expressions/{slug}\"",
            "\"/api/catalog/exercise-sets\"",
            "\"/api/catalog/exercise-sets/{slug}\"",
            "\"/api/catalog/exercises/{slug}\"",
            "\"/api/learning/exercises/{slug}/attempts\"",
            "\"/api/catalog/courses\"",
            "\"/api/catalog/courses/{slug}\"",
            "\"/api/catalog/course-lessons/{slug}\"",
            "\"/api/catalog/writing-templates\"",
            "\"/api/catalog/writing-templates/{slug}\"",
            "\"/api/catalog/country-guidance/{countryContextCode}\"",
            "\"/api/catalog/country-guidance/{countryContextCode}/{slug}\"",
            "\"/api/catalog/exam-profiles\"",
            "\"/api/catalog/exam-prep\"",
            "\"/api/catalog/exam-prep/{slug}\"",
            "\"/api/catalog/search\"",
            "\"/api/learning/progress/summary\"",
            "\"/api/learning/progress/content\"",
            "\"/api/learning/recommendations\""
        ];

        foreach (string expectedRoute in expectedApiRoutes)
        {
            Assert.Contains(expectedRoute, webApiProgramSource, StringComparison.Ordinal);
        }

        Assert.Contains("primaryMeaningLanguageCode", webApiProgramSource, StringComparison.Ordinal);
    }

    [Fact]
    public void LearnerRouteInfrastructure_ShouldUseExplicitTargetLanguageContext()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string routeConventionsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/LearningRouteConventions.cs");
        string routeFilterPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/TargetLearningLanguageRouteFilter.cs");
        string programPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Program.cs");

        string routeConventionsSource = File.ReadAllText(routeConventionsPath);
        string routeFilterSource = File.ReadAllText(routeFilterPath);
        string programSource = File.ReadAllText(programPath);

        Assert.Contains("TargetLearningLanguageRouteKey = \"targetLearningLanguageCode\"", routeConventionsSource, StringComparison.Ordinal);
        Assert.Contains("LearnPrefix = \"learn/{\" + TargetLearningLanguageRouteKey + \"}\"", routeConventionsSource, StringComparison.Ordinal);
        Assert.Contains("TargetLearningLanguageCatalog.TryFindActive", routeConventionsSource, StringComparison.Ordinal);
        Assert.Contains("ContentLanguageRequirements.DefaultTargetLearningLanguageCode", routeConventionsSource, StringComparison.Ordinal);
        Assert.Contains("httpContext?.Items.TryGetValue(TargetLearningLanguageRouteKey", routeConventionsSource, StringComparison.Ordinal);
        Assert.Contains("Inactive or unsupported target learning language route", routeConventionsSource, StringComparison.Ordinal);
        Assert.Contains("TargetLearningLanguageCatalog.TryFindActive", routeFilterSource, StringComparison.Ordinal);
        Assert.Contains("new NotFoundResult()", routeFilterSource, StringComparison.Ordinal);
        Assert.Contains("options.Filters.Add<TargetLearningLanguageRouteFilter>()", programSource, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicLearnerLinks_ShouldCarryTargetLearningLanguageRouteValue()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string[] viewRoots =
        [
            Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views"),
            Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views")
        ];

        Regex learnerControllerPattern = new("asp-controller=\"(Browse|Grammar|Expressions|Courses|Collections|Search|Exercises|Roleplays|Favorites|Recent|TalkTopics|Dialogues|ConversationStarters|ExamPrep|WritingTemplates|CountryGuidance|Words|EventPreparationPacks|ConversationEvents|OrganizerProfiles)\"", RegexOptions.Compiled);
        List<string> violations = [];

        foreach (string viewPath in viewRoots.SelectMany(root => Directory.EnumerateFiles(root, "*.cshtml", SearchOption.AllDirectories)))
        {
            string[] lines = File.ReadAllLines(viewPath);
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (!learnerControllerPattern.IsMatch(line)
                    || line.Contains("asp-area=\"Admin\"", StringComparison.Ordinal)
                    || line.Contains("asp-route-targetLearningLanguageCode", StringComparison.Ordinal))
                {
                    continue;
                }

                string relativePath = Path.GetRelativePath(repositoryRoot, viewPath).Replace('\\', '/');
                violations.Add($"{relativePath}:{lineIndex + 1}: {line.Trim()}");
            }
        }

        Assert.Empty(violations);
    }

    [Fact]
    public void WebCatalogApiClientAndControllers_ShouldPassTargetLearningLanguageForCoreCatalog()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string clientPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string grammarControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/GrammarController.cs");
        string expressionsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/ExpressionsController.cs");
        string coursesControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/CoursesController.cs");
        string searchControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/SearchController.cs");
        string browseControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/BrowseController.cs");
        string collectionsControllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/CollectionsController.cs");

        string clientSource = File.ReadAllText(clientPath);
        string grammarControllerSource = File.ReadAllText(grammarControllerPath);
        string expressionsControllerSource = File.ReadAllText(expressionsControllerPath);
        string coursesControllerSource = File.ReadAllText(coursesControllerPath);
        string searchControllerSource = File.ReadAllText(searchControllerPath);
        string browseControllerSource = File.ReadAllText(browseControllerPath);
        string collectionsControllerSource = File.ReadAllText(collectionsControllerPath);

        Assert.Contains("new(\"targetLearningLanguageCode\", targetLearningLanguageCode)", clientSource, StringComparison.Ordinal);
        Assert.Matches("GetGrammarTopicsAsync\\s*\\(\\s*GrammarTopicListFilterModel filter,\\s*string targetLearningLanguageCode", clientSource);
        Assert.Matches("GetExpressionsAsync\\s*\\(\\s*ExpressionListFilterModel filter,\\s*string targetLearningLanguageCode", clientSource);
        Assert.Matches("GetCoursesAsync\\s*\\(\\s*CoursePathListFilterModel filter,\\s*string targetLearningLanguageCode", clientSource);
        Assert.Matches("SearchLearningContentAsync\\s*\\(\\s*UnifiedLearningSearchFilterModel filter,\\s*string targetLearningLanguageCode", clientSource);
        Assert.Contains("LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext)", grammarControllerSource, StringComparison.Ordinal);
        Assert.Contains("LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext)", expressionsControllerSource, StringComparison.Ordinal);
        Assert.Contains("LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext)", coursesControllerSource, StringComparison.Ordinal);
        Assert.Contains("LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext)", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("return RedirectToAction(nameof(Index), new { targetLearningLanguageCode, q = NormalizeQuery(input.SourceQuery) });", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("return RedirectToAction(nameof(Index), new { targetLearningLanguageCode, q = suggestedWord });", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("return RedirectToAction(nameof(Index), new { targetLearningLanguageCode });", browseControllerSource, StringComparison.Ordinal);
        Assert.Contains("return RedirectToAction(nameof(Index), new { targetLearningLanguageCode });", collectionsControllerSource, StringComparison.Ordinal);
        Assert.Contains("GetGrammarTopicsAsync(filter, targetLearningLanguageCode", grammarControllerSource, StringComparison.Ordinal);
        Assert.Matches("GetExpressionBySlugAsync\\s*\\(\\s*normalizedSlug,\\s*targetLearningLanguageCode", expressionsControllerSource);
        Assert.Contains("GetCoursesAsync(filter, targetLearningLanguageCode", coursesControllerSource, StringComparison.Ordinal);
        Assert.Contains("SearchLearningContentAsync(", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("targetLearningLanguageCode,", searchControllerSource, StringComparison.Ordinal);
        Assert.Contains("new { targetLearningLanguageCode, courseSlug", coursesControllerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void GrammarDetailView_ShouldUseDedicatedLearningLayout()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string grammarDetailPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Grammar/Detail.cshtml");
        string stylePath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Styles/tailwind.css");

        string grammarDetailSource = File.ReadAllText(grammarDetailPath);
        string styleSource = File.ReadAllText(stylePath);

        Assert.Contains("grammar-detail-hero", grammarDetailSource, StringComparison.Ordinal);
        Assert.Contains("grammar-detail-layout", grammarDetailSource, StringComparison.Ordinal);
        Assert.Contains("grammar-topic-nav", grammarDetailSource, StringComparison.Ordinal);
        Assert.Contains("grammar-section panel", grammarDetailSource, StringComparison.Ordinal);
        Assert.Contains("grammar-example-card", grammarDetailSource, StringComparison.Ordinal);
        Assert.Contains("grammar-mistake-card", grammarDetailSource, StringComparison.Ordinal);
        Assert.DoesNotContain("<article class=\"detail-row\">", grammarDetailSource, StringComparison.Ordinal);
        Assert.Contains(".grammar-detail-layout", styleSource, StringComparison.Ordinal);
        Assert.Contains(".grammar-table-wrap", styleSource, StringComparison.Ordinal);
        Assert.Contains(".grammar-example-grid", styleSource, StringComparison.Ordinal);
        Assert.Contains(".grammar-mistake-grid", styleSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpressionViews_ShouldRenderWarningsLinksAndLocalizedListMeanings()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controllerPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers/ExpressionsController.cs");
        string clientPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string detailPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Expressions/Detail.cshtml");
        string indexPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Expressions/Index.cshtml");

        string controllerSource = File.ReadAllText(controllerPath);
        string clientSource = File.ReadAllText(clientPath);
        string detailSource = File.ReadAllText(detailPath);
        string indexSource = File.ReadAllText(indexPath);

        Assert.Contains("learningProfileAccessor.GetProfileAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("profile.PreferredMeaningLanguage1", controllerSource, StringComparison.Ordinal);
        Assert.Contains("InvalidOperationException", controllerSource, StringComparison.Ordinal);
        Assert.Contains("IsCatalogApiFailure", controllerSource, StringComparison.Ordinal);
        Assert.Contains("primaryMeaningLanguageCode", clientSource, StringComparison.Ordinal);
        Assert.Contains("Tone and context warnings", detailSource, StringComparison.Ordinal);
        Assert.Contains("Linked practice", detailSource, StringComparison.Ordinal);
        Assert.Contains("Related expressions", detailSource, StringComparison.Ordinal);
        Assert.Contains("LinkedWords", detailSource, StringComparison.Ordinal);
        Assert.DoesNotContain("hardcoded", indexSource, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void LearningPortalLocalization_ShouldContainEnglishAndGermanKeysForReleaseRoutes()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string localizationPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Resources/Localization");

        IReadOnlySet<string> englishKeys = ReadResourceKeys(Path.Combine(localizationPath, "SharedResource.en.resx"));
        IReadOnlySet<string> germanKeys = ReadResourceKeys(Path.Combine(localizationPath, "SharedResource.de.resx"));

        string[] expectedKeys =
        [
            "Grammar Guide",
            "Everyday Expressions",
            "Exercises",
            "Courses",
            "Exam Prep",
            "Writing Templates",
            "Country Guidance - Germany",
            "Life in Germany",
            "Target learning language",
            "Target learning language is separate from UI language and helper translations.",
            "Planned",
            "Pilot",
            "Pilot and planned languages stay disabled until reviewed content is available.",
            "Einstieg",
            "Grundlagen",
            "Selbststaendig",
            "Kompetent",
            "Souveraen",
            "Meisterschaft",
            "Erste Schritte",
            "Sicherer Alltag",
            "Alltag und Arbeit",
            "Kompetente Anwendung",
            "Souveraene Kommunikation",
            "Stil und Praezision",
            "Learning Portal",
            "Unified learning search",
            "Learning progress",
            "Mobile parity"
        ];

        foreach (string expectedKey in expectedKeys)
        {
            Assert.Contains(expectedKey, englishKeys);
            Assert.Contains(expectedKey, germanKeys);
        }
    }

    private static void AssertControllerRoute(string controllersPath, string fileName, string expectedRoute)
    {
        string source = File.ReadAllText(Path.Combine(controllersPath, fileName));

        Assert.Contains(expectedRoute, source, StringComparison.Ordinal);
    }

    private static void AssertControllerDoesNotContainRoute(string controllersPath, string fileName, string unexpectedRoute)
    {
        string source = File.ReadAllText(Path.Combine(controllersPath, fileName));

        Assert.DoesNotContain(unexpectedRoute, source, StringComparison.Ordinal);
    }

    private static IReadOnlySet<string> ReadResourceKeys(string path)
    {
        XDocument document = XDocument.Load(path);

        return document
            .Root!
            .Elements("data")
            .Attributes("name")
            .Select(attribute => attribute.Value)
            .ToHashSet(StringComparer.Ordinal);
    }

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
