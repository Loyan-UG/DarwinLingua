namespace DarwinLingua.Catalog.Application.Models;

public sealed record EventPreparationListFilterModel(
    string? CefrLevel,
    string? Category,
    string? EventType,
    string? TopicKey);

public sealed record EventPreparationPackListItemModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string EventType,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> LinkedScenarioSlugs,
    IReadOnlyList<string> LinkedConversationStarterPackSlugs);

public sealed record EventPreparationPackDetailModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string EventType,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> LinkedScenarioSlugs,
    IReadOnlyList<string> LinkedConversationStarterPackSlugs,
    IReadOnlyList<EventPreparationVocabularyReferenceModel> LinkedVocabulary,
    IReadOnlyList<EventPreparationPromptModel> Prompts);

public sealed record EventPreparationVocabularyReferenceModel(
    string Word,
    string? PartOfSpeech,
    string? CefrLevel);

public sealed record EventPreparationPromptModel(
    string PromptType,
    string Text);
