using DarwinLingua.Learning.Application.Models;

namespace DarwinLingua.Learning.Application.Abstractions;

public interface IUserContentProgressService
{
    Task<LearningProgressSummaryModel> GetSummaryAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<UserContentProgressModel> UpdateContentProgressAsync(
        string userId,
        UpdateUserContentProgressRequestModel request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<LearningRecommendationModel>> GetRecommendationsAsync(
        string userId,
        int maxRecommendations,
        CancellationToken cancellationToken);
}
