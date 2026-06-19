using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebAccountDataSelfServiceStructuralTests
{
    [Fact]
    public void AccountController_ShouldExposeSelfServiceExportAndDeletionRoutes()
    {
        string repositoryRoot = FindRepositoryRoot();
        string controller = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Controllers/AccountController.cs"));

        Assert.Contains("HttpGet(\"export-data\"", controller, StringComparison.Ordinal);
        Assert.Contains("Account_ExportData", controller, StringComparison.Ordinal);
        Assert.Contains("HttpPost(\"delete\"", controller, StringComparison.Ordinal);
        Assert.Contains("Account_Delete", controller, StringComparison.Ordinal);
        Assert.Contains("ValidateAntiForgeryToken", controller, StringComparison.Ordinal);
        Assert.Contains("JsonSerializer.SerializeToUtf8Bytes", controller, StringComparison.Ordinal);
        Assert.Contains("signInManager.SignOutAsync", controller, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountPage_ShouldShowExportRectificationAndDeletionControls()
    {
        string repositoryRoot = FindRepositoryRoot();
        string view = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Account/Index.cshtml"));

        Assert.Contains("asp-action=\"ExportData\"", view, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Delete\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"currentPassword\"", view, StringComparison.Ordinal);
        Assert.Contains("name=\"confirmationPhrase\"", view, StringComparison.Ordinal);
        Assert.Contains("Model.DeleteConfirmationPhrase", view, StringComparison.Ordinal);
        Assert.Contains("asp-controller=\"Home\" asp-action=\"Contact\"", view, StringComparison.Ordinal);
    }

    [Fact]
    public void AccountDataSelfService_ShouldRequirePasswordAndAvoidDeletingOperationalAuditBlindly()
    {
        string repositoryRoot = FindRepositoryRoot();
        string service = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Services/AccountDataSelfService.cs"));

        Assert.Contains("DeleteConfirmationPhrase => \"DELETE\"", service, StringComparison.Ordinal);
        Assert.Contains("CheckPasswordAsync", service, StringComparison.Ordinal);
        Assert.Contains("RecipientUserId = null", service, StringComparison.Ordinal);
        Assert.Contains("billingEvent.UserId = null", service, StringComparison.Ordinal);
        Assert.Contains("Billing and entitlement audit records may be retained", service, StringComparison.Ordinal);
        Assert.DoesNotContain("SubmittedAnswerJson", service, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Program_ShouldRegisterAccountDataSelfService()
    {
        string repositoryRoot = FindRepositoryRoot();
        string program = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Program.cs"));

        Assert.Contains("AddScoped<IAccountDataSelfService, AccountDataSelfService>", program, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DarwinLingua.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Repository root was not found.");
    }
}
