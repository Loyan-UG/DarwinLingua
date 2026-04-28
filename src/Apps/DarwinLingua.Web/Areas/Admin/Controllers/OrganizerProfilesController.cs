using DarwinLingua.Catalog.Application.Models;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Operator")]
[Route("admin/organizer-profiles")]
public sealed class OrganizerProfilesController(IWebCatalogApiClient catalogApiClient) : Controller
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

        string assignedBy = User.Identity?.Name ?? "web-admin";
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

    private async Task<AdminOrganizerProfilesPageViewModel> BuildViewModelAsync(
        AdminOrganizerProfileInputModel input,
        AdminOrganizerProfileOwnerInputModel ownerInput,
        string? statusMessage,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OrganizerProfileListItemModel> profiles = await catalogApiClient
            .GetOrganizerProfilesAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<OrganizerClaimRequestModel> claimRequests = await catalogApiClient
            .GetAdminOrganizerClaimRequestsAsync(cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyList<OrganizerProfileOwnerModel> owners = await catalogApiClient
            .GetAdminOrganizerProfileOwnersAsync(cancellationToken)
            .ConfigureAwait(false);

        return new AdminOrganizerProfilesPageViewModel(profiles, claimRequests, owners, input, ownerInput, statusMessage, errorMessage);
    }

    private static string[] SplitCsv(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
