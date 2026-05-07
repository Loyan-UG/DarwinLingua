using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("words")]
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
        if (!TryParseWordRouteSlug(wordSlug, out Guid id))
        {
            return NotFound();
        }

        return await DetailByIdAsync(id, wordSlug, cancellationToken).ConfigureAwait(false);
    }

    [HttpGet("{id:guid}", Name = "Words_Detail_Legacy")]
    public async Task<IActionResult> DetailLegacy(Guid id, CancellationToken cancellationToken) =>
        await DetailByIdAsync(id, suppliedWordSlug: null, cancellationToken).ConfigureAwait(false);

    private async Task<IActionResult> DetailByIdAsync(Guid id, string? suppliedWordSlug, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        DarwinLingua.Catalog.Application.Models.WordDetailModel? word;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            word = await catalogApiClient.GetWordDetailsAsync(
                id,
                profile.PreferredMeaningLanguage1,
                effectiveSecondaryMeaningLanguageCode,
                profile.UiLanguageCode,
                catalogTimeout.Token);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Word detail could not be loaded for {WordId}.", id);
            return ServiceUnavailableView(
                localizer["Word details are temporarily unavailable"].Value,
                localizer["This word could not be loaded right now. Please try again from browse or search."].Value);
        }

        if (word is null)
        {
            return NotFound();
        }

        string canonicalWordSlug = WordRouteBuilder.CreateRouteSlug(word.Lemma, word.PublicId);
        if (!string.Equals(suppliedWordSlug, canonicalWordSlug, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToActionPermanent(nameof(Detail), "Words", new { wordSlug = canonicalWordSlug });
        }

        var wordState = await userWordStateService.TrackWordViewedAsync(id, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
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
            var state = await userWordStateService.GetWordStateAsync(id, cancellationToken);

            if (state?.IsKnown == true)
            {
                await userWordStateService.ClearWordKnownStateAsync(id, cancellationToken);
            }
            else
            {
                await userWordStateService.MarkWordKnownAsync(id, cancellationToken);
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
            var state = await userWordStateService.GetWordStateAsync(id, cancellationToken);

            if (state?.IsDifficult == true)
            {
                await userWordStateService.ClearWordDifficultStateAsync(id, cancellationToken);
            }
            else
            {
                await userWordStateService.MarkWordDifficultAsync(id, cancellationToken);
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
        string wordSlug = WordRouteBuilder.CreateRouteSlug(word.Lemma, word.PublicId);
        string returnUrl = Url.Action(nameof(Detail), "Words", new { wordSlug }) ?? $"/words/{wordSlug}";

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
        var wordState = await userWordStateService.GetWordStateAsync(id, cancellationToken)
            ?? await userWordStateService.TrackWordViewedAsync(id, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken);

        string? normalizedReturnUrl = WebRouteInput.NormalizeLocalReturnUrl(returnUrl);
        string resolvedReturnUrl = normalizedReturnUrl is not null && Url.IsLocalUrl(normalizedReturnUrl)
            ? normalizedReturnUrl
            : Url.Action(nameof(DetailLegacy), "Words", new { id }) ?? $"/words/{id:D}";

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
        var wordState = await userWordStateService.GetWordStateAsync(id, cancellationToken)
            ?? await userWordStateService.TrackWordViewedAsync(id, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken);

        string? normalizedReturnUrl = WebRouteInput.NormalizeLocalReturnUrl(returnUrl);
        string resolvedReturnUrl = normalizedReturnUrl is not null && Url.IsLocalUrl(normalizedReturnUrl)
            ? normalizedReturnUrl
            : Url.Action(nameof(DetailLegacy), "Words", new { id }) ?? $"/words/{id:D}";

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

        return RedirectToAction(nameof(DetailLegacy), new { id });
    }

    private static bool TryParseWordRouteSlug(string? wordSlug, out Guid id)
    {
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(wordSlug) || wordSlug.Length < 36)
        {
            return false;
        }

        string guidCandidate = wordSlug[^36..];
        if (!Guid.TryParseExact(guidCandidate, "D", out id))
        {
            return false;
        }

        return wordSlug.Length == 36 || wordSlug[^37] == '-';
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
