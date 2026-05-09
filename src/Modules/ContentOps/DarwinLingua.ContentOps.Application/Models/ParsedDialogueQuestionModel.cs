namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one comprehension or preparation question inside a parsed dialogue lesson.
/// </summary>
public sealed record ParsedDialogueQuestionModel(
    string Prompt,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    IReadOnlyList<ParsedDialogueAnswerModel> Answers);
