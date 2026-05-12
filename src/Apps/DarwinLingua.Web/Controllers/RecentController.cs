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

        return View(new RecentActivityPageViewModel(items, learningProgress));
    }

    [HttpGet("panel", Name = "Recent_Panel")]
    public async Task<IActionResult> Panel(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, profile.PreferredMeaningLanguage1, 6, cancellationToken);

        return PartialView("_RecentActivityPanel", new RecentActivityPageViewModel(items, null));
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
}
