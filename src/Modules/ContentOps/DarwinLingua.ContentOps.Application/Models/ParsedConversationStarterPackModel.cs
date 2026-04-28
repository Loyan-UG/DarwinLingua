namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedConversationStarterPackModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string Situation,
    string Tone,
    string ConversationGoal,
    IReadOnlyList<string> Topics,
    int SortOrder,
    IReadOnlyList<string> LinkedScenarioSlugs,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs,
    IReadOnlyList<ParsedConversationStarterPhraseModel> Phrases);
