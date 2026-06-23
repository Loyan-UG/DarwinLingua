using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebTesterOperatorToolingTests
{
    [Fact]
    public void WebTesterRunbook_ShouldDocumentBatchPremiumTooling()
    {
        string repositoryRoot = FindRepositoryRoot();
        string runbook = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/87-Web-Tester-Onboarding-Runbook.md"));
        string script = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/Set-WebTesterPremiumAccess.ps1"));
        string bundleScript = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/New-WebTesterValidationBundle.ps1"));
        string manualExternalReview = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/91-Web-Manual-External-Review-Checklist.md"));
        string csvTemplate = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/WebTesterAccounts.example.csv"));

        Assert.Contains("Set-WebTesterPremiumAccess.ps1", runbook, StringComparison.Ordinal);
        Assert.Contains("WebTesterAccounts.example.csv", runbook, StringComparison.Ordinal);
        Assert.Contains("Email,TesterGroup,Notes", csvTemplate, StringComparison.Ordinal);
        Assert.Contains("UserEntitlementAuditEvents", script, StringComparison.Ordinal);
        Assert.Contains("tier-changed", script, StringComparison.Ordinal);
        Assert.Contains("Skipped unchanged users", script, StringComparison.Ordinal);
        Assert.Contains("WebTesterAccounts.csv", bundleScript, StringComparison.Ordinal);
        Assert.Contains("ManualExternalReviewChecklist.md", bundleScript, StringComparison.Ordinal);
        Assert.Contains("Set-WebTesterPremiumAccess.ps1", bundleScript, StringComparison.Ordinal);
        Assert.Contains("operator-only account list", bundleScript, StringComparison.Ordinal);
        Assert.Contains("Transactional Email Mailbox Review", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("PWA Install Review", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("Controlled Tester Pass Start Gate", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("Do not use `www.darwinlingua.com`", manualExternalReview, StringComparison.Ordinal);
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
