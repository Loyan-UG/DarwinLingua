using System.ComponentModel.DataAnnotations;
using DarwinLingua.Identity;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account;

public sealed class LoginModel(
    SignInManager<DarwinLinguaIdentityUser> signInManager,
    UserManager<DarwinLinguaIdentityUser> userManager,
    IAccountEmailService accountEmailService,
    IAccountEmailRateLimiter rateLimiter) : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ReturnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string email = Input.Email.Trim();
        DarwinLinguaIdentityUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (user is not null &&
            await userManager.CheckPasswordAsync(user, Input.Password).ConfigureAwait(false) &&
            !await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
        {
            return RedirectToPage("/Account/UnconfirmedAccount", new { area = "Identity", email });
        }

        Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(
                email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : Url.Content("~/"));
        }

        if (result.IsLockedOut)
        {
            if (user is not null &&
                rateLimiter.TryConsume("account-lockout", user.Id, 1, TimeSpan.FromMinutes(30)))
            {
                await accountEmailService.SendAccountLockedAsync(
                        user,
                        ResolveCulture(),
                        HttpContext.TraceIdentifier,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            ModelState.AddModelError(string.Empty, "This account is temporarily locked. Try again later or contact support.");
            return Page();
        }

        if (result.IsNotAllowed)
        {
            return RedirectToPage("/Account/CheckEmail", new { area = "Identity" });
        }

        ModelState.AddModelError(string.Empty, "The sign-in attempt could not be completed.");
        return Page();
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    public sealed class LoginInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
