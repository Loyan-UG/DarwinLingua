using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ICourseQueryService
{
    Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedCoursePathsAsync(filter, "en", cancellationToken);

    Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedCoursePathBySlugAsync(slug, "en", cancellationToken);

    Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedCourseLessonBySlugAsync(slug, "en", cancellationToken);

    Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
