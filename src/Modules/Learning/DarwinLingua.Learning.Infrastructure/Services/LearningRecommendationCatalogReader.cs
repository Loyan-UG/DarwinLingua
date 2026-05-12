using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.Infrastructure.Persistence;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.SharedKernel.Content;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Learning.Infrastructure.Services;

internal sealed class LearningRecommendationCatalogReader(IDbContextFactory<DarwinLinguaDbContext> dbContextFactory)
    : ILearningRecommendationCatalogReader
{
    public async Task<IReadOnlyList<LearningRecommendationModel>> GetDeterministicRecommendationsAsync(
        string userId,
        IReadOnlySet<string> completedContentKeys,
        int maxRecommendations,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(completedContentKeys);

        await using DarwinLinguaDbContext dbContext = await dbContextFactory
            .CreateDbContextAsync(cancellationToken)
            .ConfigureAwait(false);

        List<LearningRecommendationModel> recommendations = [];

        CourseLesson[] courseLessons = await dbContext.CourseLessons
            .AsNoTracking()
            .Where(static lesson => lesson.PublicationStatus == PublicationStatus.Active)
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
                $"/courses/{lesson.CoursePathSlug}/{lesson.Slug}",
                "Continue with the next available course lesson.",
                lesson.CefrLevel.ToString()));
        }

        if (recommendations.Count < maxRecommendations)
        {
            GrammarTopic[] grammarTopics = await dbContext.GrammarTopics
                .AsNoTracking()
                .Where(static topic => topic.PublicationStatus == PublicationStatus.Active)
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
                    $"/grammar/{topic.Slug}",
                    "Review a grammar topic that is not completed yet.",
                    topic.CefrLevel.ToString()));
            }
        }

        return recommendations;
    }
}
