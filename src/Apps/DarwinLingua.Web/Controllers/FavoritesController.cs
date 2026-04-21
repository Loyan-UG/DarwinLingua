using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

public sealed class FavoritesController(
    IUserFavoriteWordService userFavoriteWordService,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        var words = await userFavoriteWordService.GetFavoriteWordsAsync(profile.PreferredMeaningLanguage1, cancellationToken);
        return View(new FavoritesPageViewModel(words, profile.PreferredMeaningLanguage1));
    }
}
