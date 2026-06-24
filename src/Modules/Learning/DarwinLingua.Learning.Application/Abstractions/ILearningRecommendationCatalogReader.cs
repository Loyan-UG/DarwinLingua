using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Learning.Application.Abstractions;

public interface ILearningRecommendationCatalogReader
{
    Task<IReadOnlyList<LearningRecommendationModel>> GetDeterministicRecommendationsAsync(
        string userId,
        string targetLearningLanguageCode,
        IReadOnlySet<string> completedContentKeys,
        int maxRecommendations,
        CancellationToken cancellationToken);
}
