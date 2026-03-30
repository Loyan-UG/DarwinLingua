namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Describes the current remote full-update status for the mobile client.
/// </summary>
public sealed record RemoteContentUpdateStatus(
    bool IsRemoteConfigured,
    bool IsServerReachable,
    bool IsUpdateAvailable,
    string LocalPackageId,
    string LocalVersion,
    string RemotePackageId,
    string RemoteVersion,
    int PendingWordCount,
    DateTimeOffset? LastSuccessfulUpdateAtUtc,
    string LastFailureMessage);
