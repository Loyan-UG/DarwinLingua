namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Fetches remote manifests and applies full-database updates into local SQLite.
/// </summary>
public interface IRemoteContentUpdateService
{
    Task<IReadOnlyList<RemoteContentUpdateHistoryEntry>> GetRecentUpdateHistoryAsync(
        CancellationToken cancellationToken);

    Task<RemoteContentUpdateStatus> GetUpdateStatusAsync(
        string databasePath,
        CancellationToken cancellationToken);

    Task<RemoteContentUpdateStatus> GetAreaUpdateStatusAsync(
        string databasePath,
        string areaKey,
        CancellationToken cancellationToken);

    Task<RemoteContentUpdateStatus> GetCefrUpdateStatusAsync(
        string databasePath,
        string cefrLevel,
        CancellationToken cancellationToken);

    Task<RemoteContentUpdateResult> ApplyFullUpdateAsync(
        string databasePath,
        CancellationToken cancellationToken);

    Task<RemoteContentUpdateResult> ApplyAreaUpdateAsync(
        string databasePath,
        string areaKey,
        CancellationToken cancellationToken);

    Task<RemoteContentUpdateResult> ApplyCefrUpdateAsync(
        string databasePath,
        string cefrLevel,
        CancellationToken cancellationToken);
}
