namespace DarwinLingua.Web.Data;

public sealed class WebUserWordState
{
    public Guid Id { get; set; }

    public string ActorId { get; set; } = string.Empty;

    public Guid WordPublicId { get; set; }

    public bool IsKnown { get; set; }

    public bool IsDifficult { get; set; }

    public DateTime? FirstViewedAtUtc { get; set; }

    public DateTime? LastViewedAtUtc { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
