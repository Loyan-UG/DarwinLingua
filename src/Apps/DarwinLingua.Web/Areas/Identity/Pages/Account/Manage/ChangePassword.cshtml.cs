using System.ComponentModel.DataAnnotations;
using DarwinLingua.Identity;
using DarwinLingua.Web.Localization;
using DarwinLingua.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account.Manage;

[Authorize]
public sealed class ChangePasswordModel(
    UserManager<DarwinLinguaIdentityUser> userManager,
    SignInManager<DarwinLinguaIdentityUser> signInManager,
    IAccountEmailService accountEmailService,
    IStringLocalizer<SharedResource> localizer) : PageModel
{
    [BindProperty]
    public ChangePasswordInputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        DarwinLinguaIdentityUser? user = await userManager.GetUserAsync(User).ConfigureAwait(false);
        if (user is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        IdentityResult result = await userManager.ChangePasswordAsync(
                user,
                Input.CurrentPassword,
                Input.NewPassword)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await userManager.UpdateSecurityStampAsync(user).ConfigureAwait(false);
        await signInManager.RefreshSignInAsync(user).ConfigureAwait(false);
        await accountEmailService.SendPasswordChangedAsync(
                user,
                ResolveCulture(),
                HttpContext.TraceIdentifier,
                cancellationToken)
            .ConfigureAwait(false);

        StatusMessage = localizer["Your password has been changed."];
        return RedirectToPage();
    }

    private string ResolveCulture() =>
        Request.HttpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()
            ?.RequestCulture.UICulture.Name
        ?? Request.Headers.AcceptLanguage.ToString()
        ?? "en";

    public sealed class ChangePasswordInputModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
