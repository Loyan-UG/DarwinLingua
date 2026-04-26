using DarwinLingua.Identity;

namespace DarwinLingua.Web.Services;

public interface IWebEntitledFeatureAccessService
{
    Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken);

    Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken);

    Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken);

    Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken);
}

internal sealed class WebEntitledFeatureAccessService(
    IWebActorContextAccessor actorContextAccessor,
    IUserEntitlementService userEntitlementService) : IWebEntitledFeatureAccessService
{
    public async Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken)
    {
        WebActorContext actor = actorContextAccessor.GetCurrentActor();
        if (!actor.IsAuthenticated || string.IsNullOrWhiteSpace(actor.UserId))
        {
            return false;
        }

        return await userEntitlementService
            .HasFeatureAsync(actor.UserId, DarwinLinguaFeatureKeys.Favorites, cancellationToken)
            .ConfigureAwait(false);
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
        WebActorContext actor = actorContextAccessor.GetCurrentActor();
        if (!actor.IsAuthenticated || string.IsNullOrWhiteSpace(actor.UserId))
        {
            return false;
        }

        return await userEntitlementService
            .HasFeatureAsync(actor.UserId, DarwinLinguaFeatureKeys.DualMeaningLanguage, cancellationToken)
            .ConfigureAwait(false);
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
