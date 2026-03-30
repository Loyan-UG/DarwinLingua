using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Coordinates server-side import into the shared catalog and staging of mobile packages.
/// </summary>
public interface IServerCatalogImportService
{
    /// <summary>
    /// Imports the specified package file and stages updated mobile package payloads.
    /// </summary>
    Task<AdminImportCatalogResponse> ImportAndStageAsync(
        AdminImportCatalogRequest request,
        CancellationToken cancellationToken);
}
