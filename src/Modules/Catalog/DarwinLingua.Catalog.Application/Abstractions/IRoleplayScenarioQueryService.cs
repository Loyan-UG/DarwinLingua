using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface IRoleplayScenarioQueryService
{
    Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetPublishedRoleplayScenariosAsync(
        RoleplayScenarioListFilterModel filter,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetPublishedRoleplayScenariosAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetPublishedRoleplayScenariosAsync(
        RoleplayScenarioListFilterModel filter,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);

    Task<RoleplayScenarioDetailModel?> GetPublishedRoleplayScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        GetPublishedRoleplayScenarioBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<RoleplayScenarioDetailModel?> GetPublishedRoleplayScenarioBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken);
}
