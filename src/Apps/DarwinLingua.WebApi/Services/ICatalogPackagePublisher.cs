using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Creates mobile content package payloads and metadata from the shared catalog database.
/// </summary>
public interface ICatalogPackagePublisher
{
    /// <summary>
    /// Stages the latest catalog state for the specified client product as a draft package batch.
    /// </summary>
    Task<CatalogPackagePublicationResult> StageDraftAsync(
        string clientProductKey,
        CancellationToken cancellationToken);
}
