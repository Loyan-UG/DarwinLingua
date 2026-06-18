namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedCulturalNoteModel(
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
    IReadOnlyList<ParsedCulturalNoteListTranslationModel> SectionsTranslations,
    IReadOnlyList<ParsedCulturalNoteExampleModel> Examples,
    IReadOnlyList<string> DoNotes,
    IReadOnlyList<ParsedCulturalNoteListTranslationModel> DoNotesTranslations,
    IReadOnlyList<string> DontNotes,
    IReadOnlyList<ParsedCulturalNoteListTranslationModel> DontNotesTranslations,
    string? SensitivityWarning,
    IReadOnlyList<ParsedContentMeaningModel> SensitivityWarningTranslations,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCulturalNoteExampleModel(
    string GermanText,
    string? Explanation,
    IReadOnlyList<ParsedContentMeaningModel> ExplanationTranslations);

public sealed record ParsedCulturalNoteListTranslationModel(
    string Language,
    IReadOnlyList<string> Items);
