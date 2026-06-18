using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class WritingTemplateRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IWritingTemplateRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<WritingTemplateListItemModel>> GetPublishedWritingTemplatesAsync(WritingTemplateListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<WritingTemplate> query = dbContext.WritingTemplates.AsNoTracking().Where(template => template.PublicationStatus == PublicationStatus.Active);
        query = ApplyFilters(query, filter);

        List<WritingTemplate> templates = await query
            .OrderBy(template => template.SortOrder)
            .ThenBy(template => template.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return templates
            .Select(template => new WritingTemplateListItemModel(
                template.Slug,
                template.Title,
                ResolveTranslation(template.TitleTranslationsJson, primaryMeaningLanguageCode),
                template.ShortDescription,
                ResolveTranslation(template.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                template.CefrLevel.ToString(),
                template.Category,
                template.Situation,
                ResolveTranslation(template.SituationTranslationsJson, primaryMeaningLanguageCode),
                template.Register))
            .ToArray();
    }

    public async Task<WritingTemplateDetailModel?> GetPublishedWritingTemplateBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        WritingTemplate? template = await dbContext.WritingTemplates
            .AsNoTracking()
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return template is null
            ? null
            : new WritingTemplateDetailModel(
                template.Slug,
                template.Title,
                ResolveTranslation(template.TitleTranslationsJson, primaryMeaningLanguageCode),
                template.ShortDescription,
                ResolveTranslation(template.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                template.CefrLevel.ToString(),
                template.Category,
                template.Situation,
                ResolveTranslation(template.SituationTranslationsJson, primaryMeaningLanguageCode),
                template.Register,
                template.TemplateText,
                ResolveTranslation(template.TemplateTextTranslationsJson, primaryMeaningLanguageCode),
                template.Explanation,
                ResolveTranslation(template.ExplanationTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(template.VariablesJson),
                template.SampleFilledVersion,
                ResolveTranslation(template.SampleFilledVersionTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(template.LinkedGrammarTopicSlugsJson),
                DeserializeStringArray(template.LinkedWordSlugsJson),
                DeserializeStringArray(template.LinkedExpressionSlugsJson),
                DeserializeStringArray(template.LinkedExerciseSlugsJson),
                DeserializeStringArray(template.LinkedCourseLessonSlugsJson));
    }

    private static IQueryable<WritingTemplate> ApplyFilters(IQueryable<WritingTemplate> query, WritingTemplateListFilterModel filter)
    {
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(template => template.CefrLevel == cefrLevel);
        }

        string? category = Normalize(filter.Category);
        if (category is not null) query = query.Where(template => template.Category == category);
        string? register = Normalize(filter.Register);
        if (register is not null) query = query.Where(template => template.Register == register);
        string? situation = NormalizeSearch(filter.Situation);
        if (situation is not null) query = query.Where(template => EF.Functions.ILike(template.Situation, $"%{situation}%"));
        string? search = NormalizeSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(template =>
                EF.Functions.ILike(template.Title, $"%{search}%") ||
                EF.Functions.ILike(template.ShortDescription, $"%{search}%") ||
                EF.Functions.ILike(template.Situation, $"%{search}%") ||
                EF.Functions.ILike(template.Slug, $"%{search}%"));
        }

        return query;
    }

    private static string[] DeserializeStringArray(string json) =>
        JsonSerializer.Deserialize<string[]>(json) ?? [];

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

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record TranslationRow(string Language, string Text);
}
