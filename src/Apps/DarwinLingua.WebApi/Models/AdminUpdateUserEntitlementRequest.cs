namespace DarwinLingua.WebApi.Models;

public sealed record AdminUpdateUserEntitlementRequest(
    string Tier,
    DateTimeOffset? ExpiresAtUtc);
