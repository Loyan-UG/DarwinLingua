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
    /// Gets the stable CEFR level order used by Web filters.
    /// </summary>
    public static IReadOnlyList<string> CefrLevels =>
        CefrLevelDefinitions.Select(static level => level.Code).ToArray();

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
