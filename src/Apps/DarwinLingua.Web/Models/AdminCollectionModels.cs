using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DarwinLingua.Web.Models;

public sealed record AdminCollectionsPageViewModel(
    IReadOnlyList<AdminCollectionItemViewModel> Collections);

public sealed record AdminCollectionItemViewModel(
    Guid CollectionId,
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminCollectionLocalizationViewModel>? Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder,
    int WordCount,
    DateTime UpdatedAtUtc);

public sealed record AdminCollectionDetailViewModel(
    Guid CollectionId,
    string Slug,
    string Name,
    string? Description,
    IReadOnlyList<AdminCollectionLocalizationViewModel>? Localizations,
    string? ImageUrl,
    string PublicationStatus,
    int SortOrder,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<AdminCollectionEntryViewModel> Entries);

public sealed record AdminCollectionEntryViewModel(
    Guid EntryId,
    Guid WordPublicId,
    string Lemma,
    string PartOfSpeech,
    string CefrLevel,
    int SortOrder);

public sealed record AdminCollectionLocalizationViewModel(
    Guid LocalizationId,
    string LanguageCode,
    string Name,
    string? Description);

public sealed class AdminCollectionEditViewModel
{
    public Guid CollectionId { get; set; }

    public bool IsNew => CollectionId == Guid.Empty;

    [Required]
    [StringLength(128)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    [StringLength(1024)]
    public string? ImageUrl { get; set; }

    [Required]
    public string PublicationStatus { get; set; } = "Draft";

    public int SortOrder { get; set; }

    public static AdminCollectionEditViewModel CreateNew() => new()
    {
        CollectionId = Guid.Empty,
        PublicationStatus = "Draft",
    };

    public static AdminCollectionEditViewModel FromDetail(AdminCollectionDetailViewModel collection) => new()
    {
        CollectionId = collection.CollectionId,
        Slug = collection.Slug,
        Name = collection.Name,
        Description = collection.Description,
        ImageUrl = collection.ImageUrl,
        PublicationStatus = collection.PublicationStatus,
        SortOrder = collection.SortOrder,
    };
}

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

public sealed record AdminCollectionWordPickerViewModel(
    string? Query,
    IReadOnlySet<Guid> AssignedWordIds,
    IReadOnlyList<AdminWordListItemViewModel> Words)
{
    public IReadOnlyList<AdminWordListItemViewModel> AvailableWords =>
        Words
            .Where(word => !AssignedWordIds.Contains(word.PublicId))
            .OrderBy(word => word.Lemma)
            .ThenBy(word => word.CefrLevel)
            .ToArray();
}

public sealed class AdminBulkCollectionImportFormModel
{
    public IFormFile? JsonFile { get; set; }
}

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
