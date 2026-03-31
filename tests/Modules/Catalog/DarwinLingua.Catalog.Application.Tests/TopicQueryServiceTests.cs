using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.DependencyInjection;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Catalog.Domain.Entities;
using DarwinLingua.SharedKernel.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace DarwinLingua.Catalog.Application.Tests;

/// <summary>
/// Verifies the topic query application behavior.
/// </summary>
public sealed class TopicQueryServiceTests
{
    /// <summary>
    /// Verifies that the query service prefers the requested UI localization when available.
    /// </summary>
    [Fact]
    public async Task GetTopicsAsync_ShouldReturnRequestedLocalization()
    {
        Topic topic = new(Guid.NewGuid(), "housing", 20, true, DateTime.UtcNow);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("en"), "Housing", DateTime.UtcNow);
        topic.AddOrUpdateLocalization(Guid.NewGuid(), LanguageCode.From("de"), "Wohnen", DateTime.UtcNow);

        ServiceCollection services = new();
        services.AddCatalogApplication();
        services.AddSingleton<ITopicRepository>(new FakeTopicRepository([topic]));

        await using var serviceProvider = services.BuildServiceProvider();

        ITopicQueryService topicQueryService = serviceProvider.GetRequiredService<ITopicQueryService>();

        IReadOnlyList<TopicListItemModel> topics =
            await topicQueryService.GetTopicsAsync("de", CancellationToken.None);

        TopicListItemModel resolvedTopic = Assert.Single(topics);
        Assert.Equal("Wohnen", resolvedTopic.DisplayName);
    }

    /// <summary>
    /// Provides a fake repository for catalog application tests.
    /// </summary>
    /// <param name="topics">The topics returned by the fake repository.</param>
    private sealed class FakeTopicRepository(IReadOnlyList<Topic> topics) : ITopicRepository
    {
        /// <inheritdoc />
        public Task<IReadOnlyList<Topic>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(topics);
        }

        public Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesByIdsAsync(
            IReadOnlyCollection<Guid> topicIds,
            LanguageCode preferredLanguageCode,
            LanguageCode fallbackLanguageCode,
            CancellationToken cancellationToken)
        {
            IReadOnlyDictionary<Guid, string> values = topics
                .Where(topic => topicIds.Contains(topic.Id))
                .ToDictionary(
                    topic => topic.Id,
                    topic => topic.FindLocalization(preferredLanguageCode)?.DisplayName
                        ?? topic.FindLocalization(fallbackLanguageCode)?.DisplayName
                        ?? topic.Key);
            return Task.FromResult(values);
        }
    }
}
