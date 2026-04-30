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

public sealed class ResendEmailConfirmationModel(
    UserManager<DarwinLinguaIdentityUser> userManager,
    IAccountEmailService accountEmailService,
    IAccountEmailRateLimiter rateLimiter,
    IOptions<TransactionalEmailOptions> emailOptions) : PageModel
{
    [BindProperty]
    public ResendEmailConfirmationInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string email = Input.Email.Trim();
        bool ipAllowed = rateLimiter.TryConsume(
            "resend-confirmation-ip",
            GetRemoteAddressKey(),
            20,
            TimeSpan.FromMinutes(30));
        bool emailAllowed = rateLimiter.TryConsume(
            "resend-confirmation",
            $"{GetRemoteAddressKey()}|{email}",
            5,
            TimeSpan.FromMinutes(30));
        if (ipAllowed && emailAllowed)
        {
            DarwinLinguaIdentityUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);
            if (user is not null && !await userManager.IsEmailConfirmedAsync(user).ConfigureAwait(false))
            {
                string code = await userManager.GenerateEmailConfirmationTokenAsync(user).ConfigureAwait(false);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                string callbackUrl = BuildPublicPageUrl(
                    "/Account/ConfirmEmail",
                    new { area = "Identity", userId = user.Id, code });
                await accountEmailService.SendEmailConfirmationAsync(
                        user,
                        callbackUrl,
                        ResolveCulture(),
                        HttpContext.TraceIdentifier,
                        cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        return RedirectToPage("/Account/CheckEmail", new { area = "Identity" });
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

    public sealed class ResendEmailConfirmationInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
