namespace DarwinLingua.WebApi.Models;

public sealed record AdminSystemReportResponse(
    DateTime GeneratedAtUtc,
    AdminCatalogSystemReportResponse Catalog,
    AdminSocialSystemReportResponse Social,
    AdminModerationSystemReportResponse Moderation,
    AdminOperationsSystemReportResponse Operations);

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
