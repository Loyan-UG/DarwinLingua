namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one selectable answer for a parsed scenario question.
/// </summary>
public sealed record ParsedScenarioAnswerModel(
    string Text,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    bool IsCorrect,
    string? Feedback);
