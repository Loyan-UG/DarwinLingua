using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("collections")]
public sealed class CollectionsController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    [HttpGet("", Name = "Collections_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<WordCollectionListItemModel> collections = await catalogApiClient
            .GetCollectionsAsync(profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        return View(new WordCollectionIndexPageViewModel(collections, profile.PreferredMeaningLanguage1));
    }

    [HttpGet("{slug}", Name = "Collections_Detail")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        var collection = await catalogApiClient
            .GetCollectionBySlugAsync(slug, profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return NotFound();
        }

        return View(new WordCollectionDetailPageViewModel(collection, profile.PreferredMeaningLanguage1));
    }
}
