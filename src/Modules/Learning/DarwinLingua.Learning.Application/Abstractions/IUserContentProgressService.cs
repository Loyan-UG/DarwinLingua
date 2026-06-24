using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Learning.Application.Abstractions;

public interface IUserContentProgressService
{
    Task<LearningProgressSummaryModel> GetSummaryAsync(
        string userId,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken);

    Task<UserContentProgressModel> UpdateContentProgressAsync(
        string userId,
        string targetLearningLanguageCode,
        UpdateUserContentProgressRequestModel request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LearningRecommendationModel>> GetRecommendationsAsync(
        string userId,
        string targetLearningLanguageCode,
        int maxRecommendations,
        CancellationToken cancellationToken);
}
