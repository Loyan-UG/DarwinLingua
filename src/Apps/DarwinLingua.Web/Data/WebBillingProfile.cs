namespace DarwinLingua.Web.Data;

public sealed class WebBillingProfile
{
    public string UserId { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public string? ProviderCustomerId { get; set; }

    public string? ProviderSubscriptionId { get; set; }

    public string PlanKey { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset? CurrentPeriodEndsAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
