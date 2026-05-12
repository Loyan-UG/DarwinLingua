using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace DarwinLingua.Web.Controllers;

[Route("cultural-notes")]
public sealed class CulturalNotesController(
    IWebCatalogApiClient catalogApiClient,
    ILogger<CulturalNotesController> logger) : Controller
{
    private static readonly string[] Categories =
    [
        "du-vs-sie", "politeness", "directness", "small-talk", "workplace-culture",
        "office-communication", "school-kindergarten", "doctor-visit", "appointments",
        "punctuality", "complaints", "bureaucracy", "conversation-cafe-etiquette"
    ];

    [HttpGet("", Name = "CulturalNotes_Index")]
    [OutputCache(PolicyName = "CatalogBrowse")]
    public async Task<IActionResult> Index(string? cefrLevel, string? category, string? q, CancellationToken cancellationToken)
    {
        CulturalNoteListFilterModel filter = new(
            LearningPortalFilterConventions.NormalizeCefrLevel(cefrLevel),
            NormalizeKey(category),
            null,
            string.IsNullOrWhiteSpace(q) ? null : q.Trim());
        IReadOnlyList<CulturalNoteListItemModel> notes;
        try
        {
            notes = await catalogApiClient.GetCulturalNotesAsync(filter, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested && ex is HttpRequestException or OperationCanceledException)
        {
            logger.LogWarning(ex, "Cultural notes could not be loaded.");
            notes = [];
        }

        return View(new CulturalNoteIndexPageViewModel(
            notes,
            LearningPortalFilterConventions.CefrLevels,
            Categories,
            filter.CefrLevel,
            filter.Category,
            filter.Query));
    }

    [HttpGet("{slug}", Name = "CulturalNotes_Detail")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        CulturalNoteDetailModel? note = await catalogApiClient.GetCulturalNoteBySlugAsync(slug, cancellationToken).ConfigureAwait(false);
        return note is null ? NotFound() : View(new CulturalNoteDetailPageViewModel(note));
    }

    private static string? NormalizeKey(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
}
