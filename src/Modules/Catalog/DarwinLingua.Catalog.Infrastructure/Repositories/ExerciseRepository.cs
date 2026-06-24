using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class ExerciseRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : IExerciseRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<ExerciseSetListItemModel>> GetPublishedExerciseSetsAsync(ExerciseSetListFilterModel filter, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        LanguageCode primaryLanguage = ResolveRequestedLanguage(primaryMeaningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<ExerciseSet> query = dbContext.ExerciseSets
            .AsNoTracking()
            .Include(set => set.Items)
            .Where(set =>
                set.PublicationStatus == PublicationStatus.Active &&
                set.TargetLearningLanguageCode == targetLanguageCode);
        query = ApplySetFilters(query, filter);

        List<ExerciseSet> sets = await query.OrderBy(set => set.SortOrder).ThenBy(set => set.Title)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return sets.Select(set => MapSetListItem(set, primaryLanguage)).ToArray();
    }

    public async Task<ExerciseSetDetailModel?> GetPublishedExerciseSetBySlugAsync(string slug, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        LanguageCode primaryLanguage = ResolveRequestedLanguage(primaryMeaningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        ExerciseSet? set = await dbContext.ExerciseSets.AsNoTracking().Include(item => item.Items)
            .Where(item =>
                item.PublicationStatus == PublicationStatus.Active &&
                item.TargetLearningLanguageCode == targetLanguageCode &&
                item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (set is null)
        {
            return null;
        }

        string[] exerciseSlugs = set.Items.OrderBy(item => item.SortOrder).Select(item => item.ExerciseSlug).ToArray();
        List<Exercise> exercises = await dbContext.Exercises.AsNoTracking()
            .Where(exercise =>
                exercise.PublicationStatus == PublicationStatus.Active &&
                exercise.TargetLearningLanguageCode == targetLanguageCode &&
                exerciseSlugs.Contains(exercise.Slug))
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        ExerciseListItemModel[] exerciseModels = exerciseSlugs
            .Select(slugValue => exercises.SingleOrDefault(exercise => exercise.Slug == slugValue))
            .Where(exercise => exercise is not null)
            .Select(exercise => MapListItem(exercise!, primaryLanguage))
            .ToArray();

        return new ExerciseSetDetailModel(
            set.Slug,
            set.Title,
            ResolveText(set.TitleTranslationsJson, primaryLanguage),
            set.Description,
            ResolveText(set.DescriptionTranslationsJson, primaryLanguage),
            set.CefrLevel.ToString(),
            set.OwnerType,
            set.OwnerSlug,
            exerciseModels);
    }

    public async Task<IReadOnlyList<ExerciseListItemModel>> GetPublishedExercisesAsync(ExerciseListFilterModel filter, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        LanguageCode primaryLanguage = ResolveRequestedLanguage(primaryMeaningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<Exercise> query = dbContext.Exercises
            .AsNoTracking()
            .Where(exercise =>
                exercise.PublicationStatus == PublicationStatus.Active &&
                exercise.TargetLearningLanguageCode == targetLanguageCode);
        query = ApplyExerciseFilters(query, filter);

        List<Exercise> exercises = await query.OrderBy(exercise => exercise.SortOrder).ThenBy(exercise => exercise.Title)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return exercises.Select(exercise => MapListItem(exercise, primaryLanguage)).ToArray();
    }

    public async Task<ExerciseDetailModel?> GetPublishedExerciseBySlugAsync(string slug, string targetLearningLanguageCode, string primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        LanguageCode primaryLanguage = ResolveRequestedLanguage(primaryMeaningLanguageCode);
        Exercise? exercise = await GetPublishedExerciseEntityBySlugAsync(slug, targetLearningLanguageCode, cancellationToken).ConfigureAwait(false);
        return exercise is null
            ? null
            : new ExerciseDetailModel(
                exercise.Slug,
                exercise.Title,
                ResolveText(exercise.TitleTranslationsJson, primaryLanguage),
                exercise.Instruction,
                ResolveText(exercise.InstructionTranslationsJson, primaryLanguage),
                exercise.CefrLevel.ToString(),
                exercise.ExerciseType,
                exercise.TargetSkill,
                exercise.OwnerType,
                exercise.OwnerSlug,
                exercise.PromptJson,
                exercise.Hint,
                ResolveText(exercise.HintTranslationsJson, primaryLanguage));
    }

    public async Task<Exercise?> GetPublishedExerciseEntityBySlugAsync(string slug, string targetLearningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = slug.Trim().ToLowerInvariant();
        string targetLanguageCode = NormalizeRequiredLanguageCode(targetLearningLanguageCode);
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return await dbContext.Exercises.AsNoTracking()
            .Where(exercise =>
                exercise.PublicationStatus == PublicationStatus.Active &&
                exercise.TargetLearningLanguageCode == targetLanguageCode &&
                exercise.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveAttemptAsync(UserExerciseAttempt attempt, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        dbContext.UserExerciseAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static ExerciseSetListItemModel MapSetListItem(ExerciseSet set, LanguageCode primaryLanguage) =>
        new(
            set.Slug,
            set.Title,
            ResolveText(set.TitleTranslationsJson, primaryLanguage),
            set.Description,
            ResolveText(set.DescriptionTranslationsJson, primaryLanguage),
            set.CefrLevel.ToString(),
            set.OwnerType,
            set.OwnerSlug,
            set.Items.Count);

    private static ExerciseListItemModel MapListItem(Exercise exercise, LanguageCode primaryLanguage) =>
        new(
            exercise.Slug,
            exercise.Title,
            ResolveText(exercise.TitleTranslationsJson, primaryLanguage),
            exercise.Instruction,
            ResolveText(exercise.InstructionTranslationsJson, primaryLanguage),
            exercise.CefrLevel.ToString(),
            exercise.ExerciseType,
            exercise.TargetSkill,
            exercise.OwnerType,
            exercise.OwnerSlug);

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

    private static string NormalizeRequiredLanguageCode(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
            : value.Trim().ToLowerInvariant();
    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? ResolveText(string json, LanguageCode primaryLanguage)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            StoredTranslation[] translations = JsonSerializer.Deserialize<StoredTranslation[]>(json, JsonOptions) ?? [];
            return translations
                .Where(translation => TryResolveLanguage(translation.Language) == primaryLanguage)
                .Select(translation => translation.Text)
                .FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static LanguageCode ResolveRequestedLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return LanguageCode.From("en");
        }

        try
        {
            return LanguageCode.From(languageCode.Trim().ToLowerInvariant());
        }
        catch
        {
            return LanguageCode.From("en");
        }
    }

    private static LanguageCode? TryResolveLanguage(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return null;
        }

        try
        {
            return LanguageCode.From(languageCode.Trim().ToLowerInvariant());
        }
        catch
        {
            return null;
        }
    }

    private sealed record StoredTranslation(string? Language, string? Text);
}
