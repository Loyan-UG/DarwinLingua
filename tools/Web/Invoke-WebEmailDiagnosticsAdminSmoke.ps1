[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$OutputDirectory = "artifacts/validation/web-email-diagnostics-admin-smoke",
    [string]$AdminEmail = $env:DARWINLINGUA_WEB_ADMIN_EMAIL,
    [string]$AdminPassword = $env:DARWINLINGUA_WEB_ADMIN_PASSWORD,
    [switch]$UseLocalDevelopmentSeed,
    [string]$LocalSettingsPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres",
    [int]$TimeoutSeconds = 20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

function Normalize-BaseUrl {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "Web base URL must not be empty."
    }

    return $Value.Trim().TrimEnd("/")
}

function Resolve-RepositoryPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $repoRoot $Path
}

function Escape-SqlLiteral {
    param([string]$Value)

    return $Value.Replace("'", "''")
}

function Invoke-JsonPsql {
    param([string]$Sql)

    $raw = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql JSON query failed."
    }

    $text = ($raw | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    return $text | ConvertFrom-Json
}

function Read-LocalDevelopmentSeed {
    param([string]$Path)

    $resolvedPath = Join-Path $repoRoot $Path
    if (-not (Test-Path -LiteralPath $resolvedPath)) {
        throw "Local development settings file not found: $resolvedPath"
    }

    $settings = Get-Content -LiteralPath $resolvedPath -Raw | ConvertFrom-Json
    $bootstrap = $settings.PSObject.Properties["IdentityBootstrap"]?.Value
    if ($null -eq $bootstrap) {
        throw "IdentityBootstrap section was not found in $resolvedPath"
    }

    $email = $bootstrap.PSObject.Properties["SeedAdminEmail"]?.Value
    $password = $bootstrap.PSObject.Properties["SeedAdminPassword"]?.Value

    if ([string]::IsNullOrWhiteSpace($email) -or [string]::IsNullOrWhiteSpace($password)) {
        throw "Seed admin credentials are not configured in $resolvedPath"
    }

    return [pscustomobject]@{
        Email = [string]$email
        Password = [string]$password
    }
}

function Get-HiddenInputValue {
    param(
        [string]$Html,
        [string]$Name
    )

    $pattern = '<input[^>]+name="' + [Regex]::Escape($Name) + '"[^>]*value="([^"]*)"'
    $match = [Regex]::Match($Html, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        return $null
    }

    return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}

if ($UseLocalDevelopmentSeed.IsPresent) {
    $seed = Read-LocalDevelopmentSeed -Path $LocalSettingsPath
    $AdminEmail = $seed.Email
    $AdminPassword = $seed.Password
}

if ([string]::IsNullOrWhiteSpace($AdminEmail) -or [string]::IsNullOrWhiteSpace($AdminPassword)) {
    throw "Admin credentials are required. Set DARWINLINGUA_WEB_ADMIN_EMAIL and DARWINLINGUA_WEB_ADMIN_PASSWORD, pass -AdminEmail/-AdminPassword, or use -UseLocalDevelopmentSeed for local-only smoke."
}

$deliverySql = @"
select coalesce(row_to_json(t), '{}'::json)
from (
    select
        "ProviderMessageId",
        "ProviderLastEvent",
        "Status"::text as "Status",
        "FailureCode",
        "ProviderName"
    from "WebEmailDeliveryLogs"
    where "ProviderMessageId" is not null
      and "ProviderMessageId" <> ''
      and "ProviderLastEvent" is not null
      and "ProviderLastEvent" <> ''
    order by "CreatedAtUtc" desc
    limit 1
) t;
"@
$delivery = Invoke-JsonPsql -Sql $deliverySql
if ($null -eq $delivery -or [string]::IsNullOrWhiteSpace($delivery.ProviderMessageId)) {
    throw "No delivery log with provider message id and provider event was found. Run Invoke-BrevoWebhookSuppressionSmoke.ps1 first."
}

$suppressionSql = @"
select coalesce(row_to_json(t), '{}'::json)
from (
    select
        "RecipientEmailHash",
        "Reason",
        "ProviderName"
    from "WebEmailSuppressions"
    where "Reason" like 'brevo:%'
    order by "CreatedAtUtc" desc
    limit 1
) t;
"@
$suppression = Invoke-JsonPsql -Sql $suppressionSql
if ($null -eq $suppression -or [string]::IsNullOrWhiteSpace($suppression.RecipientEmailHash)) {
    throw "No Brevo suppression was found. Run Invoke-BrevoWebhookSuppressionSmoke.ps1 first."
}

$web = Normalize-BaseUrl -Value $WebBaseUrl
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginUrl = "$web/Identity/Account/Login?ReturnUrl=%2Fadmin%2Femail-diagnostics"

$loginPage = Invoke-WebRequest -Uri $loginUrl -WebSession $session -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
$token = Get-HiddenInputValue -Html $loginPage.Content -Name "__RequestVerificationToken"
$returnUrl = Get-HiddenInputValue -Html $loginPage.Content -Name "ReturnUrl"

if ([string]::IsNullOrWhiteSpace($token)) {
    throw "Login antiforgery token was not found."
}

if ([string]::IsNullOrWhiteSpace($returnUrl)) {
    $returnUrl = "/admin/email-diagnostics"
}

$loginBody = @{
    "__RequestVerificationToken" = $token
    "Input.Email" = $AdminEmail
    "Input.Password" = $AdminPassword
    "Input.RememberMe" = "false"
    "ReturnUrl" = $returnUrl
}

$loginResponse = Invoke-WebRequest -Uri "$web/Identity/Account/Login" -Method Post -WebSession $session -Body $loginBody -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
$loginFinalUri = $loginResponse.BaseResponse.RequestMessage.RequestUri.AbsoluteUri
if ($loginFinalUri -like "*/Identity/Account/Login*" -or $loginResponse.Content -like "*The sign-in attempt could not be completed*") {
    throw "Admin login did not complete successfully."
}

$providerMessageId = [string]$delivery.ProviderMessageId
$providerMessageIdForDisplay = if ($providerMessageId.Length -gt 48) { $providerMessageId.Substring(0, 48) } else { $providerMessageId }
$providerMessageIdForHtml = [System.Net.WebUtility]::HtmlEncode($providerMessageIdForDisplay)
$providerEvent = [string]$delivery.ProviderLastEvent
$suppressionHash = [string]$suppression.RecipientEmailHash
$suppressionHashPrefix = $suppressionHash.Substring(0, [Math]::Min(16, $suppressionHash.Length))
$suppressionReason = [string]$suppression.Reason

$diagnosticsUrl = "{0}/admin/email-diagnostics?providerMessageId={1}&providerEvent={2}&suppressionHashPrefix={3}&suppressionReason={4}" -f
    $web,
    [System.Net.WebUtility]::UrlEncode($providerMessageId),
    [System.Net.WebUtility]::UrlEncode($providerEvent),
    [System.Net.WebUtility]::UrlEncode($suppressionHashPrefix),
    [System.Net.WebUtility]::UrlEncode($suppressionReason)

$started = Get-Date
$response = Invoke-WebRequest -Uri $diagnosticsUrl -WebSession $session -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
$elapsedMs = [int]((Get-Date) - $started).TotalMilliseconds
$content = $response.Content

$checks = [ordered]@{
    httpStatusOk = [int]$response.StatusCode -eq 200
    notLoginPage = $response.BaseResponse.RequestMessage.RequestUri.AbsoluteUri -notlike "*/Identity/Account/Login*"
    readinessModeVisible = $content -like "*BrevoApi*"
    brevoApiKeyConfiguredVisible = $content -like "*Brevo API key*" -and $content -like "*Configured*"
    brevoWebhookSecretConfiguredVisible = $content -like "*Brevo webhook secret*" -and $content -like "*Configured*"
    sandboxDisabledVisible = $content -like "*Brevo sandbox mode*" -and $content -like "*Disabled*"
    providerMessageIdVisible = $content -like "*$providerMessageIdForHtml*"
    providerEventVisible = $content -like "*$providerEvent*"
    suppressionHashVisible = $content -like "*$suppressionHashPrefix*"
    suppressionReasonVisible = $content -like "*$suppressionReason*"
    adminOnlyControlsVisible = $content -like "*Unsuppress*" -and $content -like "*Manual provider event*"
    noUnhandledException = $content -notlike "*An unhandled exception occurred*" -and $content -notlike "*HTTP Error 500*"
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
$passed = $failed.Count -eq 0

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "web-email-diagnostics-admin-smoke-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-email-diagnostics-admin-smoke-$timestamp.md"

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    webBaseUrl = $web
    diagnosticsUrl = $diagnosticsUrl
    elapsedMs = $elapsedMs
    credentialSource = if ($UseLocalDevelopmentSeed.IsPresent) { "local-development-seed" } else { "provided" }
    adminEmail = $AdminEmail
    passed = $passed
    providerMessageIdDisplay = $providerMessageIdForDisplay
    providerEvent = $providerEvent
    suppressionHashPrefix = $suppressionHashPrefix
    suppressionReason = $suppressionReason
    checks = $checks
    failedChecks = @($failed | ForEach-Object { $_.Key })
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Email Diagnostics Admin Smoke")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Web base URL: $web")
$lines.Add("- Diagnostics URL: $diagnosticsUrl")
$lines.Add("- Passed: $passed")
$lines.Add("- Provider message id display: $providerMessageIdForDisplay")
$lines.Add("- Provider event: $providerEvent")
$lines.Add("- Suppression hash prefix: $suppressionHashPrefix")
$lines.Add("- Suppression reason: $suppressionReason")
$lines.Add("")
$lines.Add("| Check | Value |")
$lines.Add("| --- | --- |")
foreach ($entry in $checks.GetEnumerator()) {
    $lines.Add("| $($entry.Key) | $($entry.Value) |")
}
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web email diagnostics admin smoke report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
