namespace DarwinDeutsch.Maui.Services.Auth;

public sealed record MobileAuthSession(
    string UserId,
    string Email,
    string AccessToken,
    string? RefreshToken,
    DateTimeOffset ExpiresAtUtc,
    IReadOnlyList<string> Roles,
    string EntitlementTier,
    DateTimeOffset? TrialEndsAtUtc,
    DateTimeOffset? PremiumEndsAtUtc,
    IReadOnlyList<string> EnabledFeatures)
{
    public bool IsExpired(DateTimeOffset nowUtc) => nowUtc >= ExpiresAtUtc;
}
