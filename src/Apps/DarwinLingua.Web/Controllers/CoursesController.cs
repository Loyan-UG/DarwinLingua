using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("courses")]
public sealed class CoursesController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IUserContentProgressService progressService,
    ILogger<CoursesController> logger) : Controller
{
    [HttpGet("", Name = "Courses_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? cefrLevel, string? q, CancellationToken cancellationToken)
    {
        CoursePathListFilterModel filter = new(LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel), string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        IReadOnlyList<CoursePathListItemModel> courses;
        try
        {
            courses = await catalogApiClient.GetCoursesAsync(filter, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Courses could not be loaded.");
            courses = [];
        }

        return View(new CourseIndexPageViewModel(courses, LearningPortalFilterConventions.CefrLevels, filter.CefrLevel, filter.Query));
    }

    [HttpGet("{slug}", Name = "Courses_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        CoursePathDetailModel? course = await catalogApiClient.GetCourseBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return course is null ? NotFound() : View(new CourseDetailPageViewModel(course));
    }

    [HttpGet("{courseSlug}/{lessonSlug}", Name = "CourseLessons_Detail")]
    public async Task<IActionResult> Lesson(string courseSlug, string lessonSlug, CancellationToken cancellationToken)
    {
        CourseLessonDetailModel? lesson = await catalogApiClient.GetCourseLessonBySlugAsync(lessonSlug, cancellationToken).ConfigureAwait(false);
        if (lesson is null || !string.Equals(lesson.CoursePathSlug, courseSlug, StringComparison.OrdinalIgnoreCase))
        {
            return NotFound();
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        UserContentProgressModel? progress = null;
        try
        {
            progress = await progressService
                .UpdateContentProgressAsync(
                    profile.UserId,
                    new UpdateUserContentProgressRequestModel("course-lesson", lesson.Slug, "viewed"),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Course lesson progress could not be tracked for {LessonSlug}.", lesson.Slug);
        }

        return View(new CourseLessonPageViewModel(lesson, progress));
    }
}
