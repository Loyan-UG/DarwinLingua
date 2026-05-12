namespace DarwinLingua.Catalog.Application.Models;

public sealed record CulturalNoteListFilterModel(
    string? CefrLevel,
    string? Category,
    string? Context,
    string? Query);

public sealed record CulturalNoteListItemModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string Category,
    string Context,
    bool HasSensitivityWarning);

public sealed record CulturalNoteDetailModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string Category,
    string Context,
    IReadOnlyList<string> Sections,
    IReadOnlyList<CulturalNoteExampleModel> Examples,
    IReadOnlyList<string> DoNotes,
    IReadOnlyList<string> DontNotes,
    string? SensitivityWarning,
    IReadOnlyList<string> LinkedDialogueSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedWritingTemplateSlugs,
    IReadOnlyList<string> LinkedTalkTopicSlugs,
    IReadOnlyList<string> LinkedCourseLessonSlugs);

public sealed record CulturalNoteExampleModel(
    string GermanText,
    string? Explanation);
