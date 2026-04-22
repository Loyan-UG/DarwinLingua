using System.Security.Claims;
using DarwinLingua.Web.Models;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarwinLingua.Web.Controllers;

[Authorize]
[Route("account")]
public sealed class AccountController(IWebLearningProfileAccessor learningProfileAccessor) : Controller
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

        return View(new AccountPageViewModel(
            User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Email) ?? "Authenticated user",
            User.FindFirstValue(ClaimTypes.Email),
            roles,
            profile));
    }
}
