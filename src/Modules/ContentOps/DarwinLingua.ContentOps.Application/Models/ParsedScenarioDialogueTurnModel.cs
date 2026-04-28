namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one dialogue turn inside a parsed scenario lesson.
/// </summary>
public sealed record ParsedScenarioDialogueTurnModel(
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations);
