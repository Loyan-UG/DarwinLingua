[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/web-mailbox-rendering-review-packet",
    [string]$TemplatePreviewDirectory = "",
    [switch]$GenerateFreshTemplatePreview,
    [string]$Reviewer = "",
    [string]$TesterBundle = ".\artifacts\validation\web-tester-runs\20260623-172706-web-tester-pass-brevo-ready",
    [string]$PwaEvidence = ".\artifacts\validation\pwa-installability\",
    [string]$FeedbackTriageReport = ".\artifacts\validation\web-tester-feedback\web-tester-feedback-triage-20260623-181053.md"
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

$previewRoot = Resolve-RepositoryPath -Path "artifacts/validation/transactional-email-template-preview"
$templatePreviewPath = ""

if ($GenerateFreshTemplatePreview) {
    & (Join-Path $scriptRoot "New-TransactionalEmailTemplatePreview.ps1") | Out-Host
}

if (-not [string]::IsNullOrWhiteSpace($TemplatePreviewDirectory)) {
    $templatePreviewPath = Resolve-RepositoryPath -Path $TemplatePreviewDirectory
}
else {
    $latestPreview = Get-ChildItem -LiteralPath $previewRoot -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latestPreview) {
        & (Join-Path $scriptRoot "New-TransactionalEmailTemplatePreview.ps1") | Out-Host
        $latestPreview = Get-ChildItem -LiteralPath $previewRoot -Directory -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
    }

    if ($null -eq $latestPreview) {
        throw "No transactional email template preview directory was found or generated."
    }

    $templatePreviewPath = $latestPreview.FullName
}

if (-not (Test-Path -LiteralPath $templatePreviewPath -PathType Container)) {
    throw "Template preview directory was not found: $templatePreviewPath"
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$packetPath = Join-Path $outputRoot "web-mailbox-rendering-review-packet-$timestamp"
New-Item -ItemType Directory -Path $packetPath -Force | Out-Null

$htmlPreviewFiles = Get-ChildItem -LiteralPath $templatePreviewPath -Filter "*.html" -File |
    Where-Object { $_.Name -ne "index.html" } |
    Sort-Object Name

$scenarioRows = [System.Collections.Generic.List[string]]::new()
foreach ($file in $htmlPreviewFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $culture = if ($baseName.EndsWith("-de", [System.StringComparison]::Ordinal)) { "de" } elseif ($baseName.EndsWith("-en", [System.StringComparison]::Ordinal)) { "en" } else { "" }
    $scenario = if ([string]::IsNullOrWhiteSpace($culture)) { $baseName } else { $baseName.Substring(0, $baseName.Length - 3) }
    $scenarioRows.Add("| $scenario | $culture | `$($file.Name)` |")
}

$scenarioTable = if ($scenarioRows.Count -gt 0) {
    "| Scenario | Culture | Preview file |`n| --- | --- | --- |`n" + ($scenarioRows -join "`n")
}
else {
    "No preview files were found. Regenerate with `New-TransactionalEmailTemplatePreview.ps1`."
}

$templatePreviewRelative = Convert-ToRepositoryRelativePath -Path $templatePreviewPath
$packetRelative = Convert-ToRepositoryRelativePath -Path $packetPath

$readme = @"
# Web Mailbox Rendering Review Packet

- Generated: $((Get-Date).ToUniversalTime().ToString("O"))
- Public Web: https://darwinlingua.com
- Public API: https://api.darwinlingua.com
- Required www host: false
- Real mailbox to review: info@darwinlingua.com
- Expected sender: Darwin Lingua <no-reply@darwinlingua.com>
- Expected support/reply path: support@darwinlingua.com
- Safe template preview source: $templatePreviewRelative

This packet closes only the human mailbox-rendering gate. It does not replace Brevo API checks, action-link smoke tests, PWA install checks, tester feedback, or legal review.

Do not paste raw email action URLs, reset tokens, webhook secrets, API keys, provider message ids, diagnostic hashes, or full real email bodies into evidence files. Record only the mail client, device, scenario, pass/fail status, and short notes.

## Files

- `MailboxRenderingReview.md`: step-by-step review checklist.
- `MailboxRenderingEvidence.csv`: evidence template for operator notes.
- `TemplatePreviewManifest.md`: safe preview index copied from generated templates.

## Close The Gate

After real mailbox review is complete, generate the manual external review report from the repository root:

```powershell
.\tools\Web\New-WebManualExternalReviewReport.ps1 ``
  -MailboxReviewStatus passed-with-notes ``
  -PwaDesktopStatus not-reviewed ``
  -PwaAndroidStatus not-in-scope-for-this-pass ``
  -TesterPassStatus ready-to-invite ``
  -TesterBundle "$TesterBundle" ``
  -MailboxEvidence "$packetRelative\MailboxRenderingEvidence.csv" ``
  -PwaEvidence "$PwaEvidence" ``
  -FeedbackTriageReport "$FeedbackTriageReport" ``
  -Reviewer "$Reviewer"
```

Use `passed` only if every required real mailbox check passed with no notes. Use `passed-with-notes` when the emails are acceptable but the operator recorded minor observations.
"@

$review = @"
# Transactional Email Mailbox Rendering Review

## Scope

Review real Brevo-delivered messages in `info@darwinlingua.com`. Use the safe template preview only as a comparison aid; the gate is not closed until real inbox rendering has been checked.

Do not use `www.darwinlingua.com`. Action links must use `https://darwinlingua.com`.

## Required Real Mailbox Scenarios

- Registration confirmation.
- Resend email confirmation.
- Password reset.
- Password reset completed.
- Email change confirmation.
- Old-email change notification.
- Account deleted notification, if generated during this pass.
- Admin email delivery failure alert, if generated during this pass.

## Checks For Each Scenario

- [ ] Sender display is `Darwin Lingua`.
- [ ] Sender address is `no-reply@darwinlingua.com`.
- [ ] Reply/support path is visible where relevant and uses `support@darwinlingua.com`.
- [ ] Subject is clear and not misleading.
- [ ] HTML rendering is readable in desktop webmail.
- [ ] HTML rendering is readable at phone width.
- [ ] Plain text alternative is readable if the mail client exposes it.
- [ ] Main action button/link is clear.
- [ ] Action links use `https://darwinlingua.com`.
- [ ] No link uses `www.darwinlingua.com` or the old temporary domain.
- [ ] No raw reset token, webhook secret, API key, provider message id, diagnostic hash, or internal stack trace is visible.
- [ ] Legal/support wording points users to the configured operator contact path.

## Safe Template Preview Index

$scenarioTable

## Evidence Notes

Record results in `MailboxRenderingEvidence.csv`. Keep screenshots local only if they are needed for debugging and redact action URLs/tokens before sharing.
"@

$csv = @"
Scenario,Culture,MailClient,Device,CheckedAtLocal,Result,Notes
Registration confirmation,en-US,,,,not-reviewed,
Registration confirmation,de-DE,,,,not-reviewed,
Resend email confirmation,en-US,,,,not-reviewed,
Password reset,en-US,,,,not-reviewed,
Password reset completed,en-US,,,,not-reviewed,
Email change confirmation,en-US,,,,not-reviewed,
Old-email change notification,en-US,,,,not-reviewed,
Account deleted notification,en-US,,,,not-reviewed,
Admin email delivery failure alert,en-US,,,,not-reviewed,
"@

$previewManifestSource = Join-Path $templatePreviewPath "manifest.md"
$previewManifestTarget = Join-Path $packetPath "TemplatePreviewManifest.md"
if (Test-Path -LiteralPath $previewManifestSource -PathType Leaf) {
    Copy-Item -LiteralPath $previewManifestSource -Destination $previewManifestTarget -Force
}
else {
    "# Transactional Email Template Preview`n`nNo source manifest found in $templatePreviewRelative." |
        Set-Content -LiteralPath $previewManifestTarget -Encoding UTF8
}

Set-Content -LiteralPath (Join-Path $packetPath "README.md") -Value $readme -Encoding UTF8
Set-Content -LiteralPath (Join-Path $packetPath "MailboxRenderingReview.md") -Value $review -Encoding UTF8
Set-Content -LiteralPath (Join-Path $packetPath "MailboxRenderingEvidence.csv") -Value $csv -Encoding UTF8

Write-Host "Mailbox rendering review packet: $packetPath"
Write-Host "Template preview source: $templatePreviewPath"
