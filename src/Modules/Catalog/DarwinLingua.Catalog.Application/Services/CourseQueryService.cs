using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class CourseQueryService(ICourseRepository repository) : ICourseQueryService
{
    public Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, CancellationToken cancellationToken) =>
        repository.GetPublishedCoursePathsAsync(filter, cancellationToken);

    public Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, CancellationToken cancellationToken) =>
        repository.GetPublishedCoursePathBySlugAsync(slug, cancellationToken);

    public Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, CancellationToken cancellationToken) =>
        repository.GetPublishedCourseLessonBySlugAsync(slug, cancellationToken);
}
