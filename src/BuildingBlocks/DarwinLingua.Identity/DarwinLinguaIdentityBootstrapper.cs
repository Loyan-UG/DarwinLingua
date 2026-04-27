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
        await EnsureIdentityCoreTablesAsync(cancellationToken).ConfigureAwait(false);
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

    private async Task EnsureIdentityCoreTablesAsync(CancellationToken cancellationToken)
    {
        string providerName = dbContext.Database.ProviderName ?? string.Empty;

        if (providerName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            await EnsureSqliteIdentityCoreTablesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            await EnsurePostgresIdentityCoreTablesAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        await dbContext.Database.EnsureCreatedAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureSqliteIdentityCoreTablesAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
                "Name" TEXT NULL,
                "NormalizedName" TEXT NULL,
                "ConcurrencyStamp" TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS "AspNetUsers" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
                "UserName" TEXT NULL,
                "NormalizedUserName" TEXT NULL,
                "Email" TEXT NULL,
                "NormalizedEmail" TEXT NULL,
                "EmailConfirmed" INTEGER NOT NULL,
                "PasswordHash" TEXT NULL,
                "SecurityStamp" TEXT NULL,
                "ConcurrencyStamp" TEXT NULL,
                "PhoneNumber" TEXT NULL,
                "PhoneNumberConfirmed" INTEGER NOT NULL,
                "TwoFactorEnabled" INTEGER NOT NULL,
                "LockoutEnd" TEXT NULL,
                "LockoutEnabled" INTEGER NOT NULL,
                "AccessFailedCount" INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
                "RoleId" TEXT NOT NULL,
                "ClaimType" TEXT NULL,
                "ClaimValue" TEXT NULL,
                CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
                "UserId" TEXT NOT NULL,
                "ClaimType" TEXT NULL,
                "ClaimValue" TEXT NULL,
                CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
                "LoginProvider" TEXT NOT NULL,
                "ProviderKey" TEXT NOT NULL,
                "ProviderDisplayName" TEXT NULL,
                "UserId" TEXT NOT NULL,
                CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
                CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
                "UserId" TEXT NOT NULL,
                "RoleId" TEXT NOT NULL,
                CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
                CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
                "UserId" TEXT NOT NULL,
                "LoginProvider" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Value" TEXT NULL,
                CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
                CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "UserEntitlementStates" (
                "UserId" TEXT NOT NULL CONSTRAINT "PK_UserEntitlementStates" PRIMARY KEY,
                "Tier" TEXT NOT NULL,
                "TrialStartedAtUtc" TEXT NULL,
                "TrialEndsAtUtc" TEXT NULL,
                "PremiumStartedAtUtc" TEXT NULL,
                "PremiumEndsAtUtc" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                "LastUpdatedBy" TEXT NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
            CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName") WHERE "NormalizedName" IS NOT NULL;
            CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName") WHERE "NormalizedUserName" IS NOT NULL;
            CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
            """,
            cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task EnsurePostgresIdentityCoreTablesAsync(CancellationToken cancellationToken)
    {
        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "AspNetRoles" (
                "Id" text NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
                "Name" character varying(256) NULL,
                "NormalizedName" character varying(256) NULL,
                "ConcurrencyStamp" text NULL
            );

            CREATE TABLE IF NOT EXISTS "AspNetUsers" (
                "Id" text NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
                "UserName" character varying(256) NULL,
                "NormalizedUserName" character varying(256) NULL,
                "Email" character varying(256) NULL,
                "NormalizedEmail" character varying(256) NULL,
                "EmailConfirmed" boolean NOT NULL,
                "PasswordHash" text NULL,
                "SecurityStamp" text NULL,
                "ConcurrencyStamp" text NULL,
                "PhoneNumber" text NULL,
                "PhoneNumberConfirmed" boolean NOT NULL,
                "TwoFactorEnabled" boolean NOT NULL,
                "LockoutEnd" timestamp with time zone NULL,
                "LockoutEnabled" boolean NOT NULL,
                "AccessFailedCount" integer NOT NULL
            );

            CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY,
                "RoleId" text NOT NULL,
                "ClaimType" text NULL,
                "ClaimValue" text NULL,
                CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
                "Id" integer GENERATED BY DEFAULT AS IDENTITY CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY,
                "UserId" text NOT NULL,
                "ClaimType" text NULL,
                "ClaimValue" text NULL,
                CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
                "LoginProvider" text NOT NULL,
                "ProviderKey" text NOT NULL,
                "ProviderDisplayName" text NULL,
                "UserId" text NOT NULL,
                CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
                CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
                "UserId" text NOT NULL,
                "RoleId" text NOT NULL,
                CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
                CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
                "UserId" text NOT NULL,
                "LoginProvider" text NOT NULL,
                "Name" text NOT NULL,
                "Value" text NULL,
                CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
                CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS "UserEntitlementStates" (
                "UserId" character varying(450) NOT NULL CONSTRAINT "PK_UserEntitlementStates" PRIMARY KEY,
                "Tier" character varying(32) NOT NULL,
                "TrialStartedAtUtc" timestamp with time zone NULL,
                "TrialEndsAtUtc" timestamp with time zone NULL,
                "PremiumStartedAtUtc" timestamp with time zone NULL,
                "PremiumEndsAtUtc" timestamp with time zone NULL,
                "CreatedAtUtc" timestamp with time zone NOT NULL,
                "UpdatedAtUtc" timestamp with time zone NOT NULL,
                "LastUpdatedBy" character varying(256) NULL
            );
            """,
            cancellationToken)
            .ConfigureAwait(false);

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            CREATE INDEX IF NOT EXISTS "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
            CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
            CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");
            CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
            CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
            """,
            cancellationToken)
            .ConfigureAwait(false);
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
