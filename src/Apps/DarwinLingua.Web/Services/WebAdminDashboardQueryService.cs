using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Services;

public interface IWebAdminDashboardQueryService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken);
}

internal sealed class WebAdminDashboardQueryService(IWebCatalogApiClient catalogApiClient) : IWebAdminDashboardQueryService
{
    public Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminDashboardAsync(cancellationToken);
}
