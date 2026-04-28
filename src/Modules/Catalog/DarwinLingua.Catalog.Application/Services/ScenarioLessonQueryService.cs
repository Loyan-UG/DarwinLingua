using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class ScenarioLessonQueryService(IScenarioLessonRepository scenarioLessonRepository) : IScenarioLessonQueryService
{
    public Task<IReadOnlyList<ScenarioLessonListItemModel>> GetPublishedScenariosAsync(CancellationToken cancellationToken) =>
        scenarioLessonRepository.GetPublishedScenariosAsync(cancellationToken);

    public Task<ScenarioLessonDetailModel?> GetPublishedScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        scenarioLessonRepository.GetPublishedScenarioBySlugAsync(
            slug,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode,
            cancellationToken);
}
