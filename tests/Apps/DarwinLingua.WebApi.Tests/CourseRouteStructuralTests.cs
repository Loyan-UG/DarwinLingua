namespace DarwinLingua.WebApi.Tests;

using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Services;
using Xunit;

public sealed class CourseRouteStructuralTests
{
    [Fact]
    public void GermanCefrLevelDefinitions_ShouldExposeCurrentLearnerFriendlyLabels()
    {
        IReadOnlyDictionary<string, LearningLevelDefinition> levels = LearningLevelSystemCatalog.GermanCefrLevels
            .ToDictionary(level => level.Code, StringComparer.OrdinalIgnoreCase);

        Assert.Equal(["A1", "A2", "B1", "B2", "C1", "C2"], levels.Keys.OrderBy(code => levels[code].SortOrder).ToArray());
        Assert.Equal("Einstieg", levels["A1"].DisplayTitle);
        Assert.Equal("Grundlagen", levels["A2"].DisplayTitle);
        Assert.Equal("Selbststaendig", levels["B1"].DisplayTitle);
        Assert.Equal("Kompetent", levels["B2"].DisplayTitle);
        Assert.Equal("Souveraen", levels["C1"].DisplayTitle);
        Assert.Equal("Meisterschaft", levels["C2"].DisplayTitle);
        Assert.Equal("Kompetente Anwendung", levels["B2"].LearnerDescription);
        Assert.Equal("Souveraene Kommunikation", levels["C1"].LearnerDescription);
        Assert.Equal("Stil und Praezision", levels["C2"].LearnerDescription);
        Assert.All(levels.Values, level =>
        {
            Assert.Equal(LearningLevelSystemCatalog.CefrCode, level.LevelSystemCode);
            Assert.False(string.IsNullOrWhiteSpace(level.DisplayTitle));
            Assert.False(string.IsNullOrWhiteSpace(level.LearnerDescription));
            Assert.False(string.IsNullOrWhiteSpace(level.StandardMappingCode));
        });
    }

    [Fact]
    public void WebCourseViews_ShouldRenderLevelCardsAndSingleOpenModuleAccordion()
    {
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Detail.cshtml"));
        string lessonSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Lesson.cshtml"));
        string slugListSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "_SlugList.cshtml"));
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "CoursesController.cs"));
        string siteScript = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "js", "site.js"));
        string styleSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Styles", "tailwind.css"));
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string webClientSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "WebCatalogApiClient.cs"));

        Assert.Contains("course-level-card__level", indexSource, StringComparison.Ordinal);
        Assert.Contains("course-level-card__meta", indexSource, StringComparison.Ordinal);
        Assert.Contains("data-course-module", detailSource, StringComparison.Ordinal);
        Assert.Contains("course-module-card__summary", detailSource, StringComparison.Ordinal);
        Assert.Contains("course-lesson-list__item", detailSource, StringComparison.Ordinal);
        Assert.Contains("function configureCourseModules", siteScript, StringComparison.Ordinal);
        Assert.Contains("otherModule.open = false", siteScript, StringComparison.Ordinal);
        Assert.Contains(".course-level-card__level", styleSource, StringComparison.Ordinal);
        Assert.Contains(".course-module-card__summary", styleSource, StringComparison.Ordinal);
        Assert.Contains("Model.Lesson.ActivityBlocks", lessonSource, StringComparison.Ordinal);
        Assert.Contains("lesson-flow__item", lessonSource, StringComparison.Ordinal);
        Assert.Contains("CourseActivityTargetLinkResolver.ResolveHref", lessonSource, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"lessons/{lessonSlug}\"", controllerSource, StringComparison.Ordinal);
        Assert.Contains("CourseLessons_RedirectBySlug", controllerSource, StringComparison.Ordinal);
        Assert.Contains("RedirectToAction(nameof(Lesson)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("activityBlocks.Length == 0", lessonSource, StringComparison.Ordinal);
        Assert.Contains("Linked content", lessonSource, StringComparison.Ordinal);
        Assert.Contains("LearningContentLinkResolver.ResolveHref", slugListSource, StringComparison.Ordinal);
        Assert.Contains("\"grammar-topic\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains("\"word\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains("\"expression\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains("\"dialogue\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains("\"talk-topic\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains("\"exercise-set\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains("<a href=\"@href\">@slug</a>", slugListSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains(".lesson-flow__item", styleSource, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap: anywhere", styleSource, StringComparison.Ordinal);
        Assert.Contains(".lesson-flow__badge--required", styleSource, StringComparison.Ordinal);
        Assert.Contains(".lesson-flow__badge--optional", styleSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/courses\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/courses/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("\"/api/catalog/course-lessons/{slug}\"", programSource, StringComparison.Ordinal);
        Assert.Contains("CoursePathListFilterModel(cefrLevel, q)", programSource, StringComparison.Ordinal);
        Assert.Contains("ResolveTargetLearningLanguageCode(targetLearningLanguageCode)", programSource, StringComparison.Ordinal);
        Assert.Contains("GetPublishedCoursePathBySlugAsync(slug, ResolveTargetLearningLanguageCode(targetLearningLanguageCode), primaryMeaningLanguageCode ?? \"en\"", programSource, StringComparison.Ordinal);
        Assert.Contains("GetPublishedCourseLessonBySlugAsync(slug, ResolveTargetLearningLanguageCode(targetLearningLanguageCode), primaryMeaningLanguageCode ?? \"en\"", programSource, StringComparison.Ordinal);
        Assert.Contains("targetLearningLanguageCode", webClientSource, StringComparison.Ordinal);
        Assert.Contains("/api/catalog/courses", webClientSource, StringComparison.Ordinal);
        Assert.Contains("/api/catalog/course-lessons/", webClientSource, StringComparison.Ordinal);
        Assert.DoesNotContain("DefaultTargetLearningLanguageRouteValue", File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "LearningContentLinkResolver.cs")), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("grammar-topic", "a1-artikel", null, "/learn/de/grammar/a1-artikel")]
    [InlineData("word", "der-kurs", null, "/learn/de/words/der-kurs")]
    [InlineData("expression", "alles-klar", null, "/learn/de/expressions/alles-klar")]
    [InlineData("dialogue", "a1-begruessung", null, "/learn/de/dialogues/a1-begruessung")]
    [InlineData("talk-topic", "a1-vorstellen", null, "/learn/de/talk-topics/a1-vorstellen")]
    [InlineData("exercise-set", "a1-set", null, "/learn/de/exercises/sets/a1-set")]
    [InlineData("exercise", "a1-frage", null, "/learn/de/exercises/a1-frage")]
    [InlineData("roleplay", "a1-im-kurs", null, "/learn/de/roleplays/a1-im-kurs")]
    [InlineData("writing-template", "a1-termin", null, "/learn/de/writing-templates/a1-termin")]
    [InlineData("country-guidance", "b1-demokratie", null, "/learn/de/country-guidance/de/b1-demokratie")]
    [InlineData("exam-prep-unit", "goethe-a1-ueberblick", null, "/learn/de/exam-prep/goethe-a1-ueberblick")]
    [InlineData("course-lesson", "a1-next", "a1-course", "/learn/de/courses/a1-course/a1-next")]
    [InlineData("course-lesson", "a1-next", null, "/learn/de/courses/lessons/a1-next")]
    public void LearningContentLinkResolver_ShouldMapReusableLinkedContentTargets(
        string contentType,
        string slug,
        string? currentCourseSlug,
        string expectedHref)
    {
        Assert.Equal(expectedHref, LearningContentLinkResolver.ResolveHref(contentType, slug, "de", currentCourseSlug));
    }

    [Fact]
    public void LearningContentLinkResolver_ShouldUseExplicitTargetLanguageAndCountryContext()
    {
        Assert.Equal(
            "/learn/en/grammar/a1-articles",
            LearningContentLinkResolver.ResolveHref("grammar-topic", "a1-articles", "en"));

        Assert.Equal(
            "/learn/de/country-guidance/at/meldezettel-verstehen",
            LearningContentLinkResolver.ResolveHref("country-guidance", "meldezettel-verstehen", "de", countryContextCode: "at"));

        Assert.Null(LearningContentLinkResolver.ResolveHref("grammar-topic", "a1-articles", string.Empty));
    }

    [Fact]
    public void LearningContentLinkResolver_ShouldFailClosedForUnsupportedOrIncompleteTargets()
    {
        Assert.Null(LearningContentLinkResolver.ResolveHref("unsupported", "a1-test", "de"));
        Assert.Null(LearningContentLinkResolver.ResolveHref("exercise", null, "de"));
    }

    [Theory]
    [InlineData("course-lesson", "a1-next", "/learn/de/courses/a1-course/a1-next")]
    [InlineData("grammar-topic", "a1-artikel", "/learn/de/grammar/a1-artikel")]
    [InlineData("expression", "alles-klar", "/learn/de/expressions/alles-klar")]
    [InlineData("dialogue", "a1-begruessung", "/learn/de/dialogues/a1-begruessung")]
    [InlineData("talk-topic", "a1-vorstellen", "/learn/de/talk-topics/a1-vorstellen")]
    [InlineData("exercise-set", "a1-set", "/learn/de/exercises/sets/a1-set")]
    [InlineData("exercise", "a1-frage", "/learn/de/exercises/a1-frage")]
    [InlineData("roleplay", "a1-im-kurs", "/learn/de/roleplays/a1-im-kurs")]
    [InlineData("writing-template", "a1-termin", "/learn/de/writing-templates/a1-termin")]
    [InlineData("country-guidance", "b1-demokratie", "/learn/de/country-guidance/de/b1-demokratie")]
    [InlineData("exam-prep-unit", "goethe-a1-ueberblick", "/learn/de/exam-prep/goethe-a1-ueberblick")]
    public void CourseActivityTargetLinkResolver_ShouldMapSupportedTargets(string targetType, string targetSlug, string expectedHref)
    {
        CourseLessonActivityBlockModel activity = CreateActivity(targetType, targetSlug);

        Assert.Equal(expectedHref, CourseActivityTargetLinkResolver.ResolveHref(activity, "a1-course", "de"));
    }

    [Fact]
    public void CourseActivityTargetLinkResolver_ShouldNotLinkNoneOrMalformedTargets()
    {
        Assert.Null(CourseActivityTargetLinkResolver.ResolveHref(CreateActivity("none", null), "a1-course", "de"));
        Assert.Null(CourseActivityTargetLinkResolver.ResolveHref(CreateActivity("exercise", null), "a1-course", "de"));
        Assert.Null(CourseActivityTargetLinkResolver.ResolveHref(CreateActivity("unsupported", "a1-test"), "a1-course", "de"));
    }

    private static CourseLessonActivityBlockModel CreateActivity(string targetType, string? targetSlug) =>
        new(
            "practice",
            "Titel",
            "عنوان",
            "Arbeite kurz mit dem Material.",
            "با این مطلب کوتاه کار کن.",
            targetType,
            targetSlug,
            5,
            10,
            true);

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
