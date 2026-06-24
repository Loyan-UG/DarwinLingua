using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents the parsed structure of a content-package JSON file.
/// </summary>
public sealed record ParsedContentPackageModel(
    string PackageVersion,
    string PackageId,
    string PackageName,
    string? Source,
    string TargetLearningLanguageCode,
    string? LevelSystemCode,
    string? CountryContextCode,
    IReadOnlyList<string> DefaultMeaningLanguages,
    IReadOnlyList<ParsedContentEntryModel> Entries,
    IReadOnlyList<ParsedContentLabelDefinitionModel> Labels,
    IReadOnlyList<ParsedContentCollectionModel> Collections)
{
    public ParsedContentPackageModel(
        string packageVersion,
        string packageId,
        string packageName,
        string? source,
        IReadOnlyList<string> defaultMeaningLanguages,
        IReadOnlyList<ParsedContentEntryModel> entries,
        IReadOnlyList<ParsedContentCollectionModel> collections)
        : this(
            packageVersion,
            packageId,
            packageName,
            source,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            LearningLevelSystemCatalog.CefrCode,
            null,
            defaultMeaningLanguages,
            entries,
            CreateCompatibilityLabels(),
            collections)
    {
    }

    public ParsedContentPackageModel(
        string packageVersion,
        string packageId,
        string packageName,
        string? source,
        IReadOnlyList<string> defaultMeaningLanguages,
        IReadOnlyList<ParsedContentEntryModel> entries,
        IReadOnlyList<ParsedContentLabelDefinitionModel> labels,
        IReadOnlyList<ParsedContentCollectionModel> collections)
        : this(
            packageVersion,
            packageId,
            packageName,
            source,
            ContentLanguageRequirements.DefaultTargetLearningLanguageCode,
            LearningLevelSystemCatalog.CefrCode,
            null,
            defaultMeaningLanguages,
            entries,
            labels,
            collections)
    {
    }

    private static IReadOnlyList<ParsedContentLabelDefinitionModel> CreateCompatibilityLabels()
    {
        ParsedLocalizedTextModel[] localizations = ContentLanguageRequirements.RequiredLocalizationLanguageCodes
            .Select(languageCode => new ParsedLocalizedTextModel(languageCode, "General", null))
            .ToArray();

        return [new ParsedContentLabelDefinitionModel("Usage", "general", "General", localizations, 10)];
    }

    /// <summary>
    /// Gets parsed dialogue lessons included in the package.
    /// </summary>
    public IReadOnlyList<ParsedDialogueLessonModel> Dialogues { get; init; } = [];

    /// <summary>
    /// Gets parsed talk topics included in the package.
    /// </summary>
    public IReadOnlyList<ParsedTalkTopicModel> TalkTopics { get; init; } = [];

    /// <summary>
    /// Gets parsed grammar topics included in the package.
    /// </summary>
    public IReadOnlyList<ParsedGrammarTopicModel> GrammarTopics { get; init; } = [];

    /// <summary>
    /// Gets parsed expression entries included in the package.
    /// </summary>
    public IReadOnlyList<ParsedExpressionEntryModel> ExpressionEntries { get; init; } = [];

    public IReadOnlyList<ParsedExerciseModel> Exercises { get; init; } = [];

    public IReadOnlyList<ParsedExerciseSetModel> ExerciseSets { get; init; } = [];

    public IReadOnlyList<ParsedCoursePathModel> CoursePaths { get; init; } = [];

    public IReadOnlyList<ParsedCourseModuleModel> CourseModules { get; init; } = [];

    public IReadOnlyList<ParsedCourseLessonModel> CourseLessons { get; init; } = [];

    public IReadOnlyList<ParsedWritingTemplateModel> WritingTemplates { get; init; } = [];

    public IReadOnlyList<ParsedCountryGuidanceNoteModel> CountryGuidanceNotes { get; init; } = [];

    public IReadOnlyList<ParsedExamProfileModel> ExamProfiles { get; init; } = [];

    public IReadOnlyList<ParsedExamPrepUnitModel> ExamPrepUnits { get; init; } = [];

    /// <summary>
    /// Gets parsed conversation starter packs included in the package.
    /// </summary>
    public IReadOnlyList<ParsedConversationStarterPackModel> ConversationStarterPacks { get; init; } = [];

    /// <summary>
    /// Gets parsed event preparation packs included in the package.
    /// </summary>
    public IReadOnlyList<ParsedEventPreparationPackModel> EventPreparationPacks { get; init; } = [];

    /// <summary>
    /// Gets parsed standalone roleplay scenarios included in the package.
    /// </summary>
    public IReadOnlyList<ParsedRoleplayScenarioModel> RoleplayScenarios { get; init; } = [];
}
