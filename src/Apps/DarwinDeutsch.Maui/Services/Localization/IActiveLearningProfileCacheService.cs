using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Services.Localization;

/// <summary>
/// Provides a session-level cache for the active learning profile.
/// </summary>
public interface IActiveLearningProfileCacheService
{
    /// <summary>
    /// Returns the current learning profile, using session cache when available.
    /// </summary>
    Task<UserLearningProfileModel> GetCurrentProfileAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates the UI language preference and refreshes the session cache.
    /// </summary>
    Task<UserLearningProfileModel> UpdateUiLanguagePreferenceAsync(string uiLanguageCode, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the meaning-language preferences and refreshes the session cache.
    /// </summary>
    Task<UserLearningProfileModel> UpdateMeaningLanguagePreferencesAsync(
        string preferredMeaningLanguage1,
        string? preferredMeaningLanguage2,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears the session cache so the next read reloads the profile from persistence.
    /// </summary>
    void ResetCache();
}
