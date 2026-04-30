using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/organizer-profiles")]
public sealed class OrganizerProfilesController(
    IWebCatalogApiClient catalogApiClient,
    ICommunityNotificationEmailService notificationEmailService) : Controller
{
    [HttpGet("", Name = "Admin_OrganizerProfiles")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        AdminOrganizerProfilesPageViewModel viewModel = await BuildViewModelAsync(
            new AdminOrganizerProfileInputModel(),
            new AdminOrganizerProfileOwnerInputModel(),
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost("", Name = "Admin_OrganizerProfiles_Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        AdminOrganizerProfileInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid ||
            !IsAllowedOrganizerVerificationStatus(input.VerificationStatus) ||
            !IsAllowedOrganizerPlan(input.PlanKey))
        {
            return View("Index", await BuildViewModelAsync(
                input,
                new AdminOrganizerProfileOwnerInputModel(),
                null,
                "Required organizer profile fields are missing.",
                cancellationToken));
        }

        string[] supportedLearnerLevels = SplitCsv(input.SupportedLearnerLevels);
        string[] helperLanguageCodes = SplitCsv(input.HelperLanguageCodes);
        if (!HasAllowedCefrLevels(supportedLearnerLevels) || !HasAllowedLanguageCodes(helperLanguageCodes))
        {
            return View("Index", await BuildViewModelAsync(
                input,
                new AdminOrganizerProfileOwnerInputModel(),
                null,
                "Supported levels or helper languages contain unsupported values.",
                cancellationToken));
        }

        string slug = input.Slug.Trim();
        string displayName = input.DisplayName.Trim();
        AdminSaveOrganizerProfileRequest request = new(
            slug,
            displayName,
            input.OrganizerType.Trim(),
            input.Description.Trim(),
            TrimToNull(input.CityRegion),
            input.IsOnlineAvailable,
            supportedLearnerLevels,
            helperLanguageCodes,
            TrimToNull(input.WebsiteUrl),
            TrimToNull(input.PublicContactMethod),
            input.VerificationStatus,
            input.PlanKey,
            input.HistoricalEventCount);

        try
        {
            OrganizerProfileDetailModel savedProfile = await catalogApiClient
                .SaveAdminOrganizerProfileAsync(request, cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Saved organizer profile '{savedProfile.DisplayName}'.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("Index", await BuildViewModelAsync(
                input,
                new AdminOrganizerProfileOwnerInputModel(),
                null,
                BuildAdminOperationErrorMessage(exception, "organizer profile"),
                cancellationToken));
        }
    }

    [HttpPost("owners", Name = "Admin_OrganizerProfiles_AssignOwner")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignOwner(
        AdminOrganizerProfileOwnerInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(
                new AdminOrganizerProfileInputModel(),
                input,
                null,
                "Required owner assignment fields are missing.",
                cancellationToken));
        }

        string organizerProfileSlug = input.OrganizerProfileSlug.Trim();
        string ownerEmail = input.OwnerEmail.Trim();
        string assignedBy = WebUserIdentity.TryGetEmail(User)
            ?? User.Identity?.Name
            ?? "web-admin";
        try
        {
            OrganizerProfileOwnerModel owner = await catalogApiClient.AssignAdminOrganizerProfileOwnerAsync(
                    new AssignOrganizerProfileOwnerRequest(organizerProfileSlug, ownerEmail, assignedBy),
                    cancellationToken)
                .ConfigureAwait(false);

            await notificationEmailService.SendOrganizerProfileOwnershipChangedAsync(
                    owner.OwnerEmail,
                    owner.OrganizerProfileSlug,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);

            TempData["StatusMessage"] = $"Assigned {owner.OwnerEmail} to {owner.OrganizerProfileSlug}.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            return View("Index", await BuildViewModelAsync(
                new AdminOrganizerProfileInputModel(),
                input,
                null,
                BuildAdminOperationErrorMessage(exception, "owner assignment"),
                cancellationToken));
        }
    }

    [HttpPost("claims/{claimRequestId:guid}/status", Name = "Admin_OrganizerProfiles_SetClaimStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetClaimStatus(
        Guid claimRequestId,
        AdminOrganizerClaimDecisionInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || !IsAllowedClaimDecisionStatus(input.Status))
        {
            TempData["ErrorMessage"] = "Required claim decision fields are missing.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            OrganizerClaimRequestModel claimRequest = await catalogApiClient
                .SetAdminOrganizerClaimRequestStatusAsync(
                    claimRequestId,
                    new OrganizerClaimDecisionRequest(input.Status),
                    cancellationToken)
                .ConfigureAwait(false);

            if (string.Equals(claimRequest.Status, "approved", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(claimRequest.Status, "rejected", StringComparison.OrdinalIgnoreCase))
            {
                OrganizerProfileListItemModel? profile = (await catalogApiClient
                        .GetOrganizerProfilesAsync(cancellationToken)
                        .ConfigureAwait(false))
                    .FirstOrDefault(profile => string.Equals(profile.Slug, claimRequest.OrganizerProfileSlug, StringComparison.OrdinalIgnoreCase));
                await notificationEmailService.SendOrganizerClaimDecisionAsync(
                        claimRequest.RequesterEmail,
                        profile?.DisplayName ?? claimRequest.OrganizerProfileSlug,
                        claimRequest.Status,
                        ResolveCulture(),
                        HttpContext.TraceIdentifier,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            TempData["StatusMessage"] = BuildClaimDecisionStatusMessage(claimRequest.Status);
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = BuildAdminOperationErrorMessage(exception, "claim decision");
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<AdminOrganizerProfilesPageViewModel> BuildViewModelAsync(
        AdminOrganizerProfileInputModel input,
        AdminOrganizerProfileOwnerInputModel ownerInput,
        string? statusMessage,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        Task<IReadOnlyList<OrganizerProfileListItemModel>> profilesTask = catalogApiClient.GetOrganizerProfilesAsync(cancellationToken);
        Task<IReadOnlyList<OrganizerClaimRequestModel>> claimRequestsTask = catalogApiClient.GetAdminOrganizerClaimRequestsAsync(cancellationToken);
        Task<IReadOnlyList<OrganizerProfileOwnerModel>> ownersTask = catalogApiClient.GetAdminOrganizerProfileOwnersAsync(cancellationToken);

        await Task.WhenAll(profilesTask, claimRequestsTask, ownersTask).ConfigureAwait(false);

        return new AdminOrganizerProfilesPageViewModel(
            await profilesTask.ConfigureAwait(false),
            await claimRequestsTask.ConfigureAwait(false),
            await ownersTask.ConfigureAwait(false),
            input,
            ownerInput,
            statusMessage,
            errorMessage);
    }

    private static string[] SplitCsv(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string BuildAdminOperationErrorMessage(Exception exception, string operationName) =>
        exception.Message.Contains("409", StringComparison.OrdinalIgnoreCase)
            ? $"The {operationName} could not be saved because it conflicts with existing data."
            : $"The {operationName} could not be completed. Review the fields and try again.";

    private static bool IsAllowedOrganizerVerificationStatus(string status) =>
        string.Equals(status, "unverified", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "reviewed", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "verified", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "stale", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedOrganizerPlan(string planKey) =>
        string.Equals(planKey, "free-organizer", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(planKey, "community-organizer", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(planKey, "premium-organizer", StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowedClaimDecisionStatus(string status) =>
        string.Equals(status, "reviewing", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "rejected", StringComparison.OrdinalIgnoreCase);

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

    private static string BuildClaimDecisionStatusMessage(string status)
    {
        if (string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return "Claim request approved and notification email queued. Assign the requester as owner separately if ownership should be granted.";
        }

        if (string.Equals(status, "rejected", StringComparison.OrdinalIgnoreCase))
        {
            return "Claim request rejected and notification email queued.";
        }

        return $"Claim request marked {status}.";
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";
}
