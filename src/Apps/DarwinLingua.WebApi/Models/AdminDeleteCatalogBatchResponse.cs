namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Describes the result of deleting a superseded package batch from the server.
/// </summary>
public sealed record AdminDeleteCatalogBatchResponse(
    bool IsSuccess,
    string ClientProductKey,
    string PublicationBatchId,
    int DeletedPackageCount,
    IReadOnlyList<string> DeletedPackageIds,
    IReadOnlyList<string> IssueMessages);
