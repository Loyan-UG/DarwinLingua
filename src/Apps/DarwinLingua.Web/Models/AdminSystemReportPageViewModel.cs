using DarwinLingua.Web.Services;

namespace DarwinLingua.Web.Models;

public sealed record AdminSystemReportPageViewModel(
    DateTime GeneratedAtUtc,
    int IdentityUserCount,
    IReadOnlyList<AdminSystemReportMetricViewModel> CatalogMetrics,
    IReadOnlyList<AdminSystemReportMetricViewModel> SocialMetrics,
    IReadOnlyList<AdminSystemReportMetricViewModel> ModerationMetrics,
    IReadOnlyList<AdminSystemReportMetricViewModel> OperationsMetrics,
    IReadOnlyList<AdminSystemReportMetricViewModel> EmailMetrics,
    IReadOnlyList<WebProductAnalyticsSummaryItem> AnalyticsItems);

public sealed record AdminSystemReportMetricViewModel(
    string Label,
    string Value,
    string Description);
