namespace DarwinLingua.Catalog.Application.Models;

public sealed record CulturalNoteListFilterModel(
    string? CefrLevel,
    string? Category,
    string? Context,
    string? Query);

public sealed record CulturalNoteListItemModel(
    string Slug,
    string Title,
    string? LearnerLanguageTitle,
    string ShortDescription,
    string? LearnerLanguageShortDescription,
    string CefrLevel,
    string Category,
    string Context,
    string? LearnerLanguageContext,
    bool HasSensitivityWarning);

public sealed record CulturalNoteDetailModel(
    string Slug,
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
    IReadOnlyList<CulturalNoteExampleModel> Examples,
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

public sealed record CulturalNoteExampleModel(
    string GermanText,
    string? Explanation,
    string? LearnerLanguageExplanation);
