namespace DarwinLingua.Identity;

public sealed class UserEntitlementAuditEvent
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public string? PreviousTier { get; set; }

    public string NewTier { get; set; } = DarwinLinguaEntitlementTiers.Free;

    public DateTimeOffset? PreviousTrialEndsAtUtc { get; set; }

    public DateTimeOffset? NewTrialEndsAtUtc { get; set; }

    public DateTimeOffset? PreviousPremiumEndsAtUtc { get; set; }

    public DateTimeOffset? NewPremiumEndsAtUtc { get; set; }

    public string UpdatedBy { get; set; } = "system";

    public DateTimeOffset CreatedAtUtc { get; set; }
}
