using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Controllers;

[Route("roleplays")]
public sealed class RoleplaysController(
    IWebCatalogApiClient catalogApiClient,
    ILogger<RoleplaysController> logger,
    IStringLocalizer<SharedResource> localizer) : Controller
{
    [HttpGet("")]
    [OutputCache(Duration = 60, VaryByQueryKeys = ["cefrLevel", "category", "examProfile", "taskType", "interactionMode", "register", "q"])]
    public async Task<IActionResult> Index(
        string? cefrLevel,
        string? category,
        string? topicKey,
        string? examProfile,
        string? skillFocus,
        string? taskType,
        string? interactionMode,
        string? register,
        string? q,
        CancellationToken cancellationToken)
    {
        RoleplayScenarioListFilterModel filter = new(cefrLevel, category, topicKey, examProfile, skillFocus, taskType, interactionMode, register, q);

        try
        {
            IReadOnlyList<RoleplayScenarioListItemModel> roleplays = await catalogApiClient
                .GetRoleplaysAsync(filter, ResolveMeaningLanguage(), cancellationToken)
                .ConfigureAwait(false);

            return View(new RoleplayIndexPageViewModel(roleplays, filter));
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Roleplay scenarios could not be loaded.");
            return ServiceUnavailableView(
                localizer["Roleplays are temporarily unavailable"],
                localizer["Roleplays could not be loaded right now. Please try again later."]);
        }
    }

    [HttpGet("{slug}", Name = "Roleplays_Detail")]
    [OutputCache(Duration = 60, VaryByRouteValueNames = ["slug"])]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        string? normalizedSlug = WebRouteInput.NormalizeSlug(slug);
        if (normalizedSlug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            RoleplayScenarioDetailModel? roleplay = await catalogApiClient
                .GetRoleplayBySlugAsync(normalizedSlug, ResolveMeaningLanguage(), cancellationToken)
                .ConfigureAwait(false);

            return roleplay is null ? NotFound() : View(new RoleplayDetailPageViewModel(roleplay));
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is (HttpRequestException or OperationCanceledException))
        {
            logger.LogWarning(ex, "Roleplay scenario could not be loaded for {Slug}.", normalizedSlug);
            return ServiceUnavailableView(
                localizer["Roleplay is temporarily unavailable"],
                localizer["This roleplay could not be loaded right now. Please try again later."]);
        }
    }

    private string ResolveMeaningLanguage() =>
        Request.Query.TryGetValue("primaryMeaningLanguageCode", out Microsoft.Extensions.Primitives.StringValues value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : "en";

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
