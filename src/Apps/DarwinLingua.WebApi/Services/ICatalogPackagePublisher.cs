using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Creates mobile content package payloads and metadata from the shared catalog database.
/// </summary>
public interface ICatalogPackagePublisher
{
    /// <summary>
    /// Publishes the latest catalog state for the specified client product.
    /// </summary>
    Task<CatalogPackagePublicationResult> PublishAsync(
        string clientProductKey,
        CancellationToken cancellationToken);
}
