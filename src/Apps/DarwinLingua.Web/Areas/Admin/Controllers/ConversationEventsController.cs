using System.Globalization;
using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.SharedKernel.Globalization;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/conversation-events")]
public sealed class ConversationEventsController(IWebCatalogApiClient catalogApiClient) : Controller
{
    [HttpGet("", Name = "Admin_ConversationEvents")]
    public async Task<IActionResult> Index(
        string? targetLearningLanguageCode,
        CancellationToken cancellationToken)
    {
        AdminConversationEventsPageViewModel viewModel = await BuildViewModelAsync(
            new AdminConversationEventInputModel
            {
                TargetLearningLanguageCode = ResolveAdminTargetLearningLanguageCode(targetLearningLanguageCode),
            },
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost("", Name = "Admin_ConversationEvents_Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        AdminConversationEventInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid ||
            !IsAllowedTargetLearningLanguage(input.TargetLearningLanguageCode) ||
            !IsAllowedVerificationStatus(input.VerificationStatus) ||
            !IsAllowedPriceType(input.PriceType))
        {
            return View("Index", await BuildViewModelAsync(input, null, "Required event fields are missing or the target language is not active.", cancellationToken));
        }

        string[] supportedLearnerLevels = SplitCsv(input.SupportedLearnerLevels);
        string[] helperLanguageCodes = SplitCsv(input.HelperLanguageCodes);
        string[] linkedPreparationPackSlugs = SplitCsv(input.LinkedEventPreparationPackSlugs);
        if (!HasAllowedCefrLevels(supportedLearnerLevels) ||
            !HasAllowedLanguageCodes(helperLanguageCodes) ||
            !HasAllowedSlugs(linkedPreparationPackSlugs))
        {
            return View("Index", await BuildViewModelAsync(
                input,
                null,
                "Supported levels, helper languages, or preparation pack slugs contain unsupported values.",
                cancellationToken));
        }

        DateTime? lastVerifiedAtUtc = null;
        if (!string.IsNullOrWhiteSpace(input.LastVerifiedAtUtc))
        {
            if (!DateTime.TryParse(
                    input.LastVerifiedAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTime parsedLastVerifiedAtUtc))
            {
                return View("Index", await BuildViewModelAsync(input, null, "Last verified must be a valid UTC date/time value.", cancellationToken));
            }

            lastVerifiedAtUtc = DateTime.SpecifyKind(parsedLastVerifiedAtUtc, DateTimeKind.Utc);
        }

        if (!TryParseOptionalUtc(input.StartsAtUtc, out DateTime? startsAtUtc))
        {
            return View("Index", await BuildViewModelAsync(input, null, "Start time must be a valid UTC date/time value.", cancellationToken));
        }

        if (!TryParseOptionalUtc(input.EndsAtUtc, out DateTime? endsAtUtc))
        {
            return View("Index", await BuildViewModelAsync(input, null, "End time must be a valid UTC date/time value.", cancellationToken));
        }

        string slug = input.Slug.Trim();
        string name = input.Name.Trim();
        AdminSaveConversationEventRequest request = new(
            slug,
            input.TargetLearningLanguageCode.Trim().ToLowerInvariant(),
            name,
            input.Description.Trim(),
            TrimToNull(input.City),
            input.CountryRegion.Trim(),
            TrimToNull(input.ApproximateLocation),
            input.IsOnline,
            input.Category.Trim(),
            supportedLearnerLevels,
            helperLanguageCodes,
            input.OrganizerName.Trim(),
            TrimToNull(input.OrganizerProfileSlug),
            TrimToNull(input.ExternalLink),
            TrimToNull(input.ContactMethod),
            input.ScheduleText.Trim(),
            startsAtUtc,
            endsAtUtc,
            input.PriceType,
            input.VerificationStatus,
            TrimToNull(input.SourceName),
            TrimToNull(input.SourceUrl),
            lastVerifiedAtUtc,
            linkedPreparationPackSlugs)
        {
            RecurrenceRule = TrimToNull(input.RecurrenceRule),
            Capacity = input.Capacity,
        };

        try
        {
            ConversationEventDetailModel savedEvent = await catalogApiClient
                .SaveAdminConversationEventAsync(request, cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Saved event '{savedEvent.Name}'.";
            return RedirectToAction(nameof(Index), new { targetLearningLanguageCode = savedEvent.TargetLearningLanguageCode });
        }
        catch (InvalidOperationException exception)
        {
            return View("Index", await BuildViewModelAsync(
                input,
                null,
                BuildAdminOperationErrorMessage(exception),
                cancellationToken));
        }
    }

    private async Task<AdminConversationEventsPageViewModel> BuildViewModelAsync(
        AdminConversationEventInputModel input,
        string? statusMessage,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ConversationEventListItemModel> events = await catalogApiClient
            .GetConversationEventsAsync(
                new ConversationEventListFilterModel(null, null, null, null, null, null),
                ResolveAdminTargetLearningLanguageCode(input.TargetLearningLanguageCode),
                cancellationToken)
            .ConfigureAwait(false);

        return new AdminConversationEventsPageViewModel(events, input, statusMessage, errorMessage);
    }

    private static string[] SplitCsv(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static string ResolveAdminTargetLearningLanguageCode(string? targetLearningLanguageCode) =>
        TargetLearningLanguageCatalog.TryFindActive(
            TargetLearningLanguageScope.NormalizeOrDefault(
                string.IsNullOrWhiteSpace(targetLearningLanguageCode)
                    ? ContentLanguageRequirements.DefaultTargetLearningLanguageCode
                    : targetLearningLanguageCode),
            out TargetLearningLanguageDefinition language)
            ? language.Code
            : ContentLanguageRequirements.DefaultTargetLearningLanguageCode;

    private static bool IsAllowedTargetLearningLanguage(string? targetLearningLanguageCode) =>
        TargetLearningLanguageCatalog.TryFindActive(
            TargetLearningLanguageScope.NormalizeOrDefault(targetLearningLanguageCode),
            out _);

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static bool TryParseOptionalUtc(string? value, out DateTime? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out DateTime parsed))
        {
            return false;
        }

        result = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
        return true;
    }

    private static bool HasAllowedCefrLevels(IReadOnlyCollection<string> levels) =>
        levels.Count > 0 && levels.All(static level =>
            string.Equals(level, "A1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "A2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "B1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "B2", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "C1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(level, "C2", StringComparison.OrdinalIgnoreCase));

    private static bool HasAllowedLanguageCodes(IReadOnlyCollection<string> languageCodes) =>
        languageCodes.Count == 0 || languageCodes.All(static code =>
            code.Length is >= 2 and <= 8 &&
            code.All(static character =>
                (character >= 'a' && character <= 'z') ||
                (character >= 'A' && character <= 'Z') ||
                character == '-'));

    private static bool HasAllowedSlugs(IReadOnlyCollection<string> slugs) =>
        slugs.Count == 0 || slugs.All(IsSlug);

    private static bool IsSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
        {
            return false;
        }

        string slug = value.Trim();
        bool previousWasDash = true;
        foreach (char character in slug)
        {
            bool isAlphaNumeric = (character >= 'a' && character <= 'z') ||
                (character >= '0' && character <= '9');
            if (isAlphaNumeric)
            {
                previousWasDash = false;
                continue;
            }

            if (character != '-' || previousWasDash)
            {
                return false;
            }

            previousWasDash = true;
        }

        return !previousWasDash;
    }

    private static string BuildAdminOperationErrorMessage(Exception exception) =>
        exception.Message.Contains("409", StringComparison.OrdinalIgnoreCase)
            ? "The event could not be saved because it conflicts with existing data."
            : "The event could not be completed. Review the fields and try again.";

    private static bool IsAllowedVerificationStatus(string status) =>
        string.Equals(status, "unverified", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "reviewed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "verified", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "stale", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedPriceType(string priceType) =>
        string.Equals(priceType, "free", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(priceType, "donation", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(priceType, "paid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(priceType, "unknown", StringComparison.OrdinalIgnoreCase);
}
