namespace DarwinLingua.Web.Configuration;

public sealed class WebApiOptions
{
    public const string SectionName = "WebApi";

    public string BaseUrl { get; set; } = string.Empty;

    public bool IgnoreSslErrors { get; set; }

    public string AdminApiKey { get; set; } = string.Empty;

    public string AdminApiHeaderName { get; set; } = "X-DarwinLingua-Admin-Key";
}
