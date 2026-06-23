[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/web-human-gate-handoff",
    [string]$ReadinessAuditJson = "",
    [string]$Reviewer = "",
    [switch]$GenerateFreshAudit
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

function Resolve-RepositoryPath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path $repoRoot $Path
}

function Convert-ToRepositoryRelativePath {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return ""
    }

    $resolved = Resolve-Path -LiteralPath $Path -ErrorAction SilentlyContinue
    if ($null -eq $resolved) {
        return $Path
    }

    $fullPath = $resolved.Path
    if ($fullPath.StartsWith($repoRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        return ".\" + $fullPath.Substring($repoRoot.Length).TrimStart('\')
    }

    return $fullPath
}

function Get-LatestJsonReport {
    param([string]$RelativeDirectory)

    $directory = Resolve-RepositoryPath -Path $RelativeDirectory
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        return ""
    }

    $latest = Get-ChildItem -LiteralPath $directory -Filter "*.json" -File |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latest) {
        return ""
    }

    return $latest.FullName
}

if ($GenerateFreshAudit) {
    & (Join-Path $scriptRoot "New-WebControlledTesterReadinessAudit.ps1") -FailOnAutomatedFailure | Out-Host
}

$auditPath = if ([string]::IsNullOrWhiteSpace($ReadinessAuditJson)) {
    Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-controlled-tester-readiness"
}
else {
    Resolve-RepositoryPath -Path $ReadinessAuditJson
}

if ([string]::IsNullOrWhiteSpace($auditPath) -or -not (Test-Path -LiteralPath $auditPath -PathType Leaf)) {
    throw "Readiness audit JSON was not found. Run New-WebControlledTesterReadinessAudit.ps1 first or pass -ReadinessAuditJson."
}

$audit = Get-Content -LiteralPath $auditPath -Raw | ConvertFrom-Json
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$markdownPath = Join-Path $outputRoot "web-human-gate-handoff-$timestamp.md"
$jsonPath = Join-Path $outputRoot "web-human-gate-handoff-$timestamp.json"

$manualReportPath = [string]$audit.manualReviewReport
$testerBundle = [string]$audit.testerBundle
$mailboxPacketPath = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-mailbox-rendering-review-packet"
if ([string]::IsNullOrWhiteSpace($mailboxPacketPath)) {
    $latestPacketDirectory = Get-ChildItem -LiteralPath (Resolve-RepositoryPath -Path "artifacts/validation/web-mailbox-rendering-review-packet") -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if ($latestPacketDirectory) {
        $mailboxPacketPath = $latestPacketDirectory.FullName
    }
}

$latestPwaReport = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/pwa-installability"
$latestFeedbackReport = Get-ChildItem -LiteralPath (Resolve-RepositoryPath -Path "artifacts/validation/web-tester-feedback") -Filter "*.md" -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

$manualStatuses = $audit.manualStatuses
$openHumanStartGates = @($audit.humanStartOpenGates | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })

$handoff = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    reviewer = $Reviewer
    readinessAudit = Convert-ToRepositoryRelativePath -Path $auditPath
    automatedReady = [bool]$audit.automatedReady
    controlledTesterReadyToInvite = [bool]$audit.controlledTesterReadyToInvite
    openHumanStartGates = @($openHumanStartGates)
    manualReviewReport = Convert-ToRepositoryRelativePath -Path $manualReportPath
    testerBundle = Convert-ToRepositoryRelativePath -Path $testerBundle
    mailboxPacket = Convert-ToRepositoryRelativePath -Path $mailboxPacketPath
    pwaEvidence = Convert-ToRepositoryRelativePath -Path $latestPwaReport
    feedbackTriageReport = if ($latestFeedbackReport) { Convert-ToRepositoryRelativePath -Path $latestFeedbackReport.FullName } else { "" }
    requiredWwwHost = $false
}

$handoff | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$mailboxEvidenceCsv = if (-not [string]::IsNullOrWhiteSpace($mailboxPacketPath) -and (Test-Path -LiteralPath $mailboxPacketPath -PathType Container)) {
    Convert-ToRepositoryRelativePath -Path (Join-Path $mailboxPacketPath "MailboxRenderingEvidence.csv")
}
else {
    ".\artifacts\validation\web-mailbox-rendering-review-packet\<packet>\MailboxRenderingEvidence.csv"
}

$pwaEvidencePath = if ([string]::IsNullOrWhiteSpace($handoff.pwaEvidence)) { ".\artifacts\validation\pwa-installability\<latest>.json" } else { $handoff.pwaEvidence }
$feedbackPath = if ([string]::IsNullOrWhiteSpace($handoff.feedbackTriageReport)) { ".\artifacts\validation\web-tester-feedback\<report>.md" } else { $handoff.feedbackTriageReport }
$testerBundlePath = if ([string]::IsNullOrWhiteSpace($handoff.testerBundle)) { ".\artifacts\validation\web-tester-runs\<run-id>" } else { $handoff.testerBundle }

$statusLines = @(
    "| Gate | Current status | What closes it | Evidence |",
    "| --- | --- | --- | --- |",
    "| Mailbox rendering | $($manualStatuses.mailboxReviewStatus) | Real inbox review in `info@darwinlingua.com` passes or passes with notes. | `$mailboxEvidenceCsv` |",
    "| PWA desktop install | $($manualStatuses.pwaDesktopStatus) | Desktop Chrome/Edge install prompt is accepted and installed window behavior is checked, or explicitly marked out of scope. | `$pwaEvidencePath` plus notes |",
    "| PWA Android install | $($manualStatuses.pwaAndroidStatus) | Android Chrome install flow is checked, or explicitly marked out of scope. | `$pwaEvidencePath` plus notes |",
    "| Tester pass start | $($manualStatuses.testerPassStatus) | Operator marks pass `ready-to-invite`, `in-progress`, or `closed` after confirming tester bundle and scope. | `$testerBundlePath` |"
)

$markdown = @"
# Web Human Gate Handoff

- Generated: $($handoff.generatedAtUtc)
- Reviewer: $(if ([string]::IsNullOrWhiteSpace($Reviewer)) { "not recorded" } else { $Reviewer })
- Public Web: https://darwinlingua.com
- Public API: https://api.darwinlingua.com/health
- Required `www` host: false
- Readiness audit: $($handoff.readinessAudit)
- Automated ready: $($handoff.automatedReady)
- Controlled tester ready to invite: $($handoff.controlledTesterReadyToInvite)

This handoff is intentionally limited to human gates. It does not approve broad public launch, legal sign-off, paid billing, or mobile work.

Known human start gate keys: `mailbox-rendering`, `pwa-desktop-install`, `pwa-android-install`, `tester-pass-start-status`.

## Current Gate Status

$($statusLines -join "`n")

## Exact Operator Sequence

1. Generate or reuse the mailbox review packet:

```powershell
.\tools\Web\New-WebMailboxRenderingReviewPacket.ps1
```

2. Review real emails in `info@darwinlingua.com` using the generated `MailboxRenderingReview.md`. Record only safe notes in `MailboxRenderingEvidence.csv`; do not store raw action URLs, reset tokens, webhook secrets, API keys, provider message ids, diagnostic hashes, or full real email bodies.

3. Complete the PWA checks from `docs/56-Web-Pwa-Install-Validation-Worksheet.md`. Use `not-in-scope-for-this-pass` only when the device/browser check is intentionally deferred for the controlled tester pass.

4. Confirm the tester bundle is the current Brevo-ready bundle and that testers will use `https://darwinlingua.com`, not `www.darwinlingua.com`.

5. Generate the manual external review report:

```powershell
.\tools\Web\New-WebManualExternalReviewReport.ps1 ``
  -MailboxReviewStatus passed-with-notes ``
  -PwaDesktopStatus passed ``
  -PwaAndroidStatus not-in-scope-for-this-pass ``
  -TesterPassStatus ready-to-invite ``
  -TesterBundle "$testerBundlePath" ``
  -MailboxEvidence "$mailboxEvidenceCsv" ``
  -PwaEvidence "$pwaEvidencePath" ``
  -FeedbackTriageReport "$feedbackPath" ``
  -Reviewer "$Reviewer"
```

6. Run the final pre-invite audit:

```powershell
.\tools\Web\New-WebControlledTesterReadinessAudit.ps1 -FailOnAutomatedFailure -FailOnOpenHumanGates
```

## Brevo Authorized IP Note

If `Invoke-BrevoWebhookConfigurationCheck.ps1` returns an 'unrecognised IP address' error, add the current machine/server IP in Brevo under `Security` -> `Authorised IPs`, then rerun:

```powershell
.\tools\Web\Invoke-BrevoWebhookConfigurationCheck.ps1
```

Do not paste Brevo API keys or webhook tokens into this handoff or any Git-tracked file.

## Remaining Decisions

- Broad public launch remains blocked until legal/operator review is accepted.
- Paid billing remains deferred until Stripe production validation is intentionally resumed.
- Mobile/MAUI remains deferred until the Web tester pass has produced and resolved feedback.
"@

Set-Content -LiteralPath $markdownPath -Value $markdown -Encoding UTF8

Write-Host "Web human gate handoff Markdown: $markdownPath"
Write-Host "Web human gate handoff JSON: $jsonPath"
Write-Host "Open human start gates: $($openHumanStartGates.Count)"
