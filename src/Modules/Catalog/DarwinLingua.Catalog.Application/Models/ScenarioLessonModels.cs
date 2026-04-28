namespace DarwinLingua.Catalog.Application.Models;

public sealed record ScenarioLessonListItemModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> TopicKeys);

public sealed record ScenarioLessonDetailModel(
    string Slug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<ScenarioDialogueTurnModel> DialogueTurns,
    IReadOnlyList<ScenarioPhraseModel> UsefulPhrases,
    IReadOnlyList<ScenarioQuestionModel> Questions);

public sealed record ScenarioDialogueTurnModel(
    string SpeakerRole,
    string BaseText,
    string? PrimaryMeaning,
    string? SecondaryMeaning);

public sealed record ScenarioPhraseModel(
    string BaseText,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    string? UsageNote);

public sealed record ScenarioQuestionModel(
    string Prompt,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    IReadOnlyList<ScenarioAnswerModel> Answers);

public sealed record ScenarioAnswerModel(
    string Text,
    string? PrimaryMeaning,
    string? SecondaryMeaning,
    bool IsCorrect,
    string? Feedback);
