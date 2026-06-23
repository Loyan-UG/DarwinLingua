[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/web-external-action-packet",
    [switch]$GenerateFreshAudit,
    [switch]$RunBrevoWebhookCheck
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

function Get-LatestReport {
    param(
        [string]$RelativeDirectory,
        [string]$Filter = "*.json"
    )

    $directory = Resolve-RepositoryPath -Path $RelativeDirectory
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        return ""
    }

    $latest = Get-ChildItem -LiteralPath $directory -Filter $Filter -File -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latest) {
        return ""
    }

    return $latest.FullName
}

function Get-LatestDirectory {
    param([string]$RelativeDirectory)

    $directory = Resolve-RepositoryPath -Path $RelativeDirectory
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        return ""
    }

    $latest = Get-ChildItem -LiteralPath $directory -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latest) {
        return ""
    }

    return $latest.FullName
}

if ($GenerateFreshAudit) {
    & (Join-Path $scriptRoot "New-WebControlledTesterReadinessAudit.ps1") -FailOnAutomatedFailure | Out-Host
    & (Join-Path $scriptRoot "New-WebLegalSurfaceAudit.ps1") -FailOnIssue | Out-Host
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$markdownPath = Join-Path $outputRoot "web-external-action-packet-$timestamp.md"
$jsonPath = Join-Path $outputRoot "web-external-action-packet-$timestamp.json"

$readinessAuditPath = Get-LatestReport -RelativeDirectory "artifacts/validation/web-controlled-tester-readiness"
$legalAuditPath = Get-LatestReport -RelativeDirectory "artifacts/validation/web-legal-surface-audit"
$mailboxPacketPath = Get-LatestDirectory -RelativeDirectory "artifacts/validation/web-mailbox-rendering-review-packet"
$pwaReportPath = Get-LatestReport -RelativeDirectory "artifacts/validation/pwa-installability"
$testerBundlePath = Get-LatestDirectory -RelativeDirectory "artifacts/validation/web-tester-runs"
$manualReviewPath = Get-LatestReport -RelativeDirectory "artifacts/validation/web-manual-external-review"

$readiness = if (-not [string]::IsNullOrWhiteSpace($readinessAuditPath)) {
    Get-Content -LiteralPath $readinessAuditPath -Raw | ConvertFrom-Json
}
else {
    $null
}

$brevoStatus = "not-run"
$brevoAction = "Run .\tools\Web\Invoke-BrevoWebhookConfigurationCheck.ps1. If Brevo reports an unrecognised IP address, add that IP under Brevo Security -> Authorised IPs and rerun the check."
$brevoAuthorizedIp = ""
if ($RunBrevoWebhookCheck) {
    try {
        & (Join-Path $scriptRoot "Invoke-BrevoWebhookConfigurationCheck.ps1") | Out-Host
        $brevoStatus = "passed"
        $brevoAction = "No Brevo Authorized IP action is required by the current check."
    }
    catch {
        $message = $_.Exception.Message
        $brevoStatus = "needs-authorized-ip-or-review"
        $match = [regex]::Match($message, "unrecognised IP address\s+(?<ip>[0-9a-fA-F:\.]+)")
        if ($match.Success) {
            $brevoAuthorizedIp = $match.Groups["ip"].Value
            $brevoAction = "Open Brevo -> Security -> Authorised IPs, add the IP listed in this packet, save it, then rerun .\tools\Web\Invoke-BrevoWebhookConfigurationCheck.ps1."
        }
        else {
            $brevoAction = "Brevo webhook check failed. Review the generated terminal error without copying API keys or webhook tokens into Git."
        }
    }
}

$manualStatuses = if ($null -ne $readiness -and $readiness.manualStatuses) { $readiness.manualStatuses } else { $null }
$openHumanGates = if ($null -ne $readiness -and $readiness.humanStartOpenGates) {
    @($readiness.humanStartOpenGates | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) })
}
else {
    @()
}

$packet = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    publicWeb = "https://darwinlingua.com"
    publicApiHealth = "https://api.darwinlingua.com/health"
    requiredWwwHost = $false
    readinessAudit = Convert-ToRepositoryRelativePath -Path $readinessAuditPath
    legalSurfaceAudit = Convert-ToRepositoryRelativePath -Path $legalAuditPath
    mailboxPacket = Convert-ToRepositoryRelativePath -Path $mailboxPacketPath
    pwaEvidence = Convert-ToRepositoryRelativePath -Path $pwaReportPath
    testerBundle = Convert-ToRepositoryRelativePath -Path $testerBundlePath
    manualReviewReport = Convert-ToRepositoryRelativePath -Path $manualReviewPath
    automatedReady = if ($null -ne $readiness) { [bool]$readiness.automatedReady } else { $false }
    controlledTesterReadyToInvite = if ($null -ne $readiness) { [bool]$readiness.controlledTesterReadyToInvite } else { $false }
    openHumanStartGates = @($openHumanGates)
    brevoWebhookCheckStatus = $brevoStatus
    brevoAuthorizedIpToAdd = $brevoAuthorizedIp
    brevoAction = $brevoAction
}

$packet | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$mailboxEvidencePath = if (-not [string]::IsNullOrWhiteSpace($mailboxPacketPath)) {
    Convert-ToRepositoryRelativePath -Path (Join-Path $mailboxPacketPath "MailboxRenderingEvidence.csv")
}
else {
    ".\artifacts\validation\web-mailbox-rendering-review-packet\<packet>\MailboxRenderingEvidence.csv"
}

$mailboxStatus = if ($manualStatuses) { [string]$manualStatuses.mailboxReviewStatus } else { "unknown" }
$desktopStatus = if ($manualStatuses) { [string]$manualStatuses.pwaDesktopStatus } else { "unknown" }
$androidStatus = if ($manualStatuses) { [string]$manualStatuses.pwaAndroidStatus } else { "unknown" }
$testerStatus = if ($manualStatuses) { [string]$manualStatuses.testerPassStatus } else { "unknown" }
$brevoIpLine = if ([string]::IsNullOrWhiteSpace($brevoAuthorizedIp)) { "No IP captured in this packet." } else { $brevoAuthorizedIp }

$markdown = @"
# Web External Action Packet

- Generated: $($packet.generatedAtUtc)
- Web: https://darwinlingua.com
- API health: https://api.darwinlingua.com/health
- Required `www` host: false
- Automated ready: $($packet.automatedReady)
- Controlled tester ready to invite: $($packet.controlledTesterReadyToInvite)
- Readiness audit: $($packet.readinessAudit)
- Legal surface audit: $($packet.legalSurfaceAudit)

This packet lists only the actions that cannot be honestly completed by code alone. Do not paste Brevo API keys, webhook tokens, raw action URLs, reset tokens, provider message ids, diagnostic hashes, or full real email bodies into Git-tracked files.

## Current External Actions

| Action | Current status | What to do | Evidence |
| --- | --- | --- | --- |
| Brevo Authorized IP | $brevoStatus | $brevoAction | Rerun `.\tools\Web\Invoke-BrevoWebhookConfigurationCheck.ps1` |
| Mailbox rendering | $mailboxStatus | Review real emails in `info@darwinlingua.com`; record safe notes only. | `$mailboxEvidencePath` |
| PWA desktop install | $desktopStatus | Test install in desktop Chrome or Edge and installed-window navigation. | `docs/56-Web-Pwa-Install-Validation-Worksheet.md` |
| PWA Android install | $androidStatus | Test Android Chrome install flow, or explicitly mark not in scope for this tester pass. | `docs/56-Web-Pwa-Install-Validation-Worksheet.md` |
| Tester pass start | $testerStatus | Confirm tester bundle and mark `ready-to-invite` only after mailbox/PWA scope is reviewed. | `$($packet.testerBundle)` |

## Brevo Authorized IP

If the Brevo row says `needs-authorized-ip-or-review`, open:

```text
https://app.brevo.com/security/authorised_ips
```

Add this IP exactly, save, then rerun the webhook configuration check:

```text
$brevoIpLine
```

Do not store the Brevo API key or webhook token in this packet.

## Close-Out Command

After the manual rows are completed, generate the manual review report and run:

```powershell
.\tools\Web\New-WebControlledTesterReadinessAudit.ps1 -FailOnAutomatedFailure -FailOnOpenHumanGates
```

The controlled tester pass can start only when that command passes.
"@

Set-Content -LiteralPath $markdownPath -Value $markdown -Encoding UTF8

Write-Host "Web external action packet Markdown: $markdownPath"
Write-Host "Web external action packet JSON: $jsonPath"
Write-Host "Open human start gates: $($openHumanGates.Count)"
Write-Host "Brevo webhook check status: $brevoStatus"
