using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Reads staged and published catalog package batches for admin workflows.
/// </summary>
public interface ICatalogPackageDraftQueryService
{
    Task<IReadOnlyList<AdminDraftCatalogBatchResponse>> GetBatchesAsync(
        string? clientProductKey,
        CancellationToken cancellationToken);

    Task<AdminDraftCatalogBatchResponse> GetBatchAsync(
        string publicationBatchId,
        string? clientProductKey,
        CancellationToken cancellationToken);
}
