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
    public async Task<IReadOnlyList<WritingTemplateListItemModel>> GetPublishedWritingTemplatesAsync(WritingTemplateListFilterModel filter, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<WritingTemplate> query = dbContext.WritingTemplates.AsNoTracking().Where(template => template.PublicationStatus == PublicationStatus.Active);
        query = ApplyFilters(query, filter);

        return await query
            .OrderBy(template => template.SortOrder)
            .ThenBy(template => template.Title)
            .Select(template => new WritingTemplateListItemModel(
                template.Slug,
                template.Title,
                template.ShortDescription,
                template.CefrLevel.ToString(),
                template.Category,
                template.Situation,
                template.Register))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<WritingTemplateDetailModel?> GetPublishedWritingTemplateBySlugAsync(string slug, CancellationToken cancellationToken)
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
                template.ShortDescription,
                template.CefrLevel.ToString(),
                template.Category,
                template.Situation,
                template.Register,
                template.TemplateText,
                template.Explanation,
                DeserializeStringArray(template.VariablesJson),
                template.SampleFilledVersion,
                DeserializeStringArray(template.LinkedGrammarTopicSlugsJson),
                DeserializeStringArray(template.LinkedWordSlugsJson),
                DeserializeStringArray(template.LinkedExpressionSlugsJson),
                DeserializeStringArray(template.LinkedExerciseSlugsJson));
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

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
