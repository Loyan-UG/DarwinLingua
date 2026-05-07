using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("collections")]
public sealed class CollectionsController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    IWebFavoriteWordService favoriteWordService,
    ILogger<CollectionsController> logger,
    IStringLocalizer<SharedResource> localizer) : Controller
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
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        var collection = await catalogApiClient
            .GetCollectionBySlugAsync(normalizedSlug, profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        if (collection is null)
        {
            return NotFound();
        }

        IReadOnlyList<WordBrowseCardViewModel> words = await CreateWordCardsAsync(
                collection.Words,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        return View(new WordCollectionDetailPageViewModel(
            collection,
            words,
            profile.PreferredMeaningLanguage1,
            secondaryMeaningLanguageCode));
    }

    private async Task<IReadOnlyList<WordBrowseCardViewModel>> CreateWordCardsAsync(
        IReadOnlyList<WordListItemModel> words,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<WordListItemModel> secondaryWords = await LoadSecondaryWordsAsync(
                words,
                primaryMeaningLanguageCode,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);
        Dictionary<Guid, WordListItemModel> secondaryById = secondaryWords.ToDictionary(word => word.PublicId);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlySet<Guid> favoriteWordIds = canUseFavorites
            ? await favoriteWordService
                .GetFavoriteWordIdsAsync(words.Select(word => word.PublicId).ToArray(), cancellationToken)
                .ConfigureAwait(false)
            : new HashSet<Guid>();
        string returnUrl = Request.PathBase + Request.Path + Request.QueryString;

        return words
            .Select(word => new WordBrowseCardViewModel(
                word,
                secondaryById.TryGetValue(word.PublicId, out WordListItemModel? secondaryWord)
                    ? secondaryWord.PrimaryMeaning
                    : null,
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

    private async Task<IReadOnlyList<WordListItemModel>> LoadSecondaryWordsAsync(
        IReadOnlyList<WordListItemModel> words,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        if (words.Count == 0 ||
            string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode) ||
            string.Equals(secondaryMeaningLanguageCode, primaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        try
        {
            return await catalogApiClient
                .GetWordsByIdsAsync(words.Select(word => word.PublicId).ToArray(), secondaryMeaningLanguageCode, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Collection secondary meanings could not be loaded.");
            return [];
        }
    }
}
