namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Defines the language coverage required for production-ready authored content.
/// </summary>
public static class ContentLanguageRequirements
{
    public const string DefaultTargetLearningLanguageCode = "de";

    public const string LearningLanguageCode = DefaultTargetLearningLanguageCode;

    public static IReadOnlyList<string> SupportedTargetLearningLanguageCodes =>
        TargetLearningLanguageCatalog.ContentImportable.Select(static language => language.Code).ToArray();

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

    public static string FormatSupportedTargetLearningLanguages() =>
        string.Join(", ", SupportedTargetLearningLanguageCodes);

    public static string FormatRequiredLocalizationLanguages() =>
        string.Join(", ", RequiredLocalizationLanguageCodes);

    public static bool SupportsTargetLearningLanguage(string languageCode) =>
        !string.IsNullOrWhiteSpace(languageCode) &&
        SupportedTargetLearningLanguageCodes.Contains(languageCode.Trim(), StringComparer.OrdinalIgnoreCase);

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
