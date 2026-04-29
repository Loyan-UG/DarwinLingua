using System.Text;
using DarwinLingua.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace DarwinLingua.Web.Areas.Identity.Pages.Account;

public sealed class ConfirmEmailModel(UserManager<DarwinLinguaIdentityUser> userManager) : PageModel
{
    public bool Succeeded { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        string? userId,
        string? code,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            return Page();
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (user is null)
        {
            return Page();
        }

        string token;
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        }
        catch (FormatException)
        {
            return Page();
        }

        IdentityResult result = await userManager.ConfirmEmailAsync(user, token).ConfigureAwait(false);
        Succeeded = result.Succeeded;
        return Page();
    }
}
