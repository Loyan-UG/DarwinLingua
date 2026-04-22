using DarwinLingua.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Web.Services;

public sealed class WebIdentityBootstrapOptions
{
    public string? SeedAdminEmail { get; init; }

    public string? SeedAdminPassword { get; init; }
}

public interface IWebIdentityBootstrapper
{
    Task InitializeAsync(CancellationToken cancellationToken);
}

internal sealed class WebIdentityBootstrapper(
    WebIdentityDbContext dbContext,
    RoleManager<IdentityRole> roleManager,
    UserManager<WebApplicationUser> userManager,
    IOptions<WebIdentityBootstrapOptions> options) : IWebIdentityBootstrapper
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);

        await EnsureRoleAsync("Learner").ConfigureAwait(false);
        await EnsureRoleAsync("Operator").ConfigureAwait(false);
        await EnsureRoleAsync("Admin").ConfigureAwait(false);

        string? seedAdminEmail = options.Value.SeedAdminEmail?.Trim();
        string? seedAdminPassword = options.Value.SeedAdminPassword;

        if (string.IsNullOrWhiteSpace(seedAdminEmail) || string.IsNullOrWhiteSpace(seedAdminPassword))
        {
            return;
        }

        WebApplicationUser? existingUser = await userManager.FindByEmailAsync(seedAdminEmail).ConfigureAwait(false);

        if (existingUser is null)
        {
            existingUser = new WebApplicationUser
            {
                UserName = seedAdminEmail,
                Email = seedAdminEmail,
                EmailConfirmed = true
            };

            IdentityResult createResult = await userManager.CreateAsync(existingUser, seedAdminPassword).ConfigureAwait(false);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create the seed admin user: {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        await EnsureRoleMembershipAsync(existingUser, "Learner").ConfigureAwait(false);
        await EnsureRoleMembershipAsync(existingUser, "Operator").ConfigureAwait(false);
        await EnsureRoleMembershipAsync(existingUser, "Admin").ConfigureAwait(false);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
        {
            IdentityResult result = await roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create role '{roleName}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }
    }

    private async Task EnsureRoleMembershipAsync(WebApplicationUser user, string roleName)
    {
        if (!await userManager.IsInRoleAsync(user, roleName).ConfigureAwait(false))
        {
            IdentityResult result = await userManager.AddToRoleAsync(user, roleName).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to assign role '{roleName}' to '{user.Email}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
            }
        }
    }
}
