namespace DarwinLingua.Identity;

public sealed class UserEntitlementState
{
    public string UserId { get; set; } = string.Empty;

    public string Tier { get; set; } = DarwinLinguaEntitlementTiers.Free;

    public DateTimeOffset? TrialStartedAtUtc { get; set; }

    public DateTimeOffset? TrialEndsAtUtc { get; set; }

    public DateTimeOffset? PremiumStartedAtUtc { get; set; }

    public DateTimeOffset? PremiumEndsAtUtc { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public string? LastUpdatedBy { get; set; }
}
