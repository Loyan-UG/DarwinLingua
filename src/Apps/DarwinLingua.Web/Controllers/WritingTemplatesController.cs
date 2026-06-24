using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.WritingTemplates)]
public sealed class WritingTemplatesController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
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
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Index(string? cefrLevel, string? category, string? register, string? q, CancellationToken cancellationToken)
    {
        WritingTemplateListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            NormalizeKey(category),
            NormalizeKey(register),
            null,
            string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = string.IsNullOrWhiteSpace(profile.PreferredMeaningLanguage1) ? "en" : profile.PreferredMeaningLanguage1;
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<WritingTemplateListItemModel> templates;
        IReadOnlyList<WritingTemplateListItemModel> secondaryTemplates = [];
        try
        {
            templates = await catalogApiClient.GetWritingTemplatesAsync(filter, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode))
            {
                secondaryTemplates = await catalogApiClient.GetWritingTemplatesAsync(filter, targetLearningLanguageCode, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Writing templates could not be loaded.");
            templates = [];
            secondaryTemplates = [];
        }

        Dictionary<string, WritingTemplateListItemModel> secondaryTemplatesBySlug = secondaryTemplates
            .ToDictionary(static template => template.Slug, StringComparer.OrdinalIgnoreCase);
        WritingTemplateListItemPageViewModel[] templateViewModels = templates
            .Select(template => new WritingTemplateListItemPageViewModel(
                template,
                secondaryTemplatesBySlug.GetValueOrDefault(template.Slug)))
            .ToArray();

        return View(new WritingTemplateIndexPageViewModel(
            templateViewModels,
            LearningPortalFilterConventions.CefrLevels,
            Categories,
            Registers,
            filter.CefrLevel,
            filter.Category,
            filter.Register,
            filter.Query,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode));
    }

    [HttpGet("{slug}", Name = "WritingTemplates_Detail")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = string.IsNullOrWhiteSpace(profile.PreferredMeaningLanguage1) ? "en" : profile.PreferredMeaningLanguage1;
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        WritingTemplateDetailModel? template = await catalogApiClient.GetWritingTemplateBySlugAsync(slug, targetLearningLanguageCode, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        if (template is null)
        {
            return NotFound();
        }

        WritingTemplateDetailModel? secondaryLanguageTemplate = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : await catalogApiClient.GetWritingTemplateBySlugAsync(slug, targetLearningLanguageCode, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);

        return View(new WritingTemplateDetailPageViewModel(template, secondaryLanguageTemplate, primaryMeaningLanguageCode, secondaryMeaningLanguageCode));
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
