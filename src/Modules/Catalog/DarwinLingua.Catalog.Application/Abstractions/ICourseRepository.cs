using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ICourseRepository
{
    Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, CancellationToken cancellationToken);

    Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, CancellationToken cancellationToken);

    Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, CancellationToken cancellationToken);
}
