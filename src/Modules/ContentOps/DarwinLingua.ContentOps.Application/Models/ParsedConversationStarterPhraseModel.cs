namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedConversationStarterPhraseModel(
    string BaseText,
    string Function,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    string? UsageNote,
    string? Register,
    int SortOrder,
    IReadOnlyList<string> AlternativeBaseTexts,
    string? CommonMistake);
