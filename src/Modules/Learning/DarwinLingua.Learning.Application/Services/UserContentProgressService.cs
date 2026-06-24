using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Learning.Domain.Entities;
using DarwinLingua.SharedKernel.Exceptions;
using DarwinLingua.SharedKernel.Globalization;

namespace DarwinLingua.Learning.Application.Services;

internal sealed class UserContentProgressService(
    IUserContentProgressRepository repository,
    ILearningRecommendationCatalogReader recommendationCatalogReader) : IUserContentProgressService
{
    public async Task<LearningProgressSummaryModel> GetSummaryAsync(
        string userId,
        string targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        string normalizedUserId = NormalizeUserId(userId);
        string targetLanguageCode = NormalizeTargetLearningLanguageCode(targetLearningLanguageCode);
        IReadOnlyList<UserContentProgress> progressItems = await repository
            .GetUserProgressAsync(normalizedUserId, targetLanguageCode, 12, cancellationToken)
            .ConfigureAwait(false);

        return new LearningProgressSummaryModel(
            progressItems.Count,
            progressItems.Count(static item => item.State == "viewed"),
            progressItems.Count(static item => item.State == "in-progress"),
            progressItems.Count(static item => item.State == "completed"),
            progressItems.Count(static item => item.State == "needs-review"),
            progressItems
                .OrderByDescending(static item => item.UpdatedAtUtc)
                .Take(12)
                .Select(Map)
                .ToArray());
    }

    public async Task<UserContentProgressModel> UpdateContentProgressAsync(
        string userId,
        string targetLearningLanguageCode,
        UpdateUserContentProgressRequestModel request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string normalizedUserId = NormalizeUserId(userId);
        string targetLanguageCode = NormalizeTargetLearningLanguageCode(targetLearningLanguageCode);
        ValidateOwnerType(request.ContentOwnerType);
        ValidateState(request.State);

        UserContentProgress? progress = await repository
            .GetByUserAndContentAsync(
                normalizedUserId,
                targetLanguageCode,
                request.ContentOwnerType,
                request.ContentOwnerSlug,
                cancellationToken)
            .ConfigureAwait(false);

        if (progress is null)
        {
            progress = new UserContentProgress(
                Guid.NewGuid(),
                normalizedUserId,
                targetLanguageCode,
                request.ContentOwnerType,
                request.ContentOwnerSlug,
                DateTime.UtcNow);
            progress.ApplyState(request.State, DateTime.UtcNow);
            await repository.AddAsync(progress, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            progress.ApplyState(request.State, DateTime.UtcNow);
            await repository.UpdateAsync(progress, cancellationToken).ConfigureAwait(false);
        }

        return Map(progress);
    }

    public async Task<IReadOnlyList<LearningRecommendationModel>> GetRecommendationsAsync(
        string userId,
        string targetLearningLanguageCode,
        int maxRecommendations,
        CancellationToken cancellationToken)
    {
        string normalizedUserId = NormalizeUserId(userId);
        string targetLanguageCode = NormalizeTargetLearningLanguageCode(targetLearningLanguageCode);
        if (maxRecommendations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRecommendations), "Recommendation count must be positive.");
        }

        IReadOnlyList<UserContentProgress> progressItems = await repository
            .GetUserProgressAsync(normalizedUserId, targetLanguageCode, 1000, cancellationToken)
            .ConfigureAwait(false);
        HashSet<string> completedContentKeys = progressItems
            .Where(static item => item.State == "completed")
            .Select(static item => $"{item.ContentOwnerType}:{item.ContentOwnerSlug}")
            .ToHashSet(StringComparer.Ordinal);

        return await recommendationCatalogReader
            .GetDeterministicRecommendationsAsync(
                normalizedUserId,
                targetLanguageCode,
                completedContentKeys,
                maxRecommendations,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static UserContentProgressModel Map(UserContentProgress progress) =>
        new(
            progress.TargetLearningLanguageCode,
            progress.ContentOwnerType,
            progress.ContentOwnerSlug,
            progress.State,
            progress.FirstViewedAtUtc,
            progress.LastViewedAtUtc,
            progress.CompletedAtUtc,
            progress.ViewCount);

    private static string NormalizeUserId(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new DomainRuleException("User id is required for content progress.");
        }

        return userId.Trim();
    }

    private static string NormalizeTargetLearningLanguageCode(string targetLearningLanguageCode) =>
        TargetLearningLanguageScope.NormalizeOrDefault(
            targetLearningLanguageCode,
            "User content progress target learning language");

    private static void ValidateOwnerType(string ownerType)
    {
        string normalizedOwnerType = ownerType.Trim().ToLowerInvariant();
        if (!UserContentProgress.ValidOwnerTypes.Contains(normalizedOwnerType))
        {
            throw new DomainRuleException($"Unsupported content owner type '{ownerType}'.");
        }
    }

    private static void ValidateState(string state)
    {
        string normalizedState = state.Trim().ToLowerInvariant();
        if (!UserContentProgress.ValidStates.Contains(normalizedState))
        {
            throw new DomainRuleException($"Unsupported progress state '{state}'.");
        }
    }
}
