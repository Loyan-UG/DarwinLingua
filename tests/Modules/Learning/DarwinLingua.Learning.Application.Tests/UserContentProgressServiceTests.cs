using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.DependencyInjection;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Learning.Application.Tests;

public sealed class UserContentProgressServiceTests
{
    [Fact]
    public async Task UpdateContentProgressAsync_ShouldCreateViewedProgress()
    {
        InMemoryUserContentProgressRepository repository = new();
        ServiceProvider serviceProvider = CreateServiceProvider(repository);
        IUserContentProgressService service = serviceProvider.GetRequiredService<IUserContentProgressService>();

        UserContentProgressModel result = await service.UpdateContentProgressAsync(
            "user-1",
            new UpdateUserContentProgressRequestModel("course-lesson", "a1-lesson-1", "viewed"),
            CancellationToken.None);

        Assert.Equal("viewed", result.State);
        Assert.Equal(1, result.ViewCount);
    }

    [Fact]
    public async Task UpdateContentProgressAsync_ShouldRejectUnsupportedOwnerType()
    {
        InMemoryUserContentProgressRepository repository = new();
        ServiceProvider serviceProvider = CreateServiceProvider(repository);
        IUserContentProgressService service = serviceProvider.GetRequiredService<IUserContentProgressService>();

        await Assert.ThrowsAsync<DomainRuleException>(() => service.UpdateContentProgressAsync(
            "user-1",
            new UpdateUserContentProgressRequestModel("unknown", "a1-lesson-1", "viewed"),
            CancellationToken.None));
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldSummarizeProgressStates()
    {
        InMemoryUserContentProgressRepository repository = new();
        ServiceProvider serviceProvider = CreateServiceProvider(repository);
        IUserContentProgressService service = serviceProvider.GetRequiredService<IUserContentProgressService>();

        await service.UpdateContentProgressAsync(
            "user-1",
            new UpdateUserContentProgressRequestModel("grammar-topic", "a1-articles", "completed"),
            CancellationToken.None);
        await service.UpdateContentProgressAsync(
            "user-1",
            new UpdateUserContentProgressRequestModel("expression", "guten-morgen", "needs-review"),
            CancellationToken.None);

        LearningProgressSummaryModel summary = await service.GetSummaryAsync("user-1", CancellationToken.None);

        Assert.Equal(2, summary.TotalTracked);
        Assert.Equal(1, summary.CompletedCount);
        Assert.Equal(1, summary.NeedsReviewCount);
    }

    [Fact]
    public async Task GetRecommendationsAsync_ShouldExcludeCompletedContent()
    {
        InMemoryUserContentProgressRepository repository = new();
        FakeLearningRecommendationCatalogReader recommendations = new();
        ServiceProvider serviceProvider = CreateServiceProvider(repository, recommendations);
        IUserContentProgressService service = serviceProvider.GetRequiredService<IUserContentProgressService>();

        await service.UpdateContentProgressAsync(
            "user-1",
            new UpdateUserContentProgressRequestModel("course-lesson", "a1-lesson-1", "completed"),
            CancellationToken.None);

        IReadOnlyList<LearningRecommendationModel> result = await service.GetRecommendationsAsync("user-1", 5, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("a1-lesson-2", result[0].ContentOwnerSlug);
    }

    private static ServiceProvider CreateServiceProvider(
        InMemoryUserContentProgressRepository repository,
        ILearningRecommendationCatalogReader? recommendationCatalogReader = null)
    {
        ServiceCollection services = new();
        services.AddLearningApplication();
        services.AddSingleton<IUserContentProgressRepository>(repository);
        services.AddSingleton(recommendationCatalogReader ?? new FakeLearningRecommendationCatalogReader());
        return services.BuildServiceProvider();
    }

    private sealed class InMemoryUserContentProgressRepository : IUserContentProgressRepository
    {
        private readonly List<UserContentProgress> _items = [];

        public Task<UserContentProgress?> GetByUserAndContentAsync(
            string userId,
            string contentOwnerType,
            string contentOwnerSlug,
            CancellationToken cancellationToken)
        {
            string normalizedType = contentOwnerType.Trim().ToLowerInvariant();
            string normalizedSlug = contentOwnerSlug.Trim().ToLowerInvariant();
            return Task.FromResult(_items.SingleOrDefault(
                item =>
                    item.UserId == userId &&
                    item.ContentOwnerType == normalizedType &&
                    item.ContentOwnerSlug == normalizedSlug));
        }

        public Task AddAsync(UserContentProgress progress, CancellationToken cancellationToken)
        {
            _items.Add(progress);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserContentProgress progress, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<UserContentProgress>> GetUserProgressAsync(
            string userId,
            int recentItemCount,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<UserContentProgress> result = _items
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.UpdatedAtUtc)
                .Take(recentItemCount)
                .ToArray();

            return Task.FromResult(result);
        }
    }

    private sealed class FakeLearningRecommendationCatalogReader : ILearningRecommendationCatalogReader
    {
        public Task<IReadOnlyList<LearningRecommendationModel>> GetDeterministicRecommendationsAsync(
            string userId,
            IReadOnlySet<string> completedContentKeys,
            int maxRecommendations,
            CancellationToken cancellationToken)
        {
            LearningRecommendationModel[] candidates =
            [
                new("next-course-lesson", "course-lesson", "a1-lesson-1", "Lesson 1", "/courses/a1/a1-lesson-1", "Continue.", "A1"),
                new("next-course-lesson", "course-lesson", "a1-lesson-2", "Lesson 2", "/courses/a1/a1-lesson-2", "Continue.", "A1"),
            ];

            IReadOnlyList<LearningRecommendationModel> result = candidates
                .Where(item => !completedContentKeys.Contains($"{item.ContentOwnerType}:{item.ContentOwnerSlug}"))
                .Take(maxRecommendations)
                .ToArray();

            return Task.FromResult(result);
        }
    }
}
