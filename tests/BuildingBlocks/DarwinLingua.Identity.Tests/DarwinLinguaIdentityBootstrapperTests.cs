using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace DarwinLingua.Identity.Tests;

public sealed class DarwinLinguaIdentityBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_CreatesSeedUsersRolesAndTrialEntitlements()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_identity");
        await using ServiceProvider services = BuildServices(
                database.ConnectionString,
                new DarwinLinguaIdentityBootstrapOptions
                {
                    RequireSeedAccounts = true,
                    SeedAdminEmail = "admin@example.local",
                    SeedAdminPassword = "Admin123!",
                    SeedLearnerEmail = "learner@example.local",
                    SeedLearnerPassword = "Learner123!",
                },
                new DarwinLinguaEntitlementOptions
                {
                    NewUserTrialDays = 30,
                });

            IDarwinLinguaIdentityBootstrapper bootstrapper = services.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
            await bootstrapper.InitializeAsync(CancellationToken.None);

            UserManager<DarwinLinguaIdentityUser> userManager = services.GetRequiredService<UserManager<DarwinLinguaIdentityUser>>();
            IUserEntitlementService entitlementService = services.GetRequiredService<IUserEntitlementService>();

            DarwinLinguaIdentityUser admin = await userManager.FindByEmailAsync("admin@example.local") ?? throw new Xunit.Sdk.XunitException("Admin seed user was not created.");
            DarwinLinguaIdentityUser learner = await userManager.FindByEmailAsync("learner@example.local") ?? throw new Xunit.Sdk.XunitException("Learner seed user was not created.");

            Assert.True(await userManager.IsInRoleAsync(admin, DarwinLinguaRoles.Admin));
            Assert.True(await userManager.IsInRoleAsync(admin, DarwinLinguaRoles.Operator));
            Assert.True(await userManager.IsInRoleAsync(admin, DarwinLinguaRoles.Learner));
            Assert.True(await userManager.IsInRoleAsync(learner, DarwinLinguaRoles.Learner));

            UserEntitlementSnapshot adminEntitlement = await entitlementService.GetCurrentAsync(admin.Id, CancellationToken.None);
            UserEntitlementSnapshot learnerEntitlement = await entitlementService.GetCurrentAsync(learner.Id, CancellationToken.None);
            IReadOnlyList<UserEntitlementAuditEventModel> learnerAuditEvents = await entitlementService
                .GetRecentAuditEventsAsync(learner.Id, 10, CancellationToken.None);

            Assert.Equal(DarwinLinguaEntitlementTiers.Trial, adminEntitlement.Tier);
            Assert.Equal(DarwinLinguaEntitlementTiers.Trial, learnerEntitlement.Tier);
            Assert.Contains(DarwinLinguaFeatureKeys.BrowseCatalog, learnerEntitlement.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.Favorites, learnerEntitlement.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.EventPreparationPacks, learnerEntitlement.EnabledFeatures);
            Assert.NotNull(learnerEntitlement.TrialEndsAtUtc);
            Assert.Contains(learnerAuditEvents, auditEvent => auditEvent.EventType == "initial-trial");
    }

    [Fact]
    public async Task InitializeAsync_CreatesIdentityTablesWhenDatabaseAlreadyHasOtherTables()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_identity");
        await using ServiceProvider services = BuildServices(
                database.ConnectionString,
                new DarwinLinguaIdentityBootstrapOptions
                {
                    SeedAdminEmail = "admin@example.local",
                    SeedAdminPassword = "Admin123!",
                },
                new DarwinLinguaEntitlementOptions());

            TestIdentityDbContext dbContext = services.GetRequiredService<TestIdentityDbContext>();
            await dbContext.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE "ExistingCatalogTable" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_ExistingCatalogTable" PRIMARY KEY
                );
                """);

            IDarwinLinguaIdentityBootstrapper bootstrapper = services.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
            await bootstrapper.InitializeAsync(CancellationToken.None);

            RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            UserManager<DarwinLinguaIdentityUser> userManager = services.GetRequiredService<UserManager<DarwinLinguaIdentityUser>>();

            Assert.True(await roleManager.RoleExistsAsync(DarwinLinguaRoles.Admin));
            Assert.NotNull(await userManager.FindByEmailAsync("admin@example.local"));
    }

    [Fact]
    public async Task InitializeAsync_ThrowsWhenRequiredSeedAccountsAreMissing()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_identity");
        await using ServiceProvider services = BuildServices(
                database.ConnectionString,
                new DarwinLinguaIdentityBootstrapOptions
                {
                    RequireSeedAccounts = true,
                },
                new DarwinLinguaEntitlementOptions());

            IDarwinLinguaIdentityBootstrapper bootstrapper = services.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => bootstrapper.InitializeAsync(CancellationToken.None));

            Assert.Contains(nameof(DarwinLinguaIdentityBootstrapOptions.SeedAdminEmail), exception.Message);
            Assert.Contains(DarwinLinguaIdentityEnvironmentVariables.SeedAdminEmail, exception.Message);
    }

    [Fact]
    public void PostConfigure_ReadsExplicitEnvironmentVariables()
    {
        const string adminEmail = "env-admin@example.local";
        const string adminPassword = "EnvAdmin123!";
        const string learnerEmail = "env-learner@example.local";
        const string learnerPassword = "EnvLearner123!";
        const string trialDays = "14";

        SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.RequireSeedAccounts, "true");
        SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedAdminEmail, adminEmail);
        SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedAdminPassword, adminPassword);
        SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedLearnerEmail, learnerEmail);
        SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedLearnerPassword, learnerPassword);
        SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.NewUserTrialDays, trialDays);

        try
        {
            DarwinLinguaIdentityBootstrapOptions identityOptions = new();
            DarwinLinguaEntitlementOptions entitlementOptions = new();

            new DarwinLinguaIdentityBootstrapOptionsPostConfigure().PostConfigure(null, identityOptions);
            new DarwinLinguaEntitlementOptionsPostConfigure().PostConfigure(null, entitlementOptions);

            Assert.True(identityOptions.RequireSeedAccounts);
            Assert.Equal(adminEmail, identityOptions.SeedAdminEmail);
            Assert.Equal(adminPassword, identityOptions.SeedAdminPassword);
            Assert.Equal(learnerEmail, identityOptions.SeedLearnerEmail);
            Assert.Equal(learnerPassword, identityOptions.SeedLearnerPassword);
            Assert.Equal(14, entitlementOptions.NewUserTrialDays);
        }
        finally
        {
            SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.RequireSeedAccounts, null);
            SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedAdminEmail, null);
            SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedAdminPassword, null);
            SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedLearnerEmail, null);
            SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.SeedLearnerPassword, null);
            SetEnvironmentVariable(DarwinLinguaIdentityEnvironmentVariables.NewUserTrialDays, null);
        }
    }

    [Fact]
    public async Task EntitlementStatesResolveExpectedFeatureFlags()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_identity_features");
        await using ServiceProvider services = BuildServices(
                database.ConnectionString,
                new DarwinLinguaIdentityBootstrapOptions(),
                new DarwinLinguaEntitlementOptions
                {
                    NewUserTrialDays = 0,
                });

            IDarwinLinguaIdentityBootstrapper bootstrapper = services.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
            await bootstrapper.InitializeAsync(CancellationToken.None);

            UserManager<DarwinLinguaIdentityUser> userManager = services.GetRequiredService<UserManager<DarwinLinguaIdentityUser>>();
            DarwinLinguaIdentityUser user = new()
            {
                UserName = "features@example.local",
                Email = "features@example.local",
                EmailConfirmed = true,
            };

            IdentityResult createResult = await userManager.CreateAsync(user, "Features123!");
            Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

            IUserEntitlementService entitlementService = services.GetRequiredService<IUserEntitlementService>();

            UserEntitlementSnapshot freeSnapshot = await entitlementService.GetCurrentAsync(user.Id, CancellationToken.None);
            Assert.Equal(DarwinLinguaEntitlementTiers.Free, freeSnapshot.Tier);
            Assert.Contains(DarwinLinguaFeatureKeys.BrowseCatalog, freeSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.SearchCatalog, freeSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.LearnerProfiles, freeSnapshot.EnabledFeatures);
            Assert.DoesNotContain(DarwinLinguaFeatureKeys.Favorites, freeSnapshot.EnabledFeatures);
            Assert.DoesNotContain(DarwinLinguaFeatureKeys.DualMeaningLanguage, freeSnapshot.EnabledFeatures);
            Assert.DoesNotContain(DarwinLinguaFeatureKeys.EventPreparationPacks, freeSnapshot.EnabledFeatures);
            Assert.False(await entitlementService.HasFeatureAsync(user.Id, DarwinLinguaFeatureKeys.PartnerMatching, CancellationToken.None));

            UserEntitlementSnapshot trialSnapshot = await entitlementService.SetTierAsync(
                user.Id,
                DarwinLinguaEntitlementTiers.Trial,
                DateTimeOffset.UtcNow.AddDays(7),
                "test-admin",
                CancellationToken.None);
            Assert.Equal(DarwinLinguaEntitlementTiers.Trial, trialSnapshot.Tier);
            Assert.Contains(DarwinLinguaFeatureKeys.Favorites, trialSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.DualMeaningLanguage, trialSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.EventPreparationPacks, trialSnapshot.EnabledFeatures);
            Assert.True(await entitlementService.HasFeatureAsync(user.Id, DarwinLinguaFeatureKeys.PartnerMatching, CancellationToken.None));

            UserEntitlementSnapshot premiumSnapshot = await entitlementService.SetTierAsync(
                user.Id,
                DarwinLinguaEntitlementTiers.Premium,
                null,
                "test-admin",
                CancellationToken.None);
            Assert.Equal(DarwinLinguaEntitlementTiers.Premium, premiumSnapshot.Tier);
            Assert.Contains(DarwinLinguaFeatureKeys.Favorites, premiumSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.AdvancedDialoguePacks, premiumSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.PartnerMatching, premiumSnapshot.EnabledFeatures);
    }

    [Fact]
    public async Task ExpiredTrialAndPremiumStatesFallBackToFreeAccess()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_identity_expired");
        await using ServiceProvider services = BuildServices(
                database.ConnectionString,
                new DarwinLinguaIdentityBootstrapOptions(),
                new DarwinLinguaEntitlementOptions
                {
                    NewUserTrialDays = 0,
                });

            IDarwinLinguaIdentityBootstrapper bootstrapper = services.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
            await bootstrapper.InitializeAsync(CancellationToken.None);

            UserManager<DarwinLinguaIdentityUser> userManager = services.GetRequiredService<UserManager<DarwinLinguaIdentityUser>>();
            DarwinLinguaIdentityUser trialUser = new()
            {
                UserName = "expired-trial@example.local",
                Email = "expired-trial@example.local",
                EmailConfirmed = true,
            };
            DarwinLinguaIdentityUser premiumUser = new()
            {
                UserName = "expired-premium@example.local",
                Email = "expired-premium@example.local",
                EmailConfirmed = true,
            };

            IdentityResult trialCreateResult = await userManager.CreateAsync(trialUser, "Expired123!");
            IdentityResult premiumCreateResult = await userManager.CreateAsync(premiumUser, "Expired123!");
            Assert.True(trialCreateResult.Succeeded, string.Join("; ", trialCreateResult.Errors.Select(error => error.Description)));
            Assert.True(premiumCreateResult.Succeeded, string.Join("; ", premiumCreateResult.Errors.Select(error => error.Description)));

            IUserEntitlementService entitlementService = services.GetRequiredService<IUserEntitlementService>();
            DateTimeOffset expiredAtUtc = DateTimeOffset.UtcNow.AddSeconds(-1);

            UserEntitlementSnapshot trialSnapshot = await entitlementService.SetTierAsync(
                trialUser.Id,
                DarwinLinguaEntitlementTiers.Trial,
                expiredAtUtc,
                "test-admin",
                CancellationToken.None);
            UserEntitlementSnapshot premiumSnapshot = await entitlementService.SetTierAsync(
                premiumUser.Id,
                DarwinLinguaEntitlementTiers.Premium,
                expiredAtUtc,
                "test-admin",
                CancellationToken.None);

            Assert.Equal(DarwinLinguaEntitlementTiers.Free, trialSnapshot.Tier);
            Assert.Equal(DarwinLinguaEntitlementTiers.Free, premiumSnapshot.Tier);
            Assert.Contains(DarwinLinguaFeatureKeys.SearchCatalog, trialSnapshot.EnabledFeatures);
            Assert.Contains(DarwinLinguaFeatureKeys.SearchCatalog, premiumSnapshot.EnabledFeatures);
            Assert.DoesNotContain(DarwinLinguaFeatureKeys.Favorites, trialSnapshot.EnabledFeatures);
            Assert.DoesNotContain(DarwinLinguaFeatureKeys.Favorites, premiumSnapshot.EnabledFeatures);

            IReadOnlyList<UserEntitlementAuditEventModel> trialAuditEvents = await entitlementService
                .GetRecentAuditEventsAsync(trialUser.Id, 10, CancellationToken.None);
            IReadOnlyList<UserEntitlementAuditEventModel> premiumAuditEvents = await entitlementService
                .GetRecentAuditEventsAsync(premiumUser.Id, 10, CancellationToken.None);
            Assert.Contains(trialAuditEvents, auditEvent => auditEvent.EventType == "trial-expired");
            Assert.Contains(premiumAuditEvents, auditEvent => auditEvent.EventType == "premium-expired");
    }

    [Fact]
    public async Task SetTierAsync_RecordsAuditEventsForManualChangesAndExpiration()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_identity");
        await using ServiceProvider services = BuildServices(
                database.ConnectionString,
                new DarwinLinguaIdentityBootstrapOptions(),
                new DarwinLinguaEntitlementOptions
                {
                    NewUserTrialDays = 0,
                });

            IDarwinLinguaIdentityBootstrapper bootstrapper = services.GetRequiredService<IDarwinLinguaIdentityBootstrapper>();
            await bootstrapper.InitializeAsync(CancellationToken.None);

            UserManager<DarwinLinguaIdentityUser> userManager = services.GetRequiredService<UserManager<DarwinLinguaIdentityUser>>();
            DarwinLinguaIdentityUser user = new()
            {
                UserName = "audit@example.local",
                Email = "audit@example.local",
                EmailConfirmed = true,
            };

            IdentityResult createResult = await userManager.CreateAsync(user, "Audit123!");
            Assert.True(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(error => error.Description)));

            IUserEntitlementService entitlementService = services.GetRequiredService<IUserEntitlementService>();
            UserEntitlementSnapshot initialSnapshot = await entitlementService.GetCurrentAsync(user.Id, CancellationToken.None);
            DateTimeOffset expiredAtUtc = DateTimeOffset.UtcNow.AddSeconds(-1);

            UserEntitlementSnapshot premiumSnapshot = await entitlementService.SetTierAsync(
                user.Id,
                DarwinLinguaEntitlementTiers.Premium,
                expiredAtUtc,
                "test-admin",
                CancellationToken.None);
            UserEntitlementSnapshot normalizedSnapshot = await entitlementService.GetCurrentAsync(user.Id, CancellationToken.None);
            IReadOnlyList<UserEntitlementAuditEventModel> auditEvents = await entitlementService
                .GetRecentAuditEventsAsync(user.Id, 10, CancellationToken.None);

            Assert.Equal(DarwinLinguaEntitlementTiers.Free, initialSnapshot.Tier);
            Assert.Equal(DarwinLinguaEntitlementTiers.Free, premiumSnapshot.Tier);
            Assert.Equal(DarwinLinguaEntitlementTiers.Free, normalizedSnapshot.Tier);
            Assert.Contains(auditEvents, auditEvent => auditEvent.EventType == "initial-free");
            Assert.Contains(auditEvents, auditEvent => auditEvent.EventType == "tier-changed" && auditEvent.UpdatedBy == "test-admin");
            Assert.Contains(auditEvents, auditEvent => auditEvent.EventType == "premium-expired");
    }

    private static ServiceProvider BuildServices(
        string connectionString,
        DarwinLinguaIdentityBootstrapOptions bootstrapOptions,
        DarwinLinguaEntitlementOptions entitlementOptions)
    {
        ServiceCollection services = new();

        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.AddSingleton<IOptions<DarwinLinguaIdentityBootstrapOptions>>(Options.Create(bootstrapOptions));
        services.AddSingleton<IOptions<DarwinLinguaEntitlementOptions>>(Options.Create(entitlementOptions));
        services.AddDbContext<TestIdentityDbContext>(options => options.UseNpgsql(connectionString));
        services
            .AddIdentityCore<DarwinLinguaIdentityUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<TestIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IDarwinLinguaIdentityBootstrapper, DarwinLinguaIdentityBootstrapper<TestIdentityDbContext>>();
        services.AddScoped<IUserEntitlementService, UserEntitlementService<TestIdentityDbContext>>();

        return services.BuildServiceProvider();
    }

    private static void SetEnvironmentVariable(string key, string? value)
    {
        Environment.SetEnvironmentVariable(key, value);
    }

    private sealed class TestIdentityDbContext(DbContextOptions<TestIdentityDbContext> options)
        : DarwinLinguaIdentityDbContext(options)
    {
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";

        public string ApplicationName { get; set; } = "DarwinLingua.Identity.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
