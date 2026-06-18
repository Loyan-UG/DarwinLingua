namespace DarwinLingua.WebApi.Tests;

using Xunit;

public sealed class AdminReportsLearningPortalIssueDrilldownStructuralTests
{
    [Fact]
    public void ReportsController_ShouldFilterAndExportLearningPortalIssueSamples()
    {
        string controllerSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Areas", "Admin", "Controllers", "ReportsController.cs"));
        string viewModelSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Models", "AdminSystemReportPageViewModel.cs"));

        Assert.Contains("learningPortalIssueArea", controllerSource, StringComparison.Ordinal);
        Assert.Contains("learningPortalIssueSearch", controllerSource, StringComparison.Ordinal);
        Assert.Contains("export", controllerSource, StringComparison.Ordinal);
        Assert.Contains("FilterLearningPortalIssues", controllerSource, StringComparison.Ordinal);
        Assert.Contains("LearningPortalIssues(", controllerSource, StringComparison.Ordinal);
        Assert.Contains("GetAdminLearningPortalIssuesAsync", controllerSource, StringComparison.Ordinal);
        Assert.Contains("BuildLearningPortalIssueCsv", controllerSource, StringComparison.Ordinal);
        Assert.Contains("text/csv", controllerSource, StringComparison.Ordinal);
        Assert.Contains("learning-portal-issues.csv", controllerSource, StringComparison.Ordinal);
        Assert.Contains("FilteredLearningPortalIssues", viewModelSource, StringComparison.Ordinal);
        Assert.Contains("LearningPortalIssueAreas", viewModelSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ReportsView_ShouldRenderLearningPortalIssueFiltersAndExport()
    {
        string viewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Areas", "Admin", "Views", "Reports", "Index.cshtml"));

        Assert.Contains("learning-portal-issues", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"learningPortalIssueArea\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"learningPortalIssueSearch\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"export\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("value=\"learning-portal-issues\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.FilteredLearningPortalIssues", viewSource, StringComparison.Ordinal);
        Assert.Contains("Showing @Model.FilteredLearningPortalIssues.Count of @Model.LearningPortal.SampleIssues.Count", viewSource, StringComparison.Ordinal);
        Assert.Contains("Admin_Reports_LearningPortalIssues", viewSource, StringComparison.Ordinal);
    }

    [Fact]
    public void LearningPortalIssuesView_ShouldRenderFullDrilldownFiltersAndCsvExport()
    {
        string programSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.WebApi", "Program.cs"));
        string webClientSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Services", "WebCatalogApiClient.cs"));
        string viewSource = File.ReadAllText(ResolveRepositoryPath("src", "Apps", "DarwinLingua.Web", "Areas", "Admin", "Views", "Reports", "LearningPortalIssues.cshtml"));

        Assert.Contains("\"/api/admin/catalog/learning-portal-issues\"", programSource, StringComparison.Ordinal);
        Assert.Contains("GetLearningPortalIssuesAsync", programSource, StringComparison.Ordinal);
        Assert.Contains("GetAdminLearningPortalIssuesAsync", webClientSource, StringComparison.Ordinal);
        Assert.Contains("Learning Portal issue drill-down", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"area\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"q\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"take\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("name=\"export\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("value=\"csv\"", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.FilteredCount", viewSource, StringComparison.Ordinal);
        Assert.Contains("Model.TotalCount", viewSource, StringComparison.Ordinal);
    }

    private static string ResolveRepositoryPath(params string[] segments)
    {
        string? directory = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(directory))
        {
            string candidate = Path.Combine(new[] { directory }.Concat(segments).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        throw new FileNotFoundException($"Could not resolve repository file '{string.Join(Path.DirectorySeparatorChar, segments)}'.");
    }
}
