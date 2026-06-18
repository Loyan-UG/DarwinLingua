using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class CulturalNoteRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : ICulturalNoteRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<CulturalNote> query = dbContext.CulturalNotes.AsNoTracking().Where(note => note.PublicationStatus == PublicationStatus.Active);
        query = ApplyFilters(query, filter);

        List<CulturalNote> notes = await query
            .OrderBy(note => note.SortOrder)
            .ThenBy(note => note.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return notes
            .Select(note => new CulturalNoteListItemModel(
                note.Slug,
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

    public async Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        CulturalNote? note = await dbContext.CulturalNotes
            .AsNoTracking()
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return note is null
            ? null
            : new CulturalNoteDetailModel(
                note.Slug,
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

    private static IQueryable<CulturalNote> ApplyFilters(IQueryable<CulturalNote> query, CulturalNoteListFilterModel filter)
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

    private static CulturalNoteExampleModel[] DeserializeExamples(string json, string translationsJson, string? languageCode)
    {
        SourceExampleRow[] examples = JsonSerializer.Deserialize<SourceExampleRow[]>(json, JsonOptions) ?? [];
        ExampleTranslationRow[] translations = DeserializeExampleTranslations(translationsJson);
        string? normalizedLanguage = Normalize(languageCode);

        return examples
            .Select((example, index) => new CulturalNoteExampleModel(
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

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record TranslationRow(string Language, string Text);

    private sealed record ListTranslationRow(string Language, string[] Items);

    private sealed record SourceExampleRow(string? GermanText, string? Explanation);

    private sealed record ExampleTranslationRow(TranslationRow[] ExplanationTranslations);
}
