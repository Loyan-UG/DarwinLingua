using System.ComponentModel.DataAnnotations;
using System.Text;
using DarwinLingua.Identity;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class EmailModel(
    UserManager<DarwinLinguaIdentityUser> userManager,
    IAccountEmailService accountEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IOptions<TransactionalEmailOptions> emailOptions,
    IStringLocalizer<SharedResource> localizer) : PageModel
{
    [BindProperty]
    public EmailInputModel Input { get; set; } = new();

    public string CurrentEmail { get; private set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        DarwinLinguaIdentityUser? user = await userManager.GetUserAsync(User).ConfigureAwait(false);
        if (user is null)
        {
            return NotFound();
        }

        CurrentEmail = user.Email ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser? user = await userManager.GetUserAsync(User).ConfigureAwait(false);
        if (user is null)
        {
            return NotFound();
        }

        CurrentEmail = user.Email ?? string.Empty;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string newEmail = Input.NewEmail.Trim();
        if (string.Equals(CurrentEmail, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = localizer["This is already your account email."];
            return RedirectToPage();
        }

        if (!await userManager.CheckPasswordAsync(user, Input.CurrentPassword).ConfigureAwait(false))
        {
            ModelState.AddModelError(string.Empty, localizer["The current password could not be verified."]);
            return Page();
        }

        bool allowed = rateLimiter.TryConsume(
            "change-email",
            $"{User.Identity?.Name}|{newEmail}",
            5,
            TimeSpan.FromMinutes(30));
        if (allowed)
        {
            string code = await userManager.GenerateChangeEmailTokenAsync(user, newEmail).ConfigureAwait(false);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            string callbackUrl = BuildPublicPageUrl(
                "/Account/ConfirmEmailChange",
                new { area = "Identity", userId = user.Id, email = newEmail, code });
            await accountEmailService.SendEmailChangeConfirmationAsync(
                    user,
                    newEmail,
                    callbackUrl,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        StatusMessage = localizer["If the new address can be used, a confirmation email has been sent."];
        return RedirectToPage();
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

    public sealed class EmailInputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;
    }
}
