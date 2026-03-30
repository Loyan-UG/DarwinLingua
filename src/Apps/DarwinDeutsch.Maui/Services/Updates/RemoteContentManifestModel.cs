namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Represents one content manifest response returned by the Web API.
/// </summary>
public sealed record RemoteContentManifestModel(
    string ClientProductKey,
    string LearningLanguageCode,
    string ContentAreaKey,
    string SliceKey,
    int SchemaVersion,
    DateTimeOffset GeneratedAtUtc,
    int TotalPackageCount,
    int TotalWordCount,
    IReadOnlyList<RemoteContentPackageModel> Packages);
