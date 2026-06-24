using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route(DarwinLingua.Web.Services.LearningRouteConventions.CountryGuidance)]
public sealed class CountryGuidanceController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    ILogger<CountryGuidanceController> logger) : Controller
{
    private static readonly string[] Categories =
    [
        "du-vs-sie", "politeness", "directness", "small-talk", "workplace-culture",
        "office-communication", "school-kindergarten", "doctor-visit", "appointments",
        "punctuality", "complaints", "bureaucracy", "conversation-cafe-etiquette",
        "law-and-rights", "democracy-and-state", "history-and-responsibility",
        "society-and-family", "education-and-work", "religion-and-tolerance",
        "equality-and-non-discrimination", "federal-states-and-geography",
        "political-participation", "social-system", "exam-orientation"
    ];

    [HttpGet("")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string countryContextCode, string? cefrLevel, string? category, string? q, CancellationToken cancellationToken)
    {
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        string? normalizedCountryContextCode = NormalizeCountryContextCode(countryContextCode, targetLearningLanguageCode);
        if (normalizedCountryContextCode is null)
        {
            return NotFound();
        }

        CountryGuidanceNoteListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            NormalizeKey(category),
            null,
            string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = string.IsNullOrWhiteSpace(profile.PreferredMeaningLanguage1) ? "en" : profile.PreferredMeaningLanguage1;
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<CountryGuidanceNoteListItemModel> notes;
        IReadOnlyList<CountryGuidanceNoteListItemModel> secondaryNotes = [];
        try
        {
            notes = await catalogApiClient.GetCountryGuidanceAsync(filter, targetLearningLanguageCode, normalizedCountryContextCode, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode))
            {
                secondaryNotes = await catalogApiClient.GetCountryGuidanceAsync(filter, targetLearningLanguageCode, normalizedCountryContextCode, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Country guidance notes could not be loaded.");
            notes = [];
            secondaryNotes = [];
        }

        Dictionary<string, CountryGuidanceNoteListItemModel> secondaryNotesBySlug = secondaryNotes
            .ToDictionary(static note => note.Slug, StringComparer.OrdinalIgnoreCase);
        CountryGuidanceNoteListItemPageViewModel[] noteViewModels = notes
            .Select(note => new CountryGuidanceNoteListItemPageViewModel(
                note,
                secondaryNotesBySlug.GetValueOrDefault(note.Slug)))
            .ToArray();

        return View(new CountryGuidanceNoteIndexPageViewModel(
            noteViewModels,
            normalizedCountryContextCode,
            LearningPortalFilterConventions.CefrLevels,
            Categories,
            filter.CefrLevel,
            filter.Category,
            filter.Query,
            primaryMeaningLanguageCode,
            secondaryMeaningLanguageCode));
    }

    [HttpGet("{slug}")]
    [OutputCache(NoStore = true)]
    public async Task<IActionResult> Detail(string countryContextCode, string slug, CancellationToken cancellationToken)
    {
        string targetLearningLanguageCode = LearningRouteConventions.ResolveTargetLearningLanguageCode(HttpContext);
        string? normalizedCountryContextCode = NormalizeCountryContextCode(countryContextCode, targetLearningLanguageCode);
        if (normalizedCountryContextCode is null)
        {
            return NotFound();
        }

        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = string.IsNullOrWhiteSpace(profile.PreferredMeaningLanguage1) ? "en" : profile.PreferredMeaningLanguage1;
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        CountryGuidanceNoteDetailModel? note = await catalogApiClient.GetCountryGuidanceBySlugAsync(slug, targetLearningLanguageCode, normalizedCountryContextCode, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        if (note is null)
        {
            return NotFound();
        }

        CountryGuidanceNoteDetailModel? secondaryLanguageNote = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : await catalogApiClient.GetCountryGuidanceBySlugAsync(slug, targetLearningLanguageCode, normalizedCountryContextCode, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);

        return View(new CountryGuidanceNoteDetailPageViewModel(note, secondaryLanguageNote, primaryMeaningLanguageCode, secondaryMeaningLanguageCode));
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string? NormalizeCountryContextCode(string? value, string targetLearningLanguageCode)
    {
        string normalized = string.IsNullOrWhiteSpace(value)
            ? CountryContextCatalog.ResolveDefaultActiveCode(targetLearningLanguageCode)
            : value.Trim().ToUpperInvariant();

        return CountryContextCatalog.TryFindActive(normalized, targetLearningLanguageCode, out CountryContextDefinition countryContext)
            ? countryContext.Code
            : null;
    }
}
