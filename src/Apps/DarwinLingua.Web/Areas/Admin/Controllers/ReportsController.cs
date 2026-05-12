using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Admin")]
[Route("admin/reports")]
public sealed class ReportsController(
    IWebCatalogApiClient catalogApiClient,
    IWebProductAnalyticsService analyticsService,
    IEmailDeliveryLogRepository emailDeliveryLogRepository,
    UserManager<DarwinLinguaIdentityUser> userManager) : Controller
{
    [HttpGet("", Name = "Admin_Reports_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Task<AdminSystemReportResponse> reportTask = catalogApiClient.GetAdminSystemReportAsync(cancellationToken);
        int identityUserCount = await userManager.Users.CountAsync(cancellationToken).ConfigureAwait(false);
        EmailDeliverySummary emailSummary = await emailDeliveryLogRepository
            .GetSummarySinceAsync(DateTimeOffset.UtcNow.AddHours(-24), cancellationToken)
            .ConfigureAwait(false);
        AdminSystemReportResponse report = await reportTask.ConfigureAwait(false);

        return View(new AdminSystemReportPageViewModel(
            report.GeneratedAtUtc,
            identityUserCount,
            BuildCatalogMetrics(report.Catalog),
            BuildSocialMetrics(report.Social),
            BuildModerationMetrics(report.Moderation),
            BuildOperationsMetrics(report.Operations),
            BuildEmailMetrics(emailSummary),
            BuildLearningPortalQualityMetrics(report.LearningPortal),
            report.LearningPortal,
            analyticsService.GetSummary()
                .OrderByDescending(item => item.Count)
                .ThenBy(item => item.EventName, StringComparer.Ordinal)
                .Take(20)
                .ToArray()));
    }

    private static IReadOnlyList<AdminSystemReportMetricViewModel> BuildCatalogMetrics(AdminCatalogSystemReportResponse catalog) =>
        [
            new("Active words", catalog.ActiveWordCount.ToString(), "Published vocabulary rows visible to learners."),
            new("Draft words", catalog.DraftWordCount.ToString(), "Vocabulary rows waiting outside the learner catalog."),
            new("Topics", catalog.TopicCount.ToString(), "Taxonomy topics available for browsing and grouping."),
            new("Dialogues", catalog.DialogueLessonCount.ToString(), "Dialogue lessons available for guided practice."),
            new("Starter packs", catalog.ConversationStarterPackCount.ToString(), "Conversation starter packs available to learners."),
            new("Preparation packs", catalog.EventPreparationPackCount.ToString(), "Event preparation packs linked to dialogues and events."),
        ];

    private static IReadOnlyList<AdminSystemReportMetricViewModel> BuildLearningPortalQualityMetrics(AdminLearningPortalSystemReportResponse learningPortal) =>
        [
            new("Unresolved linked words", learningPortal.UnresolvedLinkedWordCount.ToString(), "Linked word references that do not resolve to a known catalog word key."),
            new("Unresolved content links", learningPortal.UnresolvedLinkedContentReferenceCount.ToString(), "Cross-content slug references that do not resolve to implemented learning content."),
            new("Missing translations", learningPortal.MissingTranslationCount.ToString(), "Localized child records without learner-language translation rows."),
            new("Unpublished drafts", learningPortal.UnpublishedDraftCount.ToString(), "Learning Portal content rows not marked Active."),
            new("Grammar without exercises", learningPortal.GrammarTopicsMissingExercises.ToString(), "Grammar topics without linked exercises."),
            new("Lessons without exercise sets", learningPortal.CourseLessonsMissingExerciseSets.ToString(), "Course lessons without linked exercise sets."),
        ];

    private static IReadOnlyList<AdminSystemReportMetricViewModel> BuildSocialMetrics(AdminSocialSystemReportResponse social) =>
        [
            new("Organizer profiles", social.OrganizerProfileCount.ToString(), "Public or managed organizer directory records."),
            new("Events", social.ConversationEventCount.ToString(), "Conversation events in the directory."),
            new("Online events", social.OnlineConversationEventCount.ToString(), "Events marked as online."),
            new("RSVPs", social.EventRsvpCount.ToString(), "Learner event interest and attendance records."),
            new("Claims", social.OrganizerClaimRequestCount.ToString(), "Organizer profile claim requests."),
            new("Pending claims", social.PendingOrganizerClaimRequestCount.ToString(), "Claims still submitted or under review."),
            new("Owners", social.OrganizerProfileOwnerCount.ToString(), "Organizer profile owner assignments."),
            new("Learner profiles", social.LearnerConversationProfileCount.ToString(), "Learner conversation profiles created for matching."),
            new("Public learner profiles", social.PublicLearnerConversationProfileCount.ToString(), "Profiles discoverable in partner search."),
            new("Partner requests", social.PartnerRequestCount.ToString(), "Conversation partner requests."),
            new("Pending partner requests", social.PendingPartnerRequestCount.ToString(), "Partner requests still awaiting a response."),
        ];

    private static IReadOnlyList<AdminSystemReportMetricViewModel> BuildModerationMetrics(AdminModerationSystemReportResponse moderation) =>
        [
            new("User reports", moderation.UserReportCount.ToString(), "Reports submitted by learners or organizers."),
            new("Pending reports", moderation.PendingUserReportCount.ToString(), "Reports still waiting for a moderation decision."),
            new("User blocks", moderation.UserBlockCount.ToString(), "Blocks created from learner safety flows."),
            new("Decision audits", moderation.ModerationDecisionAuditCount.ToString(), "Recorded moderation decision audit events."),
        ];

    private static IReadOnlyList<AdminSystemReportMetricViewModel> BuildOperationsMetrics(AdminOperationsSystemReportResponse operations) =>
        [
            new("Imported packages", operations.ImportedPackageCount.ToString(), "Content packages recorded in the server catalog history."),
            new("Failed imports", operations.FailedPackageCount.ToString(), "Packages with failed import status."),
            new("Last import", operations.LastImportAtUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "No imports", "Most recent content package creation time."),
        ];

    private static IReadOnlyList<AdminSystemReportMetricViewModel> BuildEmailMetrics(EmailDeliverySummary summary) =>
        [
            new("Sent in 24h", summary.SentCount.ToString(), "Transactional emails sent in the last 24 hours."),
            new("Failed in 24h", summary.FailedCount.ToString(), "Transactional email delivery failures in the last 24 hours."),
            new("Queued in 24h", summary.QueuedCount.ToString(), "Transactional emails still queued in the last 24 hours."),
            new("Skipped/suppressed in 24h", (summary.SkippedCount + summary.SuppressedCount).ToString(), "Transactional emails skipped or suppressed in the last 24 hours."),
            new(
                "Last email failure",
                summary.LastFailureAtUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "None",
                string.IsNullOrWhiteSpace(summary.LastFailureScenarioKey)
                    ? "No failed delivery has been recorded."
                    : $"{summary.LastFailureScenarioKey} ({summary.LastFailureCode ?? "no failure code"})."),
            new(
                "Last provider event",
                summary.LastProviderEventAtUtc?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "None",
                string.IsNullOrWhiteSpace(summary.LastProviderEvent)
                    ? "No Brevo/provider webhook event has been recorded."
                    : $"Last provider event: {summary.LastProviderEvent}."),
        ];
}
