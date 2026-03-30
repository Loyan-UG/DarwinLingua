namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Describes one content package batch from the admin draft-management view.
/// </summary>
public sealed record AdminDraftCatalogBatchResponse(
    string ClientProductKey,
    string PublicationBatchId,
    string PublicationStatus,
    string Version,
    int PackageCount,
    int TotalWordCount,
    int TotalEntryCount,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc,
    DateTimeOffset? SupersededAtUtc,
    IReadOnlyList<AdminDraftCatalogBatchPackageResponse> Packages);
