using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;

namespace DarwinDeutsch.Maui.Services.Localization;

/// <summary>
/// Adds a light in-memory cache in front of the active learning profile workflows.
/// </summary>
internal sealed class ActiveLearningProfileCacheService : IActiveLearningProfileCacheService
{
    private readonly IUserLearningProfileService _userLearningProfileService;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private UserLearningProfileModel? _cachedProfile;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveLearningProfileCacheService"/> class.
    /// </summary>
    public ActiveLearningProfileCacheService(IUserLearningProfileService userLearningProfileService)
    {
        ArgumentNullException.ThrowIfNull(userLearningProfileService);
        _userLearningProfileService = userLearningProfileService;
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> GetCurrentProfileAsync(CancellationToken cancellationToken)
    {
        if (_cachedProfile is not null)
        {
            return _cachedProfile;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            if (_cachedProfile is not null)
            {
                return _cachedProfile;
            }

            _cachedProfile = await _userLearningProfileService
                .GetCurrentProfileAsync(cancellationToken)
                .ConfigureAwait(false);

            return _cachedProfile;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> UpdateUiLanguagePreferenceAsync(string uiLanguageCode, CancellationToken cancellationToken)
    {
        UserLearningProfileModel profile = await _userLearningProfileService
            .UpdateUiLanguagePreferenceAsync(uiLanguageCode, cancellationToken)
            .ConfigureAwait(false);

        _cachedProfile = profile;
        return profile;
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> UpdateMeaningLanguagePreferencesAsync(
        string preferredMeaningLanguage1,
        string? preferredMeaningLanguage2,
        CancellationToken cancellationToken)
    {
        UserLearningProfileModel profile = await _userLearningProfileService
            .UpdateMeaningLanguagePreferencesAsync(preferredMeaningLanguage1, preferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        _cachedProfile = profile;
        return profile;
    }

    /// <inheritdoc />
    public void ResetCache()
    {
        _cachedProfile = null;
    }
}
