using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace DarwinLingua.Identity;

public sealed class DarwinLinguaIdentityBootstrapper<TContext>(
    TContext dbContext,
    RoleManager<IdentityRole> roleManager,
    UserManager<DarwinLinguaIdentityUser> userManager,
    IOptions<DarwinLinguaIdentityBootstrapOptions> options,
    IHostEnvironment hostEnvironment) : IDarwinLinguaIdentityBootstrapper
    where TContext : DbContext
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
        await EnsureEntitlementAuditTableAsync(cancellationToken).ConfigureAwait(false);

        foreach (string role in DarwinLinguaRoles.All)
        {
            await EnsureRoleAsync(role).ConfigureAwait(false);
        }

        DarwinLinguaIdentityBootstrapOptions bootstrapOptions = options.Value;
        if (bootstrapOptions.RequireSeedAccounts)
        {
            ValidateRequiredSeedAccounts(bootstrapOptions);
        }

        await EnsureSeedUserAsync(
                bootstrapOptions.SeedAdminEmail,
                bootstrapOptions.SeedAdminPassword,
                [DarwinLinguaRoles.Learner, DarwinLinguaRoles.Operator, DarwinLinguaRoles.Admin])
            .ConfigureAwait(false);

        await EnsureSeedUserAsync(
                bootstrapOptions.SeedLearnerEmail,
                bootstrapOptions.SeedLearnerPassword,
                [DarwinLinguaRoles.Learner])
            .ConfigureAwait(false);
    }

    private void ValidateRequiredSeedAccounts(DarwinLinguaIdentityBootstrapOptions bootstrapOptions)
    {
        List<string> missing = [];

        if (string.IsNullOrWhiteSpace(bootstrapOptions.SeedAdminEmail))
        {
            missing.Add(nameof(bootstrapOptions.SeedAdminEmail));
        }

        if (string.IsNullOrWhiteSpace(bootstrapOptions.SeedAdminPassword))
        {
            missing.Add(nameof(bootstrapOptions.SeedAdminPassword));
        }

        if (string.IsNullOrWhiteSpace(bootstrapOptions.SeedLearnerEmail))
        {
            missing.Add(nameof(bootstrapOptions.SeedLearnerEmail));
        }

        if (string.IsNullOrWhiteSpace(bootstrapOptions.SeedLearnerPassword))
        {
            missing.Add(nameof(bootstrapOptions.SeedLearnerPassword));
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Identity seed accounts are required for environment '{hostEnvironment.EnvironmentName}', but these settings are missing: {string.Join(", ", missing)}. " +
                $"You can provide them via configuration or environment variables such as " +
                $"'{DarwinLinguaIdentityEnvironmentVariables.SeedAdminEmail}', '{DarwinLinguaIdentityEnvironmentVariables.SeedAdminPassword}', " +
                $"'{DarwinLinguaIdentityEnvironmentVariables.SeedLearnerEmail}', and '{DarwinLinguaIdentityEnvironmentVariables.SeedLearnerPassword}'.");
        }
    }

    private async Task EnsureEntitlementAuditTableAsync(CancellationToken cancellationToken)
    {
        string providerName = dbContext.Database.ProviderName ?? string.Empty;

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "UserEntitlementAuditEvents" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_UserEntitlementAuditEvents" PRIMARY KEY,
                    "UserId" TEXT NOT NULL,
                    "EventType" TEXT NOT NULL,
                    "PreviousTier" TEXT NULL,
                    "NewTier" TEXT NOT NULL,
                    "PreviousTrialEndsAtUtc" TEXT NULL,
                    "NewTrialEndsAtUtc" TEXT NULL,
                    "PreviousPremiumEndsAtUtc" TEXT NULL,
                    "NewPremiumEndsAtUtc" TEXT NULL,
                    "UpdatedBy" TEXT NOT NULL,
                    "CreatedAtUtc" TEXT NOT NULL
                );
                """,
                cancellationToken)
                .ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE INDEX IF NOT EXISTS "IX_UserEntitlementAuditEvents_UserId_CreatedAtUtc"
                ON "UserEntitlementAuditEvents" ("UserId", "CreatedAtUtc");
                """,
                cancellationToken)
                .ConfigureAwait(false);

            return;
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "UserEntitlementAuditEvents" (
                    "Id" uuid NOT NULL CONSTRAINT "PK_UserEntitlementAuditEvents" PRIMARY KEY,
                    "UserId" character varying(450) NOT NULL,
                    "EventType" character varying(64) NOT NULL,
                    "PreviousTier" character varying(32) NULL,
                    "NewTier" character varying(32) NOT NULL,
                    "PreviousTrialEndsAtUtc" timestamp with time zone NULL,
                    "NewTrialEndsAtUtc" timestamp with time zone NULL,
                    "PreviousPremiumEndsAtUtc" timestamp with time zone NULL,
                    "NewPremiumEndsAtUtc" timestamp with time zone NULL,
                    "UpdatedBy" character varying(256) NOT NULL,
                    "CreatedAtUtc" timestamp with time zone NOT NULL
                );
                """,
                cancellationToken)
                .ConfigureAwait(false);

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE INDEX IF NOT EXISTS "IX_UserEntitlementAuditEvents_UserId_CreatedAtUtc"
                ON "UserEntitlementAuditEvents" ("UserId", "CreatedAtUtc");
                """,
                cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
        {
            return;
        }

        IdentityResult result = await roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Unable to create role '{roleName}': {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }

    private async Task EnsureSeedUserAsync(string? email, string? password, IReadOnlyList<string> roles)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        DarwinLinguaIdentityUser? user = await userManager.FindByEmailAsync(email.Trim()).ConfigureAwait(false);
        if (user is null)
        {
            user = new DarwinLinguaIdentityUser
            {
                UserName = email.Trim(),
                Email = email.Trim(),
                EmailConfirmed = true,
            };

            IdentityResult createResult = await userManager.CreateAsync(user, password).ConfigureAwait(false);
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create the seed user '{email}': {string.Join("; ", createResult.Errors.Select(error => error.Description))}");
            }
        }

        foreach (string role in roles)
        {
            if (await userManager.IsInRoleAsync(user, role).ConfigureAwait(false))
            {
                continue;
            }

            IdentityResult membershipResult = await userManager.AddToRoleAsync(user, role).ConfigureAwait(false);
            if (!membershipResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to assign role '{role}' to '{user.Email}': {string.Join("; ", membershipResult.Errors.Select(error => error.Description))}");
            }
        }
    }
}
