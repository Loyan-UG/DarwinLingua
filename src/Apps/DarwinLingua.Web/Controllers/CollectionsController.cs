using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("collections")]
public sealed class CollectionsController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    ILogger<CollectionsController> logger) : Controller
{
    [HttpGet("", Name = "Collections_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<WordCollectionListItemModel> collections;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            collections = await catalogApiClient
                .GetCollectionsAsync(profile.PreferredMeaningLanguage1, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Collections could not be loaded.");
            collections = [];
        }

        return View(new WordCollectionIndexPageViewModel(collections, profile.PreferredMeaningLanguage1));
    }

    [HttpGet("{slug}", Name = "Collections_Detail")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        var collection = await catalogApiClient
            .GetCollectionBySlugAsync(normalizedSlug, profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return NotFound();
        }

        return View(new WordCollectionDetailPageViewModel(collection, profile.PreferredMeaningLanguage1));
    }
}
