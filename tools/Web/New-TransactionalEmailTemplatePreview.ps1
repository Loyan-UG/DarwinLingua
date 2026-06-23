[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/transactional-email-template-preview",
    [string]$ProjectPath = "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj",
    [switch]$StopRunningApplications
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

function Resolve-RepositoryPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $repoRoot $Path
}

$resolvedProjectPath = Resolve-RepositoryPath -Path $ProjectPath
if (-not (Test-Path -LiteralPath $resolvedProjectPath -PathType Leaf)) {
    throw "Web project was not found: $resolvedProjectPath"
}

if ($StopRunningApplications) {
    $currentProcessId = $PID
    $runningApplications = Get-CimInstance Win32_Process | Where-Object {
        $_.ProcessId -ne $currentProcessId -and (
            ($_.Name -eq 'dotnet.exe' -and ($_.CommandLine -like '*src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj*' -or $_.CommandLine -like '*src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj*')) -or
            $_.Name -in @('DarwinLingua.Web.exe', 'DarwinLingua.WebApi.exe')
        )
    }

    foreach ($application in $runningApplications) {
        Stop-Process -Id $application.ProcessId -Force -ErrorAction SilentlyContinue
    }

    if ($runningApplications) {
        Start-Sleep -Seconds 2
    }
}

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runOutput = Join-Path $outputRoot "transactional-email-template-preview-$timestamp"
New-Item -ItemType Directory -Path $runOutput -Force | Out-Null

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "darwinlingua-email-preview-$timestamp"
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

$projectReference = $resolvedProjectPath.Replace('\', '/')
$runOutputArgument = $runOutput.Replace('\', '/')

$csproj = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="$projectReference" />
  </ItemGroup>
</Project>
"@

$program = @'
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DarwinLingua.Web.Services;
using Microsoft.Extensions.Options;

string outputDirectory = args.Length > 0 ? args[0] : throw new InvalidOperationException("Output directory argument is required.");
Directory.CreateDirectory(outputDirectory);

TransactionalEmailOptions options = new()
{
    Mode = "File",
    PublicBaseUrl = "https://darwinlingua.com",
    ProductName = "Darwin Lingua",
    FromEmail = "no-reply@darwinlingua.com",
    FromName = "Darwin Lingua",
    ReplyToEmail = "support@darwinlingua.com",
    SupportEmail = "support@darwinlingua.com",
    BrevoApiKey = string.Empty,
    BrevoWebhookSecret = string.Empty,
};

TransactionalEmailTemplateRenderer renderer = new(Options.Create(options));
Dictionary<string, string> values = new(StringComparer.Ordinal)
{
    ["ActionUrl"] = "https://darwinlingua.com/Identity/Account/PreviewAction?code=preview-code",
    ["ExpirationText"] = "24 hours",
    ["OrganizerName"] = "Sprachcafe Northeim",
    ["RequesterName"] = "Preview Learner",
    ["FailureCount"] = "3",
    ["WindowMinutes"] = "15",
    ["LastFailureScenarioKey"] = "Account.PasswordReset",
    ["LastFailureCode"] = "brevo:soft_bounce",
    ["BillingStatus"] = "active",
    ["CurrentPeriodEnd"] = "2026-07-23",
    ["AdminActor"] = "admin@example.com",
    ["SubscriptionId"] = "sub_preview",
    ["UserId"] = "user-preview",
    ["EntitlementTier"] = "Premium",
    ["OrganizerProfileSlug"] = "sprachcafe-northeim",
    ["EventTitle"] = "Deutsch sprechen im Alltag",
    ["RsvpStatus"] = "confirmed",
    ["DisplayName"] = "Mina",
    ["Reason"] = "abuse-report",
    ["TargetType"] = "profile",
    ["TargetKey"] = "profile-preview",
    ["Status"] = "reviewed",
};

string[] scenarioKeys = typeof(TransactionalEmailScenarios)
    .GetFields(BindingFlags.Public | BindingFlags.Static)
    .Where(static field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string))
    .Select(static field => (string)field.GetRawConstantValue()!)
    .OrderBy(static scenario => scenario, StringComparer.Ordinal)
    .ToArray();

List<string> indexRows = [];
foreach (string scenarioKey in scenarioKeys)
{
    foreach (string culture in new[] { "en-US", "de-DE" })
    {
        RenderedEmailTemplate rendered = renderer.Render(scenarioKey, culture, values);
        string fileBase = $"{SanitizeFileName(scenarioKey)}-{rendered.Culture}";
        string htmlFile = $"{fileBase}.html";
        string textFile = $"{fileBase}.txt";
        File.WriteAllText(Path.Combine(outputDirectory, htmlFile), rendered.HtmlBody, new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(outputDirectory, textFile), rendered.PlainTextBody, new UTF8Encoding(false));
        indexRows.Add($"<tr><td>{WebUtility.HtmlEncode(scenarioKey)}</td><td>{WebUtility.HtmlEncode(rendered.Culture)}</td><td>{WebUtility.HtmlEncode(rendered.Subject)}</td><td><a href=\"{htmlFile}\">HTML</a></td><td><a href=\"{textFile}\">Plain text</a></td></tr>");
    }
}

string generatedAtUtc = DateTimeOffset.UtcNow.ToString("O");
string indexHtml = $$"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Darwin Lingua transactional email preview</title>
  <style>
    body { margin: 0; padding: 32px; font-family: Arial, sans-serif; background: #f3f4f6; color: #111827; }
    main { max-width: 1100px; margin: 0 auto; }
    table { width: 100%; border-collapse: collapse; background: #fff; }
    th, td { padding: 12px 14px; border-bottom: 1px solid #e5e7eb; text-align: left; vertical-align: top; }
    th { background: #111827; color: #fff; }
    a { color: #1d4ed8; font-weight: 700; }
    .note { color: #4b5563; line-height: 1.6; }
  </style>
</head>
<body>
  <main>
    <h1>Darwin Lingua transactional email preview</h1>
    <p class="note">Generated {{WebUtility.HtmlEncode(generatedAtUtc)}} with safe sample values. No API keys, webhook secrets, reset tokens, provider message ids, or real user emails are included.</p>
    <table>
      <thead><tr><th>Scenario</th><th>Culture</th><th>Subject</th><th>HTML</th><th>Plain text</th></tr></thead>
      <tbody>
        {{string.Join(Environment.NewLine, indexRows)}}
      </tbody>
    </table>
  </main>
</body>
</html>
""";

File.WriteAllText(Path.Combine(outputDirectory, "index.html"), indexHtml, new UTF8Encoding(false));
File.WriteAllText(
    Path.Combine(outputDirectory, "manifest.md"),
    $"""
    # Transactional Email Template Preview

    - Generated UTC: {generatedAtUtc}
    - Scenario count: {scenarioKeys.Length}
    - Rendered files: {scenarioKeys.Length * 2} HTML + {scenarioKeys.Length * 2} plain-text files
    - Cultures: en, de
    - Source renderer: DarwinLingua.Web.Services.TransactionalEmailTemplateRenderer
    - Secret policy: preview data only; no API keys, webhook secrets, reset tokens, provider message ids, or real user emails.

    Open `index.html` to review every rendered email template.
    """,
    new UTF8Encoding(false));

Console.WriteLine($"PREVIEW_PATH={outputDirectory}");
Console.WriteLine($"SCENARIOS={scenarioKeys.Length}");

static string SanitizeFileName(string value) =>
    Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
'@

Set-Content -LiteralPath (Join-Path $tempRoot "EmailPreview.csproj") -Value $csproj -Encoding UTF8
Set-Content -LiteralPath (Join-Path $tempRoot "Program.cs") -Value $program -Encoding UTF8

try {
    dotnet run --project (Join-Path $tempRoot "EmailPreview.csproj") -- $runOutputArgument
    if ($LASTEXITCODE -ne 0) {
        throw "Email preview renderer failed."
    }
}
finally {
    Remove-Item -LiteralPath $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Host "Transactional email template preview: $runOutput"
