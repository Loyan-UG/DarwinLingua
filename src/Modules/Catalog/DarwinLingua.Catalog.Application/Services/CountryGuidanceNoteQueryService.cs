using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;

namespace DarwinLingua.Catalog.Application.Services;

internal sealed class CountryGuidanceNoteQueryService(ICountryGuidanceNoteRepository countryGuidanceNoteRepository) : ICountryGuidanceNoteQueryService
{
    public Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNotesAsync(filter, null, cancellationToken);

    public Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNotesAsync(filter, primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNotesAsync(filter, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string targetLearningLanguageCode, string countryContextCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNotesAsync(filter, targetLearningLanguageCode, countryContextCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNoteBySlugAsync(slug, null, cancellationToken);

    public Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNoteBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken);

    public Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNoteBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken);

    public Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string targetLearningLanguageCode, string countryContextCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken) =>
        countryGuidanceNoteRepository.GetPublishedCountryGuidanceNoteBySlugAsync(slug, targetLearningLanguageCode, countryContextCode, primaryMeaningLanguageCode, cancellationToken);
}
