using DarwinLingua.Web.Data;

namespace DarwinLingua.Web.Models;

public sealed record WordSuggestionInputModel(
    string SuggestedWord,
    string? Note,
    string? SourceQuery);

public sealed record WordSuggestionListItemViewModel(
    Guid Id,
    string SuggestedWord,
    string? Note,
    string? SourceQuery,
    string ActorId,
    string? Email,
    WebWordSuggestionStatus Status,
    string? AdminNote,
    string? DecidedBy,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DecidedAtUtc);

public sealed record AdminWordSuggestionsPageViewModel(
    string? StatusFilter,
    IReadOnlyList<WordSuggestionListItemViewModel> Suggestions,
    string? StatusMessage,
    string? ErrorMessage);

public sealed record AdminWordSuggestionDecisionInputModel(
    string Status,
    string? AdminNote);
