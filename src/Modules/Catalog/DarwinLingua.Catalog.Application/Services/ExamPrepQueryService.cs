using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ExamPrepQueryService(IExamPrepRepository examPrepRepository) : IExamPrepQueryService
{
    public Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamProfilesAsync(cancellationToken);

    public Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamPrepUnitsAsync(filter, cancellationToken);

    public Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, CancellationToken cancellationToken) =>
        examPrepRepository.GetPublishedExamPrepUnitBySlugAsync(slug, cancellationToken);
}
