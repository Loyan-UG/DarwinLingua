namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents one content-area summary for settings and diagnostics.
/// </summary>
public sealed record MobileContentAreaSummaryResponse(
    string ContentAreaKey,
    int PackageCount,
    int TotalWordCount,
    IReadOnlyList<string> SliceKeys);
