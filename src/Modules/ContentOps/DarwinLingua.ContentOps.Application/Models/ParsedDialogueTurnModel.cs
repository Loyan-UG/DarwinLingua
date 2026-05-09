namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one dialogue turn inside a parsed dialogue lesson.
/// </summary>
public sealed record ParsedDialogueTurnModel(
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations);
