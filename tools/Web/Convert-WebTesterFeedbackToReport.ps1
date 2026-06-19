[CmdletBinding()]
param(
    [string]$FeedbackCsvPath = "docs\87-Web-Tester-Feedback-Template.csv",
    [string]$OutputDirectory = "artifacts/validation/web-tester-feedback",
    [switch]$FailOnBlocker,
    [switch]$FailOnMajor
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

$requiredColumns = @(
    "tester_code",
    "date",
    "device_browser",
    "page_url",
    "task",
    "severity",
    "language_settings",
    "what_happened",
    "expected_behavior",
    "screenshot_or_video",
    "follow_up_decision",
    "owner",
    "notes"
)

$severityOrder = @{
    "blocker" = 1
    "major" = 2
    "content-quality" = 3
    "minor" = 4
    "suggestion" = 5
}

$decisionOrder = @{
    "fix-now" = 1
    "needs-review" = 2
    "backlog" = 3
    "won't-fix" = 4
    "" = 5
}

function Normalize-Value {
    param([object]$Value)

    if ($null -eq $Value) {
        return ""
    }

    return ([string]$Value).Trim()
}

function Test-IsTemplateSample {
    param([pscustomobject]$Row)

    return (
        (Normalize-Value $Row.tester_code) -eq "T001" -and
        (Normalize-Value $Row.date) -eq "YYYY-MM-DD" -and
        (Normalize-Value $Row.what_happened) -like "Describe the concrete observation*"
    )
}

function Format-MarkdownCell {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    return ($Value -replace "\|", "\|" -replace "`r?`n", "<br>")
}

$feedbackPath = if ([System.IO.Path]::IsPathRooted($FeedbackCsvPath)) {
    $FeedbackCsvPath
}
else {
    Join-Path $repoRoot $FeedbackCsvPath
}
if (-not (Test-Path -LiteralPath $feedbackPath)) {
    throw "Feedback CSV not found: $feedbackPath"
}

$rows = @(Import-Csv -LiteralPath $feedbackPath)
$headers = @()
if ($rows.Count -gt 0) {
    $headers = @($rows[0].PSObject.Properties.Name)
}
else {
    $headers = (Get-Content -LiteralPath $feedbackPath -TotalCount 1).Split(",")
}

$missingColumns = @($requiredColumns | Where-Object { $_ -notin $headers })
if ($missingColumns.Count -gt 0) {
    throw "Feedback CSV is missing required columns: $($missingColumns -join ', ')"
}

$issues = [System.Collections.Generic.List[object]]::new()
$validationErrors = [System.Collections.Generic.List[string]]::new()
$rowNumber = 1

foreach ($row in $rows) {
    $rowNumber++

    if (Test-IsTemplateSample -Row $row) {
        continue
    }

    $severity = (Normalize-Value $row.severity).ToLowerInvariant()
    $decision = (Normalize-Value $row.follow_up_decision).ToLowerInvariant()

    foreach ($column in @("tester_code", "date", "device_browser", "page_url", "task", "severity", "language_settings", "what_happened", "expected_behavior")) {
        if ([string]::IsNullOrWhiteSpace((Normalize-Value $row.$column))) {
            $validationErrors.Add("Row $rowNumber is missing required value: $column")
        }
    }

    if (-not $severityOrder.ContainsKey($severity)) {
        $validationErrors.Add("Row $rowNumber has unsupported severity: $($row.severity)")
    }

    if (-not $decisionOrder.ContainsKey($decision)) {
        $validationErrors.Add("Row $rowNumber has unsupported follow_up_decision: $($row.follow_up_decision)")
    }

    $issues.Add([pscustomobject]@{
        rowNumber = $rowNumber
        testerCode = Normalize-Value $row.tester_code
        date = Normalize-Value $row.date
        deviceBrowser = Normalize-Value $row.device_browser
        pageUrl = Normalize-Value $row.page_url
        task = Normalize-Value $row.task
        severity = $severity
        languageSettings = Normalize-Value $row.language_settings
        whatHappened = Normalize-Value $row.what_happened
        expectedBehavior = Normalize-Value $row.expected_behavior
        screenshotOrVideo = Normalize-Value $row.screenshot_or_video
        followUpDecision = $decision
        owner = Normalize-Value $row.owner
        notes = Normalize-Value $row.notes
        priorityRank = if ($severityOrder.ContainsKey($severity)) { $severityOrder[$severity] } else { 99 }
        decisionRank = if ($decisionOrder.ContainsKey($decision)) { $decisionOrder[$decision] } else { 99 }
    })
}

$sortedIssues = @($issues | Sort-Object priorityRank, decisionRank, rowNumber)
$blockers = @($sortedIssues | Where-Object { $_.severity -eq "blocker" })
$majors = @($sortedIssues | Where-Object { $_.severity -eq "major" })
$fixNow = @($sortedIssues | Where-Object { $_.followUpDecision -eq "fix-now" -or $_.severity -in @("blocker", "major") })
$rtlIssues = @($sortedIssues | Where-Object { $_.languageSettings -match "(^|[,; ])(fa|ar|ckb|kmr)([,; ]|$)" })

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Join-Path $repoRoot $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "web-tester-feedback-triage-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-tester-feedback-triage-$timestamp.md"

$summary = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    feedbackCsvPath = $FeedbackCsvPath
    totalIssues = $sortedIssues.Count
    validationErrorCount = $validationErrors.Count
    blockerCount = $blockers.Count
    majorCount = $majors.Count
    contentQualityCount = @($sortedIssues | Where-Object { $_.severity -eq "content-quality" }).Count
    minorCount = @($sortedIssues | Where-Object { $_.severity -eq "minor" }).Count
    suggestionCount = @($sortedIssues | Where-Object { $_.severity -eq "suggestion" }).Count
    fixNowCount = $fixNow.Count
    rtlIssueCount = $rtlIssues.Count
    issues = @($sortedIssues)
    validationErrors = @($validationErrors)
}

$summary | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Tester Feedback Triage")
$lines.Add("")
$lines.Add(("- Generated: {0}" -f $summary.generatedAtUtc))
$lines.Add(("- Source CSV: {0}" -f $FeedbackCsvPath))
$lines.Add(("- Total issues: {0}" -f $summary.totalIssues))
$lines.Add(("- Validation errors: {0}" -f $summary.validationErrorCount))
$lines.Add(("- Blockers: {0}" -f $summary.blockerCount))
$lines.Add(("- Major: {0}" -f $summary.majorCount))
$lines.Add(("- Content quality: {0}" -f $summary.contentQualityCount))
$lines.Add(("- Minor: {0}" -f $summary.minorCount))
$lines.Add(("- Suggestions: {0}" -f $summary.suggestionCount))
$lines.Add(("- Fix-now candidates: {0}" -f $summary.fixNowCount))
$lines.Add(("- RTL-language related issues: {0}" -f $summary.rtlIssueCount))
$lines.Add("")

if ($validationErrors.Count -gt 0) {
    $lines.Add("## Validation Errors")
    $lines.Add("")
    foreach ($errorItem in $validationErrors) {
        $lines.Add("- $errorItem")
    }
    $lines.Add("")
}

$lines.Add("## Prioritized Issues")
$lines.Add("")
if ($sortedIssues.Count -eq 0) {
    $lines.Add("No tester feedback issues were found. If this is a real run, confirm that tester rows were added to the CSV.")
}
else {
    $lines.Add("| Priority | Severity | Decision | Page | Task | Language | Observation | Expected | Owner |")
    $lines.Add("| --- | --- | --- | --- | --- | --- | --- | --- | --- |")

    foreach ($issue in $sortedIssues) {
        $lines.Add("| $($issue.rowNumber) | $(Format-MarkdownCell $issue.severity) | $(Format-MarkdownCell $issue.followUpDecision) | $(Format-MarkdownCell $issue.pageUrl) | $(Format-MarkdownCell $issue.task) | $(Format-MarkdownCell $issue.languageSettings) | $(Format-MarkdownCell $issue.whatHappened) | $(Format-MarkdownCell $issue.expectedBehavior) | $(Format-MarkdownCell $issue.owner) |")
    }
}

$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Feedback rows: $($sortedIssues.Count)"
Write-Host "Validation errors: $($validationErrors.Count)"
Write-Host "Blockers: $($blockers.Count)"
Write-Host "Major: $($majors.Count)"
Write-Host "Fix-now candidates: $($fixNow.Count)"
Write-Host "JSON report: $jsonPath"
Write-Host "Markdown report: $markdownPath"

if ($validationErrors.Count -gt 0) {
    exit 1
}

if ($FailOnBlocker.IsPresent -and $blockers.Count -gt 0) {
    exit 2
}

if ($FailOnMajor.IsPresent -and ($blockers.Count + $majors.Count) -gt 0) {
    exit 3
}
