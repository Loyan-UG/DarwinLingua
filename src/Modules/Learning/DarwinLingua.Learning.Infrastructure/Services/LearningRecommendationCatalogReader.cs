using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.SharedKernel.Content;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.SharedKernel.Lexicon;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Learning.Infrastructure.Services;

internal sealed class LearningRecommendationCatalogReader(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    : ILearningRecommendationCatalogReader
{
    public async Task<IReadOnlyList<LearningRecommendationModel>> GetDeterministicRecommendationsAsync(
        string userId,
        string targetLearningLanguageCode,
        IReadOnlySet<string> completedContentKeys,
        int maxRecommendations,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetLearningLanguageCode);
        ArgumentNullException.ThrowIfNull(completedContentKeys);
        string targetLanguageCode = targetLearningLanguageCode.Trim().ToLowerInvariant();

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<LearningRecommendationModel> recommendations = [];

        await AddWeakExerciseRecommendationsAsync(
                dbContext,
                userId,
                targetLanguageCode,
                completedContentKeys,
                maxRecommendations,
                recommendations,
                cancellationToken)
            .ConfigureAwait(false);

        if (recommendations.Count < maxRecommendations)
        {
            await AddDifficultWordRecommendationsAsync(
                    dbContext,
                    userId,
                    targetLanguageCode,
                    completedContentKeys,
                    maxRecommendations,
                    recommendations,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        CourseLesson[] courseLessons = await dbContext.CourseLessons
            .AsNoTracking()
            .Where(lesson =>
                lesson.PublicationStatus == PublicationStatus.Active &&
                lesson.TargetLearningLanguageCode == targetLanguageCode)
            .OrderBy(static lesson => lesson.SortOrder)
            .ThenBy(static lesson => lesson.LessonNumber)
            .Take(maxRecommendations * 2)
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (CourseLesson lesson in courseLessons)
        {
            if (recommendations.Count >= maxRecommendations)
            {
                break;
            }

            if (ContainsRecommendation(recommendations, "course-lesson", lesson.Slug))
            {
                continue;
            }

            string key = $"course-lesson:{lesson.Slug}";
            if (completedContentKeys.Contains(key))
            {
                continue;
            }

            recommendations.Add(new LearningRecommendationModel(
                "next-course-lesson",
                "course-lesson",
                lesson.Slug,
                lesson.Title,
                $"/learn/{targetLanguageCode}/courses/{lesson.CoursePathSlug}/{lesson.Slug}",
                "Continue with the next available course lesson.",
                lesson.CefrLevel.ToString()));
        }

        if (recommendations.Count < maxRecommendations)
        {
            GrammarTopic[] grammarTopics = await dbContext.GrammarTopics
                .AsNoTracking()
                .Where(topic =>
                    topic.PublicationStatus == PublicationStatus.Active &&
                    topic.TargetLearningLanguageCode == targetLanguageCode)
                .OrderBy(static topic => topic.SortOrder)
                .Take(maxRecommendations * 2)
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            foreach (GrammarTopic topic in grammarTopics)
            {
                if (recommendations.Count >= maxRecommendations)
                {
                    break;
                }

                if (ContainsRecommendation(recommendations, "grammar-topic", topic.Slug))
                {
                    continue;
                }

                string key = $"grammar-topic:{topic.Slug}";
                if (completedContentKeys.Contains(key))
                {
                    continue;
                }

                recommendations.Add(new LearningRecommendationModel(
                    "grammar-not-completed",
                    "grammar-topic",
                    topic.Slug,
                    topic.Title,
                    $"/learn/{targetLanguageCode}/grammar/{topic.Slug}",
                    "Review a grammar topic that is not completed yet.",
                    topic.CefrLevel.ToString()));
            }
        }

        return recommendations;
    }

    private static async Task AddWeakExerciseRecommendationsAsync(
        DarwinLinguaDbContext dbContext,
        string userId,
        string targetLanguageCode,
        IReadOnlySet<string> completedContentKeys,
        int maxRecommendations,
        List<LearningRecommendationModel> recommendations,
        CancellationToken cancellationToken)
    {
        UserExerciseAttemptRow[] recentAttempts = await dbContext.UserExerciseAttempts
            .AsNoTracking()
            .Where(attempt =>
                attempt.UserId == userId &&
                attempt.TargetLearningLanguageCode == targetLanguageCode)
            .OrderByDescending(attempt => attempt.AttemptedAtUtc)
            .ThenByDescending(attempt => attempt.CreatedAtUtc)
            .Select(attempt => new UserExerciseAttemptRow(
                attempt.ExerciseSlug,
                attempt.IsCorrect,
                attempt.AttemptedAtUtc))
            .Take(Math.Max(maxRecommendations * 12, 24))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        UserExerciseAttemptRow[] latestWeakAttempts = recentAttempts
            .GroupBy(static attempt => attempt.ExerciseSlug, StringComparer.Ordinal)
            .Select(static group => group.First())
            .Where(static attempt => !attempt.IsCorrect)
            .Take(maxRecommendations)
            .ToArray();
        if (latestWeakAttempts.Length == 0)
        {
            return;
        }

        string[] weakExerciseSlugs = latestWeakAttempts
            .Select(static attempt => attempt.ExerciseSlug)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Dictionary<string, Exercise> exercisesBySlug = await dbContext.Exercises
            .AsNoTracking()
            .Where(exercise =>
                weakExerciseSlugs.Contains(exercise.Slug) &&
                exercise.TargetLearningLanguageCode == targetLanguageCode &&
                exercise.PublicationStatus == PublicationStatus.Active)
            .ToDictionaryAsync(exercise => exercise.Slug, StringComparer.Ordinal, cancellationToken)
            .ConfigureAwait(false);

        foreach (UserExerciseAttemptRow attempt in latestWeakAttempts)
        {
            if (recommendations.Count >= maxRecommendations)
            {
                break;
            }

            if (!exercisesBySlug.TryGetValue(attempt.ExerciseSlug, out Exercise? exercise))
            {
                continue;
            }

            if (completedContentKeys.Contains($"exercise:{exercise.Slug}") ||
                ContainsRecommendation(recommendations, "exercise", exercise.Slug))
            {
                continue;
            }

            recommendations.Add(new LearningRecommendationModel(
                "weak-exercise",
                "exercise",
                exercise.Slug,
                exercise.Title,
                $"/learn/{targetLanguageCode}/exercises/{exercise.Slug}",
                "Repeat this exercise because your latest saved attempt was not correct.",
                exercise.CefrLevel.ToString()));
        }
    }

    private static async Task AddDifficultWordRecommendationsAsync(
        DarwinLinguaDbContext dbContext,
        string userId,
        string targetLanguageCode,
        IReadOnlySet<string> completedContentKeys,
        int maxRecommendations,
        List<LearningRecommendationModel> recommendations,
        CancellationToken cancellationToken)
    {
        LanguageCode targetLanguage = LanguageCode.From(targetLanguageCode);
        DifficultWordRow[] difficultWords = await (
                from state in dbContext.UserWordStates.AsNoTracking()
                join word in dbContext.WordEntries.AsNoTracking()
                    on state.WordEntryPublicId equals word.PublicId
                where
                    state.UserId == userId &&
                    state.IsDifficult &&
                    !state.IsKnown &&
                    word.LanguageCode == targetLanguage &&
                    word.PublicationStatus == PublicationStatus.Active
                orderby state.UpdatedAtUtc descending
                select new DifficultWordRow(
                    word.PublicId,
                    word.Lemma,
                    word.Article,
                    word.PrimaryCefrLevel.ToString(),
                    state.UpdatedAtUtc))
            .Take(Math.Max(maxRecommendations * 2, 8))
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        foreach (DifficultWordRow word in difficultWords)
        {
            if (recommendations.Count >= maxRecommendations)
            {
                break;
            }

            string wordSlug = LemmaUrlSlug.FromLemma(word.Lemma);
            if (completedContentKeys.Contains($"word:{wordSlug}") ||
                completedContentKeys.Contains($"word:{word.PublicId}") ||
                ContainsRecommendation(recommendations, "word", wordSlug))
            {
                continue;
            }

            string title = string.IsNullOrWhiteSpace(word.Article)
                ? word.Lemma
                : $"{word.Article} {word.Lemma}";
            recommendations.Add(new LearningRecommendationModel(
                "difficult-word",
                "word",
                wordSlug,
                title,
                $"/learn/{targetLanguageCode}/words/{wordSlug}",
                "Review this word because you marked it as difficult.",
                word.CefrLevel));
        }
    }

    private static bool ContainsRecommendation(
        IReadOnlyCollection<LearningRecommendationModel> recommendations,
        string contentOwnerType,
        string contentOwnerSlug) =>
        recommendations.Any(recommendation =>
            string.Equals(recommendation.ContentOwnerType, contentOwnerType, StringComparison.Ordinal) &&
            string.Equals(recommendation.ContentOwnerSlug, contentOwnerSlug, StringComparison.Ordinal));

    private sealed record UserExerciseAttemptRow(string ExerciseSlug, bool IsCorrect, DateTime AttemptedAtUtc);

    private sealed record DifficultWordRow(Guid PublicId, string Lemma, string? Article, string CefrLevel, DateTime UpdatedAtUtc);
}
