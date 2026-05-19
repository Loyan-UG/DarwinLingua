namespace DarwinLingua.Localization.Application.Tests;

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
        string filterConventionsPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/LearningPortalFilterConventions.cs");
        string homeViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Home/Index.cshtml");
        string recentViewPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Views/Recent/Index.cshtml");

        string layoutSource = File.ReadAllText(layoutPath);
        string mobileNavSource = File.ReadAllText(mobileNavPath);
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
        Assert.Contains("WritingTemplates", layoutSource, StringComparison.Ordinal);
        Assert.Contains("WritingTemplates", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("ExamPrep", layoutSource, StringComparison.Ordinal);
        Assert.Contains("ExamPrep", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("CulturalNotes", layoutSource, StringComparison.Ordinal);
        Assert.Contains("CulturalNotes", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("ConversationEvents", layoutSource, StringComparison.Ordinal);
        Assert.Contains("OrganizerProfiles", layoutSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Learn\"]", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Prepare\"]", mobileNavSource, StringComparison.Ordinal);
        Assert.Contains("CefrLevels = [\"A1\", \"A2\", \"B1\", \"B2\", \"C1\", \"C2\"]", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("NormalizeCefrLevel", filterConventionsSource, StringComparison.Ordinal);
        Assert.Contains("@T[\"Settings\"]", layoutSource, StringComparison.Ordinal);
        Assert.Contains("hx-get", homeViewSource, StringComparison.Ordinal);
        Assert.Contains("Recently viewed words", recentViewSource, StringComparison.Ordinal);
    }

    [Fact]
    public void LearningPortalReleaseRoutes_ShouldStayMappedForWebAndWebApi()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string webControllersPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.Web/Controllers");
        string webApiProgramPath = Path.Combine(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");

        string webApiProgramSource = File.ReadAllText(webApiProgramPath);

        AssertControllerRoute(webControllersPath, "GrammarController.cs", "[Route(\"grammar\")");
        AssertControllerRoute(webControllersPath, "ExpressionsController.cs", "[Route(\"expressions\")");
        AssertControllerRoute(webControllersPath, "ExercisesController.cs", "[Route(\"exercises\")");
        AssertControllerRoute(webControllersPath, "CoursesController.cs", "[Route(\"courses\")");
        AssertControllerRoute(webControllersPath, "ExamPrepController.cs", "[Route(\"exam-prep\")");
        AssertControllerRoute(webControllersPath, "WritingTemplatesController.cs", "[Route(\"writing-templates\")");
        AssertControllerRoute(webControllersPath, "CulturalNotesController.cs", "[Route(\"cultural-notes\")");
        AssertControllerRoute(webControllersPath, "SearchController.cs", "[Route(\"search\")");

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
            "\"/api/catalog/cultural-notes\"",
            "\"/api/catalog/cultural-notes/{slug}\"",
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
            "Cultural Notes",
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
