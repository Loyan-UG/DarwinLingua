namespace DarwinLingua.Web.Data;

public sealed class WebUserFavoriteWord
{
    public Guid Id { get; set; }

    public string ActorId { get; set; } = string.Empty;

    public Guid WordPublicId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
