namespace DarwinDeutsch.Maui.Services.Auth;

internal sealed class MobileEntitledFeatureAccessService(
    IMobileAuthService mobileAuthService) : IMobileEntitledFeatureAccessService
{
    private const string FavoritesFeatureKey = "favorites";
    private const string DualMeaningLanguageFeatureKey = "dual-meaning-language";
    private const string EventPreparationPacksFeatureKey = "event-preparation-packs";

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
        return await HasFeatureAsync(DualMeaningLanguageFeatureKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken)
    {
        return await HasFeatureAsync(EventPreparationPacksFeatureKey, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken)
    {
        if (await CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        throw new InvalidOperationException("Preparation packs require an authenticated account with an active trial or premium entitlement.");
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

    private async Task<bool> HasFeatureAsync(string featureKey, CancellationToken cancellationToken)
    {
        MobileAuthSession? session = await mobileAuthService.GetCurrentSessionAsync(cancellationToken).ConfigureAwait(false);
        if (session is null)
        {
            return false;
        }

        return session.EnabledFeatures.Contains(featureKey, StringComparer.OrdinalIgnoreCase);
    }
}
