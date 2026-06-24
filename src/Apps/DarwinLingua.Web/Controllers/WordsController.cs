using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.Words)]
public sealed class WordsController(
    IWebCatalogApiClient catalogApiClient,
    IWebFavoriteWordService userFavoriteWordService,
    IWebUserWordStateService userWordStateService,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<WordsController> logger,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("{wordSlug}", Name = "Words_Detail")]
    public async Task<IActionResult> Detail(string wordSlug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(wordSlug))
        {
            return NotFound();
        }

        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        DarwinLingua.Catalog.Application.Models.WordDetailModel? word;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            word = await catalogApiClient.GetWordDetailsBySlugAsync(
                wordSlug,
                profile.PreferredMeaningLanguage1,
                effectiveSecondaryMeaningLanguageCode,
                profile.UiLanguageCode,
                catalogTimeout.Token);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Word detail could not be loaded for {WordSlug}.", wordSlug);
            return ServiceUnavailableView(
                localizer["Word details are temporarily unavailable"].Value,
                localizer["This word could not be loaded right now. Please try again from browse or search."].Value);
        }

        if (word is null)
        {
            return NotFound();
        }

        string canonicalWordSlug = WordRouteBuilder.CreateRouteSlug(word.Lemma);
        if (!string.Equals(wordSlug, canonicalWordSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToActionPermanent(nameof(Detail), "Words", new { targetLearningLanguageCode, wordSlug = canonicalWordSlug });
        }

        var wordState = await userWordStateService.TrackWordViewedAsync(word.PublicId, targetLearningLanguageCode, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(word.PublicId, cancellationToken);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken);
        return View(CreatePageViewModel(word, isFavorite, wordState, profile, effectiveSecondaryMeaningLanguageCode, canUseFavorites, canUseFavorites ? null : localizer["Favorites require an active trial or premium plan."].Value));
    }

    [HttpPost(Name = "Words_ToggleFavorite")]
    [ValidateAntiForgeryToken]
    [Route("toggle-favorite")]
    public async Task<IActionResult> ToggleFavorite(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return HandleInvalidWordPost();
        }

        if (id != Guid.Empty)
        {
            try
            {
                bool wasFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
                await userFavoriteWordService.ToggleFavoriteAsync(id, cancellationToken);
                if (!wasFavorite)
                {
                    analyticsService?.Record(WebProductAnalyticsEvents.FavoriteSaved);
                }
            }
            catch (DarwinLingua.Identity.FeatureAccessDeniedException)
            {
                analyticsService?.Record(WebProductAnalyticsEvents.PremiumFeatureDenied, "feature:favorites");
                if (Request.Headers.ContainsKey("HX-Request"))
                {
                    return await RenderFavoriteToggleAsync(id, returnUrl, cancellationToken, localizer["Favorites require an active trial or premium plan."].Value);
                }

                return RedirectToAction("Index", "Favorites");
            }
        }

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return await RenderFavoriteToggleAsync(id, returnUrl, cancellationToken);
        }

        return RedirectToSafeWordReturn(id, returnUrl);
    }

    [HttpPost(Name = "Words_ToggleKnown")]
    [ValidateAntiForgeryToken]
    [Route("toggle-known")]
    public async Task<IActionResult> ToggleKnown(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return HandleInvalidWordPost();
        }

        if (id != Guid.Empty)
        {
            string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
            var state = await userWordStateService.GetWordStateAsync(id, targetLearningLanguageCode, cancellationToken);

            if (state?.IsKnown == true)
            {
                await userWordStateService.ClearWordKnownStateAsync(id, targetLearningLanguageCode, cancellationToken);
            }
            else
            {
                await userWordStateService.MarkWordKnownAsync(id, targetLearningLanguageCode, cancellationToken);
            }
        }

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return await RenderInteractionPanelAsync(id, returnUrl, cancellationToken);
        }

        return RedirectToSafeWordReturn(id, returnUrl);
    }

    [HttpPost(Name = "Words_ToggleDifficult")]
    [ValidateAntiForgeryToken]
    [Route("toggle-difficult")]
    public async Task<IActionResult> ToggleDifficult(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return HandleInvalidWordPost();
        }

        if (id != Guid.Empty)
        {
            string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
            var state = await userWordStateService.GetWordStateAsync(id, targetLearningLanguageCode, cancellationToken);

            if (state?.IsDifficult == true)
            {
                await userWordStateService.ClearWordDifficultStateAsync(id, targetLearningLanguageCode, cancellationToken);
            }
            else
            {
                await userWordStateService.MarkWordDifficultAsync(id, targetLearningLanguageCode, cancellationToken);
            }
        }

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return await RenderInteractionPanelAsync(id, returnUrl, cancellationToken);
        }

        return RedirectToSafeWordReturn(id, returnUrl);
    }

    private WordDetailPageViewModel CreatePageViewModel(
        DarwinLingua.Catalog.Application.Models.WordDetailModel word,
        bool isFavorite,
        DarwinLingua.Learning.Application.Models.UserWordStateModel wordState,
        DarwinLingua.Learning.Application.Models.UserLearningProfileModel profile,
        string? effectiveSecondaryMeaningLanguageCode,
        bool canUseFavorites,
        string? favoriteLockedMessage)
    {
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        string wordSlug = WordRouteBuilder.CreateRouteSlug(word.Lemma);
        string returnUrl = Url.Action(nameof(Detail), "Words", new { targetLearningLanguageCode, wordSlug }) ?? $"/learn/{targetLearningLanguageCode}/words/{wordSlug}";

        return new WordDetailPageViewModel(
            new WordDetailContentViewModel(
                word,
                new WordInteractionPanelViewModel(word.PublicId, isFavorite, wordState, returnUrl, canUseFavorites, favoriteLockedMessage)),
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode,
            profile.UiLanguageCode);
    }

    private async Task<PartialViewResult> RenderInteractionPanelAsync(Guid id, string? returnUrl, CancellationToken cancellationToken, string? favoriteLockedMessage = null)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        var wordState = await userWordStateService.GetWordStateAsync(id, targetLearningLanguageCode, cancellationToken)
            ?? await userWordStateService.TrackWordViewedAsync(id, targetLearningLanguageCode, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken);

        string? normalizedReturnUrl = WebRouteInput.NormalizeLocalReturnUrl(returnUrl);
        string resolvedReturnUrl = normalizedReturnUrl is not null && Url.IsLocalUrl(normalizedReturnUrl)
            ? normalizedReturnUrl
            : Url.Action("Index", "Browse", new { targetLearningLanguageCode }) ?? $"/learn/{targetLearningLanguageCode}/browse";

        return PartialView(
            "_InteractionPanel",
            new WordInteractionPanelViewModel(
                id,
                isFavorite,
                wordState,
                resolvedReturnUrl,
                canUseFavorites,
                canUseFavorites ? null : favoriteLockedMessage ?? localizer["Favorites require an active trial or premium plan."].Value));
    }

    private async Task<PartialViewResult> RenderFavoriteToggleAsync(Guid id, string? returnUrl, CancellationToken cancellationToken, string? favoriteLockedMessage = null)
    {
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        var wordState = await userWordStateService.GetWordStateAsync(id, targetLearningLanguageCode, cancellationToken)
            ?? await userWordStateService.TrackWordViewedAsync(id, targetLearningLanguageCode, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken);

        string? normalizedReturnUrl = WebRouteInput.NormalizeLocalReturnUrl(returnUrl);
        string resolvedReturnUrl = normalizedReturnUrl is not null && Url.IsLocalUrl(normalizedReturnUrl)
            ? normalizedReturnUrl
            : Url.Action("Index", "Browse", new { targetLearningLanguageCode }) ?? $"/learn/{targetLearningLanguageCode}/browse";

        return PartialView(
            "~/Views/Shared/_FavoriteToggle.cshtml",
            new WordInteractionPanelViewModel(
                id,
                isFavorite,
                wordState,
                resolvedReturnUrl,
                canUseFavorites,
                canUseFavorites ? null : favoriteLockedMessage ?? localizer["Favorites require an active trial or premium plan."].Value));
    }

    private IActionResult RedirectToSafeWordReturn(Guid id, string? returnUrl)
    {
        string? normalizedReturnUrl = WebRouteInput.NormalizeLocalReturnUrl(returnUrl);
        if (normalizedReturnUrl is not null && Url.IsLocalUrl(normalizedReturnUrl))
        {
            return LocalRedirect(normalizedReturnUrl);
        }

        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        return RedirectToAction("Index", "Browse", new { targetLearningLanguageCode });
    }

    private IActionResult HandleInvalidWordPost() =>
        Request.Headers.ContainsKey("HX-Request")
            ? BadRequest()
            : RedirectToAction("Index", "Home");

    private ViewResult ServiceUnavailableView(string title, string message)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
        {
            Title = title,
            Message = message,
            RequestId = HttpContext.TraceIdentifier
        });
    }
}
