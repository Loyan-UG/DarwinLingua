using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Deletes superseded package batches and their payload files from the server.
/// </summary>
public interface ICatalogPackageCleanupService
{
    Task<AdminDeleteCatalogBatchResponse> DeleteSupersededBatchAsync(
        string publicationBatchId,
        string? clientProductKey,
        CancellationToken cancellationToken);
}
