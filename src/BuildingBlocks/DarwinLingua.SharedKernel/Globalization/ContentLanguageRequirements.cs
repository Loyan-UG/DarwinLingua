namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Defines the language coverage required for production-ready authored content.
/// </summary>
public static class ContentLanguageRequirements
{
    public const string LearningLanguageCode = "de";

    public static readonly IReadOnlyList<string> RequiredMeaningLanguageCodes =
    [
        "ar",
        "ckb",
        "en",
        "fa",
        "kmr",
        "pl",
        "ro",
        "ru",
        "sq",
        "tr"
    ];

    public static readonly IReadOnlyList<string> RequiredLocalizationLanguageCodes =
    [
        LearningLanguageCode,
        "ar",
        "ckb",
        "en",
        "fa",
        "kmr",
        "pl",
        "ro",
        "ru",
        "sq",
        "tr"
    ];

    public static string FormatRequiredMeaningLanguages() =>
        string.Join(", ", RequiredMeaningLanguageCodes);

    public static string FormatRequiredLocalizationLanguages() =>
        string.Join(", ", RequiredLocalizationLanguageCodes);

    public static IReadOnlyList<string> FindMissingMeaningLanguages(IEnumerable<string> languageCodes) =>
        FindMissingLanguages(languageCodes, RequiredMeaningLanguageCodes);

    public static IReadOnlyList<string> FindMissingLocalizationLanguages(IEnumerable<string> languageCodes) =>
        FindMissingLanguages(languageCodes, RequiredLocalizationLanguageCodes);

    private static IReadOnlyList<string> FindMissingLanguages(
        IEnumerable<string> languageCodes,
        IReadOnlyList<string> requiredLanguageCodes)
    {
        ArgumentNullException.ThrowIfNull(languageCodes);

        HashSet<string> provided = new(
            languageCodes
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim().ToLowerInvariant()),
            StringComparer.OrdinalIgnoreCase);

        return requiredLanguageCodes
            .Where(required => !provided.Contains(required))
            .ToArray();
    }
}
