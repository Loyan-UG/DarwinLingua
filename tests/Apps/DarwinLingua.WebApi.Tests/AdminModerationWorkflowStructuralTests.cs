using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminModerationWorkflowStructuralTests
{
    [Fact]
    public void AdminModeration_ShouldExposeFilteredQueueDecisionFormsAndAuditTrail()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controller = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Controllers/ModerationController.cs");
        string view = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Areas/Admin/Views/Moderation/Index.cshtml");
        string viewModel = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Models/ModerationPageViewModels.cs");
        string webClient = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/WebCatalogApiClient.cs");
        string apiProgram = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.WebApi/Program.cs");
        string notificationService = ReadSource(repositoryRoot, "src/Apps/DarwinLingua.Web/Services/CommunityNotificationEmailService.cs");

        Assert.Contains("[Route(\"admin/moderation\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Operator\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"\", Name = \"Admin_Moderation_Index\")]", controller, StringComparison.Ordinal);
        Assert.Contains("[HttpPost(\"{reportId:guid}/decision\", Name = \"Admin_Moderation_Decide\")]", controller, StringComparison.Ordinal);
        Assert.Contains("NormalizeStatus(status)", controller, StringComparison.Ordinal);
        Assert.Contains("NormalizeReason(reason)", controller, StringComparison.Ordinal);
        Assert.Contains("NormalizeTargetType(targetType)", controller, StringComparison.Ordinal);
        Assert.Contains("NormalizeAssignedState(assignedState)", controller, StringComparison.Ordinal);
        Assert.Contains("GetAdminUserReportsAsync", controller, StringComparison.Ordinal);
        Assert.Contains("GetAdminModerationDecisionAuditsAsync", controller, StringComparison.Ordinal);
        Assert.Contains("IsAllowedDecisionStatus(input.Status)", controller, StringComparison.Ordinal);
        Assert.Contains("new ModerationDecisionRequest(input.Status.Trim(), TrimToNull(input.DecisionNote), GetAdminEmail())", controller, StringComparison.Ordinal);
        Assert.Contains("SendModerationReportOutcomeAsync", controller, StringComparison.Ordinal);
        Assert.Contains("BuildFilterRouteValues", controller, StringComparison.Ordinal);

        Assert.Contains("[StringLength(1000)]", viewModel, StringComparison.Ordinal);
        Assert.Contains("AdminModerationPageViewModel", viewModel, StringComparison.Ordinal);

        Assert.Contains("id=\"moderation-filter\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"status\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"reason\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"targetType\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"assignedState\"", view, StringComparison.Ordinal);
        Assert.Contains("id=\"moderation-reports\"", view, StringComparison.Ordinal);
        Assert.Contains("id=\"moderation-audit\"", view, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Decide\"", view, StringComparison.Ordinal);
        Assert.Contains("data-confirm-submit=\"Save this moderation decision?\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"DecisionNote\"", view, StringComparison.Ordinal);
        Assert.Contains("Decision recorded. Further changes require a new moderation report.", view, StringComparison.Ordinal);

        Assert.Contains("GetAdminUserReportsAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("GetAdminModerationDecisionAuditsAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("DecideAdminUserReportAsync", webClient, StringComparison.Ordinal);
        Assert.Contains("new(\"assignedState\", assignedState)", webClient, StringComparison.Ordinal);

        Assert.Contains("\"/api/admin/catalog/moderation/reports\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/moderation/reports/{reportId:guid}/decision\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("\"/api/admin/catalog/moderation/audits\"", apiProgram, StringComparison.Ordinal);
        Assert.Contains("IModerationService moderationService", apiProgram, StringComparison.Ordinal);
        Assert.Contains("DecideReportAsync(reportId, request", apiProgram, StringComparison.Ordinal);
        Assert.Contains("GetDecisionAuditsAsync", apiProgram, StringComparison.Ordinal);

        Assert.Contains("SendModerationReportOutcomeAsync", notificationService, StringComparison.Ordinal);
        Assert.Contains("TransactionalEmailScenarios.ModerationReportOutcome", notificationService, StringComparison.Ordinal);
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
