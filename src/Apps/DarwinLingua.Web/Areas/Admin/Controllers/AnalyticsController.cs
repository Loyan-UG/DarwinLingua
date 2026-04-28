using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/analytics")]
public sealed class AnalyticsController(IWebProductAnalyticsService analyticsService) : Controller
{
    [HttpGet("", Name = "Admin_Analytics_Index")]
    public IActionResult Index()
    {
        IReadOnlyList<WebProductAnalyticsSummaryItem> items = analyticsService.GetSummary();
        Dictionary<string, int> totalsByEvent = items
            .GroupBy(item => item.EventName, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Count), StringComparer.Ordinal);
        Dictionary<string, int> totalsByArea = items
            .GroupBy(item => item.EventName.Split('.')[0], StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Count), StringComparer.Ordinal);

        return View(new AdminAnalyticsPageViewModel(items, totalsByEvent, totalsByArea));
    }
}
