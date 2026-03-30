namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents a lifecycle summary for admin-facing catalog publication batches.
/// </summary>
/// <param name="ClientProductKey">The client product key or empty when querying all products.</param>
/// <param name="TotalBatchCount">The total batch count.</param>
/// <param name="DraftBatchCount">The number of draft batches.</param>
/// <param name="PublishedBatchCount">The number of published batches.</param>
/// <param name="SupersededBatchCount">The number of superseded batches.</param>
/// <param name="DeletableBatchCount">The number of batches eligible for cleanup.</param>
/// <param name="LatestPublishedBatchId">The latest published batch identifier if one exists.</param>
/// <param name="LatestPublishedAtUtc">The latest published timestamp if one exists.</param>
public sealed record AdminCatalogBatchHistorySummaryResponse(
    string ClientProductKey,
    int TotalBatchCount,
    int DraftBatchCount,
    int PublishedBatchCount,
    int SupersededBatchCount,
    int DeletableBatchCount,
    string? LatestPublishedBatchId,
    DateTimeOffset? LatestPublishedAtUtc);
