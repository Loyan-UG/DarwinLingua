namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one useful phrase inside a parsed scenario lesson.
/// </summary>
public sealed record ParsedScenarioPhraseModel(
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    string? UsageNote);
