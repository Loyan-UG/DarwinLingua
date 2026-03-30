namespace DarwinDeutsch.Maui.Services.Updates;

/// <summary>
/// Represents the outcome of a remote full-content update.
/// </summary>
public sealed record RemoteContentUpdateResult(
    bool IsSuccess,
    bool AppliedChanges,
    string AppliedPackageId,
    string AppliedVersion,
    int ImportedWords,
    DateTimeOffset? AppliedAtUtc,
    string? ErrorMessage);
