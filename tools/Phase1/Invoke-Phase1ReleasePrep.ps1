[CmdletBinding()]
param(
    [string]$OutputRoot = "artifacts/release-validation",
    [string]$Configuration = "Debug",
    [string]$SolutionPath = "DarwinLingua.slnx",
    [string]$ContentPackageVersion = "TBD"
)

$ErrorActionPreference = "Stop"
$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$runDirectory = Join-Path $repoRoot $OutputRoot
$runDirectory = Join-Path $runDirectory "phase1-$timestamp"
New-Item -ItemType Directory -Path $runDirectory -Force | Out-Null

$summaryPath = Join-Path $runDirectory "Phase1ReleasePrepSummary.md"
$commandsPath = Join-Path $runDirectory "commands.txt"

$gitCommit = (git rev-parse HEAD).Trim()
$gitBranch = (git branch --show-current).Trim()
$validationDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss zzz"

$commandSpecs = @(
    @{ Name = "dotnet-info"; Command = "dotnet --info"; Log = "dotnet-info.log" },
    @{ Name = "restore"; Command = "dotnet restore $SolutionPath"; Log = "restore.log" },
    @{ Name = "build"; Command = "dotnet build $SolutionPath -c $Configuration"; Log = "build.log" },
    @{ Name = "test"; Command = "dotnet test $SolutionPath -c $Configuration --no-restore -m:1"; Log = "test.log" }
)

$commandSpecs | ForEach-Object { $_.Command } | Set-Content -Path $commandsPath

$results = @()

foreach ($spec in $commandSpecs)
{
    $logPath = Join-Path $runDirectory $spec.Log
    $startTime = Get-Date

    ">>> $($spec.Command)" | Set-Content -Path $logPath

    try
    {
        $output = Invoke-Expression $spec.Command 2>&1
        if ($null -ne $output)
        {
            $output | Out-File -FilePath $logPath -Append -Encoding utf8
        }

        $results += [pscustomobject]@{
            Name = $spec.Name
            Command = $spec.Command
            Status = "Passed"
            DurationSeconds = [math]::Round(((Get-Date) - $startTime).TotalSeconds, 2)
            Log = Split-Path -Leaf $logPath
        }
    }
    catch
    {
        if ($null -ne $_.Exception.Message)
        {
            $_.Exception.Message | Out-File -FilePath $logPath -Append -Encoding utf8
        }

        $results += [pscustomobject]@{
            Name = $spec.Name
            Command = $spec.Command
            Status = "Failed"
            DurationSeconds = [math]::Round(((Get-Date) - $startTime).TotalSeconds, 2)
            Log = Split-Path -Leaf $logPath
        }

        break
    }
}

$allPassed = ($results.Count -eq $commandSpecs.Count) -and (($results | Where-Object Status -ne "Passed").Count -eq 0)
$recommendation = if ($allPassed) { "Ready for manual device validation." } else { "Fix automated gate failures before manual validation." }

$summaryLines = @(
    "# Phase 1 Release Prep Summary",
    "",
    "## Run Metadata",
    "",
    "- Build commit: $gitCommit",
    "- Branch: $gitBranch",
    "- Validation date: $validationDate",
    "- Solution: $SolutionPath",
    "- Configuration: $Configuration",
    "- Content package version: $ContentPackageVersion",
    "",
    "## Automated Gate Results",
    "",
    "| Step | Status | Duration (s) | Log |",
    "| --- | --- | ---: | --- |"
)

foreach ($result in $results)
{
    $summaryLines += "| $($result.Name) | $($result.Status) | $($result.DurationSeconds) | $($result.Log) |"
}

$summaryLines += @(
    "",
    "## Manual Validation Reminder",
    "",
    "The following release-readiness checks still require execution on target devices and are not covered by this script:",
    "",
    "- offline behavior on target platforms",
    "- English UI validation",
    "- German UI validation",
    "- TTS validation on supported and unsupported voice/device setups",
    "",
    "Use `docs/44-Phase-1-Manual-Validation-Worksheet.md` to record the device-bound checks.",
    "After that, copy the accepted results into `docs/45-Phase-1-Release-Notes-Template.md`.",
    "",
    "## Release Recommendation",
    "",
    "- Automated-gate recommendation: $recommendation"
)

$summaryLines | Set-Content -Path $summaryPath -Encoding utf8

Write-Host "Release-prep summary written to $summaryPath"
exit ($(if ($allPassed) { 0 } else { 1 }))
