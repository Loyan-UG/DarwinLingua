[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/web-controlled-tester-readiness",
    [string]$ManualReviewReportPath = "",
    [string]$TesterBundle = "",
    [switch]$FailOnAutomatedFailure,
    [switch]$FailOnOpenHumanGates
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

function Get-LatestJsonReport {
    param(
        [string]$RelativeDirectory,
        [string]$Pattern = "*.json"
    )

    $directory = Resolve-RepositoryPath -Path $RelativeDirectory
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        return $null
    }

    return Get-ChildItem -LiteralPath $directory -Filter $Pattern -File |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}

function Read-JsonReport {
    param([System.IO.FileInfo]$File)

    if ($null -eq $File -or -not (Test-Path -LiteralPath $File.FullName -PathType Leaf)) {
        return $null
    }

    $content = Get-Content -LiteralPath $File.FullName -Raw
    if ([string]::IsNullOrWhiteSpace($content)) {
        return $null
    }

    return $content | ConvertFrom-Json
}

function Get-PropertyValue {
    param(
        [object]$Object,
        [string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function New-Gate {
    param(
        [string]$Key,
        [string]$Status,
        [string]$EvidencePath,
        [string]$Message
    )

    [ordered]@{
        key = $Key
        status = $Status
        evidencePath = $EvidencePath
        message = $Message
    }
}

function Format-MarkdownCell {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    return ($Value -replace "\|", "\/" -replace "`r?`n", "<br>")
}

function Test-ClosedManualStatus {
    param([string]$Status)

    return $Status -in @("passed", "passed-with-notes", "not-in-scope-for-this-pass")
}

function ConvertTo-StringArray {
    param([object]$Value)

    if ($null -eq $Value) {
        return @()
    }

    return @($Value) |
        Where-Object { $null -ne $_ -and -not [string]::IsNullOrWhiteSpace([string]$_) } |
        ForEach-Object { [string]$_ }
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "web-controlled-tester-readiness-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-controlled-tester-readiness-$timestamp.md"

$automatedGates = [System.Collections.Generic.List[object]]::new()

$webPublicStackFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-public-stack"
$webPublicStack = Read-JsonReport -File $webPublicStackFile
$automatedGates.Add((New-Gate `
    -Key "public-stack" `
    -Status $(if ($webPublicStack -and $webPublicStack.smokePassed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($webPublicStackFile) { $webPublicStackFile.FullName } else { "" }) `
    -Message "Public Web/API stack smoke must pass for darwinlingua.com and api.darwinlingua.com.")) | Out-Null

$operationsFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-operations-bootstrap"
$operations = Read-JsonReport -File $operationsFile
$automatedGates.Add((New-Gate `
    -Key "operations-bootstrap" `
    -Status $(if ($operations -and $operations.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($operationsFile) { $operationsFile.FullName } else { "" }) `
    -Message "Operations bootstrap must verify Web/API health, local secrets presence, required tables, and Brevo event columns.")) | Out-Null

$manualEvidenceFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-manual-evidence-audit"
$manualEvidence = Read-JsonReport -File $manualEvidenceFile
$manualEvidenceBlockerCount = if ($manualEvidence) { [int](Get-PropertyValue -Object $manualEvidence -Name "blockerCount") } else { -1 }
$automatedGates.Add((New-Gate `
    -Key "manual-evidence-safety" `
    -Status $(if ($manualEvidence -and $manualEvidenceBlockerCount -eq 0) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($manualEvidenceFile) { $manualEvidenceFile.FullName } else { "" }) `
    -Message "Manual mailbox evidence must be structurally safe and free of raw tokens, provider ids, diagnostic hashes, old domains, or www links.")) | Out-Null

$brevoReadinessFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/brevo-readiness"
$brevoReadiness = Read-JsonReport -File $brevoReadinessFile
$automatedGates.Add((New-Gate `
    -Key "brevo-readiness" `
    -Status $(if ($brevoReadiness -and $brevoReadiness.blockerCount -eq 0 -and $brevoReadiness.warningCount -eq 0) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($brevoReadinessFile) { $brevoReadinessFile.FullName } else { "" }) `
    -Message "Brevo readiness must have zero blockers and zero warnings.")) | Out-Null

$brevoWebhookConfigFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/brevo-webhook-configuration-check"
$brevoWebhookConfig = Read-JsonReport -File $brevoWebhookConfigFile
$automatedGates.Add((New-Gate `
    -Key "brevo-webhook-configuration" `
    -Status $(if ($brevoWebhookConfig -and $brevoWebhookConfig.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($brevoWebhookConfigFile) { $brevoWebhookConfigFile.FullName } else { "" }) `
    -Message "Provider-side Brevo webhook metadata must match URL, transactional type, bearer auth, and expected events.")) | Out-Null

$realDeliveryFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/brevo-real-delivery-smoke"
$realDelivery = Read-JsonReport -File $realDeliveryFile
$realDeliveryStatus = [string](Get-PropertyValue -Object $realDelivery -Name "status")
$automatedGates.Add((New-Gate `
    -Key "brevo-real-delivery" `
    -Status $(if ($realDelivery -and $realDeliveryStatus -eq "sent") { "pass" } else { "fail" }) `
    -EvidencePath $(if ($realDeliveryFile) { $realDeliveryFile.FullName } else { "" }) `
    -Message "Controlled real delivery smoke must reach Brevo with sent status.")) | Out-Null

$accountLinkFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-account-email-link-smoke"
$accountLink = Read-JsonReport -File $accountLinkFile
$automatedGates.Add((New-Gate `
    -Key "web-account-email-link-smoke" `
    -Status $(if ($accountLink -and $accountLink.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($accountLinkFile) { $accountLinkFile.FullName } else { "" }) `
    -Message "Registration, resend confirmation, password reset, email change, and old-email notification links must complete through real Brevo-delivered content.")) | Out-Null

$webhookSuppressionFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/brevo-webhook-suppression-smoke"
$webhookSuppression = Read-JsonReport -File $webhookSuppressionFile
$automatedGates.Add((New-Gate `
    -Key "brevo-webhook-suppression" `
    -Status $(if ($webhookSuppression -and $webhookSuppression.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($webhookSuppressionFile) { $webhookSuppressionFile.FullName } else { "" }) `
    -Message "Public Brevo webhook must accept Bearer-authenticated hardBounce and create internal suppression.")) | Out-Null

$suppressedSendFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/brevo-suppressed-send-smoke"
$suppressedSend = Read-JsonReport -File $suppressedSendFile
$automatedGates.Add((New-Gate `
    -Key "brevo-suppressed-send" `
    -Status $(if ($suppressedSend -and $suppressedSend.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($suppressedSendFile) { $suppressedSendFile.FullName } else { "" }) `
    -Message "Suppressed recipients must be logged as Suppressed without calling Brevo.")) | Out-Null

$providerLogFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/brevo-transactional-log-check"
$providerLog = Read-JsonReport -File $providerLogFile
$automatedGates.Add((New-Gate `
    -Key "brevo-provider-log-check" `
    -Status $(if ($providerLog -and $providerLog.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($providerLogFile) { $providerLogFile.FullName } else { "" }) `
    -Message "Brevo provider-side transactional logs must be reachable and matched for recent delivery records.")) | Out-Null

$adminDiagnosticsFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-email-diagnostics-admin-smoke"
$adminDiagnostics = Read-JsonReport -File $adminDiagnosticsFile
$automatedGates.Add((New-Gate `
    -Key "admin-email-diagnostics" `
    -Status $(if ($adminDiagnostics -and $adminDiagnostics.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($adminDiagnosticsFile) { $adminDiagnosticsFile.FullName } else { "" }) `
    -Message "Admin Email Diagnostics must show readiness, provider events, and suppression data.")) | Out-Null

$adminActionsFile = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-email-diagnostics-admin-actions-smoke"
$adminActions = Read-JsonReport -File $adminActionsFile
$automatedGates.Add((New-Gate `
    -Key "admin-email-diagnostics-actions" `
    -Status $(if ($adminActions -and $adminActions.passed -eq $true) { "pass" } else { "fail" }) `
    -EvidencePath $(if ($adminActionsFile) { $adminActionsFile.FullName } else { "" }) `
    -Message "Admin-only suppression reconciliation actions must work.")) | Out-Null

$resolvedTesterBundle = Resolve-RepositoryPath -Path $TesterBundle
if ([string]::IsNullOrWhiteSpace($resolvedTesterBundle)) {
    $testerRunsDirectory = Resolve-RepositoryPath -Path "artifacts/validation/web-tester-runs"
    $latestTesterBundle = if (Test-Path -LiteralPath $testerRunsDirectory -PathType Container) {
        Get-ChildItem -LiteralPath $testerRunsDirectory -Directory |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
    }
    else {
        $null
    }
    $resolvedTesterBundle = if ($latestTesterBundle) { $latestTesterBundle.FullName } else { "" }
}

$testerManifestPath = if ([string]::IsNullOrWhiteSpace($resolvedTesterBundle)) { "" } else { Join-Path $resolvedTesterBundle "manifest.json" }
$testerManifest = if (Test-Path -LiteralPath $testerManifestPath -PathType Leaf) { Get-Content -LiteralPath $testerManifestPath -Raw | ConvertFrom-Json } else { $null }
$automatedGates.Add((New-Gate `
    -Key "tester-bundle-preflight" `
    -Status $(if ($testerManifest -and $testerManifest.preflightStatus -eq "passed") { "pass" } else { "fail" }) `
    -EvidencePath $testerManifestPath `
    -Message "Latest tester bundle must include a passed public preflight.")) | Out-Null

$resolvedManualReviewPath = Resolve-RepositoryPath -Path $ManualReviewReportPath
if ([string]::IsNullOrWhiteSpace($resolvedManualReviewPath)) {
    $latestManualReview = Get-LatestJsonReport -RelativeDirectory "artifacts/validation/web-manual-external-review"
    $resolvedManualReviewPath = if ($latestManualReview) { $latestManualReview.FullName } else { "" }
}

$manualReview = if (Test-Path -LiteralPath $resolvedManualReviewPath -PathType Leaf) {
    Get-Content -LiteralPath $resolvedManualReviewPath -Raw | ConvertFrom-Json
}
else {
    $null
}

$mailboxStatus = [string](Get-PropertyValue -Object $manualReview -Name "mailboxReviewStatus")
$pwaDesktopStatus = [string](Get-PropertyValue -Object $manualReview -Name "pwaDesktopStatus")
$pwaAndroidStatus = [string](Get-PropertyValue -Object $manualReview -Name "pwaAndroidStatus")
$testerPassStatus = [string](Get-PropertyValue -Object $manualReview -Name "testerPassStatus")
[string[]]$openHumanGates = if ($manualReview) { ConvertTo-StringArray -Value $manualReview.openGates } else { @("manual-review-report-missing") }
[string[]]$failedHumanGates = if ($manualReview) { ConvertTo-StringArray -Value $manualReview.failedGates } else { @("manual-review-report-missing") }

$automatedFailures = @($automatedGates | Where-Object { $_.status -ne "pass" })
$humanStartOpenGates = [System.Collections.Generic.List[string]]::new()
if (-not (Test-ClosedManualStatus -Status $mailboxStatus)) { $humanStartOpenGates.Add("mailbox-rendering") | Out-Null }
if (-not (Test-ClosedManualStatus -Status $pwaDesktopStatus)) { $humanStartOpenGates.Add("pwa-desktop-install") | Out-Null }
if (-not (Test-ClosedManualStatus -Status $pwaAndroidStatus)) { $humanStartOpenGates.Add("pwa-android-install") | Out-Null }
if ($testerPassStatus -ne "ready-to-invite" -and $testerPassStatus -ne "in-progress" -and $testerPassStatus -ne "closed") {
    $humanStartOpenGates.Add("tester-pass-start-status") | Out-Null
}

$automatedFailureCount = @($automatedFailures).Count
$humanStartOpenGateCount = @($humanStartOpenGates).Count
$failedHumanGateCount = @($failedHumanGates).Count
$automatedReady = $automatedFailureCount -eq 0
$humanStartReady = $humanStartOpenGateCount -eq 0 -and $failedHumanGateCount -eq 0
$controlledTesterReadyToInvite = $automatedReady -and $humanStartReady

$report = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow
    publicWeb = "https://darwinlingua.com"
    publicApi = "https://api.darwinlingua.com/health"
    requiredWwwHost = $false
    testerBundle = $resolvedTesterBundle
    manualReviewReport = $resolvedManualReviewPath
    automatedReady = $automatedReady
    automatedFailureCount = $automatedFailureCount
    humanStartReady = $humanStartReady
    humanStartOpenGateCount = $humanStartOpenGateCount
    controlledTesterReadyToInvite = $controlledTesterReadyToInvite
    automatedGates = $automatedGates
    openHumanGates = @($openHumanGates)
    failedHumanGates = @($failedHumanGates)
    humanStartOpenGates = @($humanStartOpenGates)
    manualStatuses = [ordered]@{
        mailboxReviewStatus = $mailboxStatus
        pwaDesktopStatus = $pwaDesktopStatus
        pwaAndroidStatus = $pwaAndroidStatus
        testerPassStatus = $testerPassStatus
    }
}

$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Controlled Tester Readiness Audit")
$lines.Add("")
$lines.Add(("- Generated: {0}" -f $report.generatedAtUtc))
$lines.Add("- Public Web: https://darwinlingua.com")
$lines.Add("- Public API: https://api.darwinlingua.com/health")
$lines.Add("- Required www host: false")
$lines.Add(("- Automated ready: {0}" -f $automatedReady))
$lines.Add(("- Human start ready: {0}" -f $humanStartReady))
$lines.Add(("- Controlled tester ready to invite: {0}" -f $controlledTesterReadyToInvite))
$lines.Add("")
$lines.Add("## Automated Gates")
$lines.Add("")
$lines.Add("| Status | Gate | Evidence | Message |")
$lines.Add("| --- | --- | --- | --- |")
foreach ($gate in $automatedGates) {
    $lines.Add(("| {0} | {1} | {2} | {3} |" -f $gate.status, $gate.key, (Format-MarkdownCell $gate.evidencePath), (Format-MarkdownCell $gate.message)))
}
$lines.Add("")
$lines.Add("## Human Gates")
$lines.Add("")
$lines.Add(("- Manual review report: {0}" -f $(if ([string]::IsNullOrWhiteSpace($resolvedManualReviewPath)) { "not found" } else { $resolvedManualReviewPath })))
$lines.Add(("- Mailbox review: {0}" -f $mailboxStatus))
$lines.Add(("- PWA desktop: {0}" -f $pwaDesktopStatus))
$lines.Add(("- PWA Android: {0}" -f $pwaAndroidStatus))
$lines.Add(("- Tester pass: {0}" -f $testerPassStatus))
$lines.Add("")
if ($humanStartOpenGateCount -gt 0) {
    $lines.Add("### Open Human Start Gates")
    $lines.Add("")
    foreach ($gate in $humanStartOpenGates) {
        $lines.Add("- $gate")
    }
    $lines.Add("")
}
if ($failedHumanGateCount -gt 0) {
    $lines.Add("### Failed Human Gates")
    $lines.Add("")
    foreach ($gate in $failedHumanGates) {
        $lines.Add("- $gate")
    }
    $lines.Add("")
}
$lines.Add("This audit intentionally separates automated readiness from human start gates. A passing automated section does not approve broad public launch or replace mailbox rendering review, target-browser PWA install checks, tester feedback, or final legal/operator review.")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web controlled tester readiness JSON report: $jsonPath"
Write-Host "Web controlled tester readiness Markdown report: $markdownPath"
Write-Host "Automated ready: $automatedReady"
Write-Host "Human start ready: $humanStartReady"
Write-Host "Controlled tester ready to invite: $controlledTesterReadyToInvite"
Write-Host "Automated failures: $automatedFailureCount"
Write-Host "Human start open gates: $humanStartOpenGateCount"

if ($FailOnAutomatedFailure.IsPresent -and -not $automatedReady) {
    exit 2
}

if ($FailOnOpenHumanGates.IsPresent -and -not $controlledTesterReadyToInvite) {
    exit 3
}
