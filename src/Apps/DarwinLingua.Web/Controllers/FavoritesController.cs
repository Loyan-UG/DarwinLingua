using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Route("favorites")]
public sealed class FavoritesController(
    IWebFavoriteWordService userFavoriteWordService,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    [HttpGet("", Name = "Favorites_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        var words = await userFavoriteWordService.GetFavoriteWordsAsync(profile.PreferredMeaningLanguage1, cancellationToken);
        return View(new FavoritesPageViewModel(words, profile.PreferredMeaningLanguage1));
    }
}
