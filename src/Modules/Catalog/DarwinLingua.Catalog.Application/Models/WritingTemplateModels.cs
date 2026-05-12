namespace DarwinLingua.Catalog.Application.Models;

public sealed record WritingTemplateListFilterModel(
    string? CefrLevel,
    string? Category,
    string? Register,
    string? Situation,
    string? Query);

public sealed record WritingTemplateListItemModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string Category,
    string Situation,
    string Register);

public sealed record WritingTemplateDetailModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string Category,
    string Situation,
    string Register,
    string TemplateText,
    string Explanation,
    IReadOnlyList<string> Variables,
    string SampleFilledVersion,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedWordSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs);
