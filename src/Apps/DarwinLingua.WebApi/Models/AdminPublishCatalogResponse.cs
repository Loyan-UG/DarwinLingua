namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents the result of publishing one staged catalog package batch.
/// </summary>
public sealed record AdminPublishCatalogResponse(
    bool IsSuccess,
    string ClientProductKey,
    string PublicationBatchId,
    string PublishedVersion,
    IReadOnlyList<string> PublishedPackageIds,
    IReadOnlyList<string> SupersededPackageIds,
    IReadOnlyList<string> IssueMessages);
