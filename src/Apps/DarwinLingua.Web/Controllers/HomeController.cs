using System.Diagnostics;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

public class HomeController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    ILogger<HomeController> logger) : Controller
{
    [HttpGet("")]
    [OutputCache(PolicyName = "LandingPage")]
    [ActionName("Index")]
    [Route("", Name = "Home_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        IReadOnlyList<DarwinLingua.Catalog.Application.Models.WordCollectionListItemModel> collections;

        try
        {
            using CancellationTokenSource featuredCollectionsTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            featuredCollectionsTimeout.CancelAfter(TimeSpan.FromSeconds(2));

            collections = await catalogApiClient
                .GetCollectionsAsync(profile.PreferredMeaningLanguage1, featuredCollectionsTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Featured collections could not be loaded for the landing page.");
            collections = [];
        }

        return View(new HomePageViewModel(collections.Take(3).ToArray()));
    }

    [HttpGet("install", Name = "Home_Install")]
    public IActionResult Install()
    {
        return View();
    }

    [HttpGet("privacy", Name = "Home_Privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("terms", Name = "Home_Terms")]
    public IActionResult Terms()
    {
        return View();
    }

    [HttpGet("error", Name = "Home_Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
