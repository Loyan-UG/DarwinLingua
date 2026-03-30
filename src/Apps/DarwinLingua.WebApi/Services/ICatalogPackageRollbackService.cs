using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

/// <summary>
/// Re-activates one superseded catalog package batch for mobile delivery.
/// </summary>
public interface ICatalogPackageRollbackService
{
    /// <summary>
    /// Rolls one superseded batch back to the published state.
    /// </summary>
    Task<AdminRollbackCatalogResponse> RollbackAsync(
        AdminRollbackCatalogRequest request,
        CancellationToken cancellationToken);
}
