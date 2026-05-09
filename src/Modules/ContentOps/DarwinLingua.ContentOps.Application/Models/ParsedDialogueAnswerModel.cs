namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one selectable answer for a parsed dialogue question.
/// </summary>
public sealed record ParsedDialogueAnswerModel(
    string Text,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    bool IsCorrect,
    string? Feedback);
