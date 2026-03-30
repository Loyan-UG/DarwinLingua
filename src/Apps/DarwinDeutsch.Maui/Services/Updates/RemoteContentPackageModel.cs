namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Represents one published package returned by the Web API manifest.
/// </summary>
public sealed record RemoteContentPackageModel(
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
