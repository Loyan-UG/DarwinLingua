namespace DarwinLingua.WebApi.Models;

/// <summary>
/// Represents one admin-facing catalog publication batch with lifecycle history.
/// </summary>
/// <param name="ClientProductKey">The owning client product key.</param>
/// <param name="PublicationBatchId">The publication batch identifier.</param>
/// <param name="PublicationStatus">The current lifecycle status.</param>
/// <param name="Version">The batch version.</param>
/// <param name="PackageCount">The number of packages in the batch.</param>
/// <param name="TotalWordCount">The total word count.</param>
/// <param name="TotalEntryCount">The total entry count.</param>
/// <param name="CanDelete">Whether the batch can be removed by cleanup.</param>
/// <param name="CreatedAtUtc">The batch creation time.</param>
/// <param name="PublishedAtUtc">The publication time if published.</param>
/// <param name="SupersededAtUtc">The superseded time if superseded.</param>
/// <param name="Packages">The packages inside the batch.</param>
public sealed record AdminCatalogBatchHistoryResponse(
    string ClientProductKey,
    string PublicationBatchId,
    string PublicationStatus,
    string Version,
    int PackageCount,
    int TotalWordCount,
    int TotalEntryCount,
    bool CanDelete,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc,
    DateTimeOffset? SupersededAtUtc,
    IReadOnlyList<AdminCatalogBatchHistoryPackageResponse> Packages);
