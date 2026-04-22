namespace DarwinLingua.Web.Data;

public sealed class WebUserPreference
{
    public Guid Id { get; set; }

    public string ActorId { get; set; } = string.Empty;

    public string UiLanguageCode { get; set; } = "en";

    public string PrimaryMeaningLanguageCode { get; set; } = "en";

    public string? SecondaryMeaningLanguageCode { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
