namespace DarwinLingua.Learning.Application.Models;

public sealed record UserContentProgressModel(
    string TargetLearningLanguageCode,
    string ContentOwnerType,
    string ContentOwnerSlug,
    string State,
    DateTime? FirstViewedAtUtc,
    DateTime? LastViewedAtUtc,
    DateTime? CompletedAtUtc,
    int ViewCount);

public sealed record UpdateUserContentProgressRequestModel(
    string ContentOwnerType,
    string ContentOwnerSlug,
    string State);

public sealed record LearningProgressSummaryModel(
    int TotalTracked,
    int ViewedCount,
    int InProgressCount,
    int CompletedCount,
    int NeedsReviewCount,
    IReadOnlyList<UserContentProgressModel> RecentItems);

public sealed record LearningRecommendationModel(
    string RecommendationType,
    string ContentOwnerType,
    string ContentOwnerSlug,
    string Title,
    string Url,
    string Reason,
    string? CefrLevel);
