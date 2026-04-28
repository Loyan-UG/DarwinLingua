namespace DarwinDeutsch.Maui.Services.Auth;

public interface IMobileEntitledFeatureAccessService
{
    Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken);

    Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken);

    Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken);

    Task<bool> CanUseEventPreparationPacksAsync(CancellationToken cancellationToken);

    Task EnsureCanUseEventPreparationPacksAsync(CancellationToken cancellationToken);

    Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken);
}
