namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents one published package returned through the manifest API.
/// </summary>
public sealed record PublishedContentPackageResponse(
    string PackageId,
    string ClientProductKey,
    string ContentAreaKey,
    string SliceKey,
    string PackageType,
    string Version,
    int SchemaVersion,
    int MinimumAppSchemaVersion,
    string Checksum,
    int EntryCount,
    int WordCount,
    DateTimeOffset CreatedAtUtc,
    string DownloadUrl);
