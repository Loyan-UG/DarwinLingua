using System.Diagnostics;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

public class HomeController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor) : Controller
{
    [HttpGet("")]
    [OutputCache(PolicyName = "LandingPage")]
    [ActionName("Index")]
    [Route("", Name = "Home_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        var collections = await catalogApiClient
            .GetCollectionsAsync(profile.PreferredMeaningLanguage1, cancellationToken)
            .ConfigureAwait(false);

        return View(new HomePageViewModel(collections.Take(3).ToArray()));
    }

    [HttpGet("install", Name = "Home_Install")]
    public IActionResult Install()
    {
        ViewData["Title"] = "Install";
        return View();
    }

    [HttpGet("error", Name = "Home_Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
