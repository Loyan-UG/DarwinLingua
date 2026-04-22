namespace DarwinLingua.Web.Models;

public sealed record SectionHeadingViewModel(
    string Eyebrow,
    string Title,
    string? Description = null);

public sealed record MetricCardViewModel(
    string Eyebrow,
    string Value,
    string Description);

public sealed record CefrBadgeViewModel(string Value);

public sealed record AsyncStatePanelViewModel(
    string Title,
    string Body,
    string Variant = "info");
