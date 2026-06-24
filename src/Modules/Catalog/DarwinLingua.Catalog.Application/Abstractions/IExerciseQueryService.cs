using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExerciseQueryService
{
    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetsAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetsAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExerciseSetBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedExercisesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExercisesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedExerciseBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, "en", cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedExerciseBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken);
}

public interface IExerciseAttemptService
{
    Task<ExerciseAttemptResultModel?> SubmitAttemptAsync(string slug, ExerciseAttemptRequestModel request, string userId, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        SubmitAttemptAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, request, userId, primaryMeaningLanguageCode, cancellationToken);

    Task<ExerciseAttemptResultModel?> SubmitAttemptAsync(string slug, string targetLearningLanguageCode, ExerciseAttemptRequestModel request, string userId, string primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExerciseAttemptResultModel?> EvaluateAttemptAsync(string slug, ExerciseAttemptRequestModel request, string primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        EvaluateAttemptAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, request, primaryMeaningLanguageCode, cancellationToken);

    Task<ExerciseAttemptResultModel?> EvaluateAttemptAsync(string slug, string targetLearningLanguageCode, ExerciseAttemptRequestModel request, string primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
