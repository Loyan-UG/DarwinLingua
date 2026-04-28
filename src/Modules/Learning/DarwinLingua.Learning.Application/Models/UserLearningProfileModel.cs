namespace DarwinLingua.Learning.Application.Models;

/// <summary>
/// Represents the local user learning profile returned to presentation code.
/// </summary>
public sealed record UserLearningProfileModel(
    string UserId,
    string PreferredMeaningLanguage1,
    string? PreferredMeaningLanguage2,
    string UiLanguageCode)
{
    /// <summary>
    /// Gets the active meaning-language codes in display order.
    /// </summary>
    public IReadOnlyList<string> ActiveMeaningLanguages =>
        string.IsNullOrWhiteSpace(PreferredMeaningLanguage2)
            ? [PreferredMeaningLanguage1]
            : [PreferredMeaningLanguage1, PreferredMeaningLanguage2];
}
