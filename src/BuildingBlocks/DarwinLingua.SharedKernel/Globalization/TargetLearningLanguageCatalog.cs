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
        Status: TargetLearningLanguageStatus.Active,
        SortOrder: 10,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["DE"]);

    public static readonly TargetLearningLanguageDefinition English = new(
        Code: "en",
        NativeName: "English",
        EnglishName: "English",
        Status: TargetLearningLanguageStatus.Pilot,
        SortOrder: 20,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["US", "GB", "AU"]);

    public static readonly TargetLearningLanguageDefinition Spanish = new(
        Code: "es",
        NativeName: "Español",
        EnglishName: "Spanish",
        Status: TargetLearningLanguageStatus.Planned,
        SortOrder: 30,
        DefaultLevelSystemCode: LearningLevelSystemCatalog.CefrCode,
        DefaultCountryContextCodes: ["ES"]);

    public static readonly TargetLearningLanguageDefinition French = new(
        Code: "fr",
        NativeName: "Français",
        EnglishName: "French",
        Status: TargetLearningLanguageStatus.Planned,
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

    public static readonly IReadOnlyList<TargetLearningLanguageDefinition> Pilot =
        All.Where(language => language.IsPilot).ToArray();

    public static readonly IReadOnlyList<TargetLearningLanguageDefinition> ContentImportable =
        All.Where(language => language.AllowsContentImport).ToArray();

    public static bool IsActive(string languageCode) =>
        TryFindActive(languageCode, out _);

    public static bool IsContentImportable(string languageCode) =>
        TryFindContentImportable(languageCode, out _);

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

    public static bool TryFindContentImportable(string languageCode, out TargetLearningLanguageDefinition language)
    {
        foreach (TargetLearningLanguageDefinition item in ContentImportable)
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
    TargetLearningLanguageStatus Status,
    int SortOrder,
    string DefaultLevelSystemCode,
    IReadOnlyList<string> DefaultCountryContextCodes)
{
    public bool IsActive => Status == TargetLearningLanguageStatus.Active;

    public bool IsPilot => Status == TargetLearningLanguageStatus.Pilot;

    public bool AllowsContentImport => Status is TargetLearningLanguageStatus.Active or TargetLearningLanguageStatus.Pilot;
}

public enum TargetLearningLanguageStatus
{
    Planned = 0,
    Pilot = 1,
    Active = 2
}
