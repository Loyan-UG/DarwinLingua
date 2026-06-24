using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Learning.Domain.Entities;

/// <summary>
/// Represents the local user learning profile used to store lightweight preferences and settings.
/// </summary>
public sealed class UserLearningProfile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserLearningProfile"/> class for EF Core materialization.
    /// </summary>
    private UserLearningProfile()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserLearningProfile"/> class.
    /// </summary>
    public UserLearningProfile(
        Guid id,
        string userId,
        LanguageCode preferredMeaningLanguage1,
        LanguageCode? preferredMeaningLanguage2,
        LanguageCode uiLanguageCode,
        DateTime createdAtUtc,
        LanguageCode? targetLearningLanguageCode = null)
    {
        if (id == Guid.Empty)
        {
            throw new DomainRuleException("User learning profile identifier cannot be empty.");
        }

        Id = id;
        UserId = NormalizeRequiredText(userId, nameof(userId));
        PreferredMeaningLanguage1 = preferredMeaningLanguage1;
        PreferredMeaningLanguage2 = NormalizeSecondaryMeaningLanguage(preferredMeaningLanguage1, preferredMeaningLanguage2);
        UiLanguageCode = uiLanguageCode;
        TargetLearningLanguageCode = targetLearningLanguageCode ?? LanguageCode.From(ContentLanguageRequirements.DefaultTargetLearningLanguageCode);
        CreatedAtUtc = NormalizeUtc(createdAtUtc, nameof(createdAtUtc));
        UpdatedAtUtc = CreatedAtUtc;
    }

    /// <summary>
    /// Gets the stable internal identifier of the learning profile.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the stable local user identifier.
    /// </summary>
    public string UserId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the required primary meaning language code.
    /// </summary>
    public LanguageCode PreferredMeaningLanguage1 { get; private set; }

    /// <summary>
    /// Gets the optional secondary meaning language code.
    /// </summary>
    public LanguageCode? PreferredMeaningLanguage2 { get; private set; }

    /// <summary>
    /// Gets the selected UI language code.
    /// </summary>
    public LanguageCode UiLanguageCode { get; private set; }

    /// <summary>
    /// Gets the language this learner is currently learning.
    /// </summary>
    public LanguageCode TargetLearningLanguageCode { get; private set; } = LanguageCode.From(ContentLanguageRequirements.DefaultTargetLearningLanguageCode);

    /// <summary>
    /// Gets the UTC creation timestamp.
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the UTC last update timestamp.
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Gets the active meaning languages in display order.
    /// </summary>
    public IReadOnlyList<LanguageCode> GetActiveMeaningLanguages()
    {
        return PreferredMeaningLanguage2 is null
            ? [PreferredMeaningLanguage1]
            : [PreferredMeaningLanguage1, PreferredMeaningLanguage2.Value];
    }

    /// <summary>
    /// Updates the persisted meaning-language preferences.
    /// </summary>
    public void UpdateMeaningLanguagePreferences(
        LanguageCode preferredMeaningLanguage1,
        LanguageCode? preferredMeaningLanguage2,
        DateTime updatedAtUtc)
    {
        PreferredMeaningLanguage1 = preferredMeaningLanguage1;
        PreferredMeaningLanguage2 = NormalizeSecondaryMeaningLanguage(
            preferredMeaningLanguage1,
            preferredMeaningLanguage2);
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Updates the persisted UI language preference.
    /// </summary>
    public void UpdateUiLanguage(LanguageCode uiLanguageCode, DateTime updatedAtUtc)
    {
        UiLanguageCode = uiLanguageCode;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    /// <summary>
    /// Updates the persisted target learning language preference.
    /// </summary>
    public void UpdateTargetLearningLanguage(LanguageCode targetLearningLanguageCode, DateTime updatedAtUtc)
    {
        TargetLearningLanguageCode = targetLearningLanguageCode;
        UpdatedAtUtc = NormalizeUtc(updatedAtUtc, nameof(updatedAtUtc));
    }

    private static string NormalizeRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Trim();
    }

    private static LanguageCode? NormalizeSecondaryMeaningLanguage(
        LanguageCode primaryMeaningLanguage,
        LanguageCode? secondaryMeaningLanguage)
    {
        if (secondaryMeaningLanguage is null)
        {
            return null;
        }

        if (secondaryMeaningLanguage.Value == primaryMeaningLanguage)
        {
            throw new DomainRuleException("Primary and secondary meaning languages must be different.");
        }

        return secondaryMeaningLanguage;
    }

    private static DateTime NormalizeUtc(DateTime value, string parameterName)
    {
        if (value == default)
        {
            throw new DomainRuleException($"{parameterName} cannot be empty.");
        }

        return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
