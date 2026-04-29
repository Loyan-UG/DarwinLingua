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
    UserManager<DarwinLinguaIdentityUser> userManager) : Controller
{
    [HttpGet("", Name = "Admin_Reports_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        Task<AdminSystemReportResponse> reportTask = catalogApiClient.GetAdminSystemReportAsync(cancellationToken);
        Task<int> identityUserCountTask = userManager.Users.CountAsync(cancellationToken);

        await Task.WhenAll(reportTask, identityUserCountTask).ConfigureAwait(false);

        AdminSystemReportResponse report = await reportTask.ConfigureAwait(false);
        int identityUserCount = await identityUserCountTask.ConfigureAwait(false);

        return View(new AdminSystemReportPageViewModel(
            report.GeneratedAtUtc,
            identityUserCount,
            BuildCatalogMetrics(report.Catalog),
            BuildSocialMetrics(report.Social),
            BuildModerationMetrics(report.Moderation),
            BuildOperationsMetrics(report.Operations),
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
            new("Scenarios", catalog.ScenarioLessonCount.ToString(), "Scenario lessons available for guided practice."),
            new("Starter packs", catalog.ConversationStarterPackCount.ToString(), "Conversation starter packs available to learners."),
            new("Preparation packs", catalog.EventPreparationPackCount.ToString(), "Event preparation packs linked to scenarios and events."),
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
}
