using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("words")]
public sealed class WordsController(
    IWebCatalogApiClient catalogApiClient,
    IWebFavoriteWordService userFavoriteWordService,
    IWebUserWordStateService userWordStateService,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService) : Controller
{
    [HttpGet("{id:guid}", Name = "Words_Detail")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        var word = await catalogApiClient.GetWordDetailsAsync(
            id,
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode,
            profile.UiLanguageCode,
            cancellationToken);

        if (word is null)
        {
            return NotFound();
        }

        var wordState = await userWordStateService.TrackWordViewedAsync(id, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
        bool canUseFavorites = await featureAccessService.CanUseFavoritesAsync(cancellationToken);
        return View(CreatePageViewModel(word, isFavorite, wordState, profile, effectiveSecondaryMeaningLanguageCode, canUseFavorites, canUseFavorites ? null : "Favorites require an active trial or premium plan."));
    }

    [HttpPost(Name = "Words_ToggleFavorite")]
    [ValidateAntiForgeryToken]
    [Route("toggle-favorite")]
    public async Task<IActionResult> ToggleFavorite(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (id != Guid.Empty)
        {
            try
            {
                await userFavoriteWordService.ToggleFavoriteAsync(id, cancellationToken);
            }
            catch (DarwinLingua.Identity.FeatureAccessDeniedException)
            {
                if (Request.Headers.ContainsKey("HX-Request"))
                {
                    return await RenderInteractionPanelAsync(id, returnUrl, cancellationToken, "Favorites require an active trial or premium plan.");
                }

                return RedirectToAction("Index", "Favorites");
            }
        }

        if (Request.Headers.ContainsKey("HX-Request"))
        {
            return await RenderInteractionPanelAsync(id, returnUrl, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost(Name = "Words_ToggleKnown")]
    [ValidateAntiForgeryToken]
    [Route("toggle-known")]
    public async Task<IActionResult> ToggleKnown(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost(Name = "Words_ToggleDifficult")]
    [ValidateAntiForgeryToken]
    [Route("toggle-difficult")]
    public async Task<IActionResult> ToggleDifficult(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Detail), new { id });
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
        string returnUrl = Url.Action(nameof(Detail), "Words", new { id = word.PublicId }) ?? $"/Words/Detail/{word.PublicId}";

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

        string resolvedReturnUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action(nameof(Detail), "Words", new { id }) ?? $"/Words/Detail/{id}";

        return PartialView(
            "_InteractionPanel",
            new WordInteractionPanelViewModel(
                id,
                isFavorite,
                wordState,
                resolvedReturnUrl,
                canUseFavorites,
                canUseFavorites ? null : favoriteLockedMessage ?? "Favorites require an active trial or premium plan."));
    }
}
