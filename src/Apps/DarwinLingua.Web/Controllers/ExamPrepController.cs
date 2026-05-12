using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("exam-prep")]
public sealed class ExamPrepController(IWebCatalogApiClient catalogApiClient, ILogger<ExamPrepController> logger) : Controller
{
    private static readonly string[] Sections = ["speaking", "writing", "reading", "listening", "grammar-vocabulary", "overview", "strategy", "mock-task"];
    private static readonly string[] TaskTypes = ["overview", "strategy", "roleplay", "discussion", "presentation", "email", "opinion-text", "form-filling", "reading-task", "listening-task", "grammar-vocabulary", "mock-task", "scoring-checklist"];
    private static readonly string[] SkillFocuses = ["grammar", "vocabulary", "reading", "listening", "speaking", "writing", "pronunciation", "exam-preparation"];

    [HttpGet("", Name = "ExamPrep_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? examProfile, string? cefrLevel, string? section, string? taskType, string? skillFocus, string? q, CancellationToken cancellationToken)
    {
        ExamPrepListFilterModel filter = new(NormalizeKey(examProfile), LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel), NormalizeKey(skillFocus), NormalizeKey(taskType), NormalizeKey(section), string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        IReadOnlyList<ExamProfileModel> profiles;
        IReadOnlyList<ExamPrepUnitListItemModel> units;
        try
        {
            profiles = await catalogApiClient.GetExamProfilesAsync(cancellationToken).ConfigureAwait(false);
            units = await catalogApiClient.GetExamPrepUnitsAsync(filter, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Exam prep content could not be loaded.");
            profiles = [];
            units = [];
        }

        return View(new ExamPrepIndexPageViewModel(profiles, units, LearningPortalFilterConventions.CefrLevels, Sections, TaskTypes, SkillFocuses, filter.ExamProfile, filter.CefrLevel, filter.Section, filter.TaskType, filter.SkillFocus, filter.Query));
    }

    [HttpGet("profile/{examProfileKey}", Name = "ExamPrep_Profile")]
    public Task<IActionResult> Profile(string examProfileKey, CancellationToken cancellationToken) =>
        Index(examProfileKey, null, null, null, null, null, cancellationToken);

    [HttpGet("{slug}", Name = "ExamPrep_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        ExamPrepUnitDetailModel? unit = await catalogApiClient.GetExamPrepUnitBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return unit is null ? NotFound() : View(new ExamPrepDetailPageViewModel(unit));
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
