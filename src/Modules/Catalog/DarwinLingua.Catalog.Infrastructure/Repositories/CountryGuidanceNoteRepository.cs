using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class CountryGuidanceNoteRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : ICountryGuidanceNoteRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        string countryContextCode = ResolveDefaultCountryContextCode(targetLanguageCode);
        return await GetPublishedCountryGuidanceNotesAsync(filter, targetLanguageCode, countryContextCode, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<CountryGuidanceNoteListItemModel>> GetPublishedCountryGuidanceNotesAsync(CountryGuidanceNoteListFilterModel filter, string targetLearningLanguageCode, string countryContextCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        string normalizedCountryContextCode = NormalizeCountryContextCode(countryContextCode, targetLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<CountryGuidanceNote> query = dbContext.CountryGuidanceNotes
            .AsNoTracking()
            .Where(note =>
                note.PublicationStatus == PublicationStatus.Active &&
                note.TargetLearningLanguageCode == targetLanguageCode &&
                note.CountryContextCode == normalizedCountryContextCode);
        query = ApplyFilters(query, filter);

        List<CountryGuidanceNote> notes = await query
            .OrderBy(note => note.SortOrder)
            .ThenBy(note => note.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return notes
            .Select(note => new CountryGuidanceNoteListItemModel(
                note.Slug,
                note.TargetLearningLanguageCode,
                note.CountryContextCode,
                note.Title,
                ResolveTranslation(note.TitleTranslationsJson, primaryMeaningLanguageCode),
                note.ShortDescription,
                ResolveTranslation(note.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                note.CefrLevel.ToString(),
                note.Category,
                note.Context,
                ResolveTranslation(note.ContextTranslationsJson, primaryMeaningLanguageCode),
                note.SensitivityWarning != null))
            .ToArray();
    }

    public async Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string targetLearningLanguageCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        string countryContextCode = ResolveDefaultCountryContextCode(targetLanguageCode);
        return await GetPublishedCountryGuidanceNoteBySlugAsync(normalizedSlug, targetLanguageCode, countryContextCode, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
    }

    public async Task<CountryGuidanceNoteDetailModel?> GetPublishedCountryGuidanceNoteBySlugAsync(string slug, string targetLearningLanguageCode, string countryContextCode, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        string normalizedCountryContextCode = NormalizeCountryContextCode(countryContextCode, targetLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        CountryGuidanceNote? note = await dbContext.CountryGuidanceNotes
            .AsNoTracking()
            .Where(item =>
                item.PublicationStatus == PublicationStatus.Active &&
                item.TargetLearningLanguageCode == targetLanguageCode &&
                item.CountryContextCode == normalizedCountryContextCode &&
                item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return note is null
            ? null
            : new CountryGuidanceNoteDetailModel(
                note.Slug,
                note.TargetLearningLanguageCode,
                note.CountryContextCode,
                note.Title,
                ResolveTranslation(note.TitleTranslationsJson, primaryMeaningLanguageCode),
                note.ShortDescription,
                ResolveTranslation(note.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                note.CefrLevel.ToString(),
                note.Category,
                note.Context,
                ResolveTranslation(note.ContextTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(note.SectionsJson),
                ResolveListTranslation(note.SectionsTranslationsJson, primaryMeaningLanguageCode),
                DeserializeExamples(note.ExamplesJson, note.ExamplesTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(note.DoNotesJson),
                ResolveListTranslation(note.DoNotesTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(note.DontNotesJson),
                ResolveListTranslation(note.DontNotesTranslationsJson, primaryMeaningLanguageCode),
                note.SensitivityWarning,
                ResolveTranslation(note.SensitivityWarningTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(note.LinkedDialogueSlugsJson),
                DeserializeStringArray(note.LinkedExpressionSlugsJson),
                DeserializeStringArray(note.LinkedWritingTemplateSlugsJson),
                DeserializeStringArray(note.LinkedTalkTopicSlugsJson),
                DeserializeStringArray(note.LinkedCourseLessonSlugsJson));
    }

    private static IQueryable<CountryGuidanceNote> ApplyFilters(IQueryable<CountryGuidanceNote> query, CountryGuidanceNoteListFilterModel filter)
    {
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(note => note.CefrLevel == cefrLevel);
        }

        string? category = Normalize(filter.Category);
        if (category is not null) query = query.Where(note => note.Category == category);
        string? context = NormalizeSearch(filter.Context);
        if (context is not null) query = query.Where(note => EF.Functions.ILike(note.Context, $"%{context}%"));
        string? search = NormalizeSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(note =>
                EF.Functions.ILike(note.Title, $"%{search}%") ||
                EF.Functions.ILike(note.ShortDescription, $"%{search}%") ||
                EF.Functions.ILike(note.Context, $"%{search}%") ||
                EF.Functions.ILike(note.Slug, $"%{search}%"));
        }

        return query;
    }

    private static string[] DeserializeStringArray(string json) =>
        JsonSerializer.Deserialize<string[]>(json) ?? [];

    private static CountryGuidanceNoteExampleModel[] DeserializeExamples(string json, string translationsJson, string? languageCode)
    {
        SourceExampleRow[] examples = JsonSerializer.Deserialize<SourceExampleRow[]>(json, JsonOptions) ?? [];
        ExampleTranslationRow[] translations = DeserializeExampleTranslations(translationsJson);
        string? normalizedLanguage = Normalize(languageCode);

        return examples
            .Select((example, index) => new CountryGuidanceNoteExampleModel(
                example.GermanText ?? string.Empty,
                example.Explanation,
                normalizedLanguage is null || index >= translations.Length
                    ? null
                    : translations[index].ExplanationTranslations.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Text))
            .ToArray();
    }

    private static ExampleTranslationRow[] DeserializeExampleTranslations(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<ExampleTranslationRow[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? ResolveTranslation(string json, string? languageCode)
    {
        string? normalizedLanguage = Normalize(languageCode);
        if (normalizedLanguage is null || string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return null;
        }

        try
        {
            TranslationRow[] rows = JsonSerializer.Deserialize<TranslationRow[]>(json, JsonOptions) ?? [];
            return rows.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Text;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string[] ResolveListTranslation(string json, string? languageCode)
    {
        string? normalizedLanguage = Normalize(languageCode);
        if (normalizedLanguage is null || string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return [];
        }

        try
        {
            ListTranslationRow[] rows = JsonSerializer.Deserialize<ListTranslationRow[]>(json, JsonOptions) ?? [];
            return rows.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Items ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string NormalizeRequiredLanguageCode(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : value.Trim().ToLowerInvariant();

    private static string ResolveDefaultCountryContextCode(string targetLearningLanguageCode) =>
        CountryContextCatalog.ResolveDefaultActiveCode(targetLearningLanguageCode);

    private static string NormalizeCountryContextCode(string value, string targetLearningLanguageCode)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? ResolveDefaultCountryContextCode(targetLearningLanguageCode) : value.Trim().ToUpperInvariant();
        if (CountryContextCatalog.TryFindActive(normalized, targetLearningLanguageCode, out CountryContextDefinition countryContext))
        {
            return countryContext.Code;
        }

        throw new DomainRuleException($"Country context '{value}' is not active for target learning language '{targetLearningLanguageCode}'.");
    }

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record TranslationRow(string Language, string Text);

    private sealed record ListTranslationRow(string Language, string[] Items);

    private sealed record SourceExampleRow(string? GermanText, string? Explanation);

    private sealed record ExampleTranslationRow(TranslationRow[] ExplanationTranslations);
}
