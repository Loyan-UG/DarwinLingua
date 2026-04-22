using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Services;

public interface IWebAdminOperationsQueryService
{
    Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken);
}

internal sealed class WebAdminOperationsQueryService(IWebCatalogApiClient catalogApiClient) : IWebAdminOperationsQueryService
{
    public Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminImportsAsync(statusFilter, cancellationToken);

    public Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminDraftWordsAsync(query, cancellationToken);

    public Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminHistoryAsync(statusFilter, cancellationToken);

    public Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminRollbackPreviewAsync(cancellationToken);
}
