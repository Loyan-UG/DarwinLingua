using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("search")]
public sealed class SearchController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    ILogger<SearchController> logger) : Controller
{
    private const int MaxSearchQueryLength = 128;

    [HttpGet("", Name = "Search_Index")]
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string query = NormalizeQuery(q);
        IReadOnlyList<WordListItemModel> results = await SearchSafelyAsync(query, profile.PreferredMeaningLanguage1, cancellationToken);

        return View(new SearchPageViewModel(query, results, profile.PreferredMeaningLanguage1));
    }

    [HttpGet("results", Name = "Search_Results")]
    public async Task<IActionResult> Results(string? q, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string query = NormalizeQuery(q);
        IReadOnlyList<WordListItemModel> results = await SearchSafelyAsync(query, profile.PreferredMeaningLanguage1, cancellationToken);

        return PartialView("_SearchResults", new SearchPageViewModel(query, results, profile.PreferredMeaningLanguage1));
    }

    private async Task<IReadOnlyList<WordListItemModel>> SearchSafelyAsync(
        string query,
        string meaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        try
        {
            using CancellationTokenSource searchTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            searchTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            return await catalogApiClient.SearchWordsAsync(query, meaningLanguageCode, searchTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Search results could not be loaded for query {Query}.", query);
            return [];
        }
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
