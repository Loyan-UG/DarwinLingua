namespace DarwinLingua.WebApi.Models;

public sealed record AdminSystemReportResponse(
    DateTime GeneratedAtUtc,
    AdminCatalogSystemReportResponse Catalog,
    AdminSocialSystemReportResponse Social,
    AdminModerationSystemReportResponse Moderation,
    AdminOperationsSystemReportResponse Operations,
    AdminLearningPortalSystemReportResponse LearningPortal);

public sealed record AdminCatalogSystemReportResponse(
    int ActiveWordCount,
    int DraftWordCount,
    int TopicCount,
    int DialogueLessonCount,
    int ConversationStarterPackCount,
    int EventPreparationPackCount);

public sealed record AdminSocialSystemReportResponse(
    int OrganizerProfileCount,
    int ConversationEventCount,
    int OnlineConversationEventCount,
    int EventRsvpCount,
    int OrganizerClaimRequestCount,
    int PendingOrganizerClaimRequestCount,
    int OrganizerProfileOwnerCount,
    int LearnerConversationProfileCount,
    int PublicLearnerConversationProfileCount,
    int PartnerRequestCount,
    int PendingPartnerRequestCount);

public sealed record AdminModerationSystemReportResponse(
    int UserReportCount,
    int PendingUserReportCount,
    int UserBlockCount,
    int ModerationDecisionAuditCount);

public sealed record AdminOperationsSystemReportResponse(
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc);

public sealed record AdminLearningPortalSystemReportResponse(
    IReadOnlyList<AdminLearningPortalCountRowResponse> CountsByType,
    IReadOnlyList<AdminLearningPortalCountRowResponse> CountsByCefr,
    IReadOnlyList<AdminLearningPortalCountRowResponse> GrammarByCategory,
    IReadOnlyList<AdminLearningPortalCountRowResponse> ExpressionsByType,
    IReadOnlyList<AdminLearningPortalCountRowResponse> ExpressionsByRegister,
    IReadOnlyList<AdminLearningPortalCountRowResponse> ExercisesByType,
    IReadOnlyList<AdminLearningPortalCountRowResponse> ExercisesByTargetSkill,
    IReadOnlyList<AdminLearningPortalCountRowResponse> CourseLessonsByCourse,
    IReadOnlyList<AdminLearningPortalCountRowResponse> CourseLessonsByCefr,
    IReadOnlyList<AdminLearningPortalCountRowResponse> CourseLessonsByModule,
    IReadOnlyList<AdminLearningPortalCountRowResponse> ExamPrepByProfile,
    IReadOnlyList<AdminLearningPortalCountRowResponse> WritingTemplatesByCategory,
    IReadOnlyList<AdminLearningPortalCountRowResponse> WritingTemplatesByRegister,
    IReadOnlyList<AdminLearningPortalCountRowResponse> CulturalNotesByCategory,
    int UnresolvedLinkedWordCount,
    int UnresolvedLinkedContentReferenceCount,
    int MissingTranslationCount,
    int UnpublishedDraftCount,
    int GrammarTopicsMissingExercises,
    int CourseLessonsMissingExerciseSets,
    IReadOnlyList<AdminLearningPortalIssueRowResponse> SampleIssues);

public sealed record AdminLearningPortalCountRowResponse(
    string Key,
    int Count);

public sealed record AdminLearningPortalIssueRowResponse(
    string Area,
    string Owner,
    string Issue,
    string? Target);
