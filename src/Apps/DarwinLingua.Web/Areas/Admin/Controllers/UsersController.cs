using System.Globalization;
using System.Security.Claims;
using DarwinLingua.Identity;
using DarwinLingua.Web.Data;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
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
    WebIdentityDbContext identityDbContext,
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

        string requestedTier = input.Tier.Trim();
        if (!DarwinLinguaEntitlementTiers.All.Contains(requestedTier, StringComparer.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "The selected entitlement tier is not supported.";
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

        if (string.Equals(requestedTier, DarwinLinguaEntitlementTiers.Free, StringComparison.OrdinalIgnoreCase) &&
            expiresAtUtc.HasValue)
        {
            TempData["ErrorMessage"] = "Free entitlements must not have an expiration date.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.Equals(requestedTier, DarwinLinguaEntitlementTiers.Free, StringComparison.OrdinalIgnoreCase) &&
            expiresAtUtc.HasValue &&
            expiresAtUtc.Value <= DateTimeOffset.UtcNow.AddMinutes(-5))
        {
            TempData["ErrorMessage"] = "Expiration must be in the future for trial or premium entitlements.";
            return RedirectToAction(nameof(Index));
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(input.UserId);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user could not be found.";
            return RedirectToAction(nameof(Index));
        }

        string updatedBy = WebUserIdentity.TryGetEmail(User)
            ?? User.Identity?.Name
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? "admin";

        UserEntitlementSnapshot snapshot = await userEntitlementService.SetTierAsync(
            user.Id,
            requestedTier,
            expiresAtUtc,
            updatedBy,
            cancellationToken);

        TempData["StatusMessage"] = $"Updated {user.Email ?? user.UserName ?? user.Id} to {snapshot.Tier}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("role", Name = "Admin_Users_UpdateRole")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(AdminUpdateUserRoleInputModel input)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "The role update request was incomplete.";
            return RedirectToAction(nameof(Index));
        }

        string requestedRole = input.Role.Trim();
        if (!DarwinLinguaRoles.All.Contains(requestedRole, StringComparer.Ordinal))
        {
            TempData["ErrorMessage"] = "The selected role is not supported.";
            return RedirectToAction(nameof(Index));
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(input.UserId);
        if (user is null)
        {
            TempData["ErrorMessage"] = "The selected user could not be found.";
            return RedirectToAction(nameof(Index));
        }

        string? currentUserId = userManager.GetUserId(User);
        if (!input.IsEnabled &&
            string.Equals(user.Id, currentUserId, StringComparison.Ordinal) &&
            string.Equals(requestedRole, DarwinLinguaRoles.Admin, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "You cannot remove the Admin role from your current account.";
            return RedirectToAction(nameof(Index));
        }

        bool alreadyInRole = await userManager.IsInRoleAsync(user, requestedRole);
        IdentityResult result = input.IsEnabled switch
        {
            true when !alreadyInRole => await userManager.AddToRoleAsync(user, requestedRole),
            false when alreadyInRole => await userManager.RemoveFromRoleAsync(user, requestedRole),
            _ => IdentityResult.Success,
        };

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "The role update could not be completed. Review the selected role and user.";
            return RedirectToAction(nameof(Index));
        }

        string action = input.IsEnabled ? "added" : "removed";
        TempData["StatusMessage"] = $"{action} {requestedRole} for {user.Email ?? user.UserName ?? user.Id}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AdminUsersPageViewModel> BuildViewModelAsync(
        string? statusMessage,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser[] users = await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .Take(200)
            .ToArrayAsync(cancellationToken);
        string[] userIds = users
            .Select(static user => user.Id)
            .ToArray();
        Dictionary<string, string[]> rolesByUserId = await LoadRolesByUserIdAsync(userIds, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyDictionary<string, UserEntitlementSnapshot> entitlementsByUserId = await userEntitlementService
            .GetCurrentManyAsync(userIds, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyDictionary<string, IReadOnlyList<UserEntitlementAuditEventModel>> auditEventsByUserId = await userEntitlementService
            .GetRecentAuditEventsManyAsync(userIds, 3, cancellationToken)
            .ConfigureAwait(false);

        List<AdminUserListItemViewModel> items = new(users.Length);
        foreach (DarwinLinguaIdentityUser user in users)
        {
            UserEntitlementSnapshot entitlement = entitlementsByUserId[user.Id];
            IReadOnlyList<UserEntitlementAuditEventModel> auditEvents = auditEventsByUserId.TryGetValue(user.Id, out IReadOnlyList<UserEntitlementAuditEventModel>? events)
                ? events
                : [];

            items.Add(new AdminUserListItemViewModel(
                user.Id,
                user.Email ?? user.UserName ?? user.Id,
                rolesByUserId.TryGetValue(user.Id, out string[]? roles) ? roles : [],
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

    private async Task<Dictionary<string, string[]>> LoadRolesByUserIdAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var roleRows = await identityDbContext.UserRoles
            .AsNoTracking()
            .Where(userRole => userIds.Contains(userRole.UserId))
            .Join(
                identityDbContext.Roles.AsNoTracking(),
                userRole => userRole.RoleId,
                role => role.Id,
                (userRole, role) => new { userRole.UserId, role.Name })
            .ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return roleRows
            .GroupBy(static row => row.UserId, StringComparer.Ordinal)
            .ToDictionary(
                static group => group.Key,
                static group => group
                    .Select(static row => row.Name)
                    .Where(static roleName => !string.IsNullOrWhiteSpace(roleName))
                    .Select(static roleName => roleName!)
                    .OrderBy(static roleName => roleName)
                    .ToArray(),
                StringComparer.Ordinal);
    }
}
