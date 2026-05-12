using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IExamPrepRepository
{
    Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, CancellationToken cancellationToken);

    Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, CancellationToken cancellationToken);
}
