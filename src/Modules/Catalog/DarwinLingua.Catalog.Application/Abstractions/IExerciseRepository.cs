using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExerciseRepository
{
    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetsAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExercisesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExerciseBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedExerciseEntityBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, cancellationToken);

    Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, string targetLearningLanguageCode, CancellationToken cancellationToken);

    Task SaveAttemptAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken);
}
