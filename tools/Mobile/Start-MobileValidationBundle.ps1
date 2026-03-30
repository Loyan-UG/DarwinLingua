param(
    [string]$WorkspaceRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$BuildCommit = "",
    [string]$BundleLabel = ""
)

$ErrorActionPreference = "Stop"

function New-DirectoryIfMissing {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Copy-Worksheet {
    param(
        [string]$SourcePath,
        [string]$DestinationPath
    )

    Copy-Item -LiteralPath $SourcePath -Destination $DestinationPath -Force
}

$resolvedRoot = (Resolve-Path $WorkspaceRoot).Path
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

if ([string]::IsNullOrWhiteSpace($BundleLabel)) {
    $BundleLabel = "mobile-validation-$timestamp"
}

if ([string]::IsNullOrWhiteSpace($BuildCommit)) {
    $BuildCommit = (git -C $resolvedRoot rev-parse --short HEAD).Trim()
}

$bundleRoot = Join-Path $resolvedRoot "artifacts\mobile-validation\$BundleLabel"
New-DirectoryIfMissing -Path $bundleRoot

$phase1Worksheet = Join-Path $resolvedRoot "docs\44-Phase-1-Manual-Validation-Worksheet.md"
$phase2Worksheet = Join-Path $resolvedRoot "docs\46-Phase-2-Practice-Validation-Worksheet.md"
$phase3Worksheet = Join-Path $resolvedRoot "docs\47-Phase-3-Mobile-UX-Validation-Worksheet.md"
$phase5Worksheet = Join-Path $resolvedRoot "docs\50-Phase-5-Remote-Update-Validation-Worksheet.md"

Copy-Worksheet -SourcePath $phase1Worksheet -DestinationPath (Join-Path $bundleRoot "Phase1ManualValidationWorksheet.md")
Copy-Worksheet -SourcePath $phase2Worksheet -DestinationPath (Join-Path $bundleRoot "Phase2PracticeValidationWorksheet.md")
Copy-Worksheet -SourcePath $phase3Worksheet -DestinationPath (Join-Path $bundleRoot "Phase3MobileUxValidationWorksheet.md")
Copy-Worksheet -SourcePath $phase5Worksheet -DestinationPath (Join-Path $bundleRoot "Phase5RemoteUpdateValidationWorksheet.md")

$summaryPath = Join-Path $bundleRoot "MobileValidationSummary.md"
$summary = @"
# Mobile Validation Summary

- Bundle label: $BundleLabel
- Build commit: $BuildCommit
- Generated at: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz")
- Workspace root: $resolvedRoot

## Included Worksheets

- Phase 1: `Phase1ManualValidationWorksheet.md`
- Phase 2: `Phase2PracticeValidationWorksheet.md`
- Phase 3: `Phase3MobileUxValidationWorksheet.md`
- Phase 5: `Phase5RemoteUpdateValidationWorksheet.md`

## Execution Order

1. Run the Phase 1 worksheet on the target device matrix.
2. Run the Phase 2 Practice worksheet on the same build.
3. Run the Phase 3 Mobile UX worksheet on the same build.
4. Run the Phase 5 remote-update worksheet on the same build.
5. Record accepted findings, blockers, and follow-up bugs below.

## Results

- Phase 1 result:
- Phase 2 result:
- Phase 3 result:
- Phase 5 result:
- Final mobile readiness recommendation:
- Accepted known issues:
- Follow-up bugs:
"@

Set-Content -LiteralPath $summaryPath -Value $summary

Write-Host "Mobile validation bundle created:"
Write-Host $bundleRoot
