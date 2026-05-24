using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("expressions")]
public sealed class ExpressionsController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IStringLocalizer<SharedResource> localizer,
    ILogger<ExpressionsController> logger,
    IWebProductAnalyticsService? analyticsService = null) : Controller
{
    [HttpGet("", Name = "Expressions_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(
        string? cefrLevel,
        string? expressionType,
        string? register,
        string? category,
        string? topic,
        bool includeRisky,
        string? q,
        CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);

        ExpressionListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            WebRouteInput.NormalizeSlug(expressionType ?? string.Empty),
            WebRouteInput.NormalizeSlug(register ?? string.Empty),
            WebRouteInput.NormalizeSlug(category ?? string.Empty),
            WebRouteInput.NormalizeSlug(topic ?? string.Empty),
            includeRisky ? null : false,
            string.IsNullOrWhiteSpace(q) ? null : q.Trim(),
            profile.PreferredMeaningLanguage1);

        IReadOnlyList<ExpressionListItemModel> expressions;
        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(10));
            expressions = await catalogApiClient.GetExpressionsAsync(filter, catalogTimeout.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && IsCatalogApiFailure(ex))
        {
            logger.LogWarning(ex, "Expressions could not be loaded.");
            expressions = [];
        }

        return View(new ExpressionIndexPageViewModel(
            expressions,
            LearningPortalFilterConventions.CefrLevels,
            expressions.Select(item => item.ExpressionType).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            expressions.Select(item => item.Register).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            expressions.Select(item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            expressions.SelectMany(item => item.TopicKeys).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(item => item).ToArray(),
            filter.CefrLevel,
            filter.ExpressionType,
            filter.Register,
            filter.Category,
            filter.TopicKey,
            includeRisky,
            filter.Query));
    }

    [HttpGet("{slug}", Name = "Expressions_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);

        ExpressionDetailModel? expression;
        try
        {
            using CancellationTokenSource catalogTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            catalogTimeout.CancelAfter(TimeSpan.FromSeconds(2));
            expression = await catalogApiClient
                .GetExpressionBySlugAsync(normalizedSlug, profile.PreferredMeaningLanguage1, catalogTimeout.Token)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && IsCatalogApiFailure(ex))
        {
            logger.LogWarning(ex, "Expression detail could not be loaded for {Slug}.", normalizedSlug);
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return View("~/Views/Shared/Error.cshtml", new ErrorViewModel
            {
                Title = localizer["Expression is temporarily unavailable"].Value,
                Message = localizer["This expression could not be loaded right now. Please return to Everyday Expressions and try again."].Value,
                RequestId = HttpContext.TraceIdentifier
            });
        }

        if (expression is null)
        {
            return NotFound();
        }

        analyticsService?.Record(WebProductAnalyticsEvents.ExpressionViewed, $"expression:{expression.Slug}");

        return View(new ExpressionDetailPageViewModel(expression, profile.PreferredMeaningLanguage1));
    }

    private static bool IsCatalogApiFailure(Exception exception) =>
        exception is HttpRequestException or OperationCanceledException or InvalidOperationException;
}
