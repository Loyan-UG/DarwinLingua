using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Coordinates server-side import into the shared catalog and publication of mobile packages.
/// </summary>
public interface IServerCatalogImportService
{
    /// <summary>
    /// Imports the specified package file and publishes updated mobile package payloads.
    /// </summary>
    Task<AdminImportCatalogResponse> ImportAndPublishAsync(
        AdminImportCatalogRequest request,
        CancellationToken cancellationToken);
}
