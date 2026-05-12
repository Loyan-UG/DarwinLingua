using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one reusable note about German communication behavior or cultural context.
/// </summary>
public sealed class CulturalNote
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private CulturalNote()
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
        LinkedDialogueSlugsJson = "[]";
        LinkedExpressionSlugsJson = "[]";
        LinkedWritingTemplateSlugsJson = "[]";
        LinkedTalkTopicSlugsJson = "[]";
        LinkedCourseLessonSlugsJson = "[]";
    }

    public CulturalNote(
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
        DateTime timestampUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Cultural note id is required.") : id;
        Slug = NormalizeKebabKey(slug, "Cultural note slug");
        Title = RequireText(title, "Cultural note title", 256);
        ShortDescription = RequireText(shortDescription, "Cultural note short description", 1000);
        CefrLevel = cefrLevel;
        Category = NormalizeKebabKey(category, "Cultural note category");
        Context = RequireText(context, "Cultural note context", 512);
        SectionsJson = RequireText(sectionsJson, "Cultural note sections JSON", 20000);
        ExamplesJson = RequireText(examplesJson, "Cultural note examples JSON", 20000);
        DoNotesJson = RequireText(doNotesJson, "Cultural note do notes JSON", 12000);
        DontNotesJson = RequireText(dontNotesJson, "Cultural note don't notes JSON", 12000);
        SensitivityWarning = NormalizeOptionalText(sensitivityWarning, 1000);
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

    private static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainRuleException($"Optional cultural note text must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
