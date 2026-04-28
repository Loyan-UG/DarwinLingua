namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedEventPreparationPackModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string EventType,
    IReadOnlyList<string> Topics,
    int SortOrder,
    IReadOnlyList<string> LinkedScenarioSlugs,
    IReadOnlyList<ParsedEventPreparationVocabularyReferenceModel> LinkedVocabulary,
    IReadOnlyList<string> LinkedConversationStarterPackSlugs,
    IReadOnlyList<string> OpeningPrompts,
    IReadOnlyList<string> RoleplayPrompts,
    IReadOnlyList<string> ReviewPrompts);
