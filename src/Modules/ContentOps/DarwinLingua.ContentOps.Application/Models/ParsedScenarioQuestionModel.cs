namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one comprehension or preparation question inside a parsed scenario lesson.
/// </summary>
public sealed record ParsedScenarioQuestionModel(
    string Prompt,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    IReadOnlyList<ParsedScenarioAnswerModel> Answers);
