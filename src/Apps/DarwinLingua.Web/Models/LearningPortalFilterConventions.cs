using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Web.Models;

/// <summary>
/// Provides shared learner-facing filter conventions for current and future learning portal surfaces.
/// </summary>
public static class LearningPortalFilterConventions
{
    /// <summary>
    /// Gets the learner-facing CEFR level definitions for the current German baseline.
    /// </summary>
    public static IReadOnlyList<LearningLevelDefinition> CefrLevelDefinitions =>
        LearningLevelSystemCatalog.GermanCefrLevels;

    /// <summary>
    /// Gets learner-facing CEFR level definitions for the selected target learning language.
    /// </summary>
    /// <param name="targetLearningLanguageCode">The target learning language code.</param>
    /// <returns>The target-specific CEFR level metadata.</returns>
    public static IReadOnlyList<LearningLevelDefinition> GetCefrLevelDefinitions(string? targetLearningLanguageCode) =>
        LearningLevelSystemCatalog.GetCefrLevelsForTargetLanguage(targetLearningLanguageCode);

    /// <summary>
    /// Gets the stable CEFR level order used by Web filters.
    /// </summary>
    public static IReadOnlyList<string> CefrLevels =>
        CefrLevelDefinitions.Select(static level => level.Code).ToArray();

    /// <summary>
    /// Formats a CEFR level with its target-language-specific learner label for filter options and chips.
    /// </summary>
    /// <param name="levelCode">The CEFR code.</param>
    /// <param name="targetLearningLanguageCode">The selected target learning language code.</param>
    /// <param name="localize">Optional UI localizer for display title resource keys.</param>
    /// <returns>A learner-facing CEFR label.</returns>
    public static string FormatCefrLevelOption(
        string? levelCode,
        string? targetLearningLanguageCode = null,
        Func<string, string>? localize = null)
    {
        if (string.IsNullOrWhiteSpace(levelCode))
        {
            return string.Empty;
        }

        LearningLevelDefinition? definition = GetCefrLevelDefinitions(targetLearningLanguageCode)
            .FirstOrDefault(level => string.Equals(level.Code, levelCode.Trim(), StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return levelCode.Trim().ToUpperInvariant();
        }

        string displayTitle = localize is null
            ? definition.DisplayTitle
            : localize(definition.DisplayTitle);

        return $"{definition.Code} · {displayTitle}";
    }

    /// <summary>
    /// Normalizes a CEFR level for filter values and rejects unsupported levels.
    /// </summary>
    /// <param name="value">The raw CEFR level value from a route, query string, or form post.</param>
    /// <returns>The normalized CEFR level, or <c>null</c> when the value is empty or unsupported.</returns>
    public static string? NormalizeCefrLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim().ToUpperInvariant();
        return CefrLevels.Contains(normalized, StringComparer.Ordinal) ? normalized : null;
    }
}
