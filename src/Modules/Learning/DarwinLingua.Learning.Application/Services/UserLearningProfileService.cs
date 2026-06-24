using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Learning.Application.Services;

/// <summary>
/// Implements the Phase 1 local user learning profile workflows.
/// </summary>
internal sealed class UserLearningProfileService : IUserLearningProfileService
{
    private static readonly LanguageCode EnglishLanguageCode = LanguageCode.From("en");

    private readonly IUserLearningProfileRepository _userLearningProfileRepository;
    private readonly ILearningPreferenceLanguageValidator _languageValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLearningProfileService"/> class.
    /// </summary>
    public UserLearningProfileService(
        IUserLearningProfileRepository userLearningProfileRepository,
        ILearningPreferenceLanguageValidator languageValidator)
    {
        ArgumentNullException.ThrowIfNull(userLearningProfileRepository);
        ArgumentNullException.ThrowIfNull(languageValidator);

        _userLearningProfileRepository = userLearningProfileRepository;
        _languageValidator = languageValidator;
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> EnsureLocalProfileExistsAsync(
        string requestedUiLanguageCode,
        CancellationToken cancellationToken)
    {
        UserLearningProfile? existingProfile = await _userLearningProfileRepository
            .GetByUserIdAsync(LocalInstallationUser.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (existingProfile is not null)
        {
            return Map(existingProfile);
        }

        LanguageCode resolvedUiLanguage = await ResolveSupportedUiLanguageAsync(
            LanguageCode.From(requestedUiLanguageCode),
            cancellationToken)
            .ConfigureAwait(false);
        LanguageCode defaultMeaningLanguage = await ResolveDefaultMeaningLanguageAsync(cancellationToken)
            .ConfigureAwait(false);

        UserLearningProfile profile = new(
            Guid.NewGuid(),
            LocalInstallationUser.UserId,
            defaultMeaningLanguage,
            preferredMeaningLanguage2: null,
            resolvedUiLanguage,
            DateTime.UtcNow);

        await _userLearningProfileRepository.AddAsync(profile, cancellationToken).ConfigureAwait(false);

        return Map(profile);
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> GetCurrentProfileAsync(CancellationToken cancellationToken)
    {
        UserLearningProfile? profile = await _userLearningProfileRepository
            .GetByUserIdAsync(LocalInstallationUser.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return await EnsureLocalProfileExistsAsync(EnglishLanguageCode.Value, cancellationToken)
                .ConfigureAwait(false);
        }

        return Map(profile);
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> UpdateUiLanguagePreferenceAsync(
        string uiLanguageCode,
        CancellationToken cancellationToken)
    {
        UserLearningProfile profile = await GetRequiredProfileAsync(cancellationToken).ConfigureAwait(false);
        LanguageCode requestedLanguageCode = LanguageCode.From(uiLanguageCode);

        if (!await _languageValidator.SupportsUserInterfaceAsync(requestedLanguageCode, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new DomainRuleException($"UI language '{requestedLanguageCode.Value}' is not supported.");
        }

        profile.UpdateUiLanguage(requestedLanguageCode, DateTime.UtcNow);
        await _userLearningProfileRepository.UpdateAsync(profile, cancellationToken).ConfigureAwait(false);

        return Map(profile);
    }

    /// <inheritdoc />
    public async Task<UserLearningProfileModel> UpdateMeaningLanguagePreferencesAsync(
        string preferredMeaningLanguage1,
        string? preferredMeaningLanguage2,
        CancellationToken cancellationToken)
    {
        UserLearningProfile profile = await GetRequiredProfileAsync(cancellationToken).ConfigureAwait(false);
        LanguageCode primaryMeaningLanguage = LanguageCode.From(preferredMeaningLanguage1);
        LanguageCode? secondaryMeaningLanguage = string.IsNullOrWhiteSpace(preferredMeaningLanguage2)
            ? null
            : LanguageCode.From(preferredMeaningLanguage2);

        if (!await _languageValidator.SupportsMeaningsAsync(primaryMeaningLanguage, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new DomainRuleException($"Meaning language '{primaryMeaningLanguage.Value}' is not supported.");
        }

        if (secondaryMeaningLanguage is not null &&
            !await _languageValidator.SupportsMeaningsAsync(secondaryMeaningLanguage.Value, cancellationToken)
                .ConfigureAwait(false))
        {
            throw new DomainRuleException($"Meaning language '{secondaryMeaningLanguage.Value.Value}' is not supported.");
        }

        profile.UpdateMeaningLanguagePreferences(
            primaryMeaningLanguage,
            secondaryMeaningLanguage,
            DateTime.UtcNow);

        await _userLearningProfileRepository.UpdateAsync(profile, cancellationToken).ConfigureAwait(false);

        return Map(profile);
    }

    /// <summary>
    /// Loads the current local profile and auto-creates the default record when it is missing.
    /// </summary>
    private async Task<UserLearningProfile> GetRequiredProfileAsync(CancellationToken cancellationToken)
    {
        UserLearningProfile? profile = await _userLearningProfileRepository
            .GetByUserIdAsync(LocalInstallationUser.UserId, cancellationToken)
            .ConfigureAwait(false);

        if (profile is not null)
        {
            return profile;
        }

        await EnsureLocalProfileExistsAsync(EnglishLanguageCode.Value, cancellationToken).ConfigureAwait(false);

        return (await _userLearningProfileRepository
            .GetByUserIdAsync(LocalInstallationUser.UserId, cancellationToken)
            .ConfigureAwait(false))!;
    }

    /// <summary>
    /// Resolves the effective supported UI language code, falling back to English when needed.
    /// </summary>
    private async Task<LanguageCode> ResolveSupportedUiLanguageAsync(
        LanguageCode requestedLanguageCode,
        CancellationToken cancellationToken)
    {
        if (await _languageValidator.SupportsUserInterfaceAsync(requestedLanguageCode, cancellationToken)
                .ConfigureAwait(false))
        {
            return requestedLanguageCode;
        }

        if (await _languageValidator.SupportsUserInterfaceAsync(EnglishLanguageCode, cancellationToken)
                .ConfigureAwait(false))
        {
            return EnglishLanguageCode;
        }

        throw new DomainRuleException("At least one supported UI language must exist for the local profile.");
    }

    /// <summary>
    /// Resolves the default primary meaning language for newly created local profiles.
    /// </summary>
    private async Task<LanguageCode> ResolveDefaultMeaningLanguageAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<LanguageCode> supportedMeaningLanguages = await _languageValidator
            .GetSupportedMeaningLanguagesAsync(cancellationToken)
            .ConfigureAwait(false);

        if (supportedMeaningLanguages.Count == 0)
        {
            throw new DomainRuleException("At least one supported meaning language must exist for the local profile.");
        }

        LanguageCode? preferredDefault = supportedMeaningLanguages
            .FirstOrDefault(languageCode => languageCode == EnglishLanguageCode);

        return preferredDefault == default ? supportedMeaningLanguages[0] : preferredDefault.Value;
    }

    /// <summary>
    /// Maps the domain aggregate to a presentation-safe profile model.
    /// </summary>
    private static UserLearningProfileModel Map(UserLearningProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new UserLearningProfileModel(
            profile.UserId,
            profile.PreferredMeaningLanguage1.Value,
            profile.PreferredMeaningLanguage2?.Value,
            profile.UiLanguageCode.Value,
            TargetLearningLanguageCode: profile.TargetLearningLanguageCode.Value);
    }
}
