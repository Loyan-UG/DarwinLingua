using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Provides admin-facing publication-history visibility for catalog package batches.
/// </summary>
public interface ICatalogPublicationHistoryService
{
    /// <summary>
    /// Gets all known publication batches with lifecycle history.
    /// </summary>
    Task<IReadOnlyList<AdminCatalogBatchHistoryResponse>> GetHistoryAsync(
        string? clientProductKey,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a status summary over all known publication batches.
    /// </summary>
    Task<AdminCatalogBatchHistorySummaryResponse> GetSummaryAsync(
        string? clientProductKey,
        CancellationToken cancellationToken);
}
