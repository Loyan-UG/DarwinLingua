using System.Globalization;
using DarwinDeutsch.Maui.Resources.Strings;
using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Services.Localization;

/// <summary>
/// Implements device-aware UI culture selection for the MAUI application.
/// </summary>
internal sealed class AppLocalizationService : IAppLocalizationService
{
    private static readonly string[] SupportedCultureNames = ["en", "de"];
    private readonly IActiveLearningProfileCacheService _activeLearningProfileCacheService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppLocalizationService"/> class.
    /// </summary>
    /// <param name="activeLearningProfileCacheService">The cache-aware learning profile service used to persist UI language selection.</param>
    public AppLocalizationService(IActiveLearningProfileCacheService activeLearningProfileCacheService)
    {
        ArgumentNullException.ThrowIfNull(activeLearningProfileCacheService);

        _activeLearningProfileCacheService = activeLearningProfileCacheService;
    }

    /// <inheritdoc />
    public event EventHandler? CultureChanged;

    /// <inheritdoc />
    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.InvariantCulture;

    /// <inheritdoc />
    public IReadOnlyList<UiLanguageOption> GetSupportedLanguages()
    {
        return
        [
            new UiLanguageOption("en", AppStrings.LanguageOptionEnglish),
            new UiLanguageOption("de", AppStrings.LanguageOptionGerman),
        ];
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        string requestedCultureName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        UserLearningProfileModel profile = await _activeLearningProfileCacheService
            .EnsureLocalProfileExistsAsync(requestedCultureName, cancellationToken)
            .ConfigureAwait(false);
        CultureInfo selectedCulture = ResolveSupportedCulture(new CultureInfo(profile.UiLanguageCode));

        ApplyCulture(selectedCulture, raiseEvent: false);
    }

    /// <inheritdoc />
    public async Task SetCultureAsync(string cultureName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cultureName);

        CultureInfo selectedCulture = ResolveSupportedCulture(new CultureInfo(cultureName));
        await _activeLearningProfileCacheService
            .UpdateUiLanguagePreferenceAsync(selectedCulture.TwoLetterISOLanguageName, cancellationToken)
            .ConfigureAwait(false);

        ApplyCulture(selectedCulture, raiseEvent: true);
    }

    /// <summary>
    /// Resolves the best supported culture from the incoming culture request.
    /// </summary>
    /// <param name="requestedCulture">The incoming requested culture.</param>
    /// <returns>The matched supported culture.</returns>
    private static CultureInfo ResolveSupportedCulture(CultureInfo requestedCulture)
    {
        ArgumentNullException.ThrowIfNull(requestedCulture);

        string? matchedCultureName = SupportedCultureNames
            .FirstOrDefault(cultureName => string.Equals(
                cultureName,
                requestedCulture.TwoLetterISOLanguageName,
                StringComparison.OrdinalIgnoreCase));

        return new CultureInfo(matchedCultureName ?? "en");
    }

    /// <summary>
    /// Applies the resolved culture to the current process and resource manager.
    /// </summary>
    /// <param name="culture">The resolved supported culture.</param>
    /// <param name="raiseEvent">A value indicating whether listeners should be notified.</param>
    private void ApplyCulture(CultureInfo culture, bool raiseEvent)
    {
        ArgumentNullException.ThrowIfNull(culture);

        CurrentCulture = culture;
        AppStrings.Culture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        if (raiseEvent)
        {
            CultureChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
