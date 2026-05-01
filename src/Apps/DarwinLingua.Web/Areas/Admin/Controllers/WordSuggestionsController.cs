using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/word-suggestions")]
public sealed class WordSuggestionsController(IWebWordSuggestionService wordSuggestionService) : Controller
{
    [HttpGet("", Name = "Admin_WordSuggestions_Index")]
    public async Task<IActionResult> Index(string? status, CancellationToken cancellationToken)
    {
        WebWordSuggestionStatus? normalizedStatus = NormalizeStatus(status);
        IReadOnlyList<WordSuggestionListItemViewModel> suggestions = await wordSuggestionService
            .GetSuggestionsAsync(normalizedStatus, cancellationToken)
            .ConfigureAwait(false);

        return View(new AdminWordSuggestionsPageViewModel(
            normalizedStatus?.ToString().ToLowerInvariant(),
            suggestions,
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string));
    }

    [HttpPost("{suggestionId:guid}/decision", Name = "Admin_WordSuggestions_Decide")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(
        Guid suggestionId,
        AdminWordSuggestionDecisionInputModel input,
        [FromForm] string? returnStatus,
        CancellationToken cancellationToken)
    {
        if (!TryParseStatus(input.Status, out WebWordSuggestionStatus status))
        {
            TempData["ErrorMessage"] = "Invalid suggestion status.";
            return RedirectToAction(nameof(Index), new { status = returnStatus });
        }

        bool updated = await wordSuggestionService
            .UpdateSuggestionStatusAsync(suggestionId, status, input.AdminNote, GetAdminEmail(), cancellationToken)
            .ConfigureAwait(false);

        TempData[updated ? "StatusMessage" : "ErrorMessage"] = updated
            ? "Suggestion status saved."
            : "The suggestion was not found.";

        return RedirectToAction(nameof(Index), new { status = returnStatus });
    }

    private static WebWordSuggestionStatus? NormalizeStatus(string? status) =>
        TryParseStatus(status, out WebWordSuggestionStatus value) ? value : null;

    private static bool TryParseStatus(string? status, out WebWordSuggestionStatus value) =>
        Enum.TryParse(status, ignoreCase: true, out value);

    private string GetAdminEmail() =>
        WebUserIdentity.TryGetEmail(User) ?? "admin@local";
}
