using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

/// <summary>
/// Represents one reusable practical or exam-oriented German writing template.
/// </summary>
public sealed class WritingTemplate
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    private WritingTemplate()
    {
        Slug = string.Empty;
        Title = string.Empty;
        ShortDescription = string.Empty;
        Category = string.Empty;
        Situation = string.Empty;
        Register = string.Empty;
        TemplateText = string.Empty;
        Explanation = string.Empty;
        VariablesJson = "[]";
        SampleFilledVersion = string.Empty;
        TitleTranslationsJson = "[]";
        ShortDescriptionTranslationsJson = "[]";
        SituationTranslationsJson = "[]";
        ExplanationTranslationsJson = "[]";
        TemplateTextTranslationsJson = "[]";
        SampleFilledVersionTranslationsJson = "[]";
        LinkedGrammarTopicSlugsJson = "[]";
        LinkedWordSlugsJson = "[]";
        LinkedExpressionSlugsJson = "[]";
        LinkedExerciseSlugsJson = "[]";
        LinkedCourseLessonSlugsJson = "[]";
        TargetLearningLanguageCode = ContentLanguageRequirements.DefaultTargetLearningLanguageCode;
    }

    public WritingTemplate(
        Guid id,
        string slug,
        string title,
        string shortDescription,
        CefrLevel cefrLevel,
        string category,
        string situation,
        string register,
        string templateText,
        string explanation,
        string variablesJson,
        string sampleFilledVersion,
        string linkedGrammarTopicSlugsJson,
        string linkedWordSlugsJson,
        string linkedExpressionSlugsJson,
        string linkedExerciseSlugsJson,
        string linkedCourseLessonSlugsJson,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime timestampUtc,
        string titleTranslationsJson = "[]",
        string shortDescriptionTranslationsJson = "[]",
        string situationTranslationsJson = "[]",
        string explanationTranslationsJson = "[]",
        string templateTextTranslationsJson = "[]",
        string sampleFilledVersionTranslationsJson = "[]",
        string? targetLearningLanguageCode = null)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Writing template id is required.") : id;
        TargetLearningLanguageCode = TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode, "Writing template target learning language");
        Slug = NormalizeKebabKey(slug, "Writing template slug");
        Title = RequireText(title, "Writing template title", 256);
        ShortDescription = RequireText(shortDescription, "Writing template short description", 1000);
        CefrLevel = cefrLevel;
        Category = NormalizeKebabKey(category, "Writing template category");
        Situation = RequireText(situation, "Writing template situation", 512);
        Register = NormalizeKebabKey(register, "Writing template register");
        TemplateText = RequireText(templateText, "Writing template text", 12000);
        Explanation = RequireText(explanation, "Writing template explanation", 4000);
        VariablesJson = RequireText(variablesJson, "Writing template variables JSON", 12000);
        SampleFilledVersion = RequireText(sampleFilledVersion, "Writing template sample filled version", 12000);
        TitleTranslationsJson = RequireText(titleTranslationsJson, "Writing template title translations JSON", 12000);
        ShortDescriptionTranslationsJson = RequireText(shortDescriptionTranslationsJson, "Writing template short description translations JSON", 12000);
        SituationTranslationsJson = RequireText(situationTranslationsJson, "Writing template situation translations JSON", 12000);
        ExplanationTranslationsJson = RequireText(explanationTranslationsJson, "Writing template explanation translations JSON", 12000);
        TemplateTextTranslationsJson = RequireText(templateTextTranslationsJson, "Writing template text translations JSON", 24000);
        SampleFilledVersionTranslationsJson = RequireText(sampleFilledVersionTranslationsJson, "Writing template sample filled version translations JSON", 24000);
        LinkedGrammarTopicSlugsJson = RequireText(linkedGrammarTopicSlugsJson, "Linked grammar topics JSON", 12000);
        LinkedWordSlugsJson = RequireText(linkedWordSlugsJson, "Linked words JSON", 12000);
        LinkedExpressionSlugsJson = RequireText(linkedExpressionSlugsJson, "Linked expressions JSON", 12000);
        LinkedExerciseSlugsJson = RequireText(linkedExerciseSlugsJson, "Linked exercises JSON", 12000);
        LinkedCourseLessonSlugsJson = RequireText(linkedCourseLessonSlugsJson, "Linked course lessons JSON", 12000);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = timestampUtc;
        UpdatedAtUtc = timestampUtc;
    }

    public Guid Id { get; private set; }
    public string TargetLearningLanguageCode { get; private set; }
    public string Slug { get; private set; }
    public string Title { get; private set; }
    public string ShortDescription { get; private set; }
    public CefrLevel CefrLevel { get; private set; }
    public string Category { get; private set; }
    public string Situation { get; private set; }
    public string Register { get; private set; }
    public string TemplateText { get; private set; }
    public string Explanation { get; private set; }
    public string VariablesJson { get; private set; }
    public string SampleFilledVersion { get; private set; }
    public string TitleTranslationsJson { get; private set; }
    public string ShortDescriptionTranslationsJson { get; private set; }
    public string SituationTranslationsJson { get; private set; }
    public string ExplanationTranslationsJson { get; private set; }
    public string TemplateTextTranslationsJson { get; private set; }
    public string SampleFilledVersionTranslationsJson { get; private set; }
    public string LinkedGrammarTopicSlugsJson { get; private set; }
    public string LinkedWordSlugsJson { get; private set; }
    public string LinkedExpressionSlugsJson { get; private set; }
    public string LinkedExerciseSlugsJson { get; private set; }
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
}
