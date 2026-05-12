namespace DarwinLingua.Catalog.Application.Models;

public sealed record ExpressionListFilterModel(
    string? CefrLevel,
    string? ExpressionType,
    string? Register,
    string? Category,
    string? TopicKey,
    bool? IsRisky,
    string? Query);

public sealed record ExpressionListItemModel(
    string Slug,
    string ExpressionText,
    string ActualMeaning,
    string? LiteralMeaning,
    string CefrLevel,
    string ExpressionType,
    string Register,
    string Category,
    string? Region,
    bool IsRisky,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<string> WarningTypes);

public sealed record ExpressionDetailModel(
    string Slug,
    string ExpressionText,
    string ActualMeaning,
    string? LiteralMeaning,
    string? UsageExplanation,
    string CefrLevel,
    string ExpressionType,
    string Register,
    string Category,
    string? Region,
    bool IsRisky,
    IReadOnlyList<string> TopicKeys,
    IReadOnlyList<ExpressionExampleModel> Examples,
    IReadOnlyList<ExpressionWarningModel> Warnings,
    IReadOnlyList<ExpressionLinkedWordModel> LinkedWords,
    IReadOnlyList<string> RelatedExpressionSlugs,
    IReadOnlyList<string> LinkedExerciseSlugs);

public sealed record ExpressionExampleModel(
    string GermanText,
    string? Note,
    string? Translation,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record ExpressionWarningModel(
    string WarningType,
    string Text,
    string? RequestedLanguageCode,
    bool UsedFallback);

public sealed record ExpressionLinkedWordModel(
    string Lemma,
    string? WordSlug);
