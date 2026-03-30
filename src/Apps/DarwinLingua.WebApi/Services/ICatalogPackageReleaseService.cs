using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Promotes staged package batches to published status for mobile clients.
/// </summary>
public interface ICatalogPackageReleaseService
{
    /// <summary>
    /// Publishes the latest staged batch, or the requested batch, for the specified client product.
    /// </summary>
    Task<AdminPublishCatalogResponse> PublishAsync(
        AdminPublishCatalogRequest request,
        CancellationToken cancellationToken);
}
