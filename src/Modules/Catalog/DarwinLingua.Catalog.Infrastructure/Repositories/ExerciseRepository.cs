using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ExerciseRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IExerciseRepository
{
    public async Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<ExerciseSet> query = dbContext.ExerciseSets.AsNoTracking().Include(set => set.Items).Where(set => set.PublicationStatus == PublicationStatus.Active);
        query = ApplySetFilters(query, filter);

        return await query.OrderBy(set => set.SortOrder).ThenBy(set => set.Title)
            .Select(set => new ExerciseSetListItemModel(set.Slug, set.Title, set.Description, set.CefrLevel.ToString(), set.OwnerType, set.OwnerSlug, set.Items.Count))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ExerciseSet? set = await dbContext.ExerciseSets.AsNoTracking().Include(item => item.Items)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (set is null)
        {
            return null;
        }

        string[] exerciseSlugs = set.Items.OrderBy(item => item.SortOrder).Select(item => item.ExerciseSlug).ToArray();
        List<Exercise> exercises = await dbContext.Exercises.AsNoTracking()
            .Where(exercise => exercise.PublicationStatus == PublicationStatus.Active && exerciseSlugs.Contains(exercise.Slug))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        ExerciseListItemModel[] exerciseModels = exerciseSlugs
            .Select(slugValue => exercises.SingleOrDefault(exercise => exercise.Slug == slugValue))
            .Where(exercise => exercise is not null)
            .Select(exercise => MapListItem(exercise!))
            .ToArray();

        return new ExerciseSetDetailModel(set.Slug, set.Title, set.Description, set.CefrLevel.ToString(), set.OwnerType, set.OwnerSlug, exerciseModels);
    }

    public async Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<Exercise> query = dbContext.Exercises.AsNoTracking().Where(exercise => exercise.PublicationStatus == PublicationStatus.Active);
        query = ApplyExerciseFilters(query, filter);

        return await query.OrderBy(exercise => exercise.SortOrder).ThenBy(exercise => exercise.Title)
            .Select(exercise => MapListItem(exercise))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        Exercise? exercise = await GetPublishedExerciseEntityBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return exercise is null
            ? null
            : new ExerciseDetailModel(exercise.Slug, exercise.Title, exercise.Instruction, exercise.CefrLevel.ToString(), exercise.ExerciseType, exercise.TargetSkill, exercise.OwnerType, exercise.OwnerSlug, exercise.PromptJson, exercise.Hint);
    }

    public async Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await dbContext.Exercises.AsNoTracking()
            .Where(exercise => exercise.PublicationStatus == PublicationStatus.Active && exercise.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveAttemptAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        dbContext.UserExerciseAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ExerciseListItemModel MapListItem(Exercise exercise) =>
        new(exercise.Slug, exercise.Title, exercise.Instruction, exercise.CefrLevel.ToString(), exercise.ExerciseType, exercise.TargetSkill, exercise.OwnerType, exercise.OwnerSlug);

    private static IQueryable<ExerciseSet> ApplySetFilters(IQueryable<ExerciseSet> query, ExerciseSetListFilterModel filter)
    {
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel)) query = query.Where(set => set.CefrLevel == cefrLevel);
        string? ownerType = Normalize(filter.OwnerType);
        if (ownerType is not null) query = query.Where(set => set.OwnerType == ownerType);
        string? ownerSlug = Normalize(filter.OwnerSlug);
        if (ownerSlug is not null) query = query.Where(set => set.OwnerSlug == ownerSlug);
        string? search = NormalizeSearch(filter.Query);
        if (search is not null) query = query.Where(set => EF.Functions.ILike(set.Title, $"%{search}%") || EF.Functions.ILike(set.Description, $"%{search}%") || EF.Functions.ILike(set.Slug, $"%{search}%"));
        return query;
    }

    private static IQueryable<Exercise> ApplyExerciseFilters(IQueryable<Exercise> query, ExerciseListFilterModel filter)
    {
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel)) query = query.Where(exercise => exercise.CefrLevel == cefrLevel);
        string? exerciseType = Normalize(filter.ExerciseType);
        if (exerciseType is not null) query = query.Where(exercise => exercise.ExerciseType == exerciseType);
        string? targetSkill = Normalize(filter.TargetSkill);
        if (targetSkill is not null) query = query.Where(exercise => exercise.TargetSkill == targetSkill);
        string? ownerType = Normalize(filter.OwnerType);
        if (ownerType is not null) query = query.Where(exercise => exercise.OwnerType == ownerType);
        string? ownerSlug = Normalize(filter.OwnerSlug);
        if (ownerSlug is not null) query = query.Where(exercise => exercise.OwnerSlug == ownerSlug);
        string? search = NormalizeSearch(filter.Query);
        if (search is not null) query = query.Where(exercise => EF.Functions.ILike(exercise.Title, $"%{search}%") || EF.Functions.ILike(exercise.Instruction, $"%{search}%") || EF.Functions.ILike(exercise.Slug, $"%{search}%"));
        return query;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
