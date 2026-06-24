using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ICourseRepository
{
    Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedCoursePathsAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedCoursePathBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedCourseLessonBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
