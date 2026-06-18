using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Learning.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("recent")]
public sealed class RecentController(
    IWebActivityQueryService activityQueryService,
    IWebLearningProfileAccessor learningProfileAccessor,
    IUserContentProgressService progressService,
    ILogger<RecentController> logger) : Controller
{
    [HttpGet("", Name = "Recent_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, profile.PreferredMeaningLanguage1, 24, cancellationToken);
        LearningProgressSummaryModel? learningProgress = await GetLearningProgressSafelyAsync(profile.UserId, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<LearningRecommendationModel> recommendations = await GetRecommendationsSafelyAsync(profile.UserId, items, cancellationToken)
            .ConfigureAwait(false);

        return View(new RecentActivityPageViewModel(items, learningProgress, recommendations));
    }

    [HttpGet("panel", Name = "Recent_Panel")]
    public async Task<IActionResult> Panel(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, profile.PreferredMeaningLanguage1, 6, cancellationToken);

        return PartialView("_RecentActivityPanel", new RecentActivityPageViewModel(items, null, []));
    }

    private async Task<LearningProgressSummaryModel?> GetLearningProgressSafelyAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await progressService.GetSummaryAsync(userId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Learning progress summary could not be loaded.");
            return null;
        }
    }

    private async Task<IReadOnlyList<LearningRecommendationModel>> GetRecommendationsSafelyAsync(
        string userId,
        IReadOnlyList<RecentWordActivityItemViewModel> recentWords,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<LearningRecommendationModel> catalogRecommendations = await progressService
                .GetRecommendationsAsync(userId, 6, cancellationToken)
                .ConfigureAwait(false);

            LearningRecommendationModel[] difficultWordRecommendations = recentWords
                .Where(static word => word.IsDifficult && !word.IsKnown)
                .Take(3)
                .Select(static word =>
                {
                    string wordSlug = WordRouteBuilder.CreateRouteSlug(word.Lemma);
                    string title = string.IsNullOrWhiteSpace(word.Article)
                        ? word.Lemma
                        : $"{word.Article} {word.Lemma}";
                    return new LearningRecommendationModel(
                        "difficult-word",
                        "word",
                        wordSlug,
                        title,
                        $"/words/{wordSlug}",
                        "Review this word because you marked it as difficult.",
                        word.CefrLevel);
                })
                .ToArray();

            List<LearningRecommendationModel> merged = [];
            AddUnique(merged, catalogRecommendations.Where(static recommendation => recommendation.RecommendationType == "weak-exercise"));
            AddUnique(merged, difficultWordRecommendations);
            AddUnique(merged, catalogRecommendations.Where(static recommendation => recommendation.RecommendationType != "weak-exercise"));
            return merged.Take(6).ToArray();
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Learning recommendations could not be loaded.");
            return [];
        }
    }

    private static void AddUnique(
        List<LearningRecommendationModel> target,
        IEnumerable<LearningRecommendationModel> source)
    {
        foreach (LearningRecommendationModel recommendation in source)
        {
            if (target.Any(existing =>
                    string.Equals(existing.ContentOwnerType, recommendation.ContentOwnerType, StringComparison.Ordinal) &&
                    string.Equals(existing.ContentOwnerSlug, recommendation.ContentOwnerSlug, StringComparison.Ordinal)))
            {
                continue;
            }

            target.Add(recommendation);
        }
    }
}
