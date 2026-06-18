using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExamPrepQueryService
{
    Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
