[CmdletBinding()]
param(
    [ValidateSet("not-reviewed", "passed", "passed-with-notes", "failed")]
    [string]$MailboxReviewStatus = "not-reviewed",

    [ValidateSet("not-reviewed", "passed", "passed-with-notes", "failed", "not-in-scope-for-this-pass")]
    [string]$PwaDesktopStatus = "not-reviewed",

    [ValidateSet("not-reviewed", "passed", "passed-with-notes", "failed", "not-in-scope-for-this-pass")]
    [string]$PwaAndroidStatus = "not-reviewed",

    [ValidateSet("not-started", "ready-to-invite", "needs-fix-before-invite", "in-progress", "closed", "failed")]
    [string]$TesterPassStatus = "not-started",

    [string]$Reviewer = "",
    [string]$TesterBundle = "",
    [string]$MailboxEvidence = "",
    [string]$PwaEvidence = "",
    [string]$FeedbackTriageReport = "",
    [string]$Notes = "",
    [string]$OutputDirectory = "artifacts/validation/web-manual-external-review",
    [switch]$FailOnIncomplete,
    [switch]$FailOnFailed
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

function Test-ClosedStatus {
    param([string]$Status)

    return $Status -in @("passed", "passed-with-notes", "not-in-scope-for-this-pass")
}

function Test-TesterStartStatus {
    param([string]$Status)

    return $Status -in @("ready-to-invite", "in-progress", "closed")
}

function Test-HasEvidence {
    param([string]$Value)

    return -not [string]::IsNullOrWhiteSpace($Value)
}

function Format-MarkdownCell {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    return ($Value -replace "\|", "\|" -replace "`r?`n", "<br>")
}

$mailboxClosed = Test-ClosedStatus -Status $MailboxReviewStatus
$pwaDesktopClosed = Test-ClosedStatus -Status $PwaDesktopStatus
$pwaAndroidClosed = Test-ClosedStatus -Status $PwaAndroidStatus
$testerClosed = Test-TesterStartStatus -Status $TesterPassStatus

$failedStatuses = @()
if ($MailboxReviewStatus -eq "failed") { $failedStatuses += "mailbox" }
if ($PwaDesktopStatus -eq "failed") { $failedStatuses += "pwa-desktop" }
if ($PwaAndroidStatus -eq "failed") { $failedStatuses += "pwa-android" }
if ($TesterPassStatus -in @("failed", "needs-fix-before-invite")) { $failedStatuses += "tester-pass" }

$openGates = @()
if (-not $mailboxClosed) { $openGates += "mailbox-rendering" }
if (-not $pwaDesktopClosed) { $openGates += "pwa-desktop-install" }
if (-not $pwaAndroidClosed) { $openGates += "pwa-android-install" }
if (-not $testerClosed) { $openGates += "controlled-tester-pass" }
if ($mailboxClosed -and -not (Test-HasEvidence -Value $MailboxEvidence)) { $openGates += "mailbox-evidence-missing" }
if ($pwaDesktopClosed -and -not (Test-HasEvidence -Value $PwaEvidence)) { $openGates += "pwa-desktop-evidence-missing" }
if ($pwaAndroidClosed -and -not (Test-HasEvidence -Value $PwaEvidence)) { $openGates += "pwa-android-evidence-missing" }
if ($testerClosed -and -not (Test-HasEvidence -Value $TesterBundle)) { $openGates += "tester-bundle-missing" }
if ($TesterPassStatus -eq "closed" -and -not (Test-HasEvidence -Value $FeedbackTriageReport)) { $openGates += "feedback-triage-evidence-missing" }

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Join-Path $repoRoot $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

$jsonPath = Join-Path $outputRoot "web-manual-external-review-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-manual-external-review-$timestamp.md"

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    reviewer = $Reviewer
    publicWeb = "https://darwinlingua.com"
    publicApi = "https://api.darwinlingua.com/health"
    requiredWwwHost = $false
    mailboxReviewStatus = $MailboxReviewStatus
    pwaDesktopStatus = $PwaDesktopStatus
    pwaAndroidStatus = $PwaAndroidStatus
    testerPassStatus = $TesterPassStatus
    testerBundle = $TesterBundle
    mailboxEvidence = $MailboxEvidence
    pwaEvidence = $PwaEvidence
    feedbackTriageReport = $FeedbackTriageReport
    notes = $Notes
    openGateCount = $openGates.Count
    failedGateCount = $failedStatuses.Count
    openGates = @($openGates)
    failedGates = @($failedStatuses)
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Manual External Review Report")
$lines.Add("")
$lines.Add(("- Generated: {0}" -f $report.generatedAtUtc))
$lines.Add(("- Reviewer: {0}" -f $(if ([string]::IsNullOrWhiteSpace($Reviewer)) { "not recorded" } else { $Reviewer })))
$lines.Add("- Public Web: https://darwinlingua.com")
$lines.Add("- Public API: https://api.darwinlingua.com/health")
$lines.Add("- Required www host: false")
$lines.Add("")
$lines.Add("## Gate Summary")
$lines.Add("")
$lines.Add("| Gate | Status | Evidence |")
$lines.Add("| --- | --- | --- |")
$lines.Add("| Transactional email mailbox rendering | $(Format-MarkdownCell $MailboxReviewStatus) | $(Format-MarkdownCell $MailboxEvidence) |")
$lines.Add("| PWA desktop install acceptance | $(Format-MarkdownCell $PwaDesktopStatus) | $(Format-MarkdownCell $PwaEvidence) |")
$lines.Add("| PWA Android install acceptance | $(Format-MarkdownCell $PwaAndroidStatus) | $(Format-MarkdownCell $PwaEvidence) |")
$lines.Add("| Controlled tester pass | $(Format-MarkdownCell $TesterPassStatus) | $(Format-MarkdownCell $FeedbackTriageReport) |")
$lines.Add("")
$lines.Add(("- Open gates: {0}" -f $openGates.Count))
$lines.Add(("- Failed gates: {0}" -f $failedStatuses.Count))
$lines.Add("")

if ($openGates.Count -gt 0) {
    $lines.Add("## Open Gates")
    $lines.Add("")
    foreach ($gate in $openGates) {
        $lines.Add("- $gate")
    }
    $lines.Add("")
}

if ($failedStatuses.Count -gt 0) {
    $lines.Add("## Failed Gates")
    $lines.Add("")
    foreach ($gate in $failedStatuses) {
        $lines.Add("- $gate")
    }
    $lines.Add("")
}

$lines.Add("## Evidence Paths")
$lines.Add("")
$lines.Add(("- Tester bundle: {0}" -f $(if ([string]::IsNullOrWhiteSpace($TesterBundle)) { "not recorded" } else { $TesterBundle })))
$lines.Add(("- Mailbox evidence: {0}" -f $(if ([string]::IsNullOrWhiteSpace($MailboxEvidence)) { "not recorded" } else { $MailboxEvidence })))
$lines.Add(("- PWA evidence: {0}" -f $(if ([string]::IsNullOrWhiteSpace($PwaEvidence)) { "not recorded" } else { $PwaEvidence })))
$lines.Add(("- Feedback triage report: {0}" -f $(if ([string]::IsNullOrWhiteSpace($FeedbackTriageReport)) { "not recorded" } else { $FeedbackTriageReport })))
$lines.Add("")
$lines.Add("## Notes")
$lines.Add("")
$lines.Add($(if ([string]::IsNullOrWhiteSpace($Notes)) { "No notes recorded." } else { $Notes }))

$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Manual external review JSON report: $jsonPath"
Write-Host "Manual external review Markdown report: $markdownPath"
Write-Host "Open gates: $($openGates.Count)"
Write-Host "Failed gates: $($failedStatuses.Count)"

if ($FailOnFailed.IsPresent -and $failedStatuses.Count -gt 0) {
    exit 2
}

if ($FailOnIncomplete.IsPresent -and $openGates.Count -gt 0) {
    exit 3
}
