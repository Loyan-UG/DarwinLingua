using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("search")]
public sealed class SearchController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebFavoriteWordService favoriteWordService,
    IWebEntitledFeatureAccessService featureAccessService,
    IWebWordSuggestionService wordSuggestionService,
    ILogger<SearchController> logger,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    private const int MaxSearchQueryLength = 128;
    private static readonly string[] ResultTypes =
    [
        "word", "grammar", "expression", "dialogue", "talk-topic", "exercise",
        "course-lesson", "exam-prep", "writing-template", "cultural-note", "event", "organizer"
    ];

    [HttpGet("", Name = "Search_Index")]
    public async Task<IActionResult> Index(string? q, string? resultType, string? cefrLevel, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string query = NormalizeQuery(q);
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordListItemModel> primaryResults = await SearchSafelyAsync(query, profile.PreferredMeaningLanguage1, cancellationToken);
        IReadOnlyList<WordListItemModel> secondaryResults = await SearchSecondarySafelyAsync(
                query,
                secondaryMeaningLanguageCode,
                profile.PreferredMeaningLanguage1,
                cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordBrowseCardViewModel> results = await CreateWordCardsAsync(
                primaryResults,
                secondaryResults,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<UnifiedLearningSearchResultModel> learningResults = await SearchLearningSafelyAsync(query, resultType, cefrLevel, cancellationToken).ConfigureAwait(false);

        return View(new SearchPageViewModel(
            query,
            results,
            learningResults,
            ResultTypes,
            LearningPortalFilterConventions.CefrLevels,
            NormalizeResultType(resultType),
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            profile.PreferredMeaningLanguage1,
            secondaryMeaningLanguageCode,
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpGet("results", Name = "Search_Results")]
    public async Task<IActionResult> Results(string? q, string? resultType, string? cefrLevel, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string query = NormalizeQuery(q);
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordListItemModel> primaryResults = await SearchSafelyAsync(query, profile.PreferredMeaningLanguage1, cancellationToken);
        IReadOnlyList<WordListItemModel> secondaryResults = await SearchSecondarySafelyAsync(
                query,
                secondaryMeaningLanguageCode,
                profile.PreferredMeaningLanguage1,
                cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordBrowseCardViewModel> results = await CreateWordCardsAsync(
                primaryResults,
                secondaryResults,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<UnifiedLearningSearchResultModel> learningResults = await SearchLearningSafelyAsync(query, resultType, cefrLevel, cancellationToken).ConfigureAwait(false);

        return PartialView("_SearchResults", new SearchPageViewModel(
            query,
            results,
            learningResults,
            ResultTypes,
            LearningPortalFilterConventions.CefrLevels,
            NormalizeResultType(resultType),
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            profile.PreferredMeaningLanguage1,
            secondaryMeaningLanguageCode,
            null,
            null));
    }

    [HttpPost("suggest", Name = "Search_Suggest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suggest(WordSuggestionInputModel input, CancellationToken cancellationToken)
    {
        string suggestedWord = NormalizeQuery(input.SuggestedWord);
        if (string.IsNullOrWhiteSpace(suggestedWord))
        {
            TempData["ErrorMessage"] = localizer["Please enter a word before sending a suggestion."].Value;
            return RedirectToAction(nameof(Index), new { q = NormalizeQuery(input.SourceQuery) });
        }

        await wordSuggestionService
            .SuggestWordAsync(input with { SuggestedWord = suggestedWord }, cancellationToken)
            .ConfigureAwait(false);

        TempData["StatusMessage"] = localizer["Thanks. {0} was sent to the content team.", suggestedWord].Value;
        return RedirectToAction(nameof(Index), new { q = suggestedWord });
    }

    private async Task<IReadOnlyList<WordListItemModel>> SearchSecondarySafelyAsync(
        string query,
        string? secondaryMeaningLanguageCode,
        string primaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            || string.Equals(secondaryMeaningLanguageCode, primaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        return await SearchSafelyAsync(query, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<WordBrowseCardViewModel>> CreateWordCardsAsync(
        IReadOnlyList<WordListItemModel> primaryResults,
        IReadOnlyList<WordListItemModel> secondaryResults,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, WordListItemModel> secondaryById = secondaryResults.ToDictionary(word => word.PublicId);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlySet<Guid> favoriteWordIds = canUseFavorites
            ? await favoriteWordService
                .GetFavoriteWordIdsAsync(primaryResults.Select(word => word.PublicId).ToArray(), cancellationToken)
                .ConfigureAwait(false)
            : new HashSet<Guid>();
        string returnUrl = Request.PathBase + Request.Path + Request.QueryString;

        return primaryResults
            .Select(word => new WordBrowseCardViewModel(
                word,
                secondaryById.TryGetValue(word.PublicId, out WordListItemModel? secondaryWord) ? secondaryWord.PrimaryMeaning : null,
                primaryMeaningLanguageCode,
                secondaryMeaningLanguageCode,
                new WordInteractionPanelViewModel(
                    word.PublicId,
                    favoriteWordIds.Contains(word.PublicId),
                    new DarwinLingua.Learning.Application.Models.UserWordStateModel(word.PublicId, false, false, null, null, 0),
                    returnUrl,
                    canUseFavorites,
                    canUseFavorites ? null : localizer["Favorites require an active trial or premium plan."].Value)))
            .ToArray();
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

    private async Task<IReadOnlyList<UnifiedLearningSearchResultModel>> SearchLearningSafelyAsync(
        string query,
        string? resultType,
        string? cefrLevel,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        try
        {
            using CancellationTokenSource searchTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            searchTimeout.CancelAfter(TimeSpan.FromSeconds(3));
            return await catalogApiClient.SearchLearningContentAsync(
                    new UnifiedLearningSearchFilterModel(
                        query,
                        LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
                        NormalizeResultType(resultType),
                        null,
                        null),
                    searchTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Unified learning search results could not be loaded for query {Query}.", query);
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

    private static string? NormalizeResultType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim().ToLowerInvariant();
        return ResultTypes.Contains(normalized, StringComparer.Ordinal) ? normalized : null;
    }
}
