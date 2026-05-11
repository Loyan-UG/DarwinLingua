namespace DarwinLingua.Catalog.Application.Models;

public sealed record DialogueLessonListFilterModel(
    string? CefrLevel,
    string? Category,
    string? TopicKey,
    string? ExamProfile,
    string? SkillFocus,
    string? TaskType,
    string? InteractionMode,
    string? Register,
    string? Query);

public sealed record DialogueLessonListItemModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> ExamProfiles,
    IReadOnlyList<string> SkillFocus,
    string TaskType,
    string InteractionMode,
    string Register,
    int EstimatedPracticeMinutes);

public sealed record DialogueLessonDetailModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> ExamProfiles,
    IReadOnlyList<string> SkillFocus,
    string TaskType,
    string InteractionMode,
    string Register,
    IReadOnlyList<string> SpeakingFunctions,
    int EstimatedPracticeMinutes,
    string? DifficultyNote,
    string? ExamRelevance,
    IReadOnlyList<DialogueUsefulWordModel> UsefulWords,
    IReadOnlyList<DialogueSpeakingPromptModel> SpeakingPrompts,
    IReadOnlyList<DialogueTurnModel> DialogueTurns,
    IReadOnlyList<DialoguePhraseModel> UsefulPhrases,
    IReadOnlyList<DialogueQuestionModel> Questions);

public sealed record DialogueUsefulWordModel(
    string Lemma,
    string? WordSlug,
    string? CefrLevel,
    int SortOrder);

public sealed record DialogueSpeakingPromptModel(
    string PromptType,
    string Prompt,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    int SortOrder);

public sealed record DialogueTurnModel(
    string SpeakerRole,
    string BaseText,
    string? PrimaryMeaning,
    string? SecondaryMeaning);

public sealed record DialoguePhraseModel(
    string BaseText,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    string? UsageNote);

public sealed record DialogueQuestionModel(
    string Prompt,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    IReadOnlyList<DialogueAnswerModel> Answers);

public sealed record DialogueAnswerModel(
    string Text,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    bool IsCorrect,
    string? Feedback);
