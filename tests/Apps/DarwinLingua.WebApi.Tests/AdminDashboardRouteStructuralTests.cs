using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class AdminDashboardRouteStructuralTests
{
    [Fact]
    public void Dashboard_ShouldLinkToReportsAndImplementedManagementPages()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string dashboardViewPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "Dashboard",
            "Index.cshtml");

        string source = File.ReadAllText(dashboardViewPath);

        Assert.Contains("asp-controller=\"Reports\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Users\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Moderation\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"BillingDiagnostics\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Words\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"WordSuggestions\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Topics\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Labels\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Collections\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Grammar\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Expressions\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Dialogues\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Exercises\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Courses\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"ExamPrep\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"WritingTemplates\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"CountryGuidance\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"OrganizerProfiles\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"ConversationEvents\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Imports\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Drafts\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Publishing\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Rollback\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"History\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Analytics\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Diagnostics\"", source, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"EmailDiagnostics\"", source, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminDashboardController_ShouldKeepOperatorPolicy()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "DashboardController.cs");

        string source = File.ReadAllText(controllerPath);

        Assert.Contains("[Authorize(Policy = \"Operator\")]", source, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"\", Name = \"Admin_Dashboard\")]", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EmailDiagnostics_ShouldExposeDeliveryLogWithoutEmailBodyOrRecoveryUrls()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string controllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "EmailDiagnosticsController.cs");
        string viewPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Views",
            "EmailDiagnostics",
            "Index.cshtml");
        string modelPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Models",
            "AdminEmailDiagnosticsPageViewModel.cs");

        string controller = File.ReadAllText(controllerPath);
        string view = File.ReadAllText(viewPath);
        string model = File.ReadAllText(modelPath);

        Assert.Contains("[Authorize(Policy = \"Operator\")]", controller, StringComparison.Ordinal);
        Assert.Contains("Recent account email delivery attempts are listed without reset tokens, confirmation tokens, or full recovery URLs.", view, StringComparison.Ordinal);
        Assert.Contains("RecipientEmailHash", model, StringComparison.Ordinal);
        Assert.Contains("ProviderMessageId", model, StringComparison.Ordinal);
        Assert.Contains("@T[\"Provider message id\"]", view, StringComparison.Ordinal);
        Assert.Contains("log.ProviderMessageId", view, StringComparison.Ordinal);
        Assert.Contains("FailureMessageSummary", model, StringComparison.Ordinal);
        Assert.DoesNotContain("PlainTextBody", model, StringComparison.Ordinal);
        Assert.DoesNotContain("HtmlBody", model, StringComparison.Ordinal);
        Assert.DoesNotContain("ActionUrl", model, StringComparison.Ordinal);
        Assert.DoesNotContain("callbackUrl", view, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ResetPassword", view, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConfirmEmail", view, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EmailDiagnosticsAdminSmokeTool_ShouldVerifyProviderIdsEventsSuppressionsAndReadiness()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string scriptPath = Path.Combine(
            repositoryRoot,
            "tools",
            "Web",
            "Invoke-WebEmailDiagnosticsAdminSmoke.ps1");
        string operationsChecklistPath = Path.Combine(
            repositoryRoot,
            "docs",
            "74-Production-Operations-Setup-Checklist.md");

        string script = File.ReadAllText(scriptPath);
        string operationsChecklist = File.ReadAllText(operationsChecklistPath);

        Assert.Contains("/admin/email-diagnostics", script, StringComparison.Ordinal);
        Assert.Contains("ProviderMessageId", script, StringComparison.Ordinal);
        Assert.Contains("ProviderLastEvent", script, StringComparison.Ordinal);
        Assert.Contains("WebEmailSuppressions", script, StringComparison.Ordinal);
        Assert.Contains("Brevo API key", script, StringComparison.Ordinal);
        Assert.Contains("Brevo webhook secret", script, StringComparison.Ordinal);
        Assert.Contains("Brevo sandbox mode", script, StringComparison.Ordinal);
        Assert.Contains("Unsuppress", script, StringComparison.Ordinal);
        Assert.Contains("Manual provider event", script, StringComparison.Ordinal);
        Assert.Contains("artifacts/validation/web-email-diagnostics-admin-smoke", script, StringComparison.Ordinal);
        Assert.Contains("Invoke-WebEmailDiagnosticsAdminSmoke.ps1", operationsChecklist, StringComparison.Ordinal);
    }

    [Fact]
    public void EmailDiagnosticsAdminActionsSmokeTool_ShouldExerciseAdminOnlyProviderEventSuppressionAndLoggingPaths()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string scriptPath = Path.Combine(
            repositoryRoot,
            "tools",
            "Web",
            "Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1");
        string controllerPath = Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Areas",
            "Admin",
            "Controllers",
            "EmailDiagnosticsController.cs");
        string operationsChecklistPath = Path.Combine(
            repositoryRoot,
            "docs",
            "74-Production-Operations-Setup-Checklist.md");

        string script = File.ReadAllText(scriptPath);
        string controller = File.ReadAllText(controllerPath);
        string operationsChecklist = File.ReadAllText(operationsChecklistPath);

        Assert.Contains("/admin/email-diagnostics/provider-events", script, StringComparison.Ordinal);
        Assert.Contains("/admin/email-diagnostics/suppressions/remove", script, StringComparison.Ordinal);
        Assert.Contains("WebEmailDeliveryLogs", script, StringComparison.Ordinal);
        Assert.Contains("WebEmailSuppressions", script, StringComparison.Ordinal);
        Assert.Contains("providerEventUpdatedDelivery", script, StringComparison.Ordinal);
        Assert.Contains("providerEventCreatedSuppression", script, StringComparison.Ordinal);
        Assert.Contains("removeSuppressionDeletedRow", script, StringComparison.Ordinal);
        Assert.Contains("adminActionsLogInformation", script, StringComparison.Ordinal);
        Assert.Contains("[Authorize(Policy = \"Admin\")]", controller, StringComparison.Ordinal);
        Assert.Contains("removed email suppression", controller, StringComparison.Ordinal);
        Assert.Contains("manually recorded provider event", controller, StringComparison.Ordinal);
        Assert.Contains("deleted {DeletedCount} email delivery log entries", controller, StringComparison.Ordinal);
        Assert.Contains("Invoke-WebEmailDiagnosticsAdminActionsSmoke.ps1", operationsChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("xkeysib-", script, StringComparison.Ordinal);
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
