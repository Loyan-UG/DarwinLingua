using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public sealed class GrammarController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<GrammarTopicListItemModel> topics = await catalogApiClient
            .GetGrammarTopicsAsync(new GrammarTopicListFilterModel(null, null, null, null), cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminGrammarPageViewModel(topics));
    }
}
