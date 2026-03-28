namespace DarwinDeutsch.Maui.Services.Storage;

/// <summary>
/// Represents the outcome of applying packaged-seed updates into the local database.
/// </summary>
public sealed record SeedDatabaseUpdateResult(
    bool IsSuccess,
    bool AppliedChanges,
    int ImportedPackages,
    int ImportedWords,
    string SeedSignature,
    DateTimeOffset? AppliedAtUtc,
    string? ErrorMessage);
