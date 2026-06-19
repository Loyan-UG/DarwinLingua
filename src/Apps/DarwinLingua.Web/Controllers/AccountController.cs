using System.Security.Claims;
using System.Text.Json;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController(
    IWebLearningProfileAccessor learningProfileAccessor,
    IUserEntitlementService userEntitlementService,
    UserManager<DarwinLinguaIdentityUser> userManager,
    SignInManager<DarwinLinguaIdentityUser> signInManager,
    IAccountDataSelfService accountDataSelfService) : Controller
{
    private static readonly JsonSerializerOptions ExportJsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    [HttpGet("", Name = "Account_Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var profile = await learningProfileAccessor.GetProfileAsync(cancellationToken);
        string[] roles = User.Claims
            .Where(static claim => claim.Type == ClaimTypes.Role)
            .Select(claim => claim.Value)
            .OrderBy(static role => role)
            .ToArray();
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The authenticated user does not have a stable identifier.");
        UserEntitlementSnapshot entitlement = await userEntitlementService.GetCurrentAsync(userId, cancellationToken);
        IReadOnlyList<UserEntitlementAuditEventModel> entitlementAuditEvents = await userEntitlementService
            .GetRecentAuditEventsAsync(userId, 5, cancellationToken);
        string? email = WebUserIdentity.TryGetEmail(User);

        return View(new AccountPageViewModel(
            email ?? User.Identity?.Name ?? "Authenticated user",
            email,
            roles,
            profile,
            entitlement,
            entitlementAuditEvents,
            accountDataSelfService.DeleteConfirmationPhrase));
    }

    [HttpGet("export-data", Name = "Account_ExportData")]
    public async Task<IActionResult> ExportData(CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser user = await GetCurrentUserAsync().ConfigureAwait(false);
        IReadOnlyCollection<string> roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToArray();
        AccountDataExportModel export = await accountDataSelfService
            .ExportAsync(user, roles, cancellationToken)
            .ConfigureAwait(false);

        byte[] payload = JsonSerializer.SerializeToUtf8Bytes(export, ExportJsonOptions);
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
        return File(payload, "application/json", $"darwinlingua-account-export-{timestamp}.json");
    }

    [HttpPost("delete", Name = "Account_Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        [FromForm] string? currentPassword,
        [FromForm] string? confirmationPhrase,
        CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser user = await GetCurrentUserAsync().ConfigureAwait(false);
        AccountDeletionResult result = await accountDataSelfService
            .DeleteAsync(user, currentPassword, confirmationPhrase, cancellationToken)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            TempData["StatusMessage"] = result.ErrorMessage ?? "Account deletion could not be completed.";
            return RedirectToAction(nameof(Index));
        }

        await signInManager.SignOutAsync().ConfigureAwait(false);
        TempData["StatusMessage"] =
            $"Your account was deleted. Removed rows: {result.Counts.RemovedRows}; anonymized rows: {result.Counts.AnonymizedRows}; detached audit rows: {result.Counts.DetachedAuditRows}.";
        return RedirectToAction("Index", "Home");
    }

    private async Task<DarwinLinguaIdentityUser> GetCurrentUserAsync()
    {
        DarwinLinguaIdentityUser? user = await userManager.GetUserAsync(User).ConfigureAwait(false);
        return user ?? throw new InvalidOperationException("The authenticated user could not be loaded.");
    }
}
