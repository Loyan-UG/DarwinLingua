using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.Browse)]
public sealed class BrowseController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebFavoriteWordService favoriteWordService,
    IWebEntitledFeatureAccessService featureAccessService,
    ILogger<BrowseController> logger) : Controller
{
    private const int PageSize = 24;

    [HttpGet("", Name = "Browse_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<TopicListItemModel> topics;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            topics = await catalogApiClient
                .GetTopicsAsync(profile.UiLanguageCode, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Browse topics could not be loaded.");
            topics = [];
        }

        BrowseIndexViewModel viewModel = new(
            topics,
            LearningPortalFilterConventions.CefrLevels,
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
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        string normalizedTopicKey = id.Trim();
        string topicTitle = normalizedTopicKey;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            IReadOnlyList<TopicListItemModel> topics = await catalogApiClient
                .GetTopicsAsync(profile.UiLanguageCode, catalogTimeout.Token)
                .ConfigureAwait(false);
            TopicListItemModel? topic = topics.FirstOrDefault(candidate =>
                string.Equals(candidate.Key, normalizedTopicKey, StringComparison.OrdinalIgnoreCase)
                || string.Equals(candidate.DisplayName, normalizedTopicKey, StringComparison.OrdinalIgnoreCase));

            if (topic is not null)
            {
                normalizedTopicKey = topic.Key;
                topicTitle = topic.DisplayName;
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Browse topic metadata could not be loaded for {TopicId}.", id);
        }

        IReadOnlyList<WordListItemModel> primaryWords = await catalogApiClient
            .GetWordsByTopicPageAsync(normalizedTopicKey, profile.PreferredMeaningLanguage1, skip, PageSize + 1, cancellationToken);
        IReadOnlyList<WordListItemModel> secondaryWords = await LoadSecondaryTopicWordsAsync(
                normalizedTopicKey,
                secondaryMeaningLanguageCode,
                profile.PreferredMeaningLanguage1,
                skip,
                primaryWords.Count,
                cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordBrowseCardViewModel> cards = await CreateWordCardsAsync(
                primaryWords.Take(PageSize).ToArray(),
                secondaryWords,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        WordBrowsePageViewModel viewModel = new(
            $"Topic: {topicTitle}",
            "Browse words linked to the selected topic.",
            normalizedTopicKey,
            null,
            cards,
            profile.PreferredMeaningLanguage1,
            secondaryMeaningLanguageCode,
            skip,
            PageSize,
            primaryWords.Count > PageSize);

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
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordListItemModel> primaryWords = await catalogApiClient
            .GetWordsByCefrPageAsync(id, profile.PreferredMeaningLanguage1, skip, PageSize + 1, cancellationToken);
        IReadOnlyList<WordListItemModel> secondaryWords = await LoadSecondaryCefrWordsAsync(
                id,
                secondaryMeaningLanguageCode,
                profile.PreferredMeaningLanguage1,
                skip,
                primaryWords.Count,
                cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WordBrowseCardViewModel> cards = await CreateWordCardsAsync(
                primaryWords.Take(PageSize).ToArray(),
                secondaryWords,
                profile.PreferredMeaningLanguage1,
                secondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        WordBrowsePageViewModel viewModel = new(
            $"CEFR {id.ToUpperInvariant()}",
            "Browse words by CEFR level.",
            null,
            id.ToUpperInvariant(),
            cards,
            profile.PreferredMeaningLanguage1,
            secondaryMeaningLanguageCode,
            skip,
            PageSize,
            primaryWords.Count > PageSize);

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return PartialView("_BrowseResults", viewModel);
        }

        return View("WordList", viewModel);
    }

    private async Task<IReadOnlyList<WordBrowseCardViewModel>> CreateWordCardsAsync(
        IReadOnlyList<WordListItemModel> primaryWords,
        IReadOnlyList<WordListItemModel> secondaryWords,
        string primaryMeaningLanguageCode,
        string? secondaryMeaningLanguageCode,
        CancellationToken cancellationToken)
    {
        Dictionary<Guid, WordListItemModel> secondaryById = secondaryWords.ToDictionary(word => word.PublicId);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlySet<Guid> favoriteWordIds = canUseFavorites
            ? await favoriteWordService
                .GetFavoriteWordIdsAsync(primaryWords.Select(word => word.PublicId).ToArray(), cancellationToken)
                .ConfigureAwait(false)
            : new HashSet<Guid>();
        List<WordBrowseCardViewModel> cards = new(primaryWords.Count);

        foreach (WordListItemModel word in primaryWords)
        {
            bool isFavorite = favoriteWordIds.Contains(word.PublicId);
            string returnUrl = Request.PathBase + Request.Path + Request.QueryString;

            cards.Add(new WordBrowseCardViewModel(
                word,
                secondaryById.TryGetValue(word.PublicId, out WordListItemModel? secondaryWord)
                    ? secondaryWord.PrimaryMeaning
                    : null,
                primaryMeaningLanguageCode,
                secondaryMeaningLanguageCode,
                new WordInteractionPanelViewModel(
                    word.PublicId,
                    isFavorite,
                    new DarwinLingua.Learning.Application.Models.UserWordStateModel(word.PublicId, false, false, null, null, 0),
                    returnUrl,
                    canUseFavorites,
                    canUseFavorites ? null : "Favorites require an active trial or premium plan.")));
        }

        return cards;
    }

    private async Task<IReadOnlyList<WordListItemModel>> LoadSecondaryTopicWordsAsync(
        string topicKey,
        string? secondaryMeaningLanguageCode,
        string primaryMeaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            || string.Equals(secondaryMeaningLanguageCode, primaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase)
            || take <= 0)
        {
            return [];
        }

        return await catalogApiClient
            .GetWordsByTopicPageAsync(topicKey, secondaryMeaningLanguageCode, skip, take, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<IReadOnlyList<WordListItemModel>> LoadSecondaryCefrWordsAsync(
        string cefrLevel,
        string? secondaryMeaningLanguageCode,
        string primaryMeaningLanguageCode,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            || string.Equals(secondaryMeaningLanguageCode, primaryMeaningLanguageCode, StringComparison.OrdinalIgnoreCase)
            || take <= 0)
        {
            return [];
        }

        return await catalogApiClient
            .GetWordsByCefrPageAsync(cefrLevel, secondaryMeaningLanguageCode, skip, take, cancellationToken)
            .ConfigureAwait(false);
    }
}
