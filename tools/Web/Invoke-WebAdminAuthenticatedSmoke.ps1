[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://localhost:7501",
    [string]$OutputDirectory = "artifacts/validation/web-admin-smoke",
    [string]$AdminEmail = $env:DARWINLINGUA_WEB_ADMIN_EMAIL,
    [string]$AdminPassword = $env:DARWINLINGUA_WEB_ADMIN_PASSWORD,
    [switch]$UseLocalDevelopmentSeed,
    [string]$LocalSettingsPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
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

function New-CheckResult {
    param(
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session,
        [string]$Name,
        [string]$Url,
        [string]$RequiredText = $null
    )

    $started = Get-Date
    $result = [ordered]@{
        name = $Name
        url = $Url
        expectedStatus = 200
        statusCode = $null
        elapsedMs = $null
        contentLength = $null
        passed = $false
        error = $null
    }

    try {
        $response = Invoke-WebRequest -Uri $Url -WebSession $Session -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
        $result.statusCode = [int]$response.StatusCode
        $result.contentLength = $response.Content.Length
        $result.elapsedMs = [int]((Get-Date) - $started).TotalMilliseconds

        $finalUri = $response.BaseResponse.RequestMessage.RequestUri.AbsoluteUri
        if ($finalUri -like "*/Identity/Account/Login*") {
            $result.error = "Request ended on the login page instead of the protected admin page."
            return [pscustomobject]$result
        }

        if ($result.statusCode -ne 200) {
            $result.error = "Expected HTTP 200 but received HTTP $($result.statusCode)."
            return [pscustomobject]$result
        }

        if ($response.Content -like "*Sign in*" -or $response.Content -like "*The sign-in attempt could not be completed*") {
            $result.error = "Response still appears to be an authentication page."
            return [pscustomobject]$result
        }

        if ($response.Content -like "*An unhandled exception occurred*" -or $response.Content -like "*HTTP Error 500*") {
            $result.error = "Response contains an unhandled exception marker."
            return [pscustomobject]$result
        }

        if (-not [string]::IsNullOrWhiteSpace($RequiredText) -and $response.Content -notlike "*$RequiredText*") {
            $result.error = "Required text was not found: $RequiredText"
            return [pscustomobject]$result
        }

        $result.passed = $true
        return [pscustomobject]$result
    }
    catch {
        $result.elapsedMs = [int]((Get-Date) - $started).TotalMilliseconds
        $result.error = $_.Exception.Message

        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $result.statusCode = [int]$_.Exception.Response.StatusCode
        }

        return [pscustomobject]$result
    }
}

if ($UseLocalDevelopmentSeed.IsPresent) {
    $seed = Read-LocalDevelopmentSeed -Path $LocalSettingsPath
    $AdminEmail = $seed.Email
    $AdminPassword = $seed.Password
}

if ([string]::IsNullOrWhiteSpace($AdminEmail) -or [string]::IsNullOrWhiteSpace($AdminPassword)) {
    throw "Admin credentials are required. Set DARWINLINGUA_WEB_ADMIN_EMAIL and DARWINLINGUA_WEB_ADMIN_PASSWORD, pass -AdminEmail/-AdminPassword, or use -UseLocalDevelopmentSeed for local-only smoke."
}

$web = Normalize-BaseUrl -Value $WebBaseUrl
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginUrl = "$web/Identity/Account/Login?ReturnUrl=%2Fadmin"

$loginPage = Invoke-WebRequest -Uri $loginUrl -WebSession $session -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
$token = Get-HiddenInputValue -Html $loginPage.Content -Name "__RequestVerificationToken"
$returnUrl = Get-HiddenInputValue -Html $loginPage.Content -Name "ReturnUrl"

if ([string]::IsNullOrWhiteSpace($token)) {
    throw "Login antiforgery token was not found."
}

if ([string]::IsNullOrWhiteSpace($returnUrl)) {
    $returnUrl = "/admin"
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

$checks = [System.Collections.Generic.List[object]]::new()
$checks.Add((New-CheckResult -Session $session -Name "Admin dashboard" -Url "$web/admin" -RequiredText "Admin"))
$checks.Add((New-CheckResult -Session $session -Name "Admin reports" -Url "$web/admin/reports" -RequiredText "Learning Portal"))
$checks.Add((New-CheckResult -Session $session -Name "Admin diagnostics" -Url "$web/admin/diagnostics" -RequiredText "Diagnostics"))
$checks.Add((New-CheckResult -Session $session -Name "Admin publishing" -Url "$web/admin/publishing" -RequiredText "Publishing"))
$checks.Add((New-CheckResult -Session $session -Name "Admin imports" -Url "$web/admin/imports" -RequiredText "Imports"))
$checks.Add((New-CheckResult -Session $session -Name "Admin words" -Url "$web/admin/words" -RequiredText "Words"))
$checks.Add((New-CheckResult -Session $session -Name "Admin topics" -Url "$web/admin/topics" -RequiredText "Topics"))
$checks.Add((New-CheckResult -Session $session -Name "Admin users" -Url "$web/admin/users" -RequiredText "Users"))
$checks.Add((New-CheckResult -Session $session -Name "Admin moderation" -Url "$web/admin/moderation" -RequiredText "Moderation"))
$checks.Add((New-CheckResult -Session $session -Name "Admin billing diagnostics" -Url "$web/admin/billing-diagnostics" -RequiredText "Billing"))
$checks.Add((New-CheckResult -Session $session -Name "Admin email diagnostics" -Url "$web/admin/email-diagnostics" -RequiredText "Email"))

$failed = @($checks | Where-Object { -not $_.passed })
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Join-Path $repoRoot $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$reportPath = Join-Path $outputRoot "web-admin-authenticated-smoke-$timestamp.json"

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    webBaseUrl = $web
    timeoutSeconds = $TimeoutSeconds
    credentialSource = if ($UseLocalDevelopmentSeed.IsPresent) { "local-development-seed" } else { "provided" }
    adminEmail = $AdminEmail
    totalChecks = $checks.Count
    passedChecks = $checks.Count - $failed.Count
    failedChecks = $failed.Count
    checks = @($checks)
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

foreach ($check in $checks) {
    $status = if ($check.passed) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1} ({2} ms) {3}" -f $status, $check.name, $check.elapsedMs, $check.url)
    if (-not $check.passed) {
        Write-Host ("      {0}" -f $check.error) -ForegroundColor Red
    }
}

Write-Host "Report: $reportPath"

if ($failed.Count -gt 0) {
    exit 1
}
