namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class SensitiveEducationalLanguageStructuralTests
{
    [Fact]
    public void WebExpressions_ShouldUseProfileBoundSensitiveEligibilityAndGuardApiUnlocks()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Controllers", "ExpressionsController.cs"));
        string clientSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "WebCatalogApiClient.cs"));
        string webApiSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string indexViewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Expressions", "Index.cshtml"));

        Assert.Contains("learningProfileAccessor.GetProfileAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("profile.AllowsRudeSlangContent", controllerSource, StringComparison.Ordinal);
        Assert.DoesNotContain("includeSensitiveEducationalLanguage,", controllerSource, StringComparison.Ordinal);

        Assert.Contains("new(\"includeSensitiveEducationalLanguage\", filter.IncludeSensitiveEducationalLanguage.ToString())", clientSource, StringComparison.Ordinal);
        Assert.Contains("client.DefaultRequestHeaders.Add(options.AdminApiHeaderName, options.AdminApiKey)", clientSource, StringComparison.Ordinal);

        Assert.Contains("IsSensitiveEducationalLanguageRequestAllowed(httpContext, includeSensitiveEducationalLanguage)", webApiSource, StringComparison.Ordinal);
        Assert.Contains("if (requested != true)", webApiSource, StringComparison.Ordinal);
        Assert.Contains("context.User.IsInRole(DarwinLinguaRoles.Admin)", webApiSource, StringComparison.Ordinal);
        Assert.Contains("return IsMatchingSecret(suppliedKey, options.ApiKey)", webApiSource, StringComparison.Ordinal);

        Assert.Contains("Sensitive Educational Language visibility is controlled in Settings", indexViewSource, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicExpressionRepositories_ShouldHideSensitiveAndBlockedContentByDefault()
    {
        string expressionRepository = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Modules",
            "Catalog",
            "DarwinLingua.Catalog.Infrastructure",
            "Repositories",
            "ExpressionRepository.cs"));
        string unifiedSearchRepository = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Modules",
            "Catalog",
            "DarwinLingua.Catalog.Infrastructure",
            "Repositories",
            "UnifiedLearningSearchRepository.cs"));

        AssertVisibilityRules(expressionRepository);
        AssertVisibilityRules(unifiedSearchRepository);
    }

    [Fact]
    public void AdminReports_ShouldExposeSensitiveEducationalLanguageQualityCounters()
    {
        string adminService = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Services",
            "WebsiteAdminQueryService.cs"));
        string adminModel = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Models",
            "AdminSystemReportResponse.cs"));
        string reportsView = File.ReadAllText(ResolveRepositoryPath(
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "Reports",
            "Index.cshtml"));

        Assert.Contains("ExpressionsBySafetyRating", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionsBySensitiveContentKind", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionsByUsagePolicy", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionEntriesMissingWarningsForRiskyContent", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionEntriesRequiringSensitiveOptIn", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionEntriesRequiringVerifiedAdult", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionEntriesBlockedOrExplicitAdult", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionEntriesMissingSensitiveUsagePolicy", adminModel, StringComparison.Ordinal);
        Assert.Contains("ExpressionEntriesWithOrdinaryLiteralLeakage", adminModel, StringComparison.Ordinal);

        Assert.Contains("expressionsBySafetyRating", adminService, StringComparison.Ordinal);
        Assert.Contains("expressionsBySensitiveContentKind", adminService, StringComparison.Ordinal);
        Assert.Contains("expressionsByUsagePolicy", adminService, StringComparison.Ordinal);
        Assert.Contains("Missing warning for risky/sensitive expression", adminService, StringComparison.Ordinal);
        Assert.Contains("Requires verified adult access but no verified-adult system exists", adminService, StringComparison.Ordinal);
        Assert.Contains("Blocked or explicit-adult entries present", adminService, StringComparison.Ordinal);
        Assert.Contains("Sensitive entry missing usagePolicy", adminService, StringComparison.Ordinal);
        Assert.Contains("Published ordinary-literal expression leakage", adminService, StringComparison.Ordinal);

        Assert.Contains("Expressions by safety rating", reportsView, StringComparison.Ordinal);
        Assert.Contains("Expressions by sensitive content kind", reportsView, StringComparison.Ordinal);
        Assert.Contains("Expressions by usage policy", reportsView, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsUi_ShouldExposeSensitiveEducationalLanguageCopyInEnglishAndGerman()
    {
        string settingsView = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Settings", "Index.cshtml"));
        string englishResources = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", "SharedResource.en.resx"));
        string germanResources = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", "SharedResource.de.resx"));

        Assert.Contains("Input.AllowsRudeSlangContent", settingsView, StringComparison.Ordinal);
        Assert.Contains("Show sensitive educational language", settingsView, StringComparison.Ordinal);
        Assert.Contains("Sensitive Educational Language may include rude words, insults, slang, and mild relationship or emotional expressions", settingsView, StringComparison.Ordinal);
        Assert.Contains("It does not show pornographic or explicit adult content and it does not verify age.", settingsView, StringComparison.Ordinal);
        Assert.Contains("Sensitive entries include warnings and should be used carefully.", settingsView, StringComparison.Ordinal);

        Assert.Contains("<value>Show sensitive educational language</value>", englishResources, StringComparison.Ordinal);
        Assert.Contains("<value>Sensible Bildungssprache anzeigen</value>", germanResources, StringComparison.Ordinal);
        Assert.Contains("Sensitive Educational Language visibility is controlled in Settings", englishResources, StringComparison.Ordinal);
        Assert.Contains("Sensible Bildungssprache wird in den Einstellungen gesteuert", germanResources, StringComparison.Ordinal);
    }

    [Fact]
    public void ExpressionDetail_ShouldRenderUnderstandOnlyAndDoNotUsePolicies()
    {
        string detailViewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Views", "Expressions", "Detail.cshtml"));
        string englishResources = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", "SharedResource.en.resx"));
        string germanResources = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Resources", "Localization", "SharedResource.de.resx"));

        Assert.Contains("Model.Expression.UsagePolicy", detailViewSource, StringComparison.Ordinal);
        Assert.Contains("\"understand-only\"", detailViewSource, StringComparison.Ordinal);
        Assert.Contains("\"do-not-use\"", detailViewSource, StringComparison.Ordinal);
        Assert.Contains("Understand this expression when you hear it, but do not use it casually.", detailViewSource, StringComparison.Ordinal);
        Assert.Contains("This expression is for recognition only and should not be used by learners.", detailViewSource, StringComparison.Ordinal);

        Assert.Contains("<value>Understand this expression when you hear it, but do not use it casually.</value>", englishResources, StringComparison.Ordinal);
        Assert.Contains("<value>This expression is for recognition only and should not be used by learners.</value>", englishResources, StringComparison.Ordinal);
        Assert.Contains("Verstehe diese Wendung", germanResources, StringComparison.Ordinal);
        Assert.Contains("nur zum Wiedererkennen", germanResources, StringComparison.Ordinal);
    }

    private static void AssertVisibilityRules(string source)
    {
        Assert.Contains("!item.RequiresAdultAccess", source.Replace("expression.", "item.", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("!item.RequiresVerifiedAdult", source.Replace("expression.", "item.", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("SafetyExplicitAdult", source, StringComparison.Ordinal);
        Assert.Contains("SafetyBlockedIllegal", source, StringComparison.Ordinal);
        Assert.Contains("SafetyDiscriminatorySlur", source, StringComparison.Ordinal);
        Assert.Contains("SensitiveBlocked", source, StringComparison.Ordinal);
        Assert.Contains("SensitiveSlurEducational", source, StringComparison.Ordinal);
        Assert.Contains("UsageBlocked", source, StringComparison.Ordinal);
        Assert.Contains("!item.RequiresSensitiveOptIn", source.Replace("expression.", "item.", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("SafetyGeneral", source, StringComparison.Ordinal);
        Assert.Contains("SensitiveNone", source, StringComparison.Ordinal);
        Assert.Contains("MinimumAge == 0", source, StringComparison.Ordinal);
        Assert.Contains("UsageSafeToUse", source, StringComparison.Ordinal);
        Assert.Contains("SafetyMildRude", source, StringComparison.Ordinal);
        Assert.Contains("SafetyStrongRude", source, StringComparison.Ordinal);
        Assert.Contains("SafetyRomanticSocial", source, StringComparison.Ordinal);
        Assert.Contains("SafetySexualEducational", source, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        DirectoryInfo? currentDirectory = new(AppContext.BaseDirectory);

        while (currentDirectory is not null)
        {
            string candidateSolutionPath = Path.Combine(currentDirectory.FullName, "DarwinLingua.slnx");
            if (File.Exists(candidateSolutionPath))
            {
                return Path.Combine([currentDirectory.FullName, .. segments]);
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new InvalidOperationException("Repository root with DarwinLingua.slnx was not found.");
    }
}
