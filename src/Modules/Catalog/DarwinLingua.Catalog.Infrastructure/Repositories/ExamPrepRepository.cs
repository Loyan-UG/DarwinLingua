using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ExamPrepRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IExamPrepRepository
{
    public async Task<IReadOnlyList<ExamProfileModel>> GetPublishedExamProfilesAsync(CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await dbContext.ExamProfiles
            .AsNoTracking()
            .Where(profile => profile.PublicationStatus == PublicationStatus.Active)
            .OrderBy(profile => profile.SortOrder)
            .ThenBy(profile => profile.DisplayName)
            .Select(profile => new ExamProfileModel(profile.Key, profile.DisplayName, profile.CefrRange, profile.Description))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ExamPrepUnitListItemModel>> GetPublishedExamPrepUnitsAsync(ExamPrepListFilterModel filter, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<ExamPrepUnit> query = dbContext.ExamPrepUnits.AsNoTracking().Where(unit => unit.PublicationStatus == PublicationStatus.Active);
        query = ApplyFilters(query, filter);

        return await query
            .OrderBy(unit => unit.SortOrder)
            .ThenBy(unit => unit.Title)
            .Select(unit => new ExamPrepUnitListItemModel(unit.Slug, unit.ExamProfileKey, unit.Title, unit.ShortDescription, unit.CefrLevel.ToString(), unit.ExamSection, unit.TaskType, unit.SkillFocus))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<ExamPrepUnitDetailModel?> GetPublishedExamPrepUnitBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ExamPrepUnit? unit = await dbContext.ExamPrepUnits
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug, cancellationToken)
            .ConfigureAwait(false);

        return unit is null
            ? null
            : new ExamPrepUnitDetailModel(
                unit.Slug,
                unit.ExamProfileKey,
                unit.Title,
                unit.ShortDescription,
                unit.CefrLevel.ToString(),
                unit.ExamSection,
                unit.TaskType,
                unit.SkillFocus,
                unit.Explanation,
                DeserializeStringArray(unit.StrategyNotesJson),
                DeserializeStringArray(unit.ChecklistJson),
                DeserializeStringArray(unit.LinkedDialogueSlugsJson),
                DeserializeStringArray(unit.LinkedTalkTopicSlugsJson),
                DeserializeStringArray(unit.LinkedGrammarTopicSlugsJson),
                DeserializeStringArray(unit.LinkedExpressionSlugsJson),
                DeserializeStringArray(unit.LinkedWritingTemplateSlugsJson),
                DeserializeStringArray(unit.LinkedExerciseSlugsJson),
                DeserializeStringArray(unit.LinkedCourseLessonSlugsJson));
    }

    private static IQueryable<ExamPrepUnit> ApplyFilters(IQueryable<ExamPrepUnit> query, ExamPrepListFilterModel filter)
    {
        string? profile = Normalize(filter.ExamProfile);
        if (profile is not null) query = query.Where(unit => unit.ExamProfileKey == profile);
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel)) query = query.Where(unit => unit.CefrLevel == cefrLevel);
        string? skill = Normalize(filter.SkillFocus);
        if (skill is not null) query = query.Where(unit => unit.SkillFocus == skill);
        string? taskType = Normalize(filter.TaskType);
        if (taskType is not null) query = query.Where(unit => unit.TaskType == taskType);
        string? section = Normalize(filter.Section);
        if (section is not null) query = query.Where(unit => unit.ExamSection == section);
        string? search = NormalizeSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(unit =>
                EF.Functions.ILike(unit.Title, $"%{search}%") ||
                EF.Functions.ILike(unit.ShortDescription, $"%{search}%") ||
                EF.Functions.ILike(unit.Explanation, $"%{search}%") ||
                EF.Functions.ILike(unit.Slug, $"%{search}%"));
        }

        return query;
    }

    private static string[] DeserializeStringArray(string json) => JsonSerializer.Deserialize<string[]>(json) ?? [];

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
