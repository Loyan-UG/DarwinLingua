using System.Text;
using DarwinLingua.Identity;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account;

[Authorize]
public sealed class ConfirmEmailChangeModel(
    UserManager<DarwinLinguaIdentityUser> userManager,
    SignInManager<DarwinLinguaIdentityUser> signInManager,
    IAccountEmailService accountEmailService) : PageModel
{
    public bool Succeeded { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        string? userId,
        string? email,
        string? code,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
        {
            return Page();
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return Page();
        }

        string oldEmail = user.Email ?? string.Empty;
        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Page();
        }

        IdentityResult result = await userManager.ChangeEmailAsync(user, email, token).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return Page();
        }

        await userManager.SetUserNameAsync(user, email).ConfigureAwait(false);
        await signInManager.RefreshSignInAsync(user).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(oldEmail))
        {
            await accountEmailService.SendEmailChangedNotificationAsync(
                    user,
                    oldEmail,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        Succeeded = true;
        return Page();
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";
}
