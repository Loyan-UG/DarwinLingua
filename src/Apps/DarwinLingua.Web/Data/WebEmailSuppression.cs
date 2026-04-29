namespace DarwinLingua.Web.Data;

public sealed class WebEmailSuppression
{
    public Guid Id { get; set; }

    public string RecipientEmailHash { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public string? ProviderMessageId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset? LastSeenAtUtc { get; set; }
}
