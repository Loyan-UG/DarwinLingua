using System.Globalization;
using System.Security.Claims;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DarwinLingua.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "Admin")]
[Route("admin/users")]
public sealed class UsersController(
    UserManager<DarwinLinguaIdentityUser> userManager,
    IUserEntitlementService userEntitlementService) : Controller
{
    [HttpGet("", Name = "Admin_Users")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        AdminUsersPageViewModel viewModel = await BuildViewModelAsync(
            TempData["StatusMessage"] as string,
            TempData["ErrorMessage"] as string,
            cancellationToken);

        return View(viewModel);
    }

    [HttpPost("entitlement", Name = "Admin_Users_UpdateEntitlement")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateEntitlement(
        AdminUpdateUserEntitlementInputModel input,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "The entitlement update request was incomplete.";
            return RedirectToAction(nameof(Index));
        }

        if (!DarwinLinguaEntitlementTiers.All.Contains(input.Tier, StringComparer.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = $"'{input.Tier}' is not a supported entitlement tier.";
            return RedirectToAction(nameof(Index));
        }

        DateTimeOffset? expiresAtUtc = null;
        if (!string.IsNullOrWhiteSpace(input.ExpiresAtUtc))
        {
            if (!DateTimeOffset.TryParse(
                    input.ExpiresAtUtc,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out DateTimeOffset parsedExpiresAtUtc))
            {
                TempData["ErrorMessage"] = "Expiration must be a valid UTC date/time value.";
                return RedirectToAction(nameof(Index));
            }

            expiresAtUtc = parsedExpiresAtUtc;
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(input.UserId);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user could not be found.";
            return RedirectToAction(nameof(Index));
        }

        string updatedBy = User.FindFirstValue(ClaimTypes.Email)
            ?? User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "admin";

        UserEntitlementSnapshot snapshot = await userEntitlementService.SetTierAsync(
            user.Id,
            input.Tier,
            expiresAtUtc,
            updatedBy,
            cancellationToken);

        TempData["StatusMessage"] = $"Updated {user.Email ?? user.UserName ?? user.Id} to {snapshot.Tier}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminUsersPageViewModel> BuildViewModelAsync(
        string? statusMessage,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser[] users = await userManager.Users
            .OrderBy(user => user.Email)
            .Take(200)
            .ToArrayAsync(cancellationToken);

        List<AdminUserListItemViewModel> items = new(users.Length);
        foreach (DarwinLinguaIdentityUser user in users)
        {
            IList<string> roles = await userManager.GetRolesAsync(user);
            UserEntitlementSnapshot entitlement = await userEntitlementService
                .GetCurrentAsync(user.Id, cancellationToken);
            IReadOnlyList<UserEntitlementAuditEventModel> auditEvents = await userEntitlementService
                .GetRecentAuditEventsAsync(user.Id, 3, cancellationToken);

            items.Add(new AdminUserListItemViewModel(
                user.Id,
                user.Email ?? user.UserName ?? user.Id,
                roles.OrderBy(static role => role).ToArray(),
                entitlement.Tier,
                entitlement.TrialEndsAtUtc,
                entitlement.PremiumEndsAtUtc,
                entitlement.EnabledFeatures,
                auditEvents
                    .Select(static auditEvent => new AdminUserEntitlementAuditEventViewModel(
                        auditEvent.EventType,
                        auditEvent.PreviousTier,
                        auditEvent.NewTier,
                        auditEvent.UpdatedBy,
                        auditEvent.CreatedAtUtc))
                    .ToArray()));
        }

        return new AdminUsersPageViewModel(items, statusMessage, errorMessage);
    }
}
