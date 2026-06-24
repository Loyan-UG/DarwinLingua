using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class RoleplayScenarioQueryService(IRoleplayScenarioRepository repository) : IRoleplayScenarioQueryService
{
    public Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetPublishedRoleplayScenariosAsync(
        RoleplayScenarioListFilterModel filter,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedRoleplayScenariosAsync(filter, primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<RoleplayScenarioListItemModel>> GetPublishedRoleplayScenariosAsync(
        RoleplayScenarioListFilterModel filter,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedRoleplayScenariosAsync(filter, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<RoleplayScenarioDetailModel?> GetPublishedRoleplayScenarioBySlugAsync(
        string slug,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedRoleplayScenarioBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);

    public Task<RoleplayScenarioDetailModel?> GetPublishedRoleplayScenarioBySlugAsync(
        string slug,
        string targetLearningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken) =>
        repository.GetPublishedRoleplayScenarioBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);
}
