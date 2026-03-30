namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Describes the current remote full-update status for the mobile client.
/// </summary>
public sealed record RemoteContentUpdateStatus(
    string ScopeKey,
    string ContentAreaKey,
    string SliceKey,
    string PackageType,
    bool IsRemoteConfigured,
    bool IsServerReachable,
    bool IsUpdateAvailable,
    string LocalPackageId,
    string LocalVersion,
    string LocalChecksum,
    int LocalSchemaVersion,
    string RemotePackageId,
    string RemoteVersion,
    string RemoteChecksum,
    int RemoteSchemaVersion,
    int PendingWordCount,
    DateTimeOffset? RemoteManifestGeneratedAtUtc,
    DateTimeOffset? LastSuccessfulUpdateAtUtc,
    string LastFailureMessage);
