namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents one parsed practical dialogue lesson from an import package.
/// </summary>
public sealed record ParsedDialogueLessonModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> Topics,
    int SortOrder,
    IReadOnlyList<ParsedDialogueTurnModel> DialogueTurns,
    IReadOnlyList<ParsedDialoguePhraseModel> UsefulPhrases,
    IReadOnlyList<ParsedDialogueQuestionModel> Questions);
