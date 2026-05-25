namespace DarwinLingua.Web.Data;

public sealed class WebPolicyAcceptance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; } = string.Empty;

    public string PolicyKey { get; set; } = string.Empty;

    public string PolicyVersion { get; set; } = string.Empty;

    public DateTime AcceptedAtUtc { get; set; } = DateTime.UtcNow;

    public string Source { get; set; } = string.Empty;

    public string? Culture { get; set; }
}
