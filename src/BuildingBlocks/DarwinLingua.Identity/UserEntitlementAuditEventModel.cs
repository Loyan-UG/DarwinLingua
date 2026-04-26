namespace DarwinLingua.Identity;

public sealed record UserEntitlementAuditEventModel(
    Guid Id,
    string UserId,
    string EventType,
    string? PreviousTier,
    string NewTier,
    DateTimeOffset? PreviousTrialEndsAtUtc,
    DateTimeOffset? NewTrialEndsAtUtc,
    DateTimeOffset? PreviousPremiumEndsAtUtc,
    DateTimeOffset? NewPremiumEndsAtUtc,
    string UpdatedBy,
    DateTimeOffset CreatedAtUtc);
