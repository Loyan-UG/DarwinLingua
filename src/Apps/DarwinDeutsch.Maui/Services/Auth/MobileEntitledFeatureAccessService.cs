namespace DarwinDeutsch.Maui.Services.Auth;

internal sealed class MobileEntitledFeatureAccessService(
    IMobileAuthService mobileAuthService) : IMobileEntitledFeatureAccessService
{
    private const string FavoritesFeatureKey = "favorites";
    private const string DualMeaningLanguageFeatureKey = "dual-meaning-language";

    public async Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken)
    {
        MobileAuthSession? session = await mobileAuthService.GetCurrentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return false;
        }

        return session.EnabledFeatures.Contains(FavoritesFeatureKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken)
    {
        if (await CanUseFavoritesAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        throw new InvalidOperationException("Favorites require an authenticated account with an active trial or premium entitlement.");
    }

    public async Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken)
    {
        MobileAuthSession? session = await mobileAuthService.GetCurrentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return false;
        }

        return session.EnabledFeatures.Contains(DualMeaningLanguageFeatureKey, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestedSecondaryMeaningLanguageCode))
        {
            return null;
        }

        return await CanUseDualMeaningLanguageAsync(cancellationToken).ConfigureAwait(false)
            ? requestedSecondaryMeaningLanguageCode
            : null;
    }
}
