namespace DarwinLingua.WebApi.Tests;

using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Services;
using Xunit;

public sealed class CourseRouteStructuralTests
{
    [Fact]
    public void WebCourseViews_ShouldRenderLevelCardsAndSingleOpenModuleAccordion()
    {
        string indexSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Index.cshtml"));
        string detailSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Detail.cshtml"));
        string lessonSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Courses", "Lesson.cshtml"));
        string siteScript = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "wwwroot", "js", "site.js"));
        string styleSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Styles", "tailwind.css"));

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
        Assert.Contains("activityBlocks.Length == 0", lessonSource, StringComparison.Ordinal);
        Assert.Contains("Linked content", lessonSource, StringComparison.Ordinal);
        Assert.Contains("dir=\"@primaryMeaningDirection\"", lessonSource, StringComparison.Ordinal);
        Assert.Contains(".lesson-flow__item", styleSource, StringComparison.Ordinal);
        Assert.Contains("overflow-wrap: anywhere", styleSource, StringComparison.Ordinal);
        Assert.Contains(".lesson-flow__badge--required", styleSource, StringComparison.Ordinal);
        Assert.Contains(".lesson-flow__badge--optional", styleSource, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("course-lesson", "a1-next", "/courses/a1-course/a1-next")]
    [InlineData("grammar-topic", "a1-artikel", "/grammar/a1-artikel")]
    [InlineData("expression", "alles-klar", "/expressions/alles-klar")]
    [InlineData("dialogue", "a1-begruessung", "/dialogues/a1-begruessung")]
    [InlineData("talk-topic", "a1-vorstellen", "/talk-topics/a1-vorstellen")]
    [InlineData("exercise-set", "a1-set", "/exercises/sets/a1-set")]
    [InlineData("exercise", "a1-frage", "/exercises/a1-frage")]
    [InlineData("roleplay", "a1-im-kurs", "/roleplays/a1-im-kurs")]
    [InlineData("writing-template", "a1-termin", "/writing-templates/a1-termin")]
    [InlineData("life-in-germany", "b1-demokratie", "/life-in-germany/b1-demokratie")]
    [InlineData("exam-prep-unit", "goethe-a1-ueberblick", "/exam-prep/goethe-a1-ueberblick")]
    public void CourseActivityTargetLinkResolver_ShouldMapSupportedTargets(string targetType, string targetSlug, string expectedHref)
    {
        CourseLessonActivityBlockModel activity = CreateActivity(targetType, targetSlug);

        Assert.Equal(expectedHref, CourseActivityTargetLinkResolver.ResolveHref(activity, "a1-course"));
    }

    [Fact]
    public void CourseActivityTargetLinkResolver_ShouldNotLinkNoneOrMalformedTargets()
    {
        Assert.Null(CourseActivityTargetLinkResolver.ResolveHref(CreateActivity("none", null), "a1-course"));
        Assert.Null(CourseActivityTargetLinkResolver.ResolveHref(CreateActivity("exercise", null), "a1-course"));
        Assert.Null(CourseActivityTargetLinkResolver.ResolveHref(CreateActivity("unsupported", "a1-test"), "a1-course"));
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
