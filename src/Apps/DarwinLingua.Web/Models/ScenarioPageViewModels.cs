using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ScenarioIndexPageViewModel(
    IReadOnlyList<ScenarioLessonListItemModel> Scenarios);

public sealed record ScenarioDetailPageViewModel(
    ScenarioLessonDetailModel Scenario,
    IReadOnlyList<ConversationStarterPackListItemModel> RelatedStarterPacks,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);
