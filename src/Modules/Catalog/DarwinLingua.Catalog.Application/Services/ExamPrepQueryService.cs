using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ExamPrepQueryService(IExamPrepRepository examPrepRepository) : IExamPrepQueryService
{
    public Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamProfilesAsync(primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamProfilesAsync(targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamPrepUnitsAsync(filter, primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamPrepUnitsAsync(filter, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamPrepUnitBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);

    public Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamPrepUnitBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);
}
