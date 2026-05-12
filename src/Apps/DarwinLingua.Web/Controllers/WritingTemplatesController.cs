using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("writing-templates")]
public sealed class WritingTemplatesController(
    IWebCatalogApiClient catalogApiClient,
    ILogger<WritingTemplatesController> logger) : Controller
{
    private static readonly string[] Categories =
    [
        "email-to-school", "email-to-kindergarten", "message-to-landlord",
        "doctor-appointment-request", "appointment-reschedule", "sick-note-to-employer",
        "complaint", "application-email", "cancellation", "insurance-message",
        "government-office-message", "exam-email", "exam-opinion-text"
    ];

    private static readonly string[] Registers = ["formal", "informal", "neutral", "official", "workplace", "exam"];

    [HttpGet("", Name = "WritingTemplates_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? cefrLevel, string? category, string? register, string? q, CancellationToken cancellationToken)
    {
        WritingTemplateListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            NormalizeKey(category),
            NormalizeKey(register),
            null,
            string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        IReadOnlyList<WritingTemplateListItemModel> templates;
        try
        {
            templates = await catalogApiClient.GetWritingTemplatesAsync(filter, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Writing templates could not be loaded.");
            templates = [];
        }

        return View(new WritingTemplateIndexPageViewModel(
            templates,
            LearningPortalFilterConventions.CefrLevels,
            Categories,
            Registers,
            filter.CefrLevel,
            filter.Category,
            filter.Register,
            filter.Query));
    }

    [HttpGet("{slug}", Name = "WritingTemplates_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        WritingTemplateDetailModel? template = await catalogApiClient.GetWritingTemplateBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return template is null ? NotFound() : View(new WritingTemplateDetailPageViewModel(template));
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
