namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Defines the shared safety and visibility rules for Everyday Expressions.
/// </summary>
public static class ExpressionSensitivityPolicy
{
    public const string SafetyGeneral = "general";
    public const string SafetyMildRude = "mild-rude";
    public const string SafetyStrongRude = "strong-rude";
    public const string SafetySexualEducational = "sexual-educational";
    public const string SafetyRomanticSocial = "romantic-social";
    public const string SafetyDiscriminatorySlur = "discriminatory-slur";
    public const string SafetyPoliticallySensitive = "politically-sensitive";
    public const string SafetyExplicitAdult = "explicit-adult";
    public const string SafetyBlockedIllegal = "blocked-illegal";

    public const string SensitiveNone = "none";
    public const string SensitiveSwearWord = "swear-word";
    public const string SensitiveInsult = "insult";
    public const string SensitiveRudeColloquial = "rude-colloquial";
    public const string SensitiveMildEmotional = "mild-emotional";
    public const string SensitiveRomanticSocial = "romantic-social";
    public const string SensitiveSexualEducationalNeutral = "sexual-educational-neutral";
    public const string SensitiveSlurEducational = "slur-educational";
    public const string SensitiveBlocked = "blocked";

    public const string UsageSafeToUse = "safe-to-use";
    public const string UsageUseWithCare = "use-with-care";
    public const string UsageUnderstandOnly = "understand-only";
    public const string UsageDoNotUse = "do-not-use";
    public const string UsageBlocked = "blocked";

    public static readonly IReadOnlySet<string> SafetyRatings = new HashSet<string>(StringComparer.Ordinal)
    {
        SafetyGeneral,
        SafetyMildRude,
        SafetyStrongRude,
        SafetySexualEducational,
        SafetyRomanticSocial,
        SafetyDiscriminatorySlur,
        SafetyPoliticallySensitive,
        SafetyExplicitAdult,
        SafetyBlockedIllegal,
    };

    public static readonly IReadOnlySet<string> SensitiveContentKinds = new HashSet<string>(StringComparer.Ordinal)
    {
        SensitiveNone,
        SensitiveSwearWord,
        SensitiveInsult,
        SensitiveRudeColloquial,
        SensitiveMildEmotional,
        SensitiveRomanticSocial,
        SensitiveSexualEducationalNeutral,
        SensitiveSlurEducational,
        SensitiveBlocked,
    };

    public static readonly IReadOnlySet<string> UsagePolicies = new HashSet<string>(StringComparer.Ordinal)
    {
        UsageSafeToUse,
        UsageUseWithCare,
        UsageUnderstandOnly,
        UsageDoNotUse,
        UsageBlocked,
    };

    public static readonly IReadOnlySet<int> MinimumAges = new HashSet<int> { 0, 12, 16, 18 };

    /// <summary>
    /// Returns whether the entry is allowed on learner surfaces for the active profile preference.
    /// </summary>
    public static bool IsVisibleToLearner(
        string safetyRating,
        string sensitiveContentKind,
        bool requiresSensitiveOptIn,
        bool requiresAdultAccess,
        bool requiresVerifiedAdult,
        string usagePolicy,
        bool includeSensitiveEducationalLanguage)
    {
        if (IsHardBlocked(safetyRating, sensitiveContentKind, requiresAdultAccess, requiresVerifiedAdult, usagePolicy))
        {
            return false;
        }

        if (IsSensitiveForLearners(safetyRating, sensitiveContentKind, requiresSensitiveOptIn, usagePolicy))
        {
            return includeSensitiveEducationalLanguage;
        }

        return true;
    }

    /// <summary>
    /// Returns whether the entry must be excluded from mobile packages until mobile opt-in enforcement exists.
    /// </summary>
    public static bool IsExcludedFromDefaultMobileExport(
        string safetyRating,
        string sensitiveContentKind,
        bool requiresSensitiveOptIn,
        bool requiresAdultAccess,
        bool requiresVerifiedAdult,
        string usagePolicy) =>
        IsHardBlocked(safetyRating, sensitiveContentKind, requiresAdultAccess, requiresVerifiedAdult, usagePolicy) ||
        IsSensitiveForLearners(safetyRating, sensitiveContentKind, requiresSensitiveOptIn, usagePolicy);

    /// <summary>
    /// Returns whether warnings are required for the entry.
    /// </summary>
    public static bool RequiresWarning(
        bool isRisky,
        string register,
        string expressionType,
        string safetyRating,
        string sensitiveContentKind,
        string usagePolicy) =>
        isRisky ||
        register is "slang" or "rude" or "friends-only" ||
        expressionType is "slang" or "warning-phrase" ||
        safetyRating != SafetyGeneral ||
        sensitiveContentKind != SensitiveNone ||
        usagePolicy is UsageUseWithCare or UsageUnderstandOnly or UsageDoNotUse or UsageBlocked;

    /// <summary>
    /// Returns whether the safety metadata means the entry needs the learner opt-in preference.
    /// </summary>
    public static bool RequiresSensitiveOptIn(string safetyRating, string sensitiveContentKind, string usagePolicy) =>
        safetyRating is SafetyMildRude or SafetyStrongRude or SafetySexualEducational or SafetyRomanticSocial or SafetyPoliticallySensitive ||
        sensitiveContentKind is not SensitiveNone and not SensitiveBlocked ||
        usagePolicy is UsageUseWithCare or UsageUnderstandOnly or UsageDoNotUse;

    private static bool IsSensitiveForLearners(
        string safetyRating,
        string sensitiveContentKind,
        bool requiresSensitiveOptIn,
        string usagePolicy) =>
        requiresSensitiveOptIn || RequiresSensitiveOptIn(safetyRating, sensitiveContentKind, usagePolicy);

    private static bool IsHardBlocked(
        string safetyRating,
        string sensitiveContentKind,
        bool requiresAdultAccess,
        bool requiresVerifiedAdult,
        string usagePolicy) =>
        requiresAdultAccess ||
        requiresVerifiedAdult ||
        safetyRating is SafetyExplicitAdult or SafetyBlockedIllegal or SafetyDiscriminatorySlur ||
        sensitiveContentKind is SensitiveBlocked or SensitiveSlurEducational ||
        usagePolicy is UsageBlocked;
}
