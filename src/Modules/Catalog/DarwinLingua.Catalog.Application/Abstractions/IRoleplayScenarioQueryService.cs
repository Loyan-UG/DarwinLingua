using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IRoleplayScenarioQueryService
{
    Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetPublishedRoleplayScenariosAsync(
        RoleplayScenarioListFilterModel filter,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<RoleplayScenarioDetailModel?> GetPublishedRoleplayScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
