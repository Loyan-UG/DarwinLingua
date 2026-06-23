[CmdletBinding()]
param(
    [string]$MailboxEvidence = "",
    [string]$OutputDirectory = "artifacts/validation/web-manual-evidence-audit",
    [switch]$FailOnIssue,
    [switch]$FailOnOpenMailboxRows
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

function Get-LatestMailboxEvidence {
    $packetRoot = Resolve-RepositoryPath -Path "artifacts/validation/web-mailbox-rendering-review-packet"
    if (-not (Test-Path -LiteralPath $packetRoot -PathType Container)) {
        return ""
    }

    $latestPacket = Get-ChildItem -LiteralPath $packetRoot -Directory -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($null -eq $latestPacket) {
        return ""
    }

    $candidate = Join-Path $latestPacket.FullName "MailboxRenderingEvidence.csv"
    if (Test-Path -LiteralPath $candidate -PathType Leaf) {
        return $candidate
    }

    return ""
}

function New-Check {
    param(
        [string]$Key,
        [string]$Status,
        [string]$Message
    )

    [ordered]@{
        key = $Key
        status = $Status
        message = $Message
    }
}

$mailboxEvidencePath = if ([string]::IsNullOrWhiteSpace($MailboxEvidence)) {
    Get-LatestMailboxEvidence
}
else {
    Resolve-RepositoryPath -Path $MailboxEvidence
}

if ([string]::IsNullOrWhiteSpace($mailboxEvidencePath) -or -not (Test-Path -LiteralPath $mailboxEvidencePath -PathType Leaf)) {
    throw "Mailbox evidence CSV was not found. Generate it with New-WebMailboxRenderingReviewPacket.ps1 or pass -MailboxEvidence."
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "web-manual-evidence-audit-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-manual-evidence-audit-$timestamp.md"

$raw = Get-Content -LiteralPath $mailboxEvidencePath -Raw
$rows = Import-Csv -LiteralPath $mailboxEvidencePath
$checks = [System.Collections.Generic.List[object]]::new()

$requiredColumns = @("Scenario", "Culture", "MailClient", "Device", "CheckedAtLocal", "Result", "Notes")
foreach ($column in $requiredColumns) {
    if (-not ($rows | Get-Member -Name $column -MemberType NoteProperty)) {
        $checks.Add((New-Check -Key "mailbox.column.$column" -Status "blocker" -Message "Mailbox evidence CSV is missing required column: $column"))
    }
}

$requiredScenarios = @(
    "Registration confirmation",
    "Resend email confirmation",
    "Password reset",
    "Password reset completed",
    "Email change confirmation",
    "Old-email change notification"
)

foreach ($scenario in $requiredScenarios) {
    if (-not ($rows | Where-Object { $_.Scenario -eq $scenario })) {
        $checks.Add((New-Check -Key "mailbox.requiredScenario" -Status "blocker" -Message "Mailbox evidence CSV is missing required scenario: $scenario"))
    }
}

$allowedResults = @("not-reviewed", "passed", "passed-with-notes", "failed", "not-in-scope-for-this-pass")
$openRows = @()
$failedRows = @()
foreach ($row in $rows) {
    $result = [string]$row.Result
    if ($result -notin $allowedResults) {
        $checks.Add((New-Check -Key "mailbox.result.invalid" -Status "blocker" -Message "Mailbox evidence row '$($row.Scenario)' has unsupported Result: $result"))
    }

    if ($result -eq "not-reviewed") {
        $openRows += "$($row.Scenario) [$($row.Culture)]"
    }

    if ($result -eq "failed") {
        $failedRows += "$($row.Scenario) [$($row.Culture)]"
    }

    if ($result -in @("passed", "passed-with-notes", "failed") -and [string]::IsNullOrWhiteSpace([string]$row.CheckedAtLocal)) {
        $checks.Add((New-Check -Key "mailbox.checkedAt.missing" -Status "blocker" -Message "Reviewed mailbox row '$($row.Scenario)' is missing CheckedAtLocal."))
    }
}

if ($openRows.Count -gt 0) {
    $checks.Add((New-Check -Key "mailbox.rows.open" -Status "open" -Message "Mailbox evidence still has not-reviewed rows: $($openRows -join '; ')"))
}

if ($failedRows.Count -gt 0) {
    $checks.Add((New-Check -Key "mailbox.rows.failed" -Status "blocker" -Message "Mailbox evidence has failed rows: $($failedRows -join '; ')"))
}

$forbiddenPatterns = @(
    @{ Pattern = ("xkey" + "sib-[A-Za-z0-9_-]+"); Label = "Brevo API key" },
    @{ Pattern = ("98959" + "d34"); Label = "Brevo webhook token prefix" },
    @{ Pattern = "token=|resetCode=|code=|userId="; Label = "raw action URL parameter" },
    @{ Pattern = "provider message id|providerMessageId"; Label = "provider message id" },
    @{ Pattern = "diagnostic hash|diagnosticHash"; Label = "diagnostic hash" },
    @{ Pattern = "www\.darwinlingua\.com"; Label = "unconfigured www host" },
    @{ Pattern = "lingua\.vafadar\.pro"; Label = "old temporary domain" }
)

foreach ($forbidden in $forbiddenPatterns) {
    if ([regex]::IsMatch($raw, $forbidden.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)) {
        $checks.Add((New-Check -Key "mailbox.secretOrUnsafeText" -Status "blocker" -Message "Mailbox evidence contains forbidden unsafe text: $($forbidden.Label)."))
    }
}

$blockerCount = @($checks | Where-Object { $_.status -eq "blocker" }).Count
$openCount = @($checks | Where-Object { $_.status -eq "open" }).Count

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    mailboxEvidence = Convert-ToRepositoryRelativePath -Path $mailboxEvidencePath
    rowCount = @($rows).Count
    blockerCount = $blockerCount
    openCount = $openCount
    passedForMailboxClose = ($blockerCount -eq 0 -and $openCount -eq 0)
    checks = @($checks.ToArray())
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Manual Evidence Audit")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Mailbox evidence: $($report.mailboxEvidence)")
$lines.Add("- Rows: $($report.rowCount)")
$lines.Add("- Blockers: $($report.blockerCount)")
$lines.Add("- Open rows: $($report.openCount)")
$lines.Add("- Passed for mailbox close: $($report.passedForMailboxClose)")
$lines.Add("")
$lines.Add("This audit only validates safe manual evidence structure. It does not mark the mailbox rendering gate as passed unless the human review evidence actually says it passed.")
$lines.Add("")
$lines.Add("| Status | Key | Message |")
$lines.Add("| --- | --- | --- |")
foreach ($check in $checks) {
    $message = ([string]$check["message"]).Replace("|", "\|")
    $checkStatus = [string]$check["status"]
    $checkKey = [string]$check["key"]
    $lines.Add("| $checkStatus | ``$checkKey`` | $message |")
}

if ($checks.Count -eq 0) {
    $lines.Add("| passed | `mailbox.evidence` | Mailbox evidence structure passed. |")
}

$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web manual evidence audit Markdown: $markdownPath"
Write-Host "Web manual evidence audit JSON: $jsonPath"
Write-Host "Blockers: $blockerCount"
Write-Host "Open rows: $openCount"
Write-Host "Passed for mailbox close: $($report.passedForMailboxClose)"

if ($FailOnIssue -and $blockerCount -gt 0) {
    throw "Manual evidence audit found $blockerCount blocker(s)."
}

if ($FailOnOpenMailboxRows -and $openCount -gt 0) {
    throw "Manual evidence audit found $openCount open mailbox row group(s)."
}
