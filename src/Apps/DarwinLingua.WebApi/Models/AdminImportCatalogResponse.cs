namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the result of one server-side catalog import and draft-package staging run.
/// </summary>
public sealed record AdminImportCatalogResponse(
    bool IsSuccess,
    string ClientProductKey,
    string? ImportedPackageId,
    string? ImportedPackageName,
    string ImportStatus,
    int TotalEntries,
    int ImportedEntries,
    int SkippedDuplicateEntries,
    int InvalidEntries,
    int WarningCount,
    string? DraftPublicationBatchId,
    IReadOnlyList<string> StagedPackageIds,
    IReadOnlyList<string> ImportedLemmas,
    IReadOnlyList<string> IssueMessages);
