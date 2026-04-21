using DarwinLingua.Catalog.Application.Abstractions;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

public sealed class WordsController(
    IWordDetailQueryService wordDetailQueryService,
    IUserFavoriteWordService userFavoriteWordService,
    IUserWordStateService userWordStateService,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    public async Task<IActionResult> Detail(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return NotFound();
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        var word = await wordDetailQueryService.GetWordDetailsAsync(
            id,
            profile.PreferredMeaningLanguage1,
            profile.PreferredMeaningLanguage2,
            profile.UiLanguageCode,
            cancellationToken);

        if (word is null)
        {
            return NotFound();
        }

        var wordState = await userWordStateService.TrackWordViewedAsync(id, cancellationToken);
        bool isFavorite = await userFavoriteWordService.IsFavoriteAsync(id, cancellationToken);
        return View(new WordDetailPageViewModel(
            word,
            isFavorite,
            wordState,
            profile.PreferredMeaningLanguage1,
            profile.PreferredMeaningLanguage2,
            profile.UiLanguageCode));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        if (id != Guid.Empty)
        {
            await userFavoriteWordService.ToggleFavoriteAsync(id, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Detail), new { id });
    }
}
