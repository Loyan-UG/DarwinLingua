using DarwinLingua.WebApi.Models;

namespace DarwinLingua.WebApi.Services;

public interface IWordAdminService
{
    Task<AdminCatalogWordDetailResponse> CreateAsync(
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> UpdateMetadataAsync(
        Guid publicId,
        AdminUpdateWordMetadataRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> AddSenseAsync(
        Guid publicId,
        AdminAddWordSenseRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> AddSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> AddSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        AdminAddWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> UpdateSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        AdminUpdateWordSenseTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> DeleteSenseTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> UpdateSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminUpdateWordSenseExampleRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> DeleteSenseExampleAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> AddSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        AdminAddWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> UpdateSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        AdminUpdateWordSenseExampleTranslationRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> DeleteSenseExampleTranslationAsync(
        Guid publicId,
        Guid senseId,
        Guid exampleId,
        Guid translationId,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> AddTopicAsync(
        Guid publicId,
        AdminAddWordTopicRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> DeleteTopicAsync(
        Guid publicId,
        Guid topicId,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> AddLabelAsync(
        Guid publicId,
        AdminAddWordLabelRequest request,
        CancellationToken cancellationToken);

    Task<AdminCatalogWordDetailResponse?> DeleteLabelAsync(
        Guid publicId,
        string kind,
        string key,
        CancellationToken cancellationToken);

    Task<AdminBulkWordImportResponse> ImportWordsAsync(
        AdminBulkWordImportRequest request,
        CancellationToken cancellationToken);
}
