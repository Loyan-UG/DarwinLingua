namespace DarwinLingua.Web.Data;

public sealed class WebBillingNotification
{
    public Guid Id { get; set; }

    public string NotificationKey { get; set; } = string.Empty;

    public string ScenarioKey { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string? ProviderSubscriptionId { get; set; }

    public string? BillingStatus { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
}
