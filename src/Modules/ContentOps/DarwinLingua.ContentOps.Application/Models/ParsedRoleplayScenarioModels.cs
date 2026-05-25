namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedRoleplayScenarioModel(
    string Slug,
    string? LinkedDialogueSlug,
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
    int EstimatedPracticeMinutes,
    IReadOnlyList<ParsedRoleplayRoleModel> Roles,
    IReadOnlyList<ParsedRoleplayTurnModel> Turns,
    IReadOnlyList<ParsedRoleplayAnswerChoiceGroupModel> AnswerChoices,
    IReadOnlyList<ParsedRoleplayStaticFeedbackModel> StaticFeedback,
    IReadOnlyList<ParsedRoleplayImageSlotModel> ImageSlots,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedRoleplayRoleModel(
    string RoleKey,
    string DisplayName,
    IReadOnlyList<ParsedContentMeaningModel> Translations);

public sealed record ParsedRoleplayTurnModel(
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    string? Function,
    string? ToneNote,
    string? ExpectedLearnerAction);

public sealed record ParsedRoleplayAnswerChoiceGroupModel(
    int TurnSortOrder,
    IReadOnlyList<ParsedRoleplayAnswerChoiceModel> Choices);

public sealed record ParsedRoleplayAnswerChoiceModel(
    string Id,
    string Text,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    bool IsCorrect,
    string Feedback,
    IReadOnlyList<ParsedContentMeaningModel> FeedbackTranslations,
    string? ExplanationKey);

public sealed record ParsedRoleplayStaticFeedbackModel(
    int TurnSortOrder,
    string FeedbackType,
    string Text,
    IReadOnlyList<ParsedContentMeaningModel> Translations);

public sealed record ParsedRoleplayImageSlotModel(
    string SlotKey,
    string Placement,
    string Purpose,
    string AltText,
    IReadOnlyList<ParsedContentMeaningModel> AltTextTranslations,
    string ImagePrompt,
    string? AssetPath,
    bool IsRequired);
