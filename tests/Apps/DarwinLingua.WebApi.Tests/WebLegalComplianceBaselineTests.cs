using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebLegalComplianceBaselineTests
{
    [Fact]
    public void HomeController_ShouldExposeRequiredPublicLegalRoutes()
    {
        string repositoryRoot = FindRepositoryRoot();
        string controller = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Controllers/HomeController.cs"));

        Assert.Contains("HttpGet(\"privacy\"", controller, StringComparison.Ordinal);
        Assert.Contains("HttpGet(\"terms\"", controller, StringComparison.Ordinal);
        Assert.Contains("HttpGet(\"legal\"", controller, StringComparison.Ordinal);
        Assert.Contains("HttpGet(\"impressum\"", controller, StringComparison.Ordinal);
        Assert.Contains("HttpGet(\"cookies\"", controller, StringComparison.Ordinal);
        Assert.Contains("HttpGet(\"cookie-policy\"", controller, StringComparison.Ordinal);
        Assert.Contains("HttpGet(\"contact\"", controller, StringComparison.Ordinal);
    }

    [Fact]
    public void Footer_ShouldLinkRequiredLegalPages()
    {
        string repositoryRoot = FindRepositoryRoot();
        string layout = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Shared/_Layout.cshtml"));

        Assert.Contains("asp-action=\"Privacy\"", layout, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Terms\"", layout, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Legal\"", layout, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Cookies\"", layout, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Contact\"", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void LegalPages_ShouldUseConfiguredOperatorDataAndReviewWarnings()
    {
        string repositoryRoot = FindRepositoryRoot();
        string legal = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Legal.cshtml"));
        string appsettings = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/appsettings.json"));
        string cookies = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Cookies.cshtml"));
        string contact = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Contact.cshtml"));

        Assert.Contains("LegalNotice:ProviderName", legal, StringComparison.Ordinal);
        Assert.Contains("This legal notice is populated from the configured operator details", legal, StringComparison.Ordinal);
        Assert.Contains("Shahram Vafadar", appsettings, StringComparison.Ordinal);
        Assert.Contains("Achterkirchenstrasse 10, 37154 Northeim, Germany", appsettings, StringComparison.Ordinal);
        Assert.Contains("info@darwinlingua.com", appsettings, StringComparison.Ordinal);
        Assert.Contains("LegalNotice:Email", contact, StringComparison.Ordinal);
        Assert.Contains("no cookie banner is shown", cookies, StringComparison.Ordinal);
        Assert.Contains("marketing cookies or third-party analytics scripts", cookies, StringComparison.Ordinal);
    }

    [Fact]
    public void WebLegalBaselineDocsAndInventory_ShouldBePresent()
    {
        string repositoryRoot = FindRepositoryRoot();
        string baseline = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/86-Web-Legal-Compliance-Baseline.md"));
        string inventory = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "artifacts/validation/web-cookie-storage-inventory.md"));
        string index = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/00-Documentation-Index.md"));

        Assert.Contains("not final legal advice", baseline, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TDDDG section 25", baseline, StringComparison.Ordinal);
        Assert.Contains("DDG section 5", baseline, StringComparison.Ordinal);
        Assert.Contains("GDPR", baseline, StringComparison.Ordinal);
        Assert.Contains("Digital Services Act", baseline, StringComparison.Ordinal);
        Assert.Contains("UWG section 7", baseline, StringComparison.Ordinal);
        Assert.Contains("BFSG", baseline, StringComparison.Ordinal);
        Assert.Contains("VSBG section 36", baseline, StringComparison.Ordinal);
        Assert.Contains("Official-Source Refresh 2026-06-23", baseline, StringComparison.Ordinal);
        Assert.Contains("BFSG/accessibility applicability reviewed", baseline, StringComparison.Ordinal);
        Assert.Contains("StGB sections 202a-202d", baseline, StringComparison.Ordinal);
        Assert.Contains("No marketing cookies", inventory, StringComparison.Ordinal);
        Assert.Contains("no cookie banner is required", inventory, StringComparison.Ordinal);
        Assert.Contains("86-Web-Legal-Compliance-Baseline.md", index, StringComparison.Ordinal);
    }

    [Fact]
    public void WebOperationalIncidentRunbook_ShouldAssignOwnersAndResponseFlows()
    {
        string repositoryRoot = FindRepositoryRoot();
        string runbook = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/90-Web-Operational-Incident-Runbook.md"));
        string index = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/00-Documentation-Index.md"));
        string releaseChecklist = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/61-Web-Release-Checklist.md"));
        string operationsChecklist = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "docs/74-Production-Operations-Setup-Checklist.md"));

        Assert.Contains("Shahram Vafadar", runbook, StringComparison.Ordinal);
        Assert.Contains("info@darwinlingua.com", runbook, StringComparison.Ordinal);
        Assert.Contains("Brevo API Key Rotation", runbook, StringComparison.Ordinal);
        Assert.Contains("Brevo Sender-Domain Or Account Failure", runbook, StringComparison.Ordinal);
        Assert.Contains("Webhook Failure", runbook, StringComparison.Ordinal);
        Assert.Contains("Privacy And Account Requests", runbook, StringComparison.Ordinal);
        Assert.Contains("Security Or Breach Triage", runbook, StringComparison.Ordinal);
        Assert.Contains("DNS Or Cloudflare Routing Incident", runbook, StringComparison.Ordinal);
        Assert.Contains("Backup/Restore Incident", runbook, StringComparison.Ordinal);
        Assert.Contains("90-Web-Operational-Incident-Runbook.md", index, StringComparison.Ordinal);
        Assert.Contains("90-Web-Operational-Incident-Runbook.md", releaseChecklist, StringComparison.Ordinal);
        Assert.Contains("90-Web-Operational-Incident-Runbook.md", operationsChecklist, StringComparison.Ordinal);
    }

    [Fact]
    public void TermsPrivacyAndContact_ShouldExposeMisuseAndRequestBoundaries()
    {
        string repositoryRoot = FindRepositoryRoot();
        string terms = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Terms.cshtml"));
        string privacy = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Privacy.cshtml"));
        string contact = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Contact.cshtml"));

        Assert.Contains("security-abuse", terms, StringComparison.Ordinal);
        Assert.Contains("Marketing email is not part of account creation", privacy, StringComparison.Ordinal);
        Assert.Contains("Landesbeauftragte fuer den Datenschutz Niedersachsen", privacy, StringComparison.Ordinal);
        Assert.Contains("Security, abuse, and illegal-content reports", contact, StringComparison.Ordinal);
        Assert.Contains("Do not send passwords, tokens, identity documents", contact, StringComparison.Ordinal);
    }

    [Fact]
    public void LegalAndSensitiveSettingsLocalizationKeys_ShouldExistInEnglishAndGerman()
    {
        string repositoryRoot = FindRepositoryRoot();
        string english = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Resources/Localization/SharedResource.en.resx"));
        string german = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Resources/Localization/SharedResource.de.resx"));

        string[] requiredKeys =
        [
            "Legal Notice",
            "Cookie / Storage Notice",
            "I agree to the Terms of Use.",
            "I understand that Darwin Lingua processes account and learning data as described in the Privacy Policy.",
            "Show sensitive educational language",
            "It does not show pornographic or explicit adult content and it does not verify age."
        ];

        foreach (string key in requiredKeys)
        {
            Assert.Contains($"name=\"{key}\"", english, StringComparison.Ordinal);
            Assert.Contains($"name=\"{key}\"", german, StringComparison.Ordinal);
        }
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
