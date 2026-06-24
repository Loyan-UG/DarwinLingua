using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.ConversationStarters)]
public sealed class ConversationStartersController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    ILogger<ConversationStartersController> logger,
    IStringLocalizer<SharedResource> localizer,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "ConversationStarters_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(
        string? cefrLevel,
        string? situation,
        string? tone,
        string? conversationGoal,
        string? topicKey,
        CancellationToken cancellationToken)
    {
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        ConversationStarterListFilterModel filter = new(cefrLevel, situation, tone, conversationGoal, topicKey);
        IReadOnlyList<ConversationStarterPackListItemModel> starterPacks;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            starterPacks = await catalogApiClient
                .GetConversationStarterPacksAsync(filter, targetLearningLanguageCode, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Conversation starter packs could not be loaded.");
            starterPacks = [];
        }

        return View(new ConversationStarterIndexPageViewModel(starterPacks, filter));
    }

    [HttpGet("{slug}", Name = "ConversationStarters_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index), new { targetLearningLanguageCode });
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string? effectiveSecondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        ConversationStarterPackDetailModel? starterPack;

        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            starterPack = await catalogApiClient
                .GetConversationStarterPackBySlugAsync(
                    normalizedSlug,
                    targetLearningLanguageCode,
                    profile.PreferredMeaningLanguage1,
                    effectiveSecondaryMeaningLanguageCode,
                    catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Conversation starter pack could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Conversation starter is temporarily unavailable"],
                localizer["This starter pack could not be loaded right now. Please return to starters and try again."]);
        }

        if (starterPack is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.ConversationStarterViewed, $"starter:{starterPack.Slug}");

        return View(new ConversationStarterDetailPageViewModel(
            starterPack,
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }

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
}
