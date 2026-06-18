using System.Text.Json;
using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Catalog.Infrastructure.Repositories;

internal sealed class CourseRepository(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory) : ICourseRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<CoursePath> query = dbContext.CoursePaths
            .AsNoTracking()
            .Include(course => course.Modules)
            .ThenInclude(module => module.Lessons)
            .Where(course => course.PublicationStatus == PublicationStatus.Active);

        query = ApplyFilters(query, filter);

        List<CoursePath> courses = await query
            .OrderBy(course => course.SortOrder)
            .ThenBy(course => course.Title)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return courses
            .Select(course => new CoursePathListItemModel(
                course.Slug,
                course.Title,
                course.Description,
                ResolveTranslation(course.TitleTranslationsJson, primaryMeaningLanguageCode),
                ResolveTranslation(course.DescriptionTranslationsJson, primaryMeaningLanguageCode),
                course.CefrLevel.HasValue ? course.CefrLevel.Value.ToString() : null,
                course.CefrRange,
                course.Modules.Count(module => module.PublicationStatus == PublicationStatus.Active),
                course.Modules.SelectMany(module => module.Lessons).Count(lesson => lesson.PublicationStatus == PublicationStatus.Active)))
            .ToArray();
    }

    public async Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        CoursePath? course = await dbContext.CoursePaths
            .AsNoTracking()
            .Include(item => item.Modules)
            .ThenInclude(module => module.Lessons)
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (course is null)
        {
            return null;
        }

        CourseModuleModel[] modules = course.Modules
            .Where(module => module.PublicationStatus == PublicationStatus.Active)
            .OrderBy(module => module.SortOrder)
            .ThenBy(module => module.ModuleNumber)
            .Select(module => new CourseModuleModel(
                module.Slug,
                module.Title,
                module.Description,
                ResolveTranslation(module.TitleTranslationsJson, primaryMeaningLanguageCode),
                ResolveTranslation(module.DescriptionTranslationsJson, primaryMeaningLanguageCode),
                module.ModuleNumber,
                module.CefrLevel.ToString(),
                module.Lessons
                    .Where(lesson => lesson.PublicationStatus == PublicationStatus.Active)
                    .OrderBy(lesson => lesson.SortOrder)
                    .ThenBy(lesson => lesson.LessonNumber)
                    .Select(lesson => MapListItem(lesson, primaryMeaningLanguageCode))
                    .ToArray()))
            .ToArray();

        return new CoursePathDetailModel(
            course.Slug,
            course.Title,
            course.Description,
            ResolveTranslation(course.TitleTranslationsJson, primaryMeaningLanguageCode),
            ResolveTranslation(course.DescriptionTranslationsJson, primaryMeaningLanguageCode),
            course.CefrLevel.HasValue ? course.CefrLevel.Value.ToString() : null,
            course.CefrRange,
            modules);
    }

    public async Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, string? primaryMeaningLanguageCode, CancellationToken cancellationToken)
    {
        string normalizedSlug = Normalize(slug) ?? string.Empty;
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        CourseLesson? lesson = await dbContext.CourseLessons
            .AsNoTracking()
            .Where(item => item.PublicationStatus == PublicationStatus.Active && item.Slug == normalizedSlug)
            .SingleOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return lesson is null
            ? null
            : new CourseLessonDetailModel(
                lesson.Slug,
                lesson.CoursePathSlug,
                lesson.ModuleSlug,
                lesson.LessonNumber,
                lesson.Title,
                lesson.ShortDescription,
                lesson.Narrative,
                ResolveTranslation(lesson.TitleTranslationsJson, primaryMeaningLanguageCode),
                ResolveTranslation(lesson.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
                ResolveTranslation(lesson.NarrativeTranslationsJson, primaryMeaningLanguageCode),
                lesson.CefrLevel.ToString(),
                lesson.EstimatedMinutes,
                DeserializeStringArray(lesson.LearningGoalsJson),
                ResolveTextListTranslation(lesson.LearningGoalsTranslationsJson, primaryMeaningLanguageCode),
                DeserializeStringArray(lesson.PrerequisiteLessonSlugsJson),
                lesson.NextLessonSlug,
                DeserializeStringArray(lesson.LinkedGrammarTopicSlugsJson),
                DeserializeStringArray(lesson.LinkedWordSlugsJson),
                DeserializeStringArray(lesson.LinkedExpressionSlugsJson),
                DeserializeStringArray(lesson.LinkedDialogueSlugsJson),
                DeserializeStringArray(lesson.LinkedTalkTopicSlugsJson),
                DeserializeStringArray(lesson.LinkedExerciseSetSlugsJson),
                DeserializeStringArray(lesson.LinkedExamPrepSlugsJson),
                ResolveActivityBlocks(lesson.ActivityBlocksJson, primaryMeaningLanguageCode),
                lesson.ReviewSummary,
                ResolveTranslation(lesson.ReviewSummaryTranslationsJson, primaryMeaningLanguageCode),
                lesson.HomeworkTask,
                ResolveTranslation(lesson.HomeworkTaskTranslationsJson, primaryMeaningLanguageCode));
    }

    private static CourseLessonListItemModel MapListItem(CourseLesson lesson, string? primaryMeaningLanguageCode) =>
        new(
            lesson.Slug,
            lesson.CoursePathSlug,
            lesson.ModuleSlug,
            lesson.LessonNumber,
            lesson.Title,
            lesson.ShortDescription,
            ResolveTranslation(lesson.TitleTranslationsJson, primaryMeaningLanguageCode),
            ResolveTranslation(lesson.ShortDescriptionTranslationsJson, primaryMeaningLanguageCode),
            lesson.CefrLevel.ToString(),
            lesson.EstimatedMinutes);

    private static IQueryable<CoursePath> ApplyFilters(IQueryable<CoursePath> query, CoursePathListFilterModel filter)
    {
        if (Enum.TryParse(filter.CefrLevel, true, out CefrLevel cefrLevel))
        {
            query = query.Where(course => course.CefrLevel == cefrLevel || EF.Functions.ILike(course.CefrRange, $"%{cefrLevel}%"));
        }

        string? search = NormalizeSearch(filter.Query);
        if (search is not null)
        {
            query = query.Where(course => EF.Functions.ILike(course.Title, $"%{search}%") || EF.Functions.ILike(course.Description, $"%{search}%") || EF.Functions.ILike(course.Slug, $"%{search}%"));
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

    private static CourseLessonActivityBlockModel[] ResolveActivityBlocks(string json, string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return [];
        }

        try
        {
            ActivityBlockRow[] rows = JsonSerializer.Deserialize<ActivityBlockRow[]>(json, JsonOptions) ?? [];
            return rows
                .OrderBy(row => row.SortOrder)
                .Select(row => new CourseLessonActivityBlockModel(
                    row.Kind ?? string.Empty,
                    row.Title ?? string.Empty,
                    ResolveTranslation(row.TitleTranslations, languageCode),
                    row.Instruction ?? string.Empty,
                    ResolveTranslation(row.InstructionTranslations, languageCode),
                    row.TargetType ?? string.Empty,
                    row.TargetSlug,
                    row.EstimatedMinutes,
                    row.SortOrder,
                    row.IsRequired))
                .ToArray();
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? ResolveTranslation(TranslationRow[]? rows, string? languageCode)
    {
        string? normalizedLanguage = Normalize(languageCode);
        if (normalizedLanguage is null || rows is null || rows.Length == 0)
        {
            return null;
        }

        return rows.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Text;
    }

    private static string[] ResolveTextListTranslation(string json, string? languageCode)
    {
        string? normalizedLanguage = Normalize(languageCode);
        if (normalizedLanguage is null || string.IsNullOrWhiteSpace(json) || json == "[]")
        {
            return [];
        }

        try
        {
            TextListTranslationRow[] rows = JsonSerializer.Deserialize<TextListTranslationRow[]>(json, JsonOptions) ?? [];
            return rows.FirstOrDefault(row => string.Equals(row.Language, normalizedLanguage, StringComparison.OrdinalIgnoreCase))?.Texts ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record TranslationRow(string Language, string Text);

    private sealed record TextListTranslationRow(string Language, string[] Texts);

    private sealed record ActivityBlockRow(
        string? Kind,
        string? Title,
        TranslationRow[]? TitleTranslations,
        string? Instruction,
        TranslationRow[]? InstructionTranslations,
        string? TargetType,
        string? TargetSlug,
        int EstimatedMinutes,
        int SortOrder,
        bool IsRequired);
}
