namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Fetches remote manifests and applies full-database updates into local SQLite.
/// </summary>
public interface IRemoteContentUpdateService
{
    /// <summary>
    /// Gets the current remote full-update status.
    /// </summary>
    Task<RemoteContentUpdateStatus> GetUpdateStatusAsync(
        string databasePath,
        CancellationToken cancellationToken);

    /// <summary>
    /// Applies the latest full-database package from the Web API into local SQLite.
    /// </summary>
    Task<RemoteContentUpdateResult> ApplyFullUpdateAsync(
        string databasePath,
        CancellationToken cancellationToken);
}
