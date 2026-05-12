namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedCulturalNoteModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string Category,
    string Context,
    IReadOnlyList<string> Sections,
    IReadOnlyList<ParsedCulturalNoteExampleModel> Examples,
    IReadOnlyList<string> DoNotes,
    IReadOnlyList<string> DontNotes,
    string? SensitivityWarning,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs,
    bool IsPublished,
    int SortOrder);

public sealed record ParsedCulturalNoteExampleModel(
    string GermanText,
    string? Explanation);
