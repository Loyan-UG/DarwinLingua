namespace DarwinLingua.Catalog.Application.Models;

public sealed record RoleplayScenarioListFilterModel(
    string? CefrLevel,
    string? Category,
    string? TopicKey,
    string? ExamProfile,
    string? SkillFocus,
    string? TaskType,
    string? InteractionMode,
    string? Register,
    string? Query);

public sealed record RoleplayScenarioListItemModel(
    string Slug,
    string? LinkedDialogueSlug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    string TaskType,
    string InteractionMode,
    string Register,
    int EstimatedPracticeMinutes,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> ExamProfiles,
    IReadOnlyList<string> SkillFocus);

public sealed record RoleplayScenarioDetailModel(
    string Slug,
    string? LinkedDialogueSlug,
    string Title,
    string Description,
    string LearnerGoal,
    string CefrLevel,
    string Category,
    string TaskType,
    string InteractionMode,
    string Register,
    int EstimatedPracticeMinutes,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> ExamProfiles,
    IReadOnlyList<string> SkillFocus,
    IReadOnlyList<RoleplayScenarioRoleModel> Roles,
    IReadOnlyList<RoleplayScenarioTurnModel> Turns,
    IReadOnlyList<RoleplayScenarioAnswerChoiceGroupModel> AnswerChoices,
    IReadOnlyList<RoleplayScenarioStaticFeedbackModel> StaticFeedback,
    IReadOnlyList<RoleplayScenarioImageSlotModel> ImageSlots);

public sealed record RoleplayScenarioRoleModel(
    string RoleKey,
    string DisplayName,
    string? LearnerLanguageDisplayName);

public sealed record RoleplayScenarioTurnModel(
    int SortOrder,
    string SpeakerRole,
    string BaseText,
    string? LearnerLanguageText,
    string? Function,
    string? ToneNote,
    string? ExpectedLearnerAction);

public sealed record RoleplayScenarioAnswerChoiceGroupModel(
    int TurnSortOrder,
    IReadOnlyList<RoleplayScenarioAnswerChoiceModel> Choices);

public sealed record RoleplayScenarioAnswerChoiceModel(
    string Id,
    string Text,
    string? LearnerLanguageText,
    bool IsCorrect,
    string Feedback,
    string? LearnerLanguageFeedback,
    string? ExplanationKey);

public sealed record RoleplayScenarioStaticFeedbackModel(
    int TurnSortOrder,
    string FeedbackType,
    string Text,
    string? LearnerLanguageText);

public sealed record RoleplayScenarioImageSlotModel(
    string SlotKey,
    string Placement,
    string Purpose,
    string AltText,
    string? LearnerLanguageAltText,
    string ImagePrompt,
    string? AssetPath,
    bool IsRequired);
