namespace DarwinLingua.Catalog.Application.Models;

public sealed record DialogueLessonListItemModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> TopicKeys);

public sealed record DialogueLessonDetailModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<DialogueTurnModel> DialogueTurns,
    IReadOnlyList<DialoguePhraseModel> UsefulPhrases,
    IReadOnlyList<DialogueQuestionModel> Questions);

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
