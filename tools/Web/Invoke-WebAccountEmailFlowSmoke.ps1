[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$RegistrationEmailPrefix = "shahramvafadar+darwinlingua-smoke",
    [string]$RegistrationEmailDomain = "gmail.com",
    [string]$PasswordResetEmail = "shahramvafadar@gmail.com",
    [string]$OutputDirectory = "artifacts/validation/web-account-email-smoke",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-RepositoryPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path (Get-Location) $Path
}

function Get-AntiForgeryToken {
    param([string]$Html)

    $match = [regex]::Match(
        $Html,
        'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"|type="hidden"\s+value="([^"]+)"\s+name="__RequestVerificationToken"',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        throw "Anti-forgery token was not found in the page."
    }

    if ($match.Groups[1].Success) {
        return $match.Groups[1].Value
    }

    return $match.Groups[2].Value
}

function Invoke-FormPost {
    param(
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session,
        [string]$Url,
        [hashtable]$Body
    )

    try {
        return Invoke-WebRequest `
            -Uri $Url `
            -Method Post `
            -WebSession $Session `
            -Body $Body `
            -MaximumRedirection 5 `
            -SkipHttpErrorCheck
    }
    catch {
        $responseProperty = $_.Exception.PSObject.Properties["Response"]
        if ($null -ne $responseProperty -and $null -ne $responseProperty.Value) {
            return $responseProperty.Value
        }

        throw
    }
}

function Invoke-ScalarPsql {
    param([string]$Sql)

    $result = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql scalar query failed."
    }

    return ($result | Out-String).Trim()
}

function Invoke-JsonPsql {
    param([string]$Sql)

    $raw = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql JSON query failed."
    }

    $text = ($raw | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return @()
    }

    return $text | ConvertFrom-Json
}

function ConvertTo-PostgresSqlLiteral {
    param([string]$Value)

    return "'" + $Value.Replace("'", "''") + "'"
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $repoRoot

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$jsonPath = Join-Path $outputRoot "web-account-email-smoke-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-account-email-smoke-$timestamp.md"

$runStartedAtUtc = (Get-Date).ToUniversalTime()
$safeRunId = $timestamp.ToLowerInvariant()
$registrationEmail = "$RegistrationEmailPrefix-$safeRunId@$RegistrationEmailDomain"
$password = "DarwinSmoke-$timestamp!"
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$normalizedWebBaseUrl = $WebBaseUrl.TrimEnd("/")

$registerPage = Invoke-WebRequest `
    -Uri "$normalizedWebBaseUrl/Identity/Account/Register" `
    -WebSession $session `
    -MaximumRedirection 5
$registerToken = Get-AntiForgeryToken -Html $registerPage.Content

$registerResponse = Invoke-FormPost `
    -Session $session `
    -Url "$normalizedWebBaseUrl/Identity/Account/Register" `
    -Body @{
        "__RequestVerificationToken" = $registerToken
        "Input.Email" = $registrationEmail
        "Input.Password" = $password
        "Input.ConfirmPassword" = $password
        "Input.AcceptTermsOfUse" = "true"
        "Input.AcknowledgePrivacyNotice" = "true"
        "ReturnUrl" = ""
    }

$forgotSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$forgotPage = Invoke-WebRequest `
    -Uri "$normalizedWebBaseUrl/Identity/Account/ForgotPassword" `
    -WebSession $forgotSession `
    -MaximumRedirection 5
$forgotToken = Get-AntiForgeryToken -Html $forgotPage.Content

$forgotResponse = Invoke-FormPost `
    -Session $forgotSession `
    -Url "$normalizedWebBaseUrl/Identity/Account/ForgotPassword" `
    -Body @{
        "__RequestVerificationToken" = $forgotToken
        "Input.Email" = $PasswordResetEmail
    }

Start-Sleep -Seconds 3

$registrationEmailSql = ConvertTo-PostgresSqlLiteral -Value $registrationEmail
$runStartedAtSql = ConvertTo-PostgresSqlLiteral -Value $runStartedAtUtc.ToString("O")
$registrationUserExists = Invoke-ScalarPsql -Sql "select count(*) from ""AspNetUsers"" where lower(""Email"") = lower($registrationEmailSql);"
$recentLogsSql = @"
select coalesce(json_agg(row_to_json(t)), '[]'::json)
from (
    select
        "ScenarioKey",
        "Status",
        "ProviderName",
        "ProviderMessageId",
        "ProviderLastEvent",
        "CreatedAtUtc"
    from "WebEmailDeliveryLogs"
    where "CreatedAtUtc" >= ${runStartedAtSql}::timestamptz
      and "ScenarioKey" in ('Account.EmailConfirmation', 'Account.PasswordReset')
    order by "CreatedAtUtc" desc
) t;
"@
$recentLogs = @(Invoke-JsonPsql -Sql $recentLogsSql)
$confirmationLog = @($recentLogs | Where-Object { $_.ScenarioKey -eq "Account.EmailConfirmation" } | Select-Object -First 1)
$passwordResetLog = @($recentLogs | Where-Object { $_.ScenarioKey -eq "Account.PasswordReset" } | Select-Object -First 1)

$checks = [ordered]@{
    registrationHttpStatus = [int]$registerResponse.StatusCode
    forgotPasswordHttpStatus = [int]$forgotResponse.StatusCode
    registrationUserCreated = $registrationUserExists -eq "1"
    emailConfirmationLogged = $confirmationLog.Count -gt 0
    passwordResetLogged = $passwordResetLog.Count -gt 0
    emailConfirmationProvider = if ($confirmationLog.Count -gt 0) { $confirmationLog[0].ProviderName } else { "" }
    passwordResetProvider = if ($passwordResetLog.Count -gt 0) { $passwordResetLog[0].ProviderName } else { "" }
    emailConfirmationHasProviderMessageId = $confirmationLog.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($confirmationLog[0].ProviderMessageId)
    passwordResetHasProviderMessageId = $passwordResetLog.Count -gt 0 -and -not [string]::IsNullOrWhiteSpace($passwordResetLog[0].ProviderMessageId)
}

$passed =
    $checks.registrationUserCreated -and
    $checks.emailConfirmationLogged -and
    $checks.passwordResetLogged -and
    $checks.emailConfirmationProvider -eq "brevo-api" -and
    $checks.passwordResetProvider -eq "brevo-api" -and
    $checks.emailConfirmationHasProviderMessageId -and
    $checks.passwordResetHasProviderMessageId

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    webBaseUrl = $normalizedWebBaseUrl
    registrationEmail = $registrationEmail
    passwordResetEmail = $PasswordResetEmail
    passed = $passed
    checks = $checks
    logs = $recentLogs
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Web Account Email Flow Smoke")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Web base URL: $normalizedWebBaseUrl")
$lines.Add("- Registration email: $registrationEmail")
$lines.Add("- Password reset email: $PasswordResetEmail")
$lines.Add("- Passed: $passed")
$lines.Add("")
$lines.Add("| Check | Value |")
$lines.Add("| --- | --- |")
foreach ($property in $checks.GetEnumerator()) {
    $lines.Add("| $($property.Key) | $($property.Value) |")
}
$lines.Add("")
$lines.Add("| Scenario | Status | Provider | Provider message id | Provider event | Created |")
$lines.Add("| --- | --- | --- | --- | --- | --- |")
foreach ($log in $recentLogs) {
    $lines.Add("| $($log.ScenarioKey) | $($log.Status) | $($log.ProviderName) | $($log.ProviderMessageId) | $($log.ProviderLastEvent) | $($log.CreatedAtUtc) |")
}

$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web account email smoke report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
