namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the result of reactivating one superseded catalog package batch.
/// </summary>
public sealed record AdminRollbackCatalogResponse(
    bool IsSuccess,
    string ClientProductKey,
    string PublicationBatchId,
    string ReactivatedVersion,
    IReadOnlyList<string> ReactivatedPackageIds,
    IReadOnlyList<string> SupersededPackageIds,
    IReadOnlyList<string> IssueMessages);
