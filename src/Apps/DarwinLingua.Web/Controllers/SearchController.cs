using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("search")]
public sealed class SearchController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    private const int MaxSearchQueryLength = 128;

    [HttpGet("", Name = "Search_Index")]
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string query = NormalizeQuery(q);
        IReadOnlyList<WordListItemModel> results = string.IsNullOrWhiteSpace(query)
            ? []
            : await catalogApiClient.SearchWordsAsync(query, profile.PreferredMeaningLanguage1, cancellationToken);

        return View(new SearchPageViewModel(query, results, profile.PreferredMeaningLanguage1));
    }

    [HttpGet("results", Name = "Search_Results")]
    public async Task<IActionResult> Results(string? q, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string query = NormalizeQuery(q);
        IReadOnlyList<WordListItemModel> results = string.IsNullOrWhiteSpace(query)
            ? []
            : await catalogApiClient.SearchWordsAsync(query, profile.PreferredMeaningLanguage1, cancellationToken);

        return PartialView("_SearchResults", new SearchPageViewModel(query, results, profile.PreferredMeaningLanguage1));
    }

    private static string NormalizeQuery(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string trimmed = value.Trim();
        return trimmed.Length <= MaxSearchQueryLength ? trimmed : trimmed[..MaxSearchQueryLength];
    }
}
