using System.Collections.Concurrent;
using System.Text;

namespace DarwinLingua.Web.Services;

public interface IWebProductAnalyticsService
{
    void Record(string eventName, string? scopeKey = null, int count = 1);

    IReadOnlyList<WebProductAnalyticsSummaryItem> GetSummary();
}

public static class WebProductAnalyticsEvents
{
    public const string ScenarioViewed = "scenario.viewed";
    public const string ScenarioCompleted = "scenario.completed";
    public const string ConversationStarterViewed = "conversation-starter.viewed";
    public const string FavoriteSaved = "favorite.saved";
    public const string EventViewed = "event.viewed";
    public const string EventRsvpSubmitted = "event-rsvp.submitted";
    public const string EventPreparationPackViewed = "event-preparation-pack.viewed";
    public const string OrganizerProfileViewed = "organizer-profile.viewed";
    public const string PartnerRequestSent = "partner-request.sent";
    public const string PartnerRequestAccepted = "partner-request.accepted";
    public const string UserReported = "moderation.report.submitted";
    public const string UserBlocked = "moderation.user.blocked";
    public const string PremiumFeatureDenied = "premium-feature.denied";
}

public sealed record WebProductAnalyticsSummaryItem(
    string EventName,
    string ScopeKey,
    int Count,
    DateTime FirstSeenAtUtc,
    DateTime LastSeenAtUtc);

internal sealed class WebProductAnalyticsService : IWebProductAnalyticsService
{
    private readonly ConcurrentDictionary<string, Counter> counters = new(StringComparer.Ordinal);

    public void Record(string eventName, string? scopeKey = null, int count = 1)
    {
        if (string.IsNullOrWhiteSpace(eventName) || count <= 0)
        {
            return;
        }

        string normalizedEventName = eventName.Trim().ToLowerInvariant();
        string normalizedScopeKey = NormalizeScope(scopeKey);
        string counterKey = $"{normalizedEventName}|{normalizedScopeKey}";
        DateTime nowUtc = DateTime.UtcNow;

        Counter counter = counters.GetOrAdd(counterKey, _ => new Counter(normalizedEventName, normalizedScopeKey, nowUtc));
        counter.Increment(count, nowUtc);
    }

    public IReadOnlyList<WebProductAnalyticsSummaryItem> GetSummary() =>
        counters.Values
            .Select(static counter => counter.ToSummary())
            .OrderBy(static item => item.EventName)
            .ThenBy(static item => item.ScopeKey)
            .ToArray();

    private static string NormalizeScope(string? scopeKey)
    {
        if (string.IsNullOrWhiteSpace(scopeKey))
        {
            return "all";
        }

        string trimmed = scopeKey.Trim().ToLowerInvariant();
        StringBuilder normalized = new(capacity: Math.Min(trimmed.Length, 128));
        foreach (char character in trimmed)
        {
            if (normalized.Length >= 128)
            {
                break;
            }

            if (char.IsAsciiLetterOrDigit(character)
                || character is '.' or '-' or '_' or ':' or '/')
            {
                normalized.Append(character);
            }
        }

        return normalized.Length == 0 ? "all" : normalized.ToString();
    }

    private sealed class Counter(string eventName, string scopeKey, DateTime createdAtUtc)
    {
        private readonly object gate = new();
        private readonly DateTime firstSeenAtUtc = createdAtUtc;
        private int count;
        private DateTime lastSeenAtUtc = createdAtUtc;

        public void Increment(int value, DateTime seenAtUtc)
        {
            lock (gate)
            {
                count += value;
                lastSeenAtUtc = seenAtUtc;
            }
        }

        public WebProductAnalyticsSummaryItem ToSummary()
        {
            lock (gate)
            {
                return new WebProductAnalyticsSummaryItem(eventName, scopeKey, count, firstSeenAtUtc, lastSeenAtUtc);
            }
        }
    }
}
