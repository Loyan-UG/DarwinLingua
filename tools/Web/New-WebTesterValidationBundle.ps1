[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$ApiBaseUrl = "https://api.darwinlingua.com",
    [string]$OutputRoot = "artifacts/validation/web-tester-runs",
    [string]$RunLabel = "web-tester-pass",
    [int]$TimeoutSeconds = 20,
    [switch]$SkipPreflight
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

function ConvertTo-SafeName {
    param([string]$Value)

    $safe = if ([string]::IsNullOrWhiteSpace($Value)) { "web-tester-pass" } else { $Value.Trim().ToLowerInvariant() }
    $safe = $safe -replace "[^a-z0-9._-]+", "-"
    $safe = $safe.Trim("-")
    if ([string]::IsNullOrWhiteSpace($safe)) {
        return "web-tester-pass"
    }

    return $safe
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$safeLabel = ConvertTo-SafeName -Value $RunLabel
$runDirectory = Join-Path (Join-Path $repoRoot $OutputRoot) "$timestamp-$safeLabel"
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null

$runbookPath = Join-Path $repoRoot "docs\87-Web-Tester-Onboarding-Runbook.md"
$quickStartPath = Join-Path $repoRoot "docs\88-Web-Tester-Quick-Start.md"
$templatePath = Join-Path $repoRoot "docs\87-Web-Tester-Feedback-Template.csv"
$testerAccountsTemplatePath = Join-Path $repoRoot "tools\Web\WebTesterAccounts.example.csv"
$preflightScript = Join-Path $repoRoot "tools\Web\Invoke-WebTesterPreflight.ps1"
$triageScript = Join-Path $repoRoot "tools\Web\Convert-WebTesterFeedbackToReport.ps1"
$premiumAccessScript = Join-Path $repoRoot "tools\Web\Set-WebTesterPremiumAccess.ps1"

foreach ($path in @($runbookPath, $quickStartPath, $templatePath, $testerAccountsTemplatePath, $preflightScript, $triageScript, $premiumAccessScript)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Required file is missing: $path"
    }
}

Copy-Item -LiteralPath $runbookPath -Destination (Join-Path $runDirectory "WebTesterOnboardingRunbook.md") -Force
Copy-Item -LiteralPath $quickStartPath -Destination (Join-Path $runDirectory "TesterQuickStart.md") -Force
Copy-Item -LiteralPath $templatePath -Destination (Join-Path $runDirectory "WebTesterFeedback.csv") -Force
Copy-Item -LiteralPath $testerAccountsTemplatePath -Destination (Join-Path $runDirectory "WebTesterAccounts.csv") -Force

$preflightOutputRelative = Join-Path $OutputRoot "$timestamp-$safeLabel\preflight"
$preflightStatus = "skipped"

if (-not $SkipPreflight.IsPresent) {
    try {
        & $preflightScript `
            -WebBaseUrl $WebBaseUrl `
            -ApiBaseUrl $ApiBaseUrl `
            -OutputDirectory $preflightOutputRelative `
            -TimeoutSeconds $TimeoutSeconds
    }
    catch {
        throw "Preflight failed. Bundle directory: $runDirectory. $($_.Exception.Message)"
    }

    if (-not $?) {
        throw "Preflight failed. Bundle directory: $runDirectory"
    }

    $preflightStatus = "passed"
}

$readmePath = Join-Path $runDirectory "README.md"
$readme = @"
# Web Tester Validation Bundle

- Generated: $((Get-Date).ToUniversalTime().ToString("O"))
- Web base URL: $WebBaseUrl
- API base URL: $ApiBaseUrl
- Preflight status: $preflightStatus

## Files

- `WebTesterOnboardingRunbook.md`: operator and tester task instructions.
- `TesterQuickStart.md`: short tester-facing instructions to share directly with testers.
- `WebTesterFeedback.csv`: copy this file and add one row per tester observation.
- `WebTesterAccounts.csv`: operator-only account list for the first wave. Replace sample rows with existing tester account emails before granting Premium.
- `preflight/`: generated JSON preflight report when preflight is enabled.

## Account Access

For the Brevo-ready controlled tester pass, let testers self-register with their own email address first so registration confirmation, password reset, and email-change behavior can be validated through the real public email path.

Use `WebTesterAccounts.csv` only as an operator-only account list for testers who should receive Premium access during the pass. After the accounts already exist, replace the sample rows with real tester account emails and run from the repository root:

~~~powershell
.\tools\Web\Set-WebTesterPremiumAccess.ps1 -TesterCsvPath "$($runDirectory)\WebTesterAccounts.csv" -UpdatedBy "$safeLabel"
~~~

The Premium tool only updates existing users, confirms their email if needed, and writes entitlement audit events. It does not create passwords or send email.

## Triage

After tester rows are collected, run from the repository root:

~~~powershell
.\tools\Web\Convert-WebTesterFeedbackToReport.ps1 -FeedbackCsvPath "$($runDirectory)\WebTesterFeedback.csv"
~~~

For release gating, use:

~~~powershell
.\tools\Web\Convert-WebTesterFeedbackToReport.ps1 -FeedbackCsvPath "$($runDirectory)\WebTesterFeedback.csv" -FailOnMajor
~~~

## Gate

Do not start new bulk content or mobile work until blocker and major feedback items are triaged.
"@

Set-Content -LiteralPath $readmePath -Value $readme -Encoding UTF8

$manifest = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    runLabel = $safeLabel
    runDirectory = $runDirectory
    webBaseUrl = $WebBaseUrl
    apiBaseUrl = $ApiBaseUrl
    preflightStatus = $preflightStatus
    files = @(
        "README.md",
        "WebTesterOnboardingRunbook.md",
        "TesterQuickStart.md",
        "WebTesterAccounts.csv",
        "WebTesterFeedback.csv"
    )
}

$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $runDirectory "manifest.json") -Encoding UTF8

Write-Host "Web tester validation bundle ready: $runDirectory"
Write-Host "Preflight status: $preflightStatus"
