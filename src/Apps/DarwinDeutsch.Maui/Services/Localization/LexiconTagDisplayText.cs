using DarwinDeutsch.Maui.Resources.Strings;

namespace DarwinDeutsch.Maui.Services.Localization;

/// <summary>
/// Builds localized display text for lexical usage and context labels.
/// </summary>
internal static class LexiconTagDisplayText
{
    /// <summary>
    /// Returns a localized display label for a normalized lexical tag key.
    /// </summary>
    public static string GetDisplayName(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return key switch
        {
            "formal" => AppStrings.WordLabelFormal,
            "informal" => AppStrings.WordLabelInformal,
            "spoken" => AppStrings.WordLabelSpoken,
            "written" => AppStrings.WordLabelWritten,
            "daily-life" => AppStrings.WordLabelDailyLife,
            "shopping" => AppStrings.WordLabelShopping,
            "work" => AppStrings.WordLabelWork,
            "paperwork" => AppStrings.WordLabelPaperwork,
            "doctor" => AppStrings.WordLabelDoctor,
            _ => Humanize(key),
        };
    }

    private static string Humanize(string key)
    {
        string[] parts = key.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return key;
        }

        return string.Join(
            " ",
            parts.Select(part => char.ToUpperInvariant(part[0]) + part[1..]));
    }
}
