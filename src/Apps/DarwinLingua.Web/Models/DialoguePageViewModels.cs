using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Web.Models;

public sealed record DialogueIndexPageViewModel(
    IReadOnlyList<DialogueLessonListItemModel> Dialogues,
    IReadOnlyList<string> TopicKeys,
    DialogueLessonListFilterModel Filter);

public sealed record DialogueDetailPageViewModel(
    DialogueLessonDetailModel Dialogue,
    IReadOnlyList<ConversationStarterPackListItemModel> RelatedStarterPacks,
    IReadOnlyList<EventPreparationPackListItemModel> RelatedEventPreparationPacks,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record DialogueRoleplayPageViewModel(
    DialogueLessonDetailModel Dialogue,
    IReadOnlyList<DialogueRoleplayStepViewModel> Steps,
    string PrimaryMeaningLanguageCode,
    string? SecondaryMeaningLanguageCode);

public sealed record DialogueRoleplayStepViewModel(
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
