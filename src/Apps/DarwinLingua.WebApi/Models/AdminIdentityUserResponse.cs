namespace DarwinLingua.WebApi.Models;

public sealed record AdminIdentityUserResponse(
    string UserId,
    string? Email,
    IReadOnlyList<string> Roles,
    string EntitlementTier,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? PremiumEndsAtUtc,
    IReadOnlyList<string> EnabledFeatures,
    IReadOnlyList<AdminIdentityEntitlementAuditEventResponse> RecentEntitlementEvents);

public sealed record AdminIdentityEntitlementAuditEventResponse(
    string EventType,
    string? PreviousTier,
    string NewTier,
    string UpdatedBy,
    DateTimeOffset CreatedAtUtc);
