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
        string csvTemplate = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/WebTesterAccounts.example.csv"));

        Assert.Contains("Set-WebTesterPremiumAccess.ps1", runbook, StringComparison.Ordinal);
        Assert.Contains("WebTesterAccounts.example.csv", runbook, StringComparison.Ordinal);
        Assert.Contains("Email,TesterGroup,Notes", csvTemplate, StringComparison.Ordinal);
        Assert.Contains("UserEntitlementAuditEvents", script, StringComparison.Ordinal);
        Assert.Contains("tier-changed", script, StringComparison.Ordinal);
        Assert.Contains("Skipped unchanged users", script, StringComparison.Ordinal);
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
