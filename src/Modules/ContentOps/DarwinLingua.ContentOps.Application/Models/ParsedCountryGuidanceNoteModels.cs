namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedCountryGuidanceNoteModel(
    string Slug,
    string Title,
    IReadOnlyList<ParsedContentMeaningModel> TitleTranslations,
    string ShortDescription,
    IReadOnlyList<ParsedContentMeaningModel> ShortDescriptionTranslations,
    string CefrLevel,
    string Category,
    string Context,
    IReadOnlyList<ParsedContentMeaningModel> ContextTranslations,
    IReadOnlyList<string> Sections,
    IReadOnlyList<ParsedCountryGuidanceNoteListTranslationModel> SectionsTranslations,
    IReadOnlyList<ParsedCountryGuidanceNoteExampleModel> Examples,
    IReadOnlyList<string> DoNotes,
    IReadOnlyList<ParsedCountryGuidanceNoteListTranslationModel> DoNotesTranslations,
    IReadOnlyList<string> DontNotes,
    IReadOnlyList<ParsedCountryGuidanceNoteListTranslationModel> DontNotesTranslations,
    string? SensitivityWarning,
    IReadOnlyList<ParsedContentMeaningModel> SensitivityWarningTranslations,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCountryGuidanceNoteExampleModel(
    string GermanText,
    string? Explanation,
    IReadOnlyList<ParsedContentMeaningModel> ExplanationTranslations);

public sealed record ParsedCountryGuidanceNoteListTranslationModel(
    string Language,
    IReadOnlyList<string> Items);
