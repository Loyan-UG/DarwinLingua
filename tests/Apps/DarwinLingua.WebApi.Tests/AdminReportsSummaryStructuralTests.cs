using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminReportsSummaryStructuralTests
{
    [Fact]
    public void AdminReports_ShouldRenderSystemSummaryMetricsAndLearningPortalQualityCounters()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controller = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/ReportsController.cs");
        string view = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Reports/Index.cshtml");
        string viewModel = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/AdminSystemReportPageViewModel.cs");
        string webClient = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string apiProgram = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");
        string adminQueryService = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Services/WebsiteAdminQueryService.cs");

        Assert.Contains("[Route(\"admin/reports\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Admin\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"\", Name = \"Admin_Reports_Index\")]", controller, StringComparison.Ordinal);
        Assert.Contains("catalogApiClient.GetAdminSystemReportAsync", controller, StringComparison.Ordinal);
        Assert.Contains("userManager.Users.CountAsync", controller, StringComparison.Ordinal);
        Assert.Contains("GetSummarySinceAsync", controller, StringComparison.Ordinal);
        Assert.Contains("analyticsService.GetSummary()", controller, StringComparison.Ordinal);
        Assert.Contains("BuildCatalogMetrics(report.Catalog)", controller, StringComparison.Ordinal);
        Assert.Contains("BuildSocialMetrics(report.Social)", controller, StringComparison.Ordinal);
        Assert.Contains("BuildModerationMetrics(report.Moderation)", controller, StringComparison.Ordinal);
        Assert.Contains("BuildOperationsMetrics(report.Operations)", controller, StringComparison.Ordinal);
        Assert.Contains("BuildEmailMetrics(emailSummary)", controller, StringComparison.Ordinal);
        Assert.Contains("BuildLearningPortalQualityMetrics(report.LearningPortal)", controller, StringComparison.Ordinal);

        Assert.Contains("IdentityUserCount", viewModel, StringComparison.Ordinal);
        Assert.Contains("CatalogMetrics", viewModel, StringComparison.Ordinal);
        Assert.Contains("SocialMetrics", viewModel, StringComparison.Ordinal);
        Assert.Contains("ModerationMetrics", viewModel, StringComparison.Ordinal);
        Assert.Contains("OperationsMetrics", viewModel, StringComparison.Ordinal);
        Assert.Contains("EmailMetrics", viewModel, StringComparison.Ordinal);
        Assert.Contains("LearningPortalQualityMetrics", viewModel, StringComparison.Ordinal);
        Assert.Contains("AnalyticsItems", viewModel, StringComparison.Ordinal);

        Assert.Contains("(\"Catalog\", Model.CatalogMetrics)", view, StringComparison.Ordinal);
        Assert.Contains("(\"Social learning\", Model.SocialMetrics)", view, StringComparison.Ordinal);
        Assert.Contains("(\"Moderation\", Model.ModerationMetrics)", view, StringComparison.Ordinal);
        Assert.Contains("(\"Operations\", Model.OperationsMetrics)", view, StringComparison.Ordinal);
        Assert.Contains("(\"Transactional email\", Model.EmailMetrics)", view, StringComparison.Ordinal);
        Assert.Contains("(\"Learning Portal quality\", Model.LearningPortalQualityMetrics)", view, StringComparison.Ordinal);
        Assert.Contains("Users", view, StringComparison.Ordinal);
        Assert.Contains("Open reports", view, StringComparison.Ordinal);
        Assert.Contains("Email failures", view, StringComparison.Ordinal);
        Assert.Contains("Learning issues", view, StringComparison.Ordinal);
        Assert.Contains("Learning scope: target language", view, StringComparison.Ordinal);
        Assert.Contains("Counts by target language", view, StringComparison.Ordinal);
        Assert.Contains("Country Guidance by country context", view, StringComparison.Ordinal);
        Assert.Contains("Target-language activation gate", view, StringComparison.Ordinal);
        Assert.Contains("Missing translations by helper language", view, StringComparison.Ordinal);
        Assert.Contains("Duplicate slug diagnostics", view, StringComparison.Ordinal);
        Assert.Contains("Top Web analytics counters", view, StringComparison.Ordinal);
        Assert.Contains("Helper translation gaps", controller, StringComparison.Ordinal);
        Assert.Contains("Duplicate slug diagnostics", controller, StringComparison.Ordinal);
        Assert.Contains("Course activity gaps", controller, StringComparison.Ordinal);
        Assert.Contains("Exercise set quality gaps", controller, StringComparison.Ordinal);
        Assert.Contains("Exam prep quality gaps", controller, StringComparison.Ordinal);
        Assert.Contains("Sensitive policy gaps", controller, StringComparison.Ordinal);

        Assert.Contains("GetAdminSystemReportAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("targetLearningLanguageCode", webClient, StringComparison.Ordinal);
        Assert.Contains("MissingTranslationsByHelperLanguage", webClient, StringComparison.Ordinal);
        Assert.Contains("DuplicateSlugsByType", webClient, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/system-report\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("ResolveDiagnosticTargetLearningLanguageCode(targetLearningLanguageCode)", apiProgram, StringComparison.Ordinal);
        Assert.Contains("TryFindContentImportable(requested", apiProgram, StringComparison.Ordinal);
        Assert.Contains("GetCatalogSystemReportAsync", adminQueryService, StringComparison.Ordinal);
        Assert.Contains("GetSocialSystemReportAsync", adminQueryService, StringComparison.Ordinal);
        Assert.Contains("GetModerationSystemReportAsync", adminQueryService, StringComparison.Ordinal);
        Assert.Contains("GetOperationsSystemReportAsync", adminQueryService, StringComparison.Ordinal);
        Assert.Contains("GetLearningPortalSystemReportAsync", adminQueryService, StringComparison.Ordinal);
    }

    private static string ReadSource(string repositoryRoot, string relativePath) =>
        File.ReadAllText(Path.Combine(repositoryRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));

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
