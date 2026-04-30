using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("conversation-starters")]
public sealed class ConversationStartersController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
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
        ConversationStarterListFilterModel filter = new(cefrLevel, situation, tone, conversationGoal, topicKey);
        IReadOnlyList<ConversationStarterPackListItemModel> starterPacks = await catalogApiClient
            .GetConversationStarterPacksAsync(filter, cancellationToken)
            .ConfigureAwait(false);

        return View(new ConversationStarterIndexPageViewModel(starterPacks, filter));
    }

    [HttpGet("{slug}", Name = "ConversationStarters_Detail")]
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

        ConversationStarterPackDetailModel? starterPack = await catalogApiClient
            .GetConversationStarterPackBySlugAsync(
                normalizedSlug,
                profile.PreferredMeaningLanguage1,
                effectiveSecondaryMeaningLanguageCode,
                cancellationToken)
            .ConfigureAwait(false);

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
}
