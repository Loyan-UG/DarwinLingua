namespace DarwinLingua.Web.Models;

public sealed record AdminDashboardViewModel(
    int ActiveWordCount,
    int DraftWordCount,
    int TotalTopicCount,
    int ImportedPackageCount,
    int FailedPackageCount,
    DateTime? LastImportAtUtc,
    int OrganizerProfileCount = 0,
    int ConversationEventCount = 0,
    int EventRsvpCount = 0,
    int PendingOrganizerClaimRequestCount = 0,
    int LearnerConversationProfileCount = 0,
    int PendingPartnerRequestCount = 0,
    int PendingUserReportCount = 0,
    int UserBlockCount = 0,
    int ModerationDecisionAuditCount = 0);
