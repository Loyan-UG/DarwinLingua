using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record ScenarioIndexPageViewModel(
    IReadOnlyList<ScenarioLessonListItemModel> Scenarios);

public sealed record ScenarioDetailPageViewModel(
    ScenarioLessonDetailModel Scenario,
    IReadOnlyList<ConversationStarterPackListItemModel> RelatedStarterPacks,
    IReadOnlyList<EventPreparationPackListItemModel> RelatedEventPreparationPacks,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record ScenarioRoleplayPageViewModel(
    ScenarioLessonDetailModel Scenario,
    IReadOnlyList<ScenarioRoleplayStepViewModel> Steps,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record ScenarioRoleplayStepViewModel(
    int SortOrder,
    string PromptRole,
    string PromptText,
    string? PromptPrimaryMeaning,
    string? PromptSecondaryMeaning,
    string LearnerRole,
    string ModelAnswerText,
    string? ModelAnswerPrimaryMeaning,
    string? ModelAnswerSecondaryMeaning,
    string StaticFeedback);
