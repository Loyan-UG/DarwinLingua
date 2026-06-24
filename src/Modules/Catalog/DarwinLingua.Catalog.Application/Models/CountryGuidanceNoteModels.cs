namespace DarwinLingua.Catalog.Application.Models;

public sealed record CountryGuidanceNoteListFilterModel(
    string? CefrLevel,
    string? Category,
    string? Context,
    string? Query);

public sealed record CountryGuidanceNoteListItemModel(
    string Slug,
    string TargetLearningLanguageCode,
    string CountryContextCode,
    string Title,
    string? LearnerLanguageTitle,
    string ShortDescription,
    string? LearnerLanguageShortDescription,
    string CefrLevel,
    string Category,
    string Context,
    string? LearnerLanguageContext,
    bool HasSensitivityWarning);

public sealed record CountryGuidanceNoteDetailModel(
    string Slug,
    string TargetLearningLanguageCode,
    string CountryContextCode,
    string Title,
    string? LearnerLanguageTitle,
    string ShortDescription,
    string? LearnerLanguageShortDescription,
    string CefrLevel,
    string Category,
    string Context,
    string? LearnerLanguageContext,
    IReadOnlyList<string> Sections,
    IReadOnlyList<string> LearnerLanguageSections,
    IReadOnlyList<CountryGuidanceNoteExampleModel> Examples,
    IReadOnlyList<string> DoNotes,
    IReadOnlyList<string> LearnerLanguageDoNotes,
    IReadOnlyList<string> DontNotes,
    IReadOnlyList<string> LearnerLanguageDontNotes,
    string? SensitivityWarning,
    string? LearnerLanguageSensitivityWarning,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs);

public sealed record CountryGuidanceNoteExampleModel(
    string GermanText,
    string? Explanation,
    string? LearnerLanguageExplanation);
