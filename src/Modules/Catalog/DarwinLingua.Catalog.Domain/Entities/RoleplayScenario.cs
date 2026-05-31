using System.Text.RegularExpressions;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Lexicon;

namespace DarwinLingua.Catalog.Domain.Entities;

public sealed partial class RoleplayScenario
{
    private readonly List<RoleplayScenarioTopic> _topics = [];

    private RoleplayScenario()
    {
    }

    public RoleplayScenario(
        Guid id,
        string slug,
        string? linkedDialogueSlug,
        string title,
        string titleTranslationsJson,
        string description,
        string descriptionTranslationsJson,
        string learnerGoal,
        string learnerGoalTranslationsJson,
        CefrLevel cefrLevel,
        string category,
        string taskType,
        string interactionMode,
        string register,
        int estimatedPracticeMinutes,
        string examProfilesJson,
        string skillFocusJson,
        string rolesJson,
        string turnsJson,
        string answerChoicesJson,
        string staticFeedbackJson,
        string imageSlotsJson,
        PublicationStatus publicationStatus,
        int sortOrder,
        DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Roleplay scenario id is required.") : id;
        Slug = NormalizeKey(slug, "Roleplay scenario slug");
        LinkedDialogueSlug = NormalizeOptionalKey(linkedDialogueSlug, "Roleplay linked dialogue slug");
        Title = NormalizeRequiredText(title, "Roleplay title", 256);
        TitleTranslationsJson = NormalizeJsonArray(titleTranslationsJson);
        Description = NormalizeRequiredText(description, "Roleplay description", 4000);
        DescriptionTranslationsJson = NormalizeJsonArray(descriptionTranslationsJson);
        LearnerGoal = NormalizeRequiredText(learnerGoal, "Roleplay learner goal", 2000);
        LearnerGoalTranslationsJson = NormalizeJsonArray(learnerGoalTranslationsJson);
        CefrLevel = cefrLevel;
        Category = NormalizeKey(category, "Roleplay category");
        TaskType = NormalizeKey(taskType, "Roleplay task type");
        InteractionMode = NormalizeKey(interactionMode, "Roleplay interaction mode");
        Register = NormalizeKey(register, "Roleplay register");
        EstimatedPracticeMinutes = estimatedPracticeMinutes <= 0 ? 10 : estimatedPracticeMinutes;
        ExamProfilesJson = NormalizeJsonArray(examProfilesJson);
        SkillFocusJson = NormalizeJsonArray(skillFocusJson);
        RolesJson = NormalizeJsonArray(rolesJson);
        TurnsJson = NormalizeJsonArray(turnsJson);
        AnswerChoicesJson = NormalizeJsonArray(answerChoicesJson);
        StaticFeedbackJson = NormalizeJsonArray(staticFeedbackJson);
        ImageSlotsJson = NormalizeJsonArray(imageSlotsJson);
        PublicationStatus = publicationStatus;
        SortOrder = Math.Max(0, sortOrder);
        CreatedAtUtc = NormalizeUtc(createdAtUtc);
        UpdatedAtUtc = CreatedAtUtc;
    }

    public Guid Id { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string? LinkedDialogueSlug { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string TitleTranslationsJson { get; private set; } = "[]";
    public string Description { get; private set; } = string.Empty;
    public string DescriptionTranslationsJson { get; private set; } = "[]";
    public string LearnerGoal { get; private set; } = string.Empty;
    public string LearnerGoalTranslationsJson { get; private set; } = "[]";
    public CefrLevel CefrLevel { get; private set; }
    public string Category { get; private set; } = string.Empty;
    public string TaskType { get; private set; } = string.Empty;
    public string InteractionMode { get; private set; } = string.Empty;
    public string Register { get; private set; } = string.Empty;
    public int EstimatedPracticeMinutes { get; private set; }
    public string ExamProfilesJson { get; private set; } = "[]";
    public string SkillFocusJson { get; private set; } = "[]";
    public string RolesJson { get; private set; } = "[]";
    public string TurnsJson { get; private set; } = "[]";
    public string AnswerChoicesJson { get; private set; } = "[]";
    public string StaticFeedbackJson { get; private set; } = "[]";
    public string ImageSlotsJson { get; private set; } = "[]";
    public PublicationStatus PublicationStatus { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<RoleplayScenarioTopic> Topics => _topics.AsReadOnly();

    public void AddTopic(Guid id, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        if (_topics.Any(topic => topic.TopicId == topicId))
        {
            throw new DomainRuleException("Roleplay scenario cannot contain duplicate topic links.");
        }

        _topics.Add(new RoleplayScenarioTopic(id, Id, topicId, isPrimary, createdAtUtc));
        UpdatedAtUtc = NormalizeUtc(createdAtUtc);
    }

    internal static string NormalizeRequiredText(string value, string fieldName, int maxLength)
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

    internal static string NormalizeKey(string value, string fieldName)
    {
        string normalized = value.Trim().ToLowerInvariant();
        if (!KeyRegex().IsMatch(normalized))
        {
            throw new DomainRuleException($"{fieldName} must use lowercase kebab-case.");
        }

        return normalized;
    }

    internal static string? NormalizeOptionalKey(string? value, string fieldName) =>
        string.IsNullOrWhiteSpace(value) ? null : NormalizeKey(value, fieldName);

    private static string NormalizeJsonArray(string value) =>
        string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();

    private static DateTime NormalizeUtc(DateTime value) =>
        value == default ? throw new DomainRuleException("Roleplay timestamp is required.") :
        value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled)]
    private static partial Regex KeyRegex();
}

public sealed class RoleplayScenarioTopic
{
    private RoleplayScenarioTopic()
    {
    }

    internal RoleplayScenarioTopic(Guid id, Guid roleplayScenarioId, Guid topicId, bool isPrimary, DateTime createdAtUtc)
    {
        Id = id == Guid.Empty ? throw new DomainRuleException("Roleplay scenario topic id is required.") : id;
        RoleplayScenarioId = roleplayScenarioId == Guid.Empty ? throw new DomainRuleException("Roleplay scenario topic owner id is required.") : roleplayScenarioId;
        TopicId = topicId == Guid.Empty ? throw new DomainRuleException("Roleplay scenario topic reference id is required.") : topicId;
        IsPrimary = isPrimary;
        CreatedAtUtc = createdAtUtc.Kind == DateTimeKind.Utc ? createdAtUtc : createdAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid RoleplayScenarioId { get; private set; }
    public Guid TopicId { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
}
