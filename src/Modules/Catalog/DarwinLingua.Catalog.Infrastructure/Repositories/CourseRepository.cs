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
    public async Task<IReadOnlyList<CoursePathListItemModel>> GetPublishedCoursePathsAsync(CoursePathListFilterModel filter, CancellationToken cancellationToken)
    {
        await using DarwinLinguaDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        IQueryable<CoursePath> query = dbContext.CoursePaths
            .AsNoTracking()
            .Include(course => course.Modules)
            .ThenInclude(module => module.Lessons)
            .Where(course => course.PublicationStatus == PublicationStatus.Active);

        query = ApplyFilters(query, filter);

        return await query
            .OrderBy(course => course.SortOrder)
            .ThenBy(course => course.Title)
            .Select(course => new CoursePathListItemModel(
                course.Slug,
                course.Title,
                course.Description,
                course.CefrLevel.HasValue ? course.CefrLevel.Value.ToString() : null,
                course.CefrRange,
                course.Modules.Count(module => module.PublicationStatus == PublicationStatus.Active),
                course.Modules.SelectMany(module => module.Lessons).Count(lesson => lesson.PublicationStatus == PublicationStatus.Active)))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<CoursePathDetailModel?> GetPublishedCoursePathBySlugAsync(string slug, CancellationToken cancellationToken)
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
                module.ModuleNumber,
                module.CefrLevel.ToString(),
                module.Lessons
                    .Where(lesson => lesson.PublicationStatus == PublicationStatus.Active)
                    .OrderBy(lesson => lesson.SortOrder)
                    .ThenBy(lesson => lesson.LessonNumber)
                    .Select(MapListItem)
                    .ToArray()))
            .ToArray();

        return new CoursePathDetailModel(
            course.Slug,
            course.Title,
            course.Description,
            course.CefrLevel.HasValue ? course.CefrLevel.Value.ToString() : null,
            course.CefrRange,
            modules);
    }

    public async Task<CourseLessonDetailModel?> GetPublishedCourseLessonBySlugAsync(string slug, CancellationToken cancellationToken)
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
                lesson.CefrLevel.ToString(),
                lesson.EstimatedMinutes,
                DeserializeStringArray(lesson.LearningGoalsJson),
                DeserializeStringArray(lesson.PrerequisiteLessonSlugsJson),
                lesson.NextLessonSlug,
                DeserializeStringArray(lesson.LinkedGrammarTopicSlugsJson),
                DeserializeStringArray(lesson.LinkedWordSlugsJson),
                DeserializeStringArray(lesson.LinkedExpressionSlugsJson),
                DeserializeStringArray(lesson.LinkedDialogueSlugsJson),
                DeserializeStringArray(lesson.LinkedTalkTopicSlugsJson),
                DeserializeStringArray(lesson.LinkedExerciseSetSlugsJson),
                DeserializeStringArray(lesson.LinkedExamPrepSlugsJson),
                lesson.ReviewSummary,
                lesson.HomeworkTask);
    }

    private static CourseLessonListItemModel MapListItem(CourseLesson lesson) =>
        new(lesson.Slug, lesson.CoursePathSlug, lesson.ModuleSlug, lesson.LessonNumber, lesson.Title, lesson.ShortDescription, lesson.CefrLevel.ToString(), lesson.EstimatedMinutes);

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

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeSearch(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
