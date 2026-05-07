using System.ComponentModel.DataAnnotations;
using DarwinLingua.Identity;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account;

public sealed class LoginModel(
    SignInManager<DarwinLinguaIdentityUser> signInManager,
    UserManager<DarwinLinguaIdentityUser> userManager,
    IAccountEmailService accountEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IStringLocalizer<SharedResource> localizer) : PageModel
{
    [BindProperty]
    public LoginInputModel Input { get; set; } = new();

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = NormalizeReturnUrl(returnUrl);
        ModelState.Remove(nameof(ReturnUrl));
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme).ConfigureAwait(false);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ReturnUrl = NormalizeReturnUrl(ReturnUrl);
        ModelState.Remove(nameof(ReturnUrl));
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

            ModelState.AddModelError(string.Empty, localizer["This account is temporarily locked. Try again later or contact support."]);
            return Page();
        }

        if (result.IsNotAllowed)
        {
            return RedirectToPage("/Account/CheckEmail", new { area = "Identity" });
        }

        ModelState.AddModelError(string.Empty, localizer["The sign-in attempt could not be completed."]);
        return Page();
    }

    private string NormalizeReturnUrl(string? returnUrl)
    {
        string homeUrl = Url.Content("~/");
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : homeUrl;
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
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
