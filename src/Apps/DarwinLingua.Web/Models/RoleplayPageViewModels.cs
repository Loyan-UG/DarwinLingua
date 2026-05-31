using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record RoleplayIndexPageViewModel(
    IReadOnlyList<RoleplayScenarioListItemModel> Roleplays,
    RoleplayScenarioListFilterModel Filter,
    string PrimaryMeaningLanguageCode);

public sealed record RoleplayDetailPageViewModel(
    RoleplayScenarioDetailModel Roleplay,
    string PrimaryMeaningLanguageCode);
