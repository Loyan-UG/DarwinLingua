using DarwinLingua.Web.Models;

namespace DarwinLingua.Web.Services;

public interface IWebAdminOperationsQueryService
{
    Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminWordsPageViewModel> GetWordsAsync(
        string? query,
        string? statusFilter,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel?> GetWordAsync(Guid publicId, CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> CreateWordAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminBulkWordImportResponse> ImportWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> UpdateWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> AddWordLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken);

    Task<AdminWordDetailViewModel> DeleteWordLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken);

    Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken);

    Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken);

    Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken);

    Task<AdminTopicsPageViewModel> GetTopicsAsync(CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel?> GetTopicAsync(Guid topicId, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> CreateTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminTopicItemViewModel> UpdateTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken);

    Task<AdminLabelsPageViewModel> GetLabelsAsync(CancellationToken cancellationToken);
}

internal sealed class WebAdminOperationsQueryService(IWebCatalogApiClient catalogApiClient) : IWebAdminOperationsQueryService
{
    public Task<AdminImportsPageViewModel> GetImportsAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminImportsAsync(statusFilter, cancellationToken);

    public Task<AdminWordsPageViewModel> GetWordsAsync(
        string? query,
        string? statusFilter,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminWordsAsync(query, statusFilter, skip, take, cancellationToken);

    public Task<AdminWordDetailViewModel?> GetWordAsync(Guid publicId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminWordAsync(publicId, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordMetadataAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> CreateWordAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminWordAsync(request, cancellationToken);

    public Task<AdminBulkWordImportResponse> ImportWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.ImportAdminWordsAsync(request, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseTranslationAsync(publicId, senseId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordSenseExampleAsync(publicId, senseId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordSenseTranslationAsync(publicId, senseId, translationId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordSenseTranslationAsync(publicId, senseId, translationId, cancellationToken);

    public Task<AdminWordDetailViewModel> UpdateWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminWordSenseExampleAsync(publicId, senseId, exampleId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordSenseExampleAsync(publicId, senseId, exampleId, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordTopicAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordTopicAsync(publicId, topicId, cancellationToken);

    public Task<AdminWordDetailViewModel> AddWordLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken) =>
        catalogApiClient.AddAdminWordLabelAsync(publicId, request, cancellationToken);

    public Task<AdminWordDetailViewModel> DeleteWordLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken) =>
        catalogApiClient.DeleteAdminWordLabelAsync(publicId, kind, key, cancellationToken);

    public Task<AdminDraftWordsPageViewModel> GetDraftWordsAsync(string? query, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminDraftWordsAsync(query, cancellationToken);

    public Task<AdminHistoryPageViewModel> GetHistoryAsync(string? statusFilter, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminHistoryAsync(statusFilter, cancellationToken);

    public Task<AdminRollbackPageViewModel> GetRollbackPreviewAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminRollbackPreviewAsync(cancellationToken);

    public Task<AdminTopicsPageViewModel> GetTopicsAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminTopicsAsync(cancellationToken);

    public Task<AdminTopicItemViewModel?> GetTopicAsync(Guid topicId, CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminTopicAsync(topicId, cancellationToken);

    public Task<AdminTopicItemViewModel> CreateTopicAsync(AdminSaveTopicRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.CreateAdminTopicAsync(request, cancellationToken);

    public Task<AdminTopicItemViewModel> UpdateTopicAsync(Guid topicId, AdminSaveTopicRequest request, CancellationToken cancellationToken) =>
        catalogApiClient.UpdateAdminTopicAsync(topicId, request, cancellationToken);

    public Task<AdminLabelsPageViewModel> GetLabelsAsync(CancellationToken cancellationToken) =>
        catalogApiClient.GetAdminLabelsAsync(cancellationToken);
}
