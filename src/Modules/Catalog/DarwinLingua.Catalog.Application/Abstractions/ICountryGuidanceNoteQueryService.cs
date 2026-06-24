using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Catalog.Application.Abstractions;

public interface ICountryGuidanceNoteQueryService
{
    Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, CancellationToken cancellationToken) =>
        GetPublishedCountryGuidanceNotesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, null, cancellationToken);

    Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedCountryGuidanceNotesAsync(filter, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string targetLearningLanguageCode, string countryContextCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, CancellationToken cancellationToken) =>
        GetPublishedCountryGuidanceNoteBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, null, cancellationToken);

    Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        GetPublishedCountryGuidanceNoteBySlugAsync(slug, ContentLanguageRequirements.DefaultTargetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);

    Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string targetLearningLanguageCode, string countryContextCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken);
}
