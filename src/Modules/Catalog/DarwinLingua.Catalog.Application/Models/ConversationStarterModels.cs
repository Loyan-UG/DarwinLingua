namespace DarwinLingua.Catalog.Application.Models;

public sealed record ConversationStarterListFilterModel(
    string? CefrLevel,
    string? Situation,
    string? Tone,
    string? ConversationGoal,
    string? TopicKey);

public sealed record ConversationStarterPackListItemModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string Situation,
    string Tone,
    string ConversationGoal,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> LinkedDialogueSlugs);

public sealed record ConversationStarterPackDetailModel(
    string Slug,
    string Title,
    string Description,
    string CefrLevel,
    string Category,
    string Situation,
    string Tone,
    string ConversationGoal,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedEventPreparationPackSlugs,
    IReadOnlyList<ConversationStarterPhraseModel> Phrases);

public sealed record ConversationStarterPhraseModel(
    string BaseText,
    string Function,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    string? UsageNote,
    string? Register,
    IReadOnlyList<string> AlternativeBaseTexts,
    string? CommonMistake);
