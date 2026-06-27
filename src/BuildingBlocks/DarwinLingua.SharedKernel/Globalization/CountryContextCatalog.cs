using DarwinLingua.SharedKernel.Exceptions;

namespace DarwinLingua.SharedKernel.Globalization;

/// <summary>
/// Defines country or region contexts used by language-specific civic and cultural guidance.
/// </summary>
public static class CountryContextCatalog
{
    public static readonly CountryContextDefinition Germany = new(
        Code: "DE",
        EnglishName: "Germany",
        NativeName: "Deutschland",
        TargetLearningLanguageCodes: ["de"],
        IsActive: true,
        SortOrder: 10);

    public static readonly CountryContextDefinition Austria = new(
        Code: "AT",
        EnglishName: "Austria",
        NativeName: "Oesterreich",
        TargetLearningLanguageCodes: ["de"],
        IsActive: false,
        SortOrder: 20);

    public static readonly CountryContextDefinition SwitzerlandGerman = new(
        Code: "CH",
        EnglishName: "Switzerland",
        NativeName: "Schweiz",
        TargetLearningLanguageCodes: ["de", "fr", "it"],
        IsActive: false,
        SortOrder: 30);

    public static readonly CountryContextDefinition UnitedStates = new(
        Code: "US",
        EnglishName: "United States",
        NativeName: "United States",
        TargetLearningLanguageCodes: ["en"],
        IsActive: false,
        SortOrder: 40);

    public static readonly CountryContextDefinition UnitedKingdom = new(
        Code: "GB",
        EnglishName: "United Kingdom",
        NativeName: "United Kingdom",
        TargetLearningLanguageCodes: ["en"],
        IsActive: false,
        SortOrder: 50);

    public static readonly CountryContextDefinition Australia = new(
        Code: "AU",
        EnglishName: "Australia",
        NativeName: "Australia",
        TargetLearningLanguageCodes: ["en"],
        IsActive: false,
        SortOrder: 60);

    public static readonly IReadOnlyList<CountryContextDefinition> All =
    [
        Germany,
        Austria,
        SwitzerlandGerman,
        UnitedStates,
        UnitedKingdom,
        Australia
    ];

    public static readonly IReadOnlyList<CountryContextDefinition> Active =
        All.Where(static context => context.IsActive).ToArray();

    public static bool TryFindActive(string countryContextCode, string targetLearningLanguageCode, out CountryContextDefinition countryContext)
    {
        foreach (CountryContextDefinition context in Active)
        {
            if (string.Equals(context.Code, countryContextCode?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                context.TargetLearningLanguageCodes.Contains(targetLearningLanguageCode, StringComparer.OrdinalIgnoreCase))
            {
                countryContext = context;
                return true;
            }
        }

        countryContext = null!;
        return false;
    }

    public static string ResolveDefaultActiveCode(string targetLearningLanguageCode)
    {
        string? defaultCode = Active
            .Where(context => context.TargetLearningLanguageCodes.Contains(targetLearningLanguageCode, StringComparer.OrdinalIgnoreCase))
            .OrderBy(static context => context.SortOrder)
            .Select(static context => context.Code)
            .FirstOrDefault();

        return defaultCode ?? throw new DomainRuleException(
            $"No active country context is configured for target learning language '{targetLearningLanguageCode}'.");
    }
}

/// <summary>
/// Describes a country or region context that can be attached to learning content.
/// </summary>
public sealed record CountryContextDefinition(
    string Code,
    string EnglishName,
    string NativeName,
    IReadOnlyList<string> TargetLearningLanguageCodes,
    bool IsActive,
    int SortOrder);
