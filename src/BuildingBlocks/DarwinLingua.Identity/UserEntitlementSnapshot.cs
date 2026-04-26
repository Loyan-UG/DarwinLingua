namespace DarwinLingua.Identity;

public sealed record UserEntitlementSnapshot(
    string UserId,
    string Tier,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? PremiumEndsAtUtc,
    IReadOnlyList<string> EnabledFeatures);
