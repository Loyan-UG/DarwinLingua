using DarwinLingua.Web.Services;

namespace DarwinLingua.Web.Models;

public sealed record AdminAnalyticsPageViewModel(
    IReadOnlyList<WebProductAnalyticsSummaryItem> Items,
    IReadOnlyDictionary<string, int> TotalsByEvent,
    IReadOnlyDictionary<string, int> TotalsByArea);
