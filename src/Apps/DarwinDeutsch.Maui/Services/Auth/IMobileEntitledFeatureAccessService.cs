namespace DarwinDeutsch.Maui.Services.Auth;

public interface IMobileEntitledFeatureAccessService
{
    Task<bool> CanUseFavoritesAsync(CancellationToken cancellationToken);

    Task EnsureCanUseFavoritesAsync(CancellationToken cancellationToken);

    Task<bool> CanUseDualMeaningLanguageAsync(CancellationToken cancellationToken);

    Task<string?> ResolveSecondaryMeaningLanguageAsync(string? requestedSecondaryMeaningLanguageCode, CancellationToken cancellationToken);
}
