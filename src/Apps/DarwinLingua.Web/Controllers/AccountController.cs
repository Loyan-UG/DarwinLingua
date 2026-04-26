using System.Security.Claims;
using DarwinLingua.Identity;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController(
    IWebLearningProfileAccessor learningProfileAccessor,
    IUserEntitlementService userEntitlementService) : Controller
{
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

        return View(new AccountPageViewModel(
            User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "Authenticated user",
            User.FindFirstValue(ClaimTypes.Email),
            roles,
            profile,
            entitlement));
    }
}
