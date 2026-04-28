using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IScenarioLessonQueryService
{
    Task<IReadOnlyList<ScenarioLessonListItemModel>> GetPublishedScenariosAsync(CancellationToken cancellationToken);

    Task<ScenarioLessonDetailModel?> GetPublishedScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
