using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Normalizes and validates target-learning-language scope values.
/// </summary>
public static class TargetLearningLanguageScope
{
    /// <summary>
    /// Normalizes a target-learning-language code and defaults missing values to German.
    /// </summary>
    /// <param name="languageCode">The supplied target-learning-language code.</param>
    /// <param name="fieldName">The field name used in validation errors.</param>
    /// <returns>A supported lower-case target-learning-language code.</returns>
    public static string NormalizeOrDefault(string? languageCode, string fieldName = "Target learning language")
    {
        string normalized = string.IsNullOrWhiteSpace(languageCode)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : languageCode.Trim().ToLowerInvariant();

        if (!TargetLearningLanguageCatalog.TryFindContentImportable(normalized, out _))
        {
            throw new DomainRuleException($"{fieldName} is not a content-importable target learning language.");
        }

        return normalized;
    }
}
