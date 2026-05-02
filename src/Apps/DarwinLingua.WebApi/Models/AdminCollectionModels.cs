namespace DarwinLingua.WebApi.Models;

public sealed record AdminCollectionsResponse(
    IReadOnlyList<AdminCollectionItemResponse> Collections);

public sealed record AdminCollectionItemResponse(
    Guid CollectionId,
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminCollectionLocalizationResponse> Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder,
    int WordCount,
    DateTime UpdatedAtUtc);

public sealed record AdminCollectionDetailResponse(
    Guid CollectionId,
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminCollectionLocalizationResponse> Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminCollectionEntryResponse> Entries);

public sealed record AdminCollectionEntryResponse(
    Guid EntryId,
    Guid WordPublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    int SortOrder);

public sealed record AdminCollectionLocalizationResponse(
    Guid LocalizationId,
    string LanguageCode,
    string Name,
    string? Description);

public sealed record AdminSaveCollectionRequest(
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminSaveCollectionLocalizationRequest>? Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder);

public sealed record AdminSaveCollectionLocalizationRequest(
    string LanguageCode,
    string Name,
    string? Description);

public sealed record AdminAddCollectionWordRequest(
    Guid WordPublicId,
    int SortOrder);

public sealed record AdminBulkCollectionImportRequest(
    IReadOnlyList<AdminBulkCollectionImportItemRequest> Collections);

public sealed record AdminBulkCollectionImportItemRequest(
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminSaveCollectionLocalizationRequest>? Localizations,
    string? ImageUrl,
    string? PublicationStatus,
    int SortOrder,
    IReadOnlyList<AdminBulkCollectionWordImportRequest>? Words);

public sealed record AdminBulkCollectionWordImportRequest(
    Guid WordPublicId,
    int SortOrder);

public sealed record AdminBulkCollectionImportResponse(
    int TotalCount,
    int ImportedCount,
    int SkippedCount,
    int FailedCount,
    IReadOnlyList<AdminBulkCollectionImportItemResult> Items);

public sealed record AdminBulkCollectionImportItemResult(
    int RowNumber,
    string? Slug,
    Guid? CollectionId,
    string Status,
    string Message);
