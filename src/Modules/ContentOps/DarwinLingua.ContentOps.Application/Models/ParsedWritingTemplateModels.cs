namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedWritingTemplateModel(
    string Slug,
    string Title,
    string ShortDescription,
    string CefrLevel,
    string Category,
    string Situation,
    string Register,
    string TemplateText,
    string Explanation,
    IReadOnlyList<string> ReplaceableVariables,
    string SampleFilledVersion,
    IReadOnlyList<string> LinkedGrammarTopicSlugs,
    IReadOnlyList<string> LinkedWordSlugs,
    IReadOnlyList<string> LinkedExpressionSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs,
    bool IsPublished,
    int SortOrder);
