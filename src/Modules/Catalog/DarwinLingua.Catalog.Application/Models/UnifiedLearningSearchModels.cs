namespace DarwinLingua.Catalog.Application.Models;

public sealed record UnifiedLearningSearchFilterModel(
    string? Query,
    string? CefrLevel,
    string? ResultType,
    string? Category,
    string? TopicKey);

public sealed record UnifiedLearningSearchResultModel(
    string ResultType,
    string Title,
    string ShortSnippet,
    string? CefrLevel,
    string? Category,
    IReadOnlyList<string> TopicKeys,
    string Url,
    int RelevanceScore,
    IReadOnlyList<string> MatchedFields);
