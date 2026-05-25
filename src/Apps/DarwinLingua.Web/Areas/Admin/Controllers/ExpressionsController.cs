using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class ExpressionsController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<ExpressionListItemModel> expressions = await catalogApiClient
            .GetExpressionsAsync(new ExpressionListFilterModel(null, null, null, null, null, null, null, null, true), cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminExpressionsPageViewModel(expressions));
    }
}
