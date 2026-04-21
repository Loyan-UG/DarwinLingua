using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

public sealed class RecentController(
    IWebActivityQueryService activityQueryService,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, 24, cancellationToken);

        return View(new RecentActivityPageViewModel(items));
    }

    public async Task<IActionResult> Panel(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, 6, cancellationToken);

        return PartialView("_RecentActivityPanel", new RecentActivityPageViewModel(items));
    }
}
