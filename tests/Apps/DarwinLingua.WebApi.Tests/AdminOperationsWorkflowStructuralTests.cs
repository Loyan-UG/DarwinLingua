using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminOperationsWorkflowStructuralTests
{
    [Fact]
    public void AdminOperationsControllers_ShouldExposeReadOnlyImportDraftHistoryPublishingAndProtectedRollbackRoutes()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string importsController = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/ImportsController.cs");
        string draftsController = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/DraftsController.cs");
        string historyController = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/HistoryController.cs");
        string publishingController = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/PublishingController.cs");
        string rollbackController = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/RollbackController.cs");

        Assert.Contains("[Route(\"admin/imports\")]", importsController, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", importsController, StringComparison.Ordinal);
        Assert.Contains("NormalizeStatus(status)", importsController, StringComparison.Ordinal);
        Assert.Contains("IsAllowedPackageStatus", importsController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"table\", Name = \"Admin_ImportsTable\")]", importsController, StringComparison.Ordinal);

        Assert.Contains("[Route(\"admin/drafts\")]", draftsController, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", draftsController, StringComparison.Ordinal);
        Assert.Contains("private const int MaxQueryLength = 128", draftsController, StringComparison.Ordinal);
        Assert.Contains("trimmed[..MaxQueryLength]", draftsController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"table\", Name = \"Admin_DraftsTable\")]", draftsController, StringComparison.Ordinal);

        Assert.Contains("[Route(\"admin/history\")]", historyController, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", historyController, StringComparison.Ordinal);
        Assert.Contains("NormalizeStatus(status)", historyController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"panel\", Name = \"Admin_HistoryPanel\")]", historyController, StringComparison.Ordinal);

        Assert.Contains("[Route(\"admin/publishing\")]", publishingController, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", publishingController, StringComparison.Ordinal);
        Assert.Contains("GetDashboardAsync(cancellationToken)", publishingController, StringComparison.Ordinal);

        Assert.Contains("[Route(\"admin/rollback\")]", rollbackController, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Admin\")]", rollbackController, StringComparison.Ordinal);
        Assert.Contains("GetRollbackPreviewAsync(cancellationToken)", rollbackController, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminOperationsViews_ShouldRenderFiltersTablesSummaryAndRollbackWarning()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string importsView = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Imports/Index.cshtml");
        string draftsView = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Drafts/Index.cshtml");
        string historyView = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/History/Index.cshtml");
        string publishingView = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Publishing/Index.cshtml");
        string rollbackView = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Rollback/Index.cshtml");
        string dashboardView = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Dashboard/Index.cshtml");

        Assert.Contains("id=\"imports-filter\"", importsView, StringComparison.Ordinal);
        Assert.Contains("hx-get=\"@Url.Action(\"Table\", \"Imports\", new { area = \"Admin\" })\"", importsView, StringComparison.Ordinal);
        Assert.Contains("Import validation summary by module", importsView, StringComparison.Ordinal);
        Assert.Contains("<partial name=\"_ImportsTable\"", importsView, StringComparison.Ordinal);

        Assert.Contains("id=\"drafts-filter\"", draftsView, StringComparison.Ordinal);
        Assert.Contains("hx-get=\"@Url.Action(\"Table\", \"Drafts\", new { area = \"Admin\" })\"", draftsView, StringComparison.Ordinal);
        Assert.Contains("<partial name=\"_DraftsTable\"", draftsView, StringComparison.Ordinal);

        Assert.Contains("id=\"history-filter\"", historyView, StringComparison.Ordinal);
        Assert.Contains("hx-get=\"@Url.Action(\"Panel\", \"History\", new { area = \"Admin\" })\"", historyView, StringComparison.Ordinal);
        Assert.Contains("<partial name=\"_HistoryPanel\"", historyView, StringComparison.Ordinal);

        Assert.Contains("Publishing status", publishingView, StringComparison.Ordinal);
        Assert.Contains("Catalog readiness", publishingView, StringComparison.Ordinal);
        Assert.Contains("Package operations", publishingView, StringComparison.Ordinal);

        Assert.Contains("Protected rollback review", rollbackView, StringComparison.Ordinal);
        Assert.Contains("Current catalog state", rollbackView, StringComparison.Ordinal);
        Assert.Contains("Rollback warning", rollbackView, StringComparison.Ordinal);
        Assert.Contains("data-modal-open=\"rollback-confirmation\"", rollbackView, StringComparison.Ordinal);
        Assert.Contains("_ConfirmActionModal", rollbackView, StringComparison.Ordinal);

        Assert.Contains("asp-controller=\"Imports\"", dashboardView, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Drafts\"", dashboardView, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Publishing\"", dashboardView, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Rollback\"", dashboardView, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"History\"", dashboardView, StringComparison.Ordinal);
    }

    [Fact]
    public void WebApiAdminOperationsEndpoints_ShouldExposeDashboardImportsDraftHistoryAndRollbackPreview()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string program = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");
        string webClient = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string operationsService = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebAdminOperationsQueryService.cs");

        Assert.Contains("\"/api/admin/catalog/dashboard\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/imports\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/draft-words\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/history-view\"", program, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/rollback-preview\"", program, StringComparison.Ordinal);
        Assert.Contains("EnforceAdminApiAccessAsync", program, StringComparison.Ordinal);

        Assert.Contains("GetAdminImportsAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminDraftWordsAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminHistoryAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminRollbackPreviewAsync", webClient, StringComparison.Ordinal);

        Assert.Contains("catalogApiClient.GetAdminImportsAsync", operationsService, StringComparison.Ordinal);
        Assert.Contains("catalogApiClient.GetAdminDraftWordsAsync", operationsService, StringComparison.Ordinal);
        Assert.Contains("catalogApiClient.GetAdminHistoryAsync", operationsService, StringComparison.Ordinal);
        Assert.Contains("catalogApiClient.GetAdminRollbackPreviewAsync", operationsService, StringComparison.Ordinal);
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
