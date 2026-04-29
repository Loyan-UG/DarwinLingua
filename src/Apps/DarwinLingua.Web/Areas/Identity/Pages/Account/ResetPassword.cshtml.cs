using System.ComponentModel.DataAnnotations;
using System.Text;
using DarwinLingua.Identity;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account;

public sealed class ResetPasswordModel(
    UserManager<DarwinLinguaIdentityUser> userManager,
    IAccountEmailService accountEmailService) : PageModel
{
    [BindProperty]
    public ResetPasswordInputModel Input { get; set; } = new();

    public IActionResult OnGet(string? code = null, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError(string.Empty, "This reset link cannot be used.");
        }

        Input.Code = code ?? string.Empty;
        Input.Email = email ?? string.Empty;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByEmailAsync(Input.Email.Trim()).ConfigureAwait(false);
        if (user is null)
        {
            return RedirectToPage("/Account/ResetPasswordConfirmation", new { area = "Identity" });
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
        }
        catch (FormatException)
        {
            ModelState.AddModelError(string.Empty, "This reset link cannot be used. Request a new password reset email.");
            return Page();
        }

        IdentityResult result = await userManager.ResetPasswordAsync(user, token, Input.Password).ConfigureAwait(false);
        if (result.Succeeded)
        {
            await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);
            await accountEmailService.SendPasswordResetCompletedAsync(
                    user,
                    ResolveCulture(),
                    HttpContext.TraceIdentifier,
                    cancellationToken)
                .ConfigureAwait(false);
            return RedirectToPage("/Account/ResetPasswordConfirmation", new { area = "Identity" });
        }

        foreach (IdentityError error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    public sealed class ResetPasswordInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Code { get; set; } = string.Empty;
    }
}
