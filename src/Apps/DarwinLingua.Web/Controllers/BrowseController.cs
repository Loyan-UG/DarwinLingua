using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("browse")]
public sealed class BrowseController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    private const int PageSize = 24;

    [HttpGet("", Name = "Browse_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<TopicListItemModel> topics = await catalogApiClient
            .GetTopicsAsync(profile.UiLanguageCode, cancellationToken);

        BrowseIndexViewModel viewModel = new(
            topics,
            ["A1", "A2", "B1", "B2", "C1", "C2"],
            profile.UiLanguageCode,
            profile.PreferredMeaningLanguage1);

        return View(viewModel);
    }

    [HttpGet("topic/{id}", Name = "Browse_Topic")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Topic(string id, int skip = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<WordListItemModel> words = await catalogApiClient
            .GetWordsByTopicPageAsync(id, profile.PreferredMeaningLanguage1, skip, PageSize + 1, cancellationToken);

        WordBrowsePageViewModel viewModel = new(
            $"Topic: {id}",
            "Browse words linked to the selected topic.",
            id,
            null,
            words.Take(PageSize).ToArray(),
            profile.PreferredMeaningLanguage1,
            skip,
            PageSize,
            words.Count > PageSize);

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_BrowseResults", viewModel);
        }

        return View("WordList", viewModel);
    }

    [HttpGet("cefr/{id}", Name = "Browse_Cefr")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Cefr(string id, int skip = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<WordListItemModel> words = await catalogApiClient
            .GetWordsByCefrPageAsync(id, profile.PreferredMeaningLanguage1, skip, PageSize + 1, cancellationToken);

        WordBrowsePageViewModel viewModel = new(
            $"CEFR {id.ToUpperInvariant()}",
            "Browse words by CEFR level.",
            null,
            id.ToUpperInvariant(),
            words.Take(PageSize).ToArray(),
            profile.PreferredMeaningLanguage1,
            skip,
            PageSize,
            words.Count > PageSize);

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_BrowseResults", viewModel);
        }

        return View("WordList", viewModel);
    }
}
