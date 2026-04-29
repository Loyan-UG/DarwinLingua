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
        if (!ModelState.IsValid)
        {
            return View("Index", await BuildViewModelAsync(
                input,
                new AdminOrganizerProfileOwnerInputModel(),
                null,
                "Required organizer profile fields are missing.",
                cancellationToken));
        }

        AdminSaveOrganizerProfileRequest request = new(
            input.Slug,
            input.DisplayName,
            input.OrganizerType,
            input.Description,
            input.CityRegion,
            input.IsOnlineAvailable,
            SplitCsv(input.SupportedLearnerLevels),
            SplitCsv(input.HelperLanguageCodes),
            input.WebsiteUrl,
            input.PublicContactMethod,
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
                exception.Message,
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

        string assignedBy = WebUserIdentity.TryGetEmail(User)
            ?? User.Identity?.Name
            ?? "web-admin";
        try
        {
            OrganizerProfileOwnerModel owner = await catalogApiClient.AssignAdminOrganizerProfileOwnerAsync(
                    new AssignOrganizerProfileOwnerRequest(input.OrganizerProfileSlug, input.OwnerEmail, assignedBy),
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
                exception.Message,
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
        if (!ModelState.IsValid)
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

            TempData["StatusMessage"] = $"Claim request marked {claimRequest.Status}.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException exception)
        {
            TempData["ErrorMessage"] = exception.Message;
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

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";
}
