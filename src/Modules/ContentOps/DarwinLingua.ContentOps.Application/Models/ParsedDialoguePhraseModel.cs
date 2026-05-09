namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one useful phrase inside a parsed dialogue lesson.
/// </summary>
public sealed record ParsedDialoguePhraseModel(
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    string? UsageNote);
