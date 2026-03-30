namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents one manifest response returned to a mobile client.
/// </summary>
public sealed record MobileContentManifestResponse(
    string ClientProductKey,
    string LearningLanguageCode,
    string ContentAreaKey,
    string SliceKey,
    int SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    int TotalPackageCount,
    int TotalWordCount,
    IReadOnlyList<PublishedContentPackageResponse> Packages);
