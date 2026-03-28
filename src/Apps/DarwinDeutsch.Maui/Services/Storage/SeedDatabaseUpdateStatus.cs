namespace DarwinDeutsch.Maui.Services.Storage;

/// <summary>
/// Describes whether the packaged seed database contains unapplied content updates.
/// </summary>
public sealed record SeedDatabaseUpdateStatus(
    bool IsSeedAvailable,
    bool IsUpdateAvailable,
    int PendingPackageCount,
    int PendingWordCount,
    string SeedSignature,
    string AppliedSignature,
    DateTimeOffset? LastAppliedAtUtc,
    int LastAppliedPackageCount,
    int LastAppliedWordCount);
