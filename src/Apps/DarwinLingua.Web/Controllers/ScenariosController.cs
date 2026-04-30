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
    ILogger<ScenariosController> logger,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "Scenarios_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        IReadOnlyList<ScenarioLessonListItemModel> scenarios;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            scenarios = await catalogApiClient
                .GetScenariosAsync(catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Scenarios could not be loaded.");
            scenarios = [];
        }

        return View(new ScenarioIndexPageViewModel(scenarios));
    }

    [HttpGet("{slug}", Name = "Scenarios_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        ScenarioLessonDetailModel? scenario;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            scenario = await catalogApiClient
                .GetScenarioBySlugAsync(
                    normalizedSlug,
                    profile.PreferredMeaningLanguage1,
                    effectiveSecondaryMeaningLanguageCode,
                    catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Scenario detail could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView("Scenario is temporarily unavailable", "This practice scenario could not be loaded right now. Please return to scenarios and try again.");
        }

        if (scenario is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.ScenarioViewed, $"scenario:{scenario.Slug}");

        IReadOnlyList<ConversationStarterPackListItemModel> relatedStarterPacks = [];
        IReadOnlyList<EventPreparationPackListItemModel> relatedEventPreparationPacks = [];

        try
        {
            Task<IReadOnlyList<ConversationStarterPackListItemModel>> relatedStarterPacksTask = catalogApiClient
                .GetConversationStarterPacksForScenarioAsync(normalizedSlug, cancellationToken);
            Task<IReadOnlyList<EventPreparationPackListItemModel>> relatedEventPreparationPacksTask =
                LoadRelatedEventPreparationPacksAsync(normalizedSlug, cancellationToken);
            await Task.WhenAll(relatedStarterPacksTask, relatedEventPreparationPacksTask).ConfigureAwait(false);
            relatedStarterPacks = await relatedStarterPacksTask.ConfigureAwait(false);
            relatedEventPreparationPacks = await relatedEventPreparationPacksTask.ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Scenario related content could not be loaded for {Slug}.", normalizedSlug);
        }

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
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        ScenarioLessonDetailModel? scenario;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            scenario = await catalogApiClient
                .GetScenarioBySlugAsync(
                    normalizedSlug,
                    profile.PreferredMeaningLanguage1,
                    effectiveSecondaryMeaningLanguageCode,
                    catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Scenario roleplay could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView("Roleplay is temporarily unavailable", "This guided roleplay could not be loaded right now. Please return to scenarios and try again.");
        }

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

    private async Task<IReadOnlyList<EventPreparationPackListItemModel>> LoadRelatedEventPreparationPacksAsync(
        string scenarioSlug,
        CancellationToken cancellationToken)
    {
        return await featureAccessService.CanUseEventPreparationPacksAsync(cancellationToken).ConfigureAwait(false)
            ? await catalogApiClient.GetEventPreparationPacksForScenarioAsync(scenarioSlug, cancellationToken).ConfigureAwait(false)
            : [];
    }
}
