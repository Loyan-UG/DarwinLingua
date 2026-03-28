namespace DarwinLingua.ContentOps.Application.Models;

/// <summary>
/// Represents the final result of a Phase 1 content-package import operation.
/// </summary>
public sealed record ImportContentPackageResult(
    bool IsSuccess,
    string? PackageId,
    string? PackageName,
    string Status,
    int TotalEntries,
    int ImportedEntries,
    int SkippedDuplicateEntries,
    int InvalidEntries,
    int WarningCount,
    IReadOnlyList<ImportIssueModel> Issues,
    IReadOnlyList<string> ImportedLemmas);
