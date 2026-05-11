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
    IReadOnlyList<string> ExamProfiles,
    IReadOnlyList<string> SkillFocus,
    string TaskType,
    string InteractionMode,
    string Register,
    IReadOnlyList<string> SpeakingFunctions,
    int EstimatedPracticeMinutes,
    string? DifficultyNote,
    string? ExamRelevance,
    IReadOnlyList<ParsedDialogueUsefulWordModel> UsefulWords,
    IReadOnlyList<ParsedDialogueSpeakingPromptModel> SpeakingPrompts,
    int SortOrder,
    IReadOnlyList<ParsedDialogueTurnModel> DialogueTurns,
    IReadOnlyList<ParsedDialoguePhraseModel> UsefulPhrases,
    IReadOnlyList<ParsedDialogueQuestionModel> Questions);

public sealed record ParsedDialogueUsefulWordModel(
    string Lemma,
    string? WordSlug,
    string? CefrLevel,
    int SortOrder);

public sealed record ParsedDialogueSpeakingPromptModel(
    string PromptType,
    string Prompt,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);
