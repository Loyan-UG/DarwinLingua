using Xunit;

namespace DarwinLingua.WebApi.Tests;

public sealed class WebRuntimeBootstrapStructuralTests
{
    [Fact]
    public void Startup_ShouldUsePostgresFallbackConnectionStringsForWebAndApi()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string webProgramSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Program.cs"));
        string webApiProgramSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Program.cs"));

        Assert.Contains("GetConnectionString(\"IdentityAdmin\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConnectionString(\"Identity\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConnectionString(\"WebIdentity\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConnectionString(\"SharedCatalogAdmin\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConnectionString(\"SharedCatalog\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConnectionString(\"ServerContentAdmin\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("GetConnectionString(\"ServerContent\")", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("?? webIdentityConnectionString", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("options.UseNpgsql(webIdentityConnectionString)", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("AddDarwinLinguaInfrastructureForPostgres(sharedCatalogConnectionString)", webProgramSource, StringComparison.Ordinal);

        Assert.Contains("\"ServerContentAdmin\"", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"ServerContent\"", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"SharedCatalogAdmin\"", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"SharedCatalog\"", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"IdentityAdmin\"", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"Identity\"", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("?? serverContentConnectionString", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("options.UseNpgsql(serverContentConnectionString)", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("options.UseNpgsql(identityConnectionString)", webApiProgramSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WebCatalogApiClient_ShouldTreatEmptySuccessfulDetailResponsesAsNull()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Services",
            "WebCatalogApiClient.cs"));

        Assert.Contains("private async Task<T?> GetAsync<T>", source, StringComparison.Ordinal);
        Assert.Contains("response.StatusCode == System.Net.HttpStatusCode.NotFound", source, StringComparison.Ordinal);
        Assert.Contains("return default;", source, StringComparison.Ordinal);
        Assert.Contains("string body = await response.Content.ReadAsStringAsync", source, StringComparison.Ordinal);
        Assert.Contains("string.IsNullOrWhiteSpace(body)", source, StringComparison.Ordinal);
        Assert.Contains("JsonSerializer.Deserialize<T>(body, JsonSerializerOptions.Web)", source, StringComparison.Ordinal);
        Assert.Contains("GetRequiredAsync<T>", source, StringComparison.Ordinal);
        Assert.Contains("response ?? throw new InvalidOperationException", source, StringComparison.Ordinal);
    }

    [Fact]
    public void WebOutputCachePolicies_ShouldVaryByUiCulture()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Program.cs"));

        Assert.Contains("options.AddBasePolicy(policy => policy", source, StringComparison.Ordinal);
        Assert.Contains("options.AddPolicy(\"LandingPage\"", source, StringComparison.Ordinal);
        Assert.Contains("options.AddPolicy(\"CatalogBrowse\"", source, StringComparison.Ordinal);
        Assert.True(CountOccurrences(source, ".SetVaryByQuery(\"culture\", \"ui-culture\")") >= 2);
        Assert.True(CountOccurrences(source, "\"culture\"") >= 3);
        Assert.True(CountOccurrences(source, "\"ui-culture\"") >= 3);
        Assert.True(CountOccurrences(source, ".SetVaryByHeader(\"Cookie\")") >= 3);
    }

    [Fact]
    public void WebAndApiRuntime_ShouldApplyReleaseSecurityHeaders()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string webProgramSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Program.cs"));
        string webApiProgramSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Program.cs"));

        Assert.Contains("ShouldSendStrictTransportSecurity(context)", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("Strict-Transport-Security", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("max-age=31536000; includeSubDomains", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("IPAddress.IsLoopback(address)", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("X-Content-Type-Options", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("Referrer-Policy", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("X-Frame-Options", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("Permissions-Policy", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("Content-Security-Policy", webProgramSource, StringComparison.Ordinal);

        Assert.Contains("ShouldSendStrictTransportSecurity(context)", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("Strict-Transport-Security", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("max-age=31536000; includeSubDomains", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("IPAddress.IsLoopback(address)", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("X-Content-Type-Options", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("Referrer-Policy", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("X-Frame-Options", webApiProgramSource, StringComparison.Ordinal);
        Assert.Contains("Permissions-Policy", webApiProgramSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WebRuntime_ShouldRedirectWwwHostToCanonicalApexHost()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string webProgramSource = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.Web",
            "Program.cs"));

        Assert.Contains("ShouldRedirectToCanonicalPublicHost(context)", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("www.darwinlingua.com", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("\"https://darwinlingua.com\"", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("context.Request.PathBase.ToUriComponent()", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("context.Request.Path.ToUriComponent()", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("context.Request.QueryString.ToUriComponent()", webProgramSource, StringComparison.Ordinal);
        Assert.Contains("context.Response.Redirect(canonicalUrl, permanent: true)", webProgramSource, StringComparison.Ordinal);
        Assert.True(
            webProgramSource.IndexOf("app.UseForwardedHeaders();", StringComparison.Ordinal) <
            webProgramSource.IndexOf("ShouldRedirectToCanonicalPublicHost(context)", StringComparison.Ordinal));
    }

    [Fact]
    public void ServerContentBootstrapper_ShouldRetrofitExistingPostgresSchema()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Services",
            "ServerContentDatabaseBootstrapper.cs"));

        Assert.Contains("EnsureCreatedAsync", source, StringComparison.Ordinal);
        Assert.Contains("EnsureServerContentBaseSchemaAsync", source, StringComparison.Ordinal);
        Assert.Contains("EnsurePublishedPackageCompatibilitySchemaAsync", source, StringComparison.Ordinal);
        Assert.Contains("TableExistsAsync(\"ClientProducts\"", source, StringComparison.Ordinal);
        Assert.Contains("TableExistsAsync(\"ContentStreams\"", source, StringComparison.Ordinal);
        Assert.Contains("TableExistsAsync(\"PublishedPackages\"", source, StringComparison.Ordinal);
        Assert.Contains("TableExistsAsync(\"ContentImportReceipts\"", source, StringComparison.Ordinal);
        Assert.Contains("ColumnExistsAsync(\"PublishedPackages\", \"PublicationBatchId\"", source, StringComparison.Ordinal);
        Assert.Contains("ColumnExistsAsync(\"PublishedPackages\", \"PublicationStatus\"", source, StringComparison.Ordinal);
        Assert.Contains("ColumnExistsAsync(\"PublishedPackages\", \"PublishedAtUtc\"", source, StringComparison.Ordinal);
        Assert.Contains("ColumnExistsAsync(\"PublishedPackages\", \"SupersededAtUtc\"", source, StringComparison.Ordinal);
        Assert.Contains("TableExistsAsync(\"ContentPublicationEvents\"", source, StringComparison.Ordinal);
        Assert.Contains("ColumnExistsAsync(\"ContentImportReceipts\", \"PublishedPackageCount\"", source, StringComparison.Ordinal);
        Assert.Contains("ColumnExistsAsync(\"ContentImportReceipts\", \"UpdatedAtUtc\"", source, StringComparison.Ordinal);
        Assert.Contains("dbContext.Database.IsNpgsql()", source, StringComparison.Ordinal);
        Assert.Contains("requires the PostgreSQL Npgsql provider", source, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalServerBootstrapScripts_ShouldSupportStrictModeSingleFileAndDirectoryImports()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string serverContentScript = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools",
            "Server",
            "Initialize-LocalServerContent.ps1"));
        string operationalSeedsScript = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools",
            "Server",
            "Initialize-LocalWebOperationalSeeds.ps1"));

        Assert.Contains("Set-StrictMode -Version Latest", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("function Get-JsonFiles", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("Test-Path -LiteralPath $TargetPath -PathType Leaf", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("Test-Path -LiteralPath $TargetPath -PathType Container", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("Get-ChildItem -LiteralPath $TargetPath -Filter *.json -File -Recurse", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("return ,(Resolve-Path -LiteralPath $TargetPath).Path", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("$contentFiles = @(Get-JsonFiles -TargetPath $ContentPath)", serverContentScript, StringComparison.Ordinal);
        Assert.Contains("foreach ($file in $contentFiles)", serverContentScript, StringComparison.Ordinal);

        Assert.Contains("Set-StrictMode -Version Latest", operationalSeedsScript, StringComparison.Ordinal);
        Assert.Contains("Get-Content -LiteralPath $seedFullPath -Raw | ConvertFrom-Json", operationalSeedsScript, StringComparison.Ordinal);
        Assert.Contains("Get-SeedItems", operationalSeedsScript, StringComparison.Ordinal);
    }

    [Fact]
    public void WebPublicDevStackScript_ShouldStartLaunchProfilePortsAndTunnelSmoke()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools",
            "Web",
            "Start-WebPublicDevStack.ps1"));

        Assert.Contains("Set-StrictMode -Version Latest", source, StringComparison.Ordinal);
        Assert.Contains("DarwinLingua.WebApi.csproj", source, StringComparison.Ordinal);
        Assert.Contains("DarwinLingua.Web.csproj", source, StringComparison.Ordinal);
        Assert.Contains("--launch-profile", source, StringComparison.Ordinal);
        Assert.Contains("DarwinLingua.WebApi", source, StringComparison.Ordinal);
        Assert.Contains("https", source, StringComparison.Ordinal);
        Assert.Contains("Start-CloudflaredFromInstalledService", source, StringComparison.Ordinal);
        Assert.Contains("user-process-fallback", source, StringComparison.Ordinal);
        Assert.Contains("http://localhost:5192", source, StringComparison.Ordinal);
        Assert.Contains("http://localhost:53945/health", source, StringComparison.Ordinal);
        Assert.Contains("https://darwinlingua.com", source, StringComparison.Ordinal);
        Assert.Contains("https://api.darwinlingua.com/health", source, StringComparison.Ordinal);
        Assert.Contains("artifacts/validation/web-public-stack", source, StringComparison.Ordinal);
        Assert.DoesNotContain("eyJh", source, StringComparison.Ordinal);
    }

    [Fact]
    public void WebOperationsBootstrapCheck_ShouldVerifyRuntimeConfigAndDatabaseTablesWithoutPrintingSecrets()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "tools",
            "Web",
            "Invoke-WebOperationsBootstrapCheck.ps1"));

        Assert.Contains("appsettings.Development.Local.json", source, StringComparison.Ordinal);
        Assert.Contains("identityConnectionConfigured", source, StringComparison.Ordinal);
        Assert.Contains("identityAdminConnectionConfigured", source, StringComparison.Ordinal);
        Assert.Contains("webApiBaseUrlConfigured", source, StringComparison.Ordinal);
        Assert.Contains("webApiIgnoreSslErrorsFalse", source, StringComparison.Ordinal);
        Assert.Contains("transactionalAdminRecipientConfigured", source, StringComparison.Ordinal);
        Assert.Contains("AspNetUsers", source, StringComparison.Ordinal);
        Assert.Contains("WebEmailDeliveryLogs", source, StringComparison.Ordinal);
        Assert.Contains("WebEmailSuppressions", source, StringComparison.Ordinal);
        Assert.Contains("ProviderLastEvent", source, StringComparison.Ordinal);
        Assert.Contains("ProviderLastEventAtUtc", source, StringComparison.Ordinal);
        Assert.Contains("ProviderLastEventReason", source, StringComparison.Ordinal);
        Assert.Contains("https://darwinlingua.com", source, StringComparison.Ordinal);
        Assert.Contains("https://api.darwinlingua.com/health", source, StringComparison.Ordinal);
        Assert.Contains("artifacts/validation/web-operations-bootstrap", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Password=@", source, StringComparison.Ordinal);
        Assert.DoesNotContain("xkeysib-", source, StringComparison.Ordinal);
    }

    [Fact]
    public void WebApiProgram_ShouldSupportHeadForHealthChecks()
    {
        string repositoryRoot = ResolveRepositoryRoot();
        string source = File.ReadAllText(Path.Combine(
            repositoryRoot,
            "src",
            "Apps",
            "DarwinLingua.WebApi",
            "Program.cs"));

        Assert.Contains("MapMethods(", source, StringComparison.Ordinal);
        Assert.Contains("\"/health\"", source, StringComparison.Ordinal);
        Assert.Contains("HttpMethods.Get", source, StringComparison.Ordinal);
        Assert.Contains("HttpMethods.Head", source, StringComparison.Ordinal);
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

    private static int CountOccurrences(string source, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
