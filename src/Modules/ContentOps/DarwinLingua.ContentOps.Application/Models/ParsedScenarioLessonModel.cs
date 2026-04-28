namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed practical scenario lesson from an import package.
/// </summary>
public sealed record ParsedScenarioLessonModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> Topics,
    int SortOrder,
    IReadOnlyList<ParsedScenarioDialogueTurnModel> DialogueTurns,
    IReadOnlyList<ParsedScenarioPhraseModel> UsefulPhrases,
    IReadOnlyList<ParsedScenarioQuestionModel> Questions);
