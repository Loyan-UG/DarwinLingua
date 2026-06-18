using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Learning.Application.Abstractions;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("life-in-germany")]
public sealed class CulturalNotesController(
    IWebCatalogApiClient catalogApiClient,
    IWebLearningProfileAccessor learningProfileAccessor,
    IWebEntitledFeatureAccessService featureAccessService,
    ILogger<CulturalNotesController> logger) : Controller
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
    public async Task<IActionResult> Index(string? cefrLevel, string? category, string? q, CancellationToken cancellationToken)
    {
        CulturalNoteListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            NormalizeKey(category),
            null,
            string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = string.IsNullOrWhiteSpace(profile.PreferredMeaningLanguage1) ? "en" : profile.PreferredMeaningLanguage1;
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<CulturalNoteListItemModel> notes;
        IReadOnlyList<CulturalNoteListItemModel> secondaryNotes = [];
        try
        {
            notes = await catalogApiClient.GetCulturalNotesAsync(filter, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode))
            {
                secondaryNotes = await catalogApiClient.GetCulturalNotesAsync(filter, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Cultural notes could not be loaded.");
            notes = [];
            secondaryNotes = [];
        }

        Dictionary<string, CulturalNoteListItemModel> secondaryNotesBySlug = secondaryNotes
            .ToDictionary(static note => note.Slug, StringComparer.OrdinalIgnoreCase);
        CulturalNoteListItemPageViewModel[] noteViewModels = notes
            .Select(note => new CulturalNoteListItemPageViewModel(
                note,
                secondaryNotesBySlug.GetValueOrDefault(note.Slug)))
            .ToArray();

        return View(new CulturalNoteIndexPageViewModel(
            noteViewModels,
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
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken).ConfigureAwait(false);
        string primaryMeaningLanguageCode = string.IsNullOrWhiteSpace(profile.PreferredMeaningLanguage1) ? "en" : profile.PreferredMeaningLanguage1;
        string? secondaryMeaningLanguageCode = await featureAccessService
            .ResolveSecondaryMeaningLanguageAsync(profile.PreferredMeaningLanguage2, cancellationToken)
            .ConfigureAwait(false);

        CulturalNoteDetailModel? note = await catalogApiClient.GetCulturalNoteBySlugAsync(slug, primaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);
        if (note is null)
        {
            return NotFound();
        }

        CulturalNoteDetailModel? secondaryLanguageNote = string.IsNullOrWhiteSpace(secondaryMeaningLanguageCode)
            ? null
            : await catalogApiClient.GetCulturalNoteBySlugAsync(slug, secondaryMeaningLanguageCode, cancellationToken).ConfigureAwait(false);

        return View(new CulturalNoteDetailPageViewModel(note, secondaryLanguageNote, primaryMeaningLanguageCode, secondaryMeaningLanguageCode));
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
