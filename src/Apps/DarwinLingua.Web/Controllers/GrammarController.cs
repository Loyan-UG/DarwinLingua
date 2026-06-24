using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.Grammar)]
public sealed class GrammarController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IStringLocalizer<SharedResource> localizer,
    ILogger<GrammarController> logger,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "Grammar_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(
        string? cefrLevel,
        string? grammarCategory,
        string? topic,
        string? q,
        CancellationToken cancellationToken)
    {
        GrammarTopicListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            WebRouteInput.NormalizeSlug(grammarCategory ?? string.Empty),
            WebRouteInput.NormalizeSlug(topic ?? string.Empty),
            string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);

        IReadOnlyList<GrammarTopicListItemModel> grammarTopics;
        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            grammarTopics = await catalogApiClient
                .GetGrammarTopicsAsync(filter, targetLearningLanguageCode, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Grammar topics could not be loaded.");
            grammarTopics = [];
        }

        return View(new GrammarIndexPageViewModel(
            grammarTopics,
            LearningPortalFilterConventions.CefrLevels,
            grammarTopics.Select(item => item.GrammarCategory).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            grammarTopics.SelectMany(item => item.TopicKeys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            filter.CefrLevel,
            filter.GrammarCategory,
            filter.TopicKey,
            filter.Query));
    }

    [HttpGet("{slug}", Name = "Grammar_Detail")]
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

        GrammarTopicDetailModel? grammarTopic;
        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            grammarTopic = await catalogApiClient
                .GetGrammarTopicBySlugAsync(normalizedSlug, targetLearningLanguageCode, profile.PreferredMeaningLanguage1, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Grammar topic detail could not be loaded for {Slug}.", normalizedSlug);
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                Title = localizer["Grammar topic is temporarily unavailable"].Value,
                Message = localizer["This grammar topic could not be loaded right now. Please return to Grammar Guide and try again."].Value,
                RequestId = HttpContext.TraceIdentifier
            });
        }

        if (grammarTopic is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.GrammarTopicViewed, $"grammar:{grammarTopic.Slug}");

        return View(new GrammarDetailPageViewModel(grammarTopic, profile.PreferredMeaningLanguage1));
    }
}
