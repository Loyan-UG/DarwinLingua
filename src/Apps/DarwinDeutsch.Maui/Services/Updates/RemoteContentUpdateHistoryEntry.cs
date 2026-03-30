namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Represents one recent remote content update attempt recorded on the device.
/// </summary>
public sealed record RemoteContentUpdateHistoryEntry(
    string ScopeKey,
    bool IsSuccess,
    bool AppliedChanges,
    string PackageId,
    string Version,
    int ImportedWords,
    DateTimeOffset OccurredAtUtc,
    string ErrorMessage);
