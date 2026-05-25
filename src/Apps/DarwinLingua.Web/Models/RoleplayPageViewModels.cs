using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record RoleplayIndexPageViewModel(
    IReadOnlyList<RoleplayScenarioListItemModel> Roleplays,
    RoleplayScenarioListFilterModel Filter);

public sealed record RoleplayDetailPageViewModel(
    RoleplayScenarioDetailModel Roleplay);
