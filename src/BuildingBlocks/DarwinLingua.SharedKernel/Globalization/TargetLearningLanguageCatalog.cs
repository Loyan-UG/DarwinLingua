namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Defines target learning languages that can own authored learning content.
/// </summary>
public static class TargetLearningLanguageCatalog
{
    public static readonly TargetLearningLanguageDefinition German = new(
        Code: ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
        NativeName: "Deutsch",
        EnglishName: "German",
        IsActive: true,
        SortOrder: 10,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["DE"]);

    public static readonly TargetLearningLanguageDefinition English = new(
        Code: "en",
        NativeName: "English",
        EnglishName: "English",
        IsActive: false,
        SortOrder: 20,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["US", "GB", "AU"]);

    public static readonly TargetLearningLanguageDefinition Spanish = new(
        Code: "es",
        NativeName: "Español",
        EnglishName: "Spanish",
        IsActive: false,
        SortOrder: 30,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["ES"]);

    public static readonly TargetLearningLanguageDefinition French = new(
        Code: "fr",
        NativeName: "Français",
        EnglishName: "French",
        IsActive: false,
        SortOrder: 40,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["FR"]);

    public static readonly IReadOnlyList<TargetLearningLanguageDefinition> All =
    [
        German,
        English,
        Spanish,
        French
    ];

    public static readonly IReadOnlyList<TargetLearningLanguageDefinition> Active =
        All.Where(language => language.IsActive).ToArray();

    public static bool IsActive(string languageCode) =>
        TryFindActive(languageCode, out _);

    public static bool TryFindActive(string languageCode, out TargetLearningLanguageDefinition language)
    {
        foreach (TargetLearningLanguageDefinition item in Active)
        {
            if (string.Equals(item.Code, languageCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                language = item;
                return true;
            }
        }

        language = null!;
        return false;
    }
}

/// <summary>
/// Describes a target learning language.
/// </summary>
public sealed record TargetLearningLanguageDefinition(
    string Code,
    string NativeName,
    string EnglishName,
    bool IsActive,
    int SortOrder,
    string DefaultLevelSystemCode,
    IReadOnlyList<string> DefaultCountryContextCodes);
