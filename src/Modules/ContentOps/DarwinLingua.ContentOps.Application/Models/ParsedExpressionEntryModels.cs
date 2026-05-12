namespace DarwinLingua.ContentOps.Application.Models;

public sealed record ParsedExpressionEntryModel(
    string Slug,
    string ExpressionText,
    string? LiteralMeaningText,
    string ActualMeaningText,
    string? UsageExplanation,
    string CefrLevel,
    string ExpressionType,
    string Register,
    string Category,
    string? Region,
    bool IsRisky,
    IReadOnlyList<string> Topics,
    bool IsPublished,
    int SortOrder,
    IReadOnlyList<ParsedExpressionMeaningModel> Meanings,
    IReadOnlyList<ParsedExpressionExampleModel> Examples,
    IReadOnlyList<ParsedExpressionWarningModel> Warnings,
    IReadOnlyList<ParsedExpressionLinkedWordModel> LinkedWords,
    IReadOnlyList<string> RelatedExpressionSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs);

public sealed record ParsedExpressionMeaningModel(
    string Language,
    string ActualMeaningText,
    string? LiteralMeaningText,
    string? UsageExplanation);

public sealed record ParsedExpressionExampleModel(
    string GermanText,
    string? Note,
    IReadOnlyList<ParsedContentMeaningModel> Translations,
    int SortOrder);

public sealed record ParsedExpressionWarningModel(
    string WarningType,
    string Text,
    IReadOnlyList<ParsedContentMeaningModel> Translations);

public sealed record ParsedExpressionLinkedWordModel(
    string Lemma,
    string? WordSlug,
    int SortOrder);
