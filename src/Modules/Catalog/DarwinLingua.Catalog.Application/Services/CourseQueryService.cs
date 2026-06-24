using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class CourseQueryService(ICourseRepository repository) : ICourseQueryService
{
    public Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        repository.GetPublishedCoursePathsAsync(filter, primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        repository.GetPublishedCoursePathsAsync(filter, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        repository.GetPublishedCoursePathBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);

    public Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        repository.GetPublishedCoursePathBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        repository.GetPublishedCourseLessonBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);

    public Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        repository.GetPublishedCourseLessonBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);
}
