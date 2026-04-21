using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

public sealed class BrowseController(
    ITopicQueryService topicQueryService,
    IWordQueryService wordQueryService,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    private const int PageSize = 24;

    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<TopicListItemModel> topics = await topicQueryService
            .GetTopicsAsync(profile.UiLanguageCode, cancellationToken);

        BrowseIndexViewModel viewModel = new(
            topics,
            ["A1", "A2", "B1", "B2", "C1", "C2"],
            profile.UiLanguageCode,
            profile.PreferredMeaningLanguage1);

        return View(viewModel);
    }

    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Topic(string id, int skip = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<WordListItemModel> words = await wordQueryService
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

        return View("WordList", viewModel);
    }

    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Cefr(string id, int skip = 0, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<WordListItemModel> words = await wordQueryService
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

        return View("WordList", viewModel);
    }
}
