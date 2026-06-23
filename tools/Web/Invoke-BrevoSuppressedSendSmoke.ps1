param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$PasswordResetEmail = "shahramvafadar@gmail.com",
    [string]$OutputDirectory = "artifacts/validation/brevo-suppressed-send-smoke",
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

function Escape-SqlLiteral {
    param([string]$Value)

    return $Value.Replace("'", "''")
}

function Invoke-ScalarPsql {
    param([string]$Sql)

    $result = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql scalar query failed."
    }

    return ($result | Out-String).Trim()
}

function Invoke-CheckedPsql {
    param([string]$Sql)

    docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -v ON_ERROR_STOP=1 -c $Sql | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "psql command failed."
    }
}

function Get-Sha256Hex {
    param([string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value.Trim().ToUpperInvariant())
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
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

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $repoRoot

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runStartedAtUtc = [DateTimeOffset]::UtcNow
$normalizedWebBaseUrl = $WebBaseUrl.TrimEnd("/")
$escapedEmail = Escape-SqlLiteral -Value $PasswordResetEmail
$recipientEmailHash = Get-Sha256Hex -Value $PasswordResetEmail
$suppressionId = [Guid]::NewGuid().ToString()
$providerMessageId = "<darwinlingua-suppressed-send-smoke-$timestamp@smtp-relay.mailin.fr>"
$suppressionInserted = $false
$cleanupAttempted = $false
$cleanupSucceeded = $false

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "brevo-suppressed-send-smoke-$timestamp.json"
$markdownPath = Join-Path $outputRoot "brevo-suppressed-send-smoke-$timestamp.md"

$userStatusSql = @"
select coalesce(max(case when "EmailConfirmed" then 1 else 0 end), 0)::text
from "AspNetUsers"
where lower("Email") = lower('$escapedEmail');
"@
$userConfirmed = Invoke-ScalarPsql -Sql $userStatusSql
if ($userConfirmed -ne "1") {
    throw "Password reset target user was not found or is not email-confirmed. Recipient value was not printed."
}

$existingSuppressionCount = Invoke-ScalarPsql -Sql "select count(*) from ""WebEmailSuppressions"" where ""RecipientEmailHash"" = '$recipientEmailHash';"
if ($existingSuppressionCount -ne "0") {
    throw "Recipient hash is already suppressed. The smoke does not modify existing suppressions."
}

try {
    $insertSuppressionSql = @"
insert into "WebEmailSuppressions" (
    "Id",
    "RecipientEmailHash",
    "Reason",
    "ProviderName",
    "ProviderMessageId",
    "CreatedAtUtc",
    "LastSeenAtUtc"
) values (
    '$suppressionId',
    '$recipientEmailHash',
    'smoke:temporary-suppression',
    'brevo-api',
    '$(Escape-SqlLiteral -Value $providerMessageId)',
    '$($runStartedAtUtc.UtcDateTime.ToString("O"))',
    '$($runStartedAtUtc.UtcDateTime.ToString("O"))'
);
"@
    Invoke-CheckedPsql -Sql $insertSuppressionSql
    $suppressionInserted = $true

    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $forgotPage = Invoke-WebRequest `
        -Uri "$normalizedWebBaseUrl/Identity/Account/ForgotPassword" `
        -WebSession $session `
        -MaximumRedirection 5
    $forgotToken = Get-AntiForgeryToken -Html $forgotPage.Content

    $forgotResponse = Invoke-FormPost `
        -Session $session `
        -Url "$normalizedWebBaseUrl/Identity/Account/ForgotPassword" `
        -Body @{
            "__RequestVerificationToken" = $forgotToken
            "Input.Email" = $PasswordResetEmail
        }

    Start-Sleep -Seconds 2

    $runStartedAtSql = Escape-SqlLiteral -Value $runStartedAtUtc.UtcDateTime.ToString("O")
    $logSql = @"
select
    "Status" || '|' ||
    coalesce("ProviderName", '') || '|' ||
    coalesce("FailureCode", '') || '|' ||
    coalesce("ProviderMessageId", '') || '|' ||
    coalesce("ScenarioKey", '')
from "WebEmailDeliveryLogs"
where "CreatedAtUtc" >= '$runStartedAtSql'::timestamptz
  and "RecipientEmailHash" = '$recipientEmailHash'
  and "ScenarioKey" = 'Account.PasswordReset'
order by "CreatedAtUtc" desc
limit 1;
"@
    $logResult = Invoke-ScalarPsql -Sql $logSql
    $logParts = $logResult.Split("|", 5)

    $passed = [int]$forgotResponse.StatusCode -in @(200, 302) -and
        $logParts.Length -eq 5 -and
        $logParts[0] -eq "Suppressed" -and
        $logParts[1] -eq "brevo-api" -and
        $logParts[2] -eq "recipient-suppressed" -and
        [string]::IsNullOrWhiteSpace($logParts[3]) -and
        $logParts[4] -eq "Account.PasswordReset"

    $report = [ordered]@{
        generatedAtUtc = [DateTimeOffset]::UtcNow
        webBaseUrl = $normalizedWebBaseUrl
        httpStatus = [int]$forgotResponse.StatusCode
        passed = $passed
        recipientEmailHash = $recipientEmailHash
        temporarySuppressionInserted = $suppressionInserted
        temporarySuppressionId = $suppressionId
        deliveryStatus = $(if ($logParts.Length -ge 1) { $logParts[0] } else { $null })
        providerName = $(if ($logParts.Length -ge 2) { $logParts[1] } else { $null })
        failureCode = $(if ($logParts.Length -ge 3) { $logParts[2] } else { $null })
        providerMessageIdStored = $(if ($logParts.Length -ge 4) { -not [string]::IsNullOrWhiteSpace($logParts[3]) } else { $null })
        scenarioKey = $(if ($logParts.Length -ge 5) { $logParts[4] } else { $null })
    }

    if (-not $passed) {
        $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8
        throw "Suppressed-send smoke failed. Recipient value was not printed."
    }
}
finally {
    if ($suppressionInserted) {
        $cleanupAttempted = $true
        Invoke-CheckedPsql -Sql "delete from ""WebEmailSuppressions"" where ""Id"" = '$suppressionId';"
        $cleanupSucceeded = $true
    }
}

$report.cleanupAttempted = $cleanupAttempted
$report.cleanupSucceeded = $cleanupSucceeded
$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Brevo Suppressed Send Smoke")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Web base URL: $normalizedWebBaseUrl")
$lines.Add("- HTTP status: $($report.httpStatus)")
$lines.Add("- Passed: $($report.passed)")
$lines.Add("- Recipient hash: $recipientEmailHash")
$lines.Add("- Temporary suppression cleanup attempted: $cleanupAttempted")
$lines.Add("- Temporary suppression cleanup succeeded: $cleanupSucceeded")
$lines.Add("")
$lines.Add("| Check | Value |")
$lines.Add("| --- | --- |")
$lines.Add("| deliveryStatus | $($report.deliveryStatus) |")
$lines.Add("| providerName | $($report.providerName) |")
$lines.Add("| failureCode | $($report.failureCode) |")
$lines.Add("| providerMessageIdStored | $($report.providerMessageIdStored) |")
$lines.Add("| scenarioKey | $($report.scenarioKey) |")
$lines.Add("")
$lines.Add("The smoke creates a temporary internal suppression for a confirmed account, submits the public forgot-password form, verifies that the app records a `Suppressed` delivery log without calling Brevo, and removes only the temporary suppression it created.")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Brevo suppressed-send smoke report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $($report.passed)"

if (-not $report.passed) {
    exit 1
}
