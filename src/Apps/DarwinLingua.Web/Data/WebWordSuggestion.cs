namespace DarwinLingua.Web.Data;

public enum WebWordSuggestionStatus
{
    Pending = 0,
    Reviewed = 1,
    Accepted = 2,
    Declined = 3,
}

public sealed class WebWordSuggestion
{
    public Guid Id { get; set; }

    public string SuggestedWord { get; set; } = string.Empty;

    public string NormalizedSuggestedWord { get; set; } = string.Empty;

    public string? Note { get; set; }

    public string? SourceQuery { get; set; }

    public string ActorId { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string? Email { get; set; }

    public WebWordSuggestionStatus Status { get; set; }

    public string? AdminNote { get; set; }

    public string? DecidedBy { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? DecidedAtUtc { get; set; }
}
