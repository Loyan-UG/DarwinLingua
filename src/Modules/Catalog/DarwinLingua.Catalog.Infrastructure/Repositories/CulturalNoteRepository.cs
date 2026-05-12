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
    public async Task<IReadOnlyList<CulturalNoteListItemModel>> GetPublishedCulturalNotesAsync(CulturalNoteListFilterModel filter, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<CulturalNote> query = dbContext.CulturalNotes.AsNoTracking().Where(note => note.PublicationStatus == PublicationStatus.Active);
        query = ApplyFilters(query, filter);

        return await query
            .OrderBy(note => note.SortOrder)
            .ThenBy(note => note.Title)
            .Select(note => new CulturalNoteListItemModel(
                note.Slug,
                note.Title,
                note.ShortDescription,
                note.CefrLevel.ToString(),
                note.Category,
                note.Context,
                note.SensitivityWarning != null))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<CulturalNoteDetailModel?> GetPublishedCulturalNoteBySlugAsync(string slug, CancellationToken cancellationToken)
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
                note.ShortDescription,
                note.CefrLevel.ToString(),
                note.Category,
                note.Context,
                DeserializeStringArray(note.SectionsJson),
                DeserializeExamples(note.ExamplesJson),
                DeserializeStringArray(note.DoNotesJson),
                DeserializeStringArray(note.DontNotesJson),
                note.SensitivityWarning,
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

    private static CulturalNoteExampleModel[] DeserializeExamples(string json) =>
        JsonSerializer.Deserialize<CulturalNoteExampleModel[]>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
