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
    IWebEntitledFeatureAccessService featureAccessService,
    IWebProductAnalyticsService? analyticsService = null) : Controller
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
    [OutputCache(NoStore = true)]
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

        analyticsService?.Record(WebProductAnalyticsEvents.ScenarioViewed, $"scenario:{scenario.Slug}");

        IReadOnlyList<ConversationStarterPackListItemModel> relatedStarterPacks = await catalogApiClient
            .GetConversationStarterPacksForScenarioAsync(slug, cancellationToken)
            .ConfigureAwait(false);

        IReadOnlyList<EventPreparationPackListItemModel> relatedEventPreparationPacks =
            await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false)
                ? await catalogApiClient
                    .GetEventPreparationPacksForScenarioAsync(slug, cancellationToken)
                    .ConfigureAwait(false)
                : [];

        return View(new ScenarioDetailPageViewModel(
            scenario,
            relatedStarterPacks,
            relatedEventPreparationPacks,
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }

    [HttpGet("{slug}/roleplay", Name = "Scenarios_Roleplay")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Roleplay(string slug, CancellationToken cancellationToken)
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

        analyticsService?.Record(WebProductAnalyticsEvents.RoleplayViewed, $"scenario:{scenario.Slug}");

        return View(new ScenarioRoleplayPageViewModel(
            scenario,
            BuildRoleplaySteps(scenario),
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }

    private static IReadOnlyList<ScenarioRoleplayStepViewModel> BuildRoleplaySteps(ScenarioLessonDetailModel scenario)
    {
        List<ScenarioRoleplayStepViewModel> steps = [];
        for (int index = 0; index < scenario.DialogueTurns.Count; index++)
        {
            ScenarioDialogueTurnModel prompt = scenario.DialogueTurns[index];
            if (IsLearnerRole(prompt.SpeakerRole))
            {
                continue;
            }

            ScenarioDialogueTurnModel? answer = scenario.DialogueTurns
                .Skip(index + 1)
                .FirstOrDefault(static turn => IsLearnerRole(turn.SpeakerRole));

            if (answer is null)
            {
                continue;
            }

            steps.Add(new ScenarioRoleplayStepViewModel(
                steps.Count + 1,
                prompt.SpeakerRole,
                prompt.BaseText,
                prompt.PrimaryMeaning,
                prompt.SecondaryMeaning,
                answer.SpeakerRole,
                answer.BaseText,
                answer.PrimaryMeaning,
                answer.SecondaryMeaning,
                "Compare your answer with the model answer, then replay the line aloud."));
        }

        return steps;
    }

    private static bool IsLearnerRole(string speakerRole) =>
        string.Equals(speakerRole, "learner", StringComparison.OrdinalIgnoreCase);
}
