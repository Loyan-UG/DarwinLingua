using DarwinLingua.Web.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Web.Services;

public interface IWebAdminDashboardQueryService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken);
}

internal sealed class WebAdminDashboardQueryService(IWebCatalogApiClient catalogApiClient) : IWebAdminDashboardQueryService
{
    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken)
    {
        AdminSystemReportResponse report = await catalogApiClient
            .GetAdminSystemReportAsync(ContentLanguageRequirements.DefaultTargetLearningLanguageCode, cancellationToken)
            .ConfigureAwait(false);

        return new AdminDashboardViewModel(
            report.Catalog.ActiveWordCount,
            report.Catalog.DraftWordCount,
            report.Catalog.TopicCount,
            report.Operations.ImportedPackageCount,
            report.Operations.FailedPackageCount,
            report.Operations.LastImportAtUtc,
            report.Social.OrganizerProfileCount,
            report.Social.ConversationEventCount,
            report.Social.EventRsvpCount,
            report.Social.PendingOrganizerClaimRequestCount,
            report.Social.LearnerConversationProfileCount,
            report.Social.PendingPartnerRequestCount,
            report.Moderation.PendingUserReportCount,
            report.Moderation.UserBlockCount,
            report.Moderation.ModerationDecisionAuditCount);
    }
}
