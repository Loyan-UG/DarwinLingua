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
            return View("Index", await BuildViewModelAsync(input, null, "Required organizer profile fields are missing.", cancellationToken));
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
            return View("Index", await BuildViewModelAsync(input, null, exception.Message, cancellationToken));
        }
    }

    private async Task<AdminOrganizerProfilesPageViewModel> BuildViewModelAsync(
        AdminOrganizerProfileInputModel input,
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

        return new AdminOrganizerProfilesPageViewModel(profiles, claimRequests, input, statusMessage, errorMessage);
    }

    private static string[] SplitCsv(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
