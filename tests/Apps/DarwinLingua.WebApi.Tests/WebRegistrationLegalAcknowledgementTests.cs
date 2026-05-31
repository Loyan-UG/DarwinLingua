using DarwinLingua.Web.Data;
using DarwinLingua.Web.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebRegistrationLegalAcknowledgementTests
{
    [Fact]
    public async Task PolicyAcceptanceService_RecordsVersionedRegistrationAcceptances()
    {
        await using PostgresTestDatabase database = await PostgresTestDatabase.CreateAsync("darwin_web_identity_policy");

        DbContextOptions<WebIdentityDbContext> options = new DbContextOptionsBuilder<WebIdentityDbContext>()
            .UseNpgsql(database.ConnectionString)
            .Options;

        await using WebIdentityDbContext dbContext = new(options);
        await new WebUserStateDatabaseBootstrapper(dbContext).InitializeAsync(CancellationToken.None);

        PolicyAcceptanceService service = new(dbContext);

        await service.RecordRegistrationAcceptancesAsync("user-1", "de-DE", CancellationToken.None);

        List<WebPolicyAcceptance> acceptances = await dbContext.PolicyAcceptances
            .OrderBy(acceptance => acceptance.PolicyKey)
            .ToListAsync();

        Assert.Equal(2, acceptances.Count);
        Assert.Contains(acceptances, acceptance =>
            acceptance.PolicyKey == PolicyAcceptanceService.PrivacyNoticePolicyKey &&
            acceptance.PolicyVersion == PolicyAcceptanceService.CurrentPrivacyNoticeVersion &&
            acceptance.Source == PolicyAcceptanceService.RegistrationSource &&
            acceptance.Culture == "de-DE");
        Assert.Contains(acceptances, acceptance =>
            acceptance.PolicyKey == PolicyAcceptanceService.TermsOfUsePolicyKey &&
            acceptance.PolicyVersion == PolicyAcceptanceService.CurrentTermsOfUseVersion &&
            acceptance.Source == PolicyAcceptanceService.RegistrationSource);
    }

    [Fact]
    public void RegisterPage_ShouldRequireTermsAndPrivacyAcknowledgements()
    {
        string repositoryRoot = FindRepositoryRoot();
        string registerPage = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/Register.cshtml"));
        string registerModel = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Areas/Identity/Pages/Account/Register.cshtml.cs"));

        Assert.Contains("Input.AcceptTermsOfUse", registerPage, StringComparison.Ordinal);
        Assert.Contains("Input.AcknowledgePrivacyNotice", registerPage, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Terms\"", registerPage, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"Privacy\"", registerPage, StringComparison.Ordinal);
        Assert.Contains("ValidateRequiredAcknowledgements", registerModel, StringComparison.Ordinal);
        Assert.Contains("RecordRegistrationAcceptancesAsync", registerModel, StringComparison.Ordinal);
    }

    [Fact]
    public void WebIdentityBootstrapper_ShouldCreatePolicyAcceptanceTable()
    {
        string repositoryRoot = FindRepositoryRoot();
        string bootstrapper = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Services/WebUserStateDatabaseBootstrapper.cs"));

        Assert.Contains("\"WebPolicyAcceptances\"", bootstrapper, StringComparison.Ordinal);
        Assert.Contains("\"IX_WebPolicyAcceptances_User_Policy_Version\"", bootstrapper, StringComparison.Ordinal);
    }

    [Fact]
    public void TermsAndPrivacyPages_ShouldDescribeSensitiveEducationalLanguageAndDataMinimisation()
    {
        string repositoryRoot = FindRepositoryRoot();
        string termsPage = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Terms.cshtml"));
        string privacyPage = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src/Apps/DarwinLingua.Web/Views/Home/Privacy.cshtml"));

        Assert.Contains("Sensitive Educational Language", termsPage, StringComparison.Ordinal);
        Assert.Contains("not age verification", termsPage, StringComparison.Ordinal);
        Assert.Contains("Policy acceptance records", privacyPage, StringComparison.Ordinal);
        Assert.Contains("does not store a full birth date", privacyPage, StringComparison.Ordinal);
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
