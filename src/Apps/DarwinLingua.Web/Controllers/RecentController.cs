using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("recent")]
public sealed class RecentController(
    IWebActivityQueryService activityQueryService,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    [HttpGet("", Name = "Recent_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, 24, cancellationToken);

        return View(new RecentActivityPageViewModel(items));
    }

    [HttpGet("panel", Name = "Recent_Panel")]
    public async Task<IActionResult> Panel(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<RecentWordActivityItemViewModel> items = await activityQueryService
            .GetRecentWordActivityAsync(profile.UserId, 6, cancellationToken);

        return PartialView("_RecentActivityPanel", new RecentActivityPageViewModel(items));
    }
}
