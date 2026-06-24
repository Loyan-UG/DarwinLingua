using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.Favorites)]
public sealed class FavoritesController(
    IWebFavoriteWordService userFavoriteWordService,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService) : Controller
{
    [HttpGet("", Name = "Favorites_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        if (!await featureAccessService.CanUseFavoritesAsync(cancellationToken))
        {
            return View(new FavoritesPageViewModel([], profile.PreferredMeaningLanguage1, true, "Favorites are available during an active trial or premium plan."));
        }

        var words = await userFavoriteWordService.GetFavoriteWordsAsync(targetLearningLanguageCode, profile.PreferredMeaningLanguage1, cancellationToken);
        return View(new FavoritesPageViewModel(words, profile.PreferredMeaningLanguage1, false, null));
    }
}
