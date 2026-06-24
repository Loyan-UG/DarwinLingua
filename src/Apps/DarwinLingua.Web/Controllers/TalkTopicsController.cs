using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.TalkTopics)]
public sealed class TalkTopicsController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    IStringLocalizer<SharedResource> localizer,
    ILogger<TalkTopicsController> logger,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "TalkTopics_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(
        string? cefrLevel,
        string? category,
        string? topic,
        string? contentType,
        string? speakingGoal,
        CancellationToken cancellationToken)
    {
        TalkTopicListFilterModel filter = new(
            NormalizeCefrLevel(cefrLevel),
            WebRouteInput.NormalizeSlug(category ?? string.Empty),
            WebRouteInput.NormalizeSlug(topic ?? string.Empty),
            WebRouteInput.NormalizeSlug(contentType ?? string.Empty),
            WebRouteInput.NormalizeSlug(speakingGoal ?? string.Empty),
            null);
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);

        IReadOnlyList<TalkTopicListItemModel> talkTopics;
        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            talkTopics = await catalogApiClient.GetTalkTopicsAsync(filter, targetLearningLanguageCode, catalogTimeout.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Talk topics could not be loaded.");
            talkTopics = [];
        }

        return View(new TalkTopicIndexPageViewModel(
            talkTopics,
            talkTopics.Select(item => item.CefrLevel).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            talkTopics.Select(item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            talkTopics.SelectMany(item => item.TopicKeys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            talkTopics.Select(item => item.ContentType).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            talkTopics.SelectMany(item => item.SpeakingGoals).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            filter.CefrLevel,
            filter.Category,
            filter.TopicKey,
            filter.ContentType,
            filter.SpeakingGoal));
    }

    [HttpGet("{slug}", Name = "TalkTopics_Detail")]
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

        TalkTopicDetailModel? talkTopic;
        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            talkTopic = await catalogApiClient
                .GetTalkTopicBySlugAsync(
                    normalizedSlug,
                    targetLearningLanguageCode,
                    profile.PreferredMeaningLanguage1,
                    effectiveSecondaryMeaningLanguageCode,
                    catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Talk topic detail could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Talk Topic is temporarily unavailable"].Value,
                localizer["This Talk Topic could not be loaded right now. Please return to Talk Topics and try again."].Value);
        }

        if (talkTopic is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.TalkTopicViewed, $"talk-topic:{talkTopic.Slug}");

        return View(new TalkTopicDetailPageViewModel(
            talkTopic,
            profile.PreferredMeaningLanguage1,
            effectiveSecondaryMeaningLanguageCode));
    }

    private static string? NormalizeCefrLevel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string normalized = value.Trim().ToUpperInvariant();
        return normalized is "A1" or "A2" or "B1" or "B2" or "C1" or "C2"
            ? normalized
            : null;
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
