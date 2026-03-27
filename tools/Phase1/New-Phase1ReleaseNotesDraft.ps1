[CmdletBinding()]
param(
    [string]$ValidationRoot = "artifacts/release-validation",
    [string]$OutputPath = "",
    [string]$ReleaseLabel = "Phase 1 MVP Candidate",
    [string]$Validator = "TBD",
    [string]$SupportedDeviceMatrix = "TBD",
    [string]$ManualWorksheetResult = "Pending manual device validation.",
    [string]$OfflineValidationResult = "Pending manual device validation.",
    [string]$EnglishUiValidationResult = "Pending manual device validation.",
    [string]$GermanUiValidationResult = "Pending manual device validation.",
    [string]$TtsValidationResult = "Pending manual device validation.",
    [string]$PerformanceValidationResult = "Validated by automated starter-dataset coverage.",
    [string]$Recommendation = "Conditional: release after manual worksheet passes.",
    [string]$BlockingConcerns = "Manual offline/UI/TTS validation is still outstanding.",
    [string]$PostReleaseFollowUpActions = "Record accepted known issues and archive the signed worksheet."
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

$validationDirectory = Join-Path $repoRoot $ValidationRoot
$latestRun = Get-ChildItem -Path $validationDirectory -Directory | Sort-Object Name -Descending | Select-Object -First 1

if ($null -eq $latestRun)
{
    throw "No release-validation runs found under $validationDirectory"
}

$summaryPath = Join-Path $latestRun.FullName "Phase1ReleasePrepSummary.md"
if (-not (Test-Path $summaryPath))
{
    throw "Could not find Phase1ReleasePrepSummary.md under $($latestRun.FullName)"
}

$summaryContent = Get-Content -Path $summaryPath

function Get-MetadataValue([string]$label)
{
    $pattern = "- ${label}:*"
    $line = $summaryContent | Where-Object { $_ -like $pattern } | Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($line))
    {
        return "TBD"
    }

    return $line.Substring($label.Length + 4).Trim()
}

function Get-AutomatedGateSummary
{
    $tableLines = $summaryContent | Where-Object { $_ -like "| * | * | * | * |" }
    $resultLines = $tableLines | Where-Object { $_ -notlike "| --- *" -and $_ -notlike "| Step *" }

    if ($resultLines.Count -eq 0)
    {
        return "No automated-gate rows found."
    }

    $parts = foreach ($line in $resultLines)
    {
        $cells = $line.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries) | ForEach-Object { $_.Trim() }
        if ($cells.Count -ge 4)
        {
            "$($cells[0]): $($cells[1]) in $($cells[2])s"
        }
    }

    return ($parts -join "; ")
}

$buildCommit = Get-MetadataValue "Build commit"
$validationDate = Get-MetadataValue "Validation date"
$contentPackageVersion = Get-MetadataValue "Content package version"
$automatedGateSummary = Get-AutomatedGateSummary

if ([string]::IsNullOrWhiteSpace($OutputPath))
{
    $OutputPath = Join-Path $latestRun.FullName "Phase1ReleaseNotesDraft.md"
}
elseif (-not [System.IO.Path]::IsPathRooted($OutputPath))
{
    $OutputPath = Join-Path $repoRoot $OutputPath
}

$lines = @(
    "# Phase 1 Release Notes",
    "",
    "## Release Summary",
    "",
    "- Release label: $ReleaseLabel",
    "- Build commit: $buildCommit",
    "- Validation date: $validationDate",
    "- Validator / release owner: $Validator",
    "- Content package version: $contentPackageVersion",
    "- Supported device matrix: $SupportedDeviceMatrix",
    "",
    "---",
    "",
    "## Included Capabilities",
    "",
    "- local SQLite initialization and migration-based startup",
    "- English and German UI localization",
    "- CEFR browsing",
    "- topic browsing",
    "- German lemma search",
    "- word details",
    "- favorites",
    "- lightweight learning state",
    "- local sample content import",
    "- German word and example TTS with graceful failure handling",
    "",
    "---",
    "",
    "## Validation Evidence",
    "",
    "- Build and test gate result: $automatedGateSummary",
    "- Manual worksheet result: $ManualWorksheetResult",
    "- Offline validation result: $OfflineValidationResult",
    "- English UI validation result: $EnglishUiValidationResult",
    "- German UI validation result: $GermanUiValidationResult",
    "- TTS validation result: $TtsValidationResult",
    "- Performance validation result: $PerformanceValidationResult",
    "",
    "---",
    "",
    "## Known Issues Accepted For Release",
    "",
    "| Severity | Area | Description | Workaround | Follow-up |",
    "| --- | --- | --- | --- | --- |",
    "|  |  |  |  |  |",
    "",
    "---",
    "",
    "## Deferred Items",
    "",
    "- cloud sync",
    "- spaced repetition",
    "- quiz/review engine",
    "- grammar engine",
    "- admin editing workflows",
    "",
    "---",
    "",
    "## Release Recommendation",
    "",
    "- Recommendation: $Recommendation",
    "- Blocking concerns: $BlockingConcerns",
    "- Post-release follow-up actions: $PostReleaseFollowUpActions"
)

$lines | Set-Content -Path $OutputPath -Encoding utf8
Write-Host "Release-notes draft written to $OutputPath"
