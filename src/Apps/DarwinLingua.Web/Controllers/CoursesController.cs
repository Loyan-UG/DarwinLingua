using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.Courses)]
public sealed class CoursesController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IUserContentProgressService progressService,
    ILogger<CoursesController> logger) : Controller
{
    private static readonly IReadOnlySet<string> LearnerSelectableLessonProgressStates = new HashSet<string>(StringComparer.Ordinal)
    {
        "in-progress",
        "completed",
        "needs-review",
    };

    [HttpGet("", Name = "Courses_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? cefrLevel, string? q, CancellationToken cancellationToken)
    {
        CoursePathListFilterModel filter = new(LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel), string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? primaryMeaningLanguageCode = profile.PreferredMeaningLanguage1;
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        IReadOnlyList<CoursePathListItemModel> courses;
        try
        {
            courses = await catalogApiClient
                .GetCoursesAsync(filter, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Courses could not be loaded.");
            courses = [];
        }

        return View(new CourseIndexPageViewModel(
            courses,
            LearningPortalFilterConventions.CefrLevels,
            ResolveLevelDefinitions(targetLearningLanguageCode),
            filter.CefrLevel,
            filter.Query,
            primaryMeaningLanguageCode ?? "en"));
    }

    [HttpGet("{slug}", Name = "Courses_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? primaryMeaningLanguageCode = profile.PreferredMeaningLanguage1;
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        CoursePathDetailModel? course = await catalogApiClient
            .GetCourseBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
        return course is null ? NotFound() : View(new CourseDetailPageViewModel(course, primaryMeaningLanguageCode ?? "en"));
    }

    [HttpGet("lessons/{lessonSlug}", Name = "CourseLessons_RedirectBySlug")]
    public async Task<IActionResult> LessonBySlug(string lessonSlug, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? primaryMeaningLanguageCode = profile.PreferredMeaningLanguage1;
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        CourseLessonDetailModel? lesson = await catalogApiClient
            .GetCourseLessonBySlugAsync(lessonSlug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
        if (lesson is null)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Lesson), new { targetLearningLanguageCode, courseSlug = lesson.CoursePathSlug, lessonSlug = lesson.Slug });
    }

    [HttpGet("{courseSlug}/{lessonSlug}", Name = "CourseLessons_Detail")]
    public async Task<IActionResult> Lesson(string courseSlug, string lessonSlug, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? primaryMeaningLanguageCode = profile.PreferredMeaningLanguage1;
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        CourseLessonDetailModel? lesson = await catalogApiClient
            .GetCourseLessonBySlugAsync(lessonSlug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken)
            .ConfigureAwait(false);
        if (lesson is null || !string.Equals(lesson.CoursePathSlug, courseSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        UserContentProgressModel? progress = null;
        try
        {
            progress = await progressService
                .UpdateContentProgressAsync(
                    profile.UserId,
                    targetLearningLanguageCode,
                    new UpdateUserContentProgressRequestModel("course-lesson", lesson.Slug, "viewed"),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Course lesson progress could not be tracked for {LessonSlug}.", lesson.Slug);
        }

        return View(new CourseLessonPageViewModel(lesson, progress, primaryMeaningLanguageCode ?? "en"));
    }

    [HttpPost("{courseSlug}/{lessonSlug}/progress", Name = "CourseLessons_UpdateProgress")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLessonProgress(
        string courseSlug,
        string lessonSlug,
        string state,
        CancellationToken cancellationToken)
    {
        string normalizedState = string.IsNullOrWhiteSpace(state) ? string.Empty : state.Trim().ToLowerInvariant();
        if (!LearnerSelectableLessonProgressStates.Contains(normalizedState))
        {
            return BadRequest();
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        try
        {
            await progressService
                .UpdateContentProgressAsync(
                    profile.UserId,
                    targetLearningLanguageCode,
                    new UpdateUserContentProgressRequestModel("course-lesson", lessonSlug, normalizedState),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Course lesson progress could not be updated for {LessonSlug}.", lessonSlug);
            return BadRequest();
        }

        return RedirectToAction(nameof(Lesson), new { targetLearningLanguageCode, courseSlug, lessonSlug });
    }

    private static IReadOnlyList<LearningLevelDefinition> ResolveLevelDefinitions(string targetLearningLanguageCode)
    {
        if (!TargetLearningLanguageCatalog.TryFindActive(targetLearningLanguageCode, out TargetLearningLanguageDefinition targetLanguage))
        {
            return LearningLevelSystemCatalog.GermanCefrLevels;
        }

        return string.Equals(targetLanguage.DefaultLevelSystemCode, LearningLevelSystemCatalog.CefrCode, StringComparison.OrdinalIgnoreCase)
            ? LearningLevelSystemCatalog.GermanCefrLevels
            : [];
    }
}
