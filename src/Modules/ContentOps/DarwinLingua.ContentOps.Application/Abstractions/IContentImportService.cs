using DarwinLingua.ContentOps.Application.Models;

namespace DarwinLingua.ContentOps.Application.Abstractions;

/// <summary>
/// Coordinates the Phase 1 content-package import workflow.
/// </summary>
public interface IContentImportService
{
    /// <summary>
    /// Imports a JSON content package from the specified file path.
    /// </summary>
    Task<ImportContentPackageResult> ImportAsync(
        ImportContentPackageRequest request,
        CancellationToken cancellationToken);
}
