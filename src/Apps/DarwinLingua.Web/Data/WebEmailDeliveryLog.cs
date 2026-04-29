namespace DarwinLingua.Web.Data;

public enum WebEmailDeliveryStatus
{
    Queued = 0,
    Sent = 1,
    Failed = 2,
    Skipped = 3,
    Suppressed = 4,
}

public sealed class WebEmailDeliveryLog
{
    public Guid Id { get; set; }

    public string ScenarioKey { get; set; } = string.Empty;

    public string RecipientEmailHash { get; set; } = string.Empty;

    public string? RecipientUserId { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public string Culture { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public string? ProviderMessageId { get; set; }

    public string? ProviderLastEvent { get; set; }

    public DateTimeOffset? ProviderLastEventAtUtc { get; set; }

    public string? ProviderLastEventReason { get; set; }

    public WebEmailDeliveryStatus Status { get; set; }

    public string? FailureCode { get; set; }

    public string? FailureMessageSummary { get; set; }

    public int RetryCount { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? SentAtUtc { get; set; }

    public DateTimeOffset? LastAttemptAtUtc { get; set; }

    public string? CorrelationId { get; set; }
}
