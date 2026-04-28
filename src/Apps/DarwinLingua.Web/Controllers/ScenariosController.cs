using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("scenarios")]
public sealed class ScenariosController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService) : Controller
{
    [HttpGet("", Name = "Scenarios_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<ScenarioLessonListItemModel> scenarios = await catalogApiClient
            .GetScenariosAsync(cancellationToken)
            .ConfigureAwait(false);

        return View(new ScenarioIndexPageViewModel(scenarios));
    }

    [HttpGet("{slug}", Name = "Scenarios_Detail")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        ScenarioLessonDetailModel? scenario = await catalogApiClient
            .GetScenarioBySlugAsync(
                slug,
                profile.PreferredMeaningLanguage1,
                effectiveSecondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

        if (scenario is null)
        {
            return NotFound();
        }

        return View(new ScenarioDetailPageViewModel(
            scenario,
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }
}
