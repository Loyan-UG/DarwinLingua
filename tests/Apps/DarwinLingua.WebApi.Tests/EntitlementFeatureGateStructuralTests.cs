using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class EntitlementFeatureGateStructuralTests
{
    [Fact]
    public void AdminUsers_ShouldUpdateEntitlementsThroughAuditedServiceAndRenderHistory()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string usersControllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "UsersController.cs");
        string usersDetailViewPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "Users",
            "Details.cshtml");
        string usersIndexViewPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "Users",
            "Index.cshtml");

        string controllerSource = File.ReadAllText(usersControllerPath);
        string detailSource = File.ReadAllText(usersDetailViewPath);
        string indexSource = File.ReadAllText(usersIndexViewPath);

        Assert.Contains("[Authorize(Policy = \"Admin\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[HttpPost(\"entitlement\", Name = \"Admin_Users_UpdateEntitlement\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("userEntitlementService.SetTierAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("updatedBy", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetRecentAuditEventsManyAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetRecentAuditEventsManyAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("RecentEntitlementEvents", detailSource, StringComparison.Ordinal);
        Assert.Contains("Entitlement history", detailSource, StringComparison.Ordinal);
        Assert.Contains("Last entitlement event", indexSource, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminUsers_ShouldValidateEntitlementChangesAndExposeApiManagementEndpoint()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string usersControllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "UsersController.cs");
        string usersDetailViewPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "Users",
            "Details.cshtml");
        string usersIndexViewPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "Users",
            "Index.cshtml");
        string apiProgramPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Program.cs");

        string controllerSource = File.ReadAllText(usersControllerPath);
        string detailSource = File.ReadAllText(usersDetailViewPath);
        string indexSource = File.ReadAllText(usersIndexViewPath);
        string apiProgramSource = File.ReadAllText(apiProgramPath);

        Assert.Contains("[HttpGet(\"\", Name = \"Admin_Users\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"{userId}\", Name = \"Admin_UserDetails\")]", controllerSource, StringComparison.Ordinal);
        Assert.Contains("DarwinLinguaEntitlementTiers.All.Contains(requestedTier", controllerSource, StringComparison.Ordinal);
        Assert.Contains("DateTimeOffset.TryParse", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Free entitlements must not have an expiration date.", controllerSource, StringComparison.Ordinal);
        Assert.Contains("Expiration must be in the future for trial or premium entitlements.", controllerSource, StringComparison.Ordinal);
        Assert.Contains("WebUserIdentity.TryGetEmail(User)", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetCurrentManyAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetRecentAuditEventsManyAsync([user.Id], 12", controllerSource, StringComparison.Ordinal);

        Assert.Contains("Enabled features", detailSource, StringComparison.Ordinal);
        Assert.Contains("Entitlement history", detailSource, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"UpdateEntitlement\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("data-confirm-submit=\"Apply this entitlement change to the user account?\"", detailSource, StringComparison.Ordinal);
        Assert.Contains("DarwinLinguaEntitlementTiers.All", detailSource, StringComparison.Ordinal);
        Assert.Contains("Last entitlement event", indexSource, StringComparison.Ordinal);
        Assert.Contains("Details", indexSource, StringComparison.Ordinal);

        Assert.Contains("app.MapGroup(\"/api/admin/identity\")", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains(".RequireAuthorization(\"Admin\")", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"/users/{userId}/entitlement\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("unsupported_entitlement_tier", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("user_not_found", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("TryGetPrincipalEmail(principal)", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains(".SetTierAsync(user.Id, request.Tier, request.ExpiresAtUtc, updatedBy", apiProgramSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WebFeatureGates_ShouldProtectPremiumPreparationAndAdvancedFeatures()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string apiProgramPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Program.cs");
        string catalogApiClientPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "WebCatalogApiClient.cs");
        string eventPreparationControllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Controllers",
            "EventPreparationPacksController.cs");
        string partnerMatchingControllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Controllers",
            "PartnerMatchingController.cs");
        string scenariosControllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Controllers",
            "DialoguesController.cs");
        string wordsControllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Controllers",
            "WordsController.cs");

        string apiProgramSource = File.ReadAllText(apiProgramPath);
        string catalogApiClientSource = File.ReadAllText(catalogApiClientPath);
        string eventPreparationSource = File.ReadAllText(eventPreparationControllerPath);
        string partnerMatchingSource = File.ReadAllText(partnerMatchingControllerPath);
        string scenariosSource = File.ReadAllText(scenariosControllerPath);
        string wordsSource = File.ReadAllText(wordsControllerPath);

        Assert.Contains("ResolveEntitledQueryRequestAsync", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("DarwinLinguaFeatureKeys.EventPreparationPacks", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("userEntitlementService.HasFeatureAsync(actorUserId, featureKey", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("path.StartsWithSegments(\"/api/catalog/event-preparation-packs\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("Contains(\"/event-preparation-packs\"", apiProgramSource, StringComparison.Ordinal);
        Assert.Contains("X-DarwinLingua-Actor-Email", catalogApiClientSource, StringComparison.Ordinal);
        Assert.Contains("GetEventPreparationPackBySlugAsync(", catalogApiClientSource, StringComparison.Ordinal);
        Assert.Contains("GetEventPreparationPacksForDialogueAsync(", catalogApiClientSource, StringComparison.Ordinal);
        Assert.Contains("string actorEmail", catalogApiClientSource, StringComparison.Ordinal);
        Assert.Contains("CanUseEventPreparationPacksAsync", eventPreparationSource, StringComparison.Ordinal);
        Assert.Contains("WebUserIdentity.GetRequiredEmail", eventPreparationSource, StringComparison.Ordinal);
        Assert.Contains("GetEventPreparationPackBySlugAsync(normalizedSlug, actorEmail", eventPreparationSource, StringComparison.Ordinal);
        Assert.Contains("PremiumFeatureDenied", eventPreparationSource, StringComparison.Ordinal);
        Assert.Contains("EnsureCanUsePartnerMatchingAsync", partnerMatchingSource, StringComparison.Ordinal);
        Assert.Contains("PremiumFeatureDenied", partnerMatchingSource, StringComparison.Ordinal);
        Assert.Contains("CanUseEventPreparationPacksAsync", scenariosSource, StringComparison.Ordinal);
        Assert.Contains("GetEventPreparationPacksForDialogueAsync(dialogueSlug, actorEmail", scenariosSource, StringComparison.Ordinal);
        Assert.Contains("ResolveSecondaryMeaningLanguageAsync", wordsSource, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryRoot()
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");

            if (File.Exists(candidateSolutionPath))
            {
                return currentDirectory.FullName;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Unable to resolve repository root from test execution directory.");
    }
}
