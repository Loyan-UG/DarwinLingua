using System.ComponentModel.DataAnnotations;
using System.Text;
using DarwinLingua.Identity;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account;

public sealed class RegisterModel(
    UserManager<DarwinLinguaIdentityUser> userManager,
    IUserEntitlementService userEntitlementService,
    IAccountEmailService accountEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IPolicyAcceptanceService policyAcceptanceService,
    IOptions<TransactionalEmailOptions> emailOptions) : PageModel
{
    [BindProperty]
    public RegisterInputModel Input { get; set; } = new();

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = NormalizeReturnUrl(returnUrl);
        ModelState.Remove(nameof(ReturnUrl));
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        ReturnUrl = NormalizeReturnUrl(ReturnUrl);
        ModelState.Remove(nameof(ReturnUrl));
        ValidateRequiredAcknowledgements();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string email = Input.Email.Trim();
        if (!rateLimiter.TryConsume("register", $"{GetRemoteAddressKey()}|{email}", 10, TimeSpan.FromMinutes(30)))
        {
            return RedirectToPage("/Account/CheckEmail", new { area = "Identity" });
        }

        DarwinLinguaIdentityUser? existingUser = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
        if (existingUser is not null)
        {
            if (!await userManager.IsEmailConfirmedAsync(existingUser).ConfigureAwait(false) &&
                rateLimiter.TryConsume("register-confirmation", email, 3, TimeSpan.FromMinutes(30)))
            {
                await SendConfirmationAsync(existingUser, ReturnUrl, cancellationToken).ConfigureAwait(false);
            }

            return RedirectToPage("/Account/CheckEmail", new { area = "Identity" });
        }

        DarwinLinguaIdentityUser user = new()
        {
            UserName = email,
            Email = email,
        };

        IdentityResult result = await userManager.CreateAsync(user, Input.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await userManager.AddToRoleAsync(user, DarwinLinguaRoles.Learner).ConfigureAwait(false);
        await userEntitlementService.GetCurrentAsync(user.Id, cancellationToken).ConfigureAwait(false);
        await policyAcceptanceService
            .RecordRegistrationAcceptancesAsync(user.Id, ResolveCulture(), cancellationToken)
            .ConfigureAwait(false);

        if (rateLimiter.TryConsume("register-confirmation", email, 3, TimeSpan.FromMinutes(30)))
        {
            await SendConfirmationAsync(user, ReturnUrl, cancellationToken).ConfigureAwait(false);
        }

        return RedirectToPage("/Account/CheckEmail", new { area = "Identity" });
    }

    private async Task SendConfirmationAsync(
        DarwinLinguaIdentityUser user,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        string code = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        string callbackUrl = BuildPublicPageUrl(
            "/Account/ConfirmEmail",
            new { area = "Identity", userId = user.Id, code, returnUrl = NormalizeReturnUrl(returnUrl) });
        await accountEmailService.SendEmailConfirmationAsync(
                user,
                callbackUrl,
                ResolveCulture(),
                HttpContext.TraceIdentifier,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private string BuildPublicPageUrl(string page, object values)
    {
        string path = Url.Page(page, null, values) ?? "/";
        if (!string.IsNullOrWhiteSpace(emailOptions.Value.PublicBaseUrl) &&
            Uri.TryCreate(emailOptions.Value.PublicBaseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            return new Uri(baseUri, path).ToString();
        }

        return Url.Page(page, null, values, Request.Scheme) ?? path;
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    private string GetRemoteAddressKey() =>
        HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private string NormalizeReturnUrl(string? returnUrl) =>
        Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");

    private void ValidateRequiredAcknowledgements()
    {
        if (!Input.AcceptTermsOfUse)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.AcceptTermsOfUse)}",
                "You must accept the Terms of Use to create an account.");
        }

        if (!Input.AcknowledgePrivacyNotice)
        {
            ModelState.AddModelError(
                $"{nameof(Input)}.{nameof(Input.AcknowledgePrivacyNotice)}",
                "You must acknowledge the Privacy Policy notice to create an account.");
        }
    }

    public sealed class RegisterInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "I agree to the Terms of Use.")]
        public bool AcceptTermsOfUse { get; set; }

        [Display(Name = "I understand that Darwin Lingua processes account and learning data as described in the Privacy Policy.")]
        public bool AcknowledgePrivacyNotice { get; set; }
    }
}
