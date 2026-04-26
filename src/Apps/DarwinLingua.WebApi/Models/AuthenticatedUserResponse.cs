namespace DarwinLingua.WebApi.Models;

public sealed record AuthenticatedUserResponse(
    string UserId,
    string? Email,
    bool IsAuthenticated,
    IReadOnlyList<string> Roles,
    string EntitlementTier,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? PremiumEndsAtUtc,
    IReadOnlyList<string> EnabledFeatures);
