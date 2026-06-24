using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one reusable note about German communication behavior or cultural context.
/// </summary>
public sealed class CountryGuidanceNote
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private CountryGuidanceNote()
    {
        Slug = string.Empty;
        Title = string.Empty;
        ShortDescription = string.Empty;
        Category = string.Empty;
        Context = string.Empty;
        SectionsJson = "[]";
        ExamplesJson = "[]";
        DoNotesJson = "[]";
        DontNotesJson = "[]";
        SensitivityWarning = string.Empty;
        TitleTranslationsJson = "[]";
        ShortDescriptionTranslationsJson = "[]";
        ContextTranslationsJson = "[]";
        SectionsTranslationsJson = "[]";
        ExamplesTranslationsJson = "[]";
        DoNotesTranslationsJson = "[]";
        DontNotesTranslationsJson = "[]";
        SensitivityWarningTranslationsJson = "[]";
        LinkedDialogueSlugsJson = "[]";
        LinkedExpressionSlugsJson = "[]";
        LinkedWritingTemplateSlugsJson = "[]";
        LinkedTalkTopicSlugsJson = "[]";
        LinkedCourseLessonSlugsJson = "[]";
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
        CountryContextCode = CountryContextCatalog.Germany.Code;
    }

    public CountryGuidanceNote(
        Guid id,
        string slug,
        string title,
        string shortDescription,
        CefrLevel cefrLevel,
        string category,
        string context,
        string sectionsJson,
        string examplesJson,
        string doNotesJson,
        string dontNotesJson,
        string? sensitivityWarning,
        string linkedDialogueSlugsJson,
        string linkedExpressionSlugsJson,
        string linkedWritingTemplateSlugsJson,
        string linkedTalkTopicSlugsJson,
        string linkedCourseLessonSlugsJson,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string titleTranslationsJson = "[]",
        string shortDescriptionTranslationsJson = "[]",
        string contextTranslationsJson = "[]",
        string sectionsTranslationsJson = "[]",
        string examplesTranslationsJson = "[]",
        string doNotesTranslationsJson = "[]",
        string dontNotesTranslationsJson = "[]",
        string sensitivityWarningTranslationsJson = "[]",
        string? targetLearningLanguageCode = null,
        string? countryContextCode = null)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Country guidance note id is required.") : id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Country guidance note target learning language");
        CountryContextCode = NormalizeCountryContextCode(countryContextCode, TargetLearningLanguageCode);
        Slug = NormalizeKebabKey(slug, "Country guidance note slug");
        Title = RequireText(title, "Country guidance note title", 256);
        ShortDescription = RequireText(shortDescription, "Country guidance note short description", 1000);
        CefrLevel = cefrLevel;
        Category = NormalizeKebabKey(category, "Country guidance note category");
        Context = RequireText(context, "Country guidance note context", 512);
        SectionsJson = RequireText(sectionsJson, "Country guidance note sections JSON", 20000);
        ExamplesJson = RequireText(examplesJson, "Country guidance note examples JSON", 20000);
        DoNotesJson = RequireText(doNotesJson, "Country guidance note do notes JSON", 12000);
        DontNotesJson = RequireText(dontNotesJson, "Country guidance note don't notes JSON", 12000);
        SensitivityWarning = NormalizeOptionalText(sensitivityWarning, 1000);
        TitleTranslationsJson = RequireText(titleTranslationsJson, "Country guidance note title translations JSON", 12000);
        ShortDescriptionTranslationsJson = RequireText(shortDescriptionTranslationsJson, "Country guidance note short description translations JSON", 12000);
        ContextTranslationsJson = RequireText(contextTranslationsJson, "Country guidance note context translations JSON", 12000);
        SectionsTranslationsJson = RequireText(sectionsTranslationsJson, "Country guidance note sections translations JSON", 24000);
        ExamplesTranslationsJson = RequireText(examplesTranslationsJson, "Country guidance note examples translations JSON", 24000);
        DoNotesTranslationsJson = RequireText(doNotesTranslationsJson, "Country guidance note do notes translations JSON", 16000);
        DontNotesTranslationsJson = RequireText(dontNotesTranslationsJson, "Country guidance note don't notes translations JSON", 16000);
        SensitivityWarningTranslationsJson = RequireText(sensitivityWarningTranslationsJson, "Country guidance note sensitivity warning translations JSON", 12000);
        LinkedDialogueSlugsJson = RequireText(linkedDialogueSlugsJson, "Linked dialogues JSON", 12000);
        LinkedExpressionSlugsJson = RequireText(linkedExpressionSlugsJson, "Linked expressions JSON", 12000);
        LinkedWritingTemplateSlugsJson = RequireText(linkedWritingTemplateSlugsJson, "Linked writing templates JSON", 12000);
        LinkedTalkTopicSlugsJson = RequireText(linkedTalkTopicSlugsJson, "Linked Talk Topics JSON", 12000);
        LinkedCourseLessonSlugsJson = RequireText(linkedCourseLessonSlugsJson, "Linked course lessons JSON", 12000);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string TargetLearningLanguageCode { get; private set; }
    public string CountryContextCode { get; private set; }
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string ShortDescription { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public string Category { get; private set; }
    public string Context { get; private set; }
    public string SectionsJson { get; private set; }
    public string ExamplesJson { get; private set; }
    public string DoNotesJson { get; private set; }
    public string DontNotesJson { get; private set; }
    public string? SensitivityWarning { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string ShortDescriptionTranslationsJson { get; private set; }
    public string ContextTranslationsJson { get; private set; }
    public string SectionsTranslationsJson { get; private set; }
    public string ExamplesTranslationsJson { get; private set; }
    public string DoNotesTranslationsJson { get; private set; }
    public string DontNotesTranslationsJson { get; private set; }
    public string SensitivityWarningTranslationsJson { get; private set; }
    public string LinkedDialogueSlugsJson { get; private set; }
    public string LinkedExpressionSlugsJson { get; private set; }
    public string LinkedWritingTemplateSlugsJson { get; private set; }
    public string LinkedTalkTopicSlugsJson { get; private set; }
    public string LinkedCourseLessonSlugsJson { get; private set; }
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    internal static string RequireText(string value, string fieldName, int maxLength)
    {
        string normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainRuleException($"{fieldName} is required.");
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }

    internal static string NormalizeKebabKey(string value, string fieldName)
    {
        string normalized = value.Trim().ToLowerInvariant();
        if (!KebabCaseRegex.IsMatch(normalized))
        {
            throw new DomainRuleException($"{fieldName} must use lowercase kebab-case.");
        }

        return normalized;
    }

    internal static string NormalizeCountryContextCode(string? value, string targetLearningLanguageCode)
    {
        string normalized = string.IsNullOrWhiteSpace(value)
            ? CountryContextCatalog.ResolveDefaultActiveCode(targetLearningLanguageCode)
            : value.Trim().ToUpperInvariant();

        if (!CountryContextCatalog.TryFindActive(normalized, targetLearningLanguageCode, out _))
        {
            throw new DomainRuleException("Country guidance note country context is not active for the target learning language.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"Optional Country guidance note text must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
