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
        string manualExternalReviewScript = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/New-WebManualExternalReviewReport.ps1"));
        string mailboxReviewPacketScript = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/New-WebMailboxRenderingReviewPacket.ps1"));
        string controlledTesterAuditScript = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools/Web/New-WebControlledTesterReadinessAudit.ps1"));
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
        Assert.Contains("New-WebManualExternalReviewReport.ps1", runbook, StringComparison.Ordinal);
        Assert.Contains("New-WebManualExternalReviewReport.ps1", bundleScript, StringComparison.Ordinal);
        Assert.Contains("Set-WebTesterPremiumAccess.ps1", bundleScript, StringComparison.Ordinal);
        Assert.Contains("operator-only account list", bundleScript, StringComparison.Ordinal);
        Assert.Contains("Transactional Email Mailbox Review", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("PWA Install Review", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("Controlled Tester Pass Start Gate", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("Do not use `www.darwinlingua.com`", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("MailboxReviewStatus", manualExternalReviewScript, StringComparison.Ordinal);
        Assert.Contains("PwaDesktopStatus", manualExternalReviewScript, StringComparison.Ordinal);
        Assert.Contains("TesterPassStatus", manualExternalReviewScript, StringComparison.Ordinal);
        Assert.Contains("FailOnIncomplete", manualExternalReviewScript, StringComparison.Ordinal);
        Assert.Contains("requiredWwwHost = $false", manualExternalReviewScript, StringComparison.Ordinal);
        Assert.Contains("Web Manual External Review Report", manualExternalReviewScript, StringComparison.Ordinal);
        Assert.Contains("New-WebMailboxRenderingReviewPacket.ps1", runbook, StringComparison.Ordinal);
        Assert.Contains("New-WebMailboxRenderingReviewPacket.ps1", manualExternalReview, StringComparison.Ordinal);
        Assert.Contains("New-TransactionalEmailTemplatePreview.ps1", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("MailboxRenderingEvidence.csv", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("Registration confirmation", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("Password reset", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("Email change confirmation", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("no-reply@darwinlingua.com", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("support@darwinlingua.com", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("https://darwinlingua.com", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("www.darwinlingua.com", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.Contains("webhook secrets, API keys", mailboxReviewPacketScript, StringComparison.Ordinal);
        Assert.DoesNotContain("xkeysib-", mailboxReviewPacketScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("98959d34", mailboxReviewPacketScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("New-WebControlledTesterReadinessAudit.ps1", runbook, StringComparison.Ordinal);
        Assert.Contains("FailOnAutomatedFailure", controlledTesterAuditScript, StringComparison.Ordinal);
        Assert.Contains("FailOnOpenHumanGates", controlledTesterAuditScript, StringComparison.Ordinal);
        Assert.Contains("controlledTesterReadyToInvite", controlledTesterAuditScript, StringComparison.Ordinal);
        Assert.Contains("requiredWwwHost = $false", controlledTesterAuditScript, StringComparison.Ordinal);
        Assert.Contains("does not approve broad public launch", controlledTesterAuditScript, StringComparison.Ordinal);
        Assert.Contains("artifacts/validation/brevo-webhook-configuration-check", controlledTesterAuditScript, StringComparison.Ordinal);
        Assert.DoesNotContain("xkeysib-", controlledTesterAuditScript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("98959d34", controlledTesterAuditScript, StringComparison.OrdinalIgnoreCase);
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
