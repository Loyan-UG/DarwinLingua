using DarwinLingua.Identity;

namespace DarwinLingua.Web.Services;

public interface IWebEntitledFeatureAccessService
{
    Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken);

    Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken);

    Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken);

    Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken);

    Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken);

    Task<bool> CanUseAdvancedScenarioPacksAsync(CancellationToken cancellationToken) =>
        Task.FromResult(false);

    Task EnsureCanUseAdvancedScenarioPacksAsync(CancellationToken cancellationToken) =>
        throw new FeatureAccessDeniedException(
            DarwinLinguaFeatureKeys.AdvancedScenarioPacks,
            "Advanced scenario packs require an authenticated account with an active trial or premium entitlement.");

    Task<bool> CanUsePartnerMatchingAsync(CancellationToken cancellationToken) =>
        Task.FromResult(false);

    Task EnsureCanUsePartnerMatchingAsync(CancellationToken cancellationToken) =>
        throw new FeatureAccessDeniedException(
            DarwinLinguaFeatureKeys.PartnerMatching,
            "Partner matching requires an authenticated account with an active trial or premium entitlement.");

    Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken);
}

internal sealed class WebEntitledFeatureAccessService(
    IWebActorContextAccessor actorContextAccessor,
    IUserEntitlementService userEntitlementService) : IWebEntitledFeatureAccessService
{
    public async Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken)
    {
        return await HasFeatureAsync(DarwinLinguaFeatureKeys.Favorites, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken)
    {
        if (await CanUseFavoritesAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        throw new FeatureAccessDeniedException(
            DarwinLinguaFeatureKeys.Favorites,
            "Favorites require an authenticated account with an active trial or premium entitlement.");
    }

    public async Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken)
    {
        return await HasFeatureAsync(DarwinLinguaFeatureKeys.DualMeaningLanguage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken)
    {
        return await HasFeatureAsync(DarwinLinguaFeatureKeys.EventPreparationPacks, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken)
    {
        if (await CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        throw new FeatureAccessDeniedException(
            DarwinLinguaFeatureKeys.EventPreparationPacks,
            "Preparation packs require an authenticated account with an active trial or premium entitlement.");
    }

    public async Task<bool> CanUseAdvancedScenarioPacksAsync(CancellationToken cancellationToken)
    {
        return await HasFeatureAsync(DarwinLinguaFeatureKeys.AdvancedScenarioPacks, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureCanUseAdvancedScenarioPacksAsync(CancellationToken cancellationToken)
    {
        if (await CanUseAdvancedScenarioPacksAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        throw new FeatureAccessDeniedException(
            DarwinLinguaFeatureKeys.AdvancedScenarioPacks,
            "Advanced scenario packs require an authenticated account with an active trial or premium entitlement.");
    }

    public async Task<bool> CanUsePartnerMatchingAsync(CancellationToken cancellationToken)
    {
        return await HasFeatureAsync(DarwinLinguaFeatureKeys.PartnerMatching, cancellationToken).ConfigureAwait(false);
    }

    public async Task EnsureCanUsePartnerMatchingAsync(CancellationToken cancellationToken)
    {
        if (await CanUsePartnerMatchingAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        throw new FeatureAccessDeniedException(
            DarwinLinguaFeatureKeys.PartnerMatching,
            "Partner matching requires an authenticated account with an active trial or premium entitlement.");
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
        WebActorContext actor = actorContextAccessor.GetCurrentActor();
        if (!actor.IsAuthenticated || string.IsNullOrWhiteSpace(actor.UserId))
        {
            return false;
        }

        return await userEntitlementService
            .HasFeatureAsync(actor.UserId, featureKey, cancellationToken)
            .ConfigureAwait(false);
    }
}
