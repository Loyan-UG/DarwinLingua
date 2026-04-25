using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Identity;

public sealed class DefaultLearnerRoleClaimsPrincipalFactory(
    UserManager<DarwinLinguaIdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<DarwinLinguaIdentityUser, IdentityRole>(userManager, roleManager, optionsAccessor)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(DarwinLinguaIdentityUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        IList<string> roles = await UserManager.GetRolesAsync(user).ConfigureAwait(false);
        if (roles.Count == 0 && await RoleManager.RoleExistsAsync(DarwinLinguaRoles.Learner).ConfigureAwait(false))
        {
            IdentityResult result = await UserManager.AddToRoleAsync(user, DarwinLinguaRoles.Learner).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to assign role '{DarwinLinguaRoles.Learner}' to '{user.Email}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }

        return await base.GenerateClaimsAsync(user).ConfigureAwait(false);
    }
}
