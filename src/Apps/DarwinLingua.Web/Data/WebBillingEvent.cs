namespace DarwinLingua.Web.Data;

public sealed class WebBillingEvent
{
    public Guid Id { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string ProviderEventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string? ProviderCustomerId { get; set; }

    public string? ProviderSubscriptionId { get; set; }

    public string? ErrorSummary { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
