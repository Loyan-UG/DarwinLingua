namespace DarwinLingua.Identity;

public interface IUserEntitlementService
{
    Task<UserEntitlementSnapshot> GetCurrentAsync(string userId, CancellationToken cancellationToken);

    Task<bool> HasFeatureAsync(string userId, string featureKey, CancellationToken cancellationToken);

    Task<UserEntitlementSnapshot> SetTierAsync(
        string userId,
        string tier,
        DateTimeOffset? expiresAtUtc,
        string? updatedBy,
        CancellationToken cancellationToken);
}
