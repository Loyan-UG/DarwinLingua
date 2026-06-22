[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/web-public-stack",
    [switch]$StopExisting,
    [switch]$SkipCloudflared,
    [switch]$SkipSmoke
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-RepositoryPath {
    param([string]$Path)

    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }

    return Join-Path (Get-Location) $Path
}

function Stop-DarwinLinguaProcess {
    $processes = Get-CimInstance Win32_Process |
        Where-Object {
            $_.CommandLine -like "*DarwinLingua.WebApi.csproj*" -or
            $_.CommandLine -like "*DarwinLingua.Web.csproj*" -or
            $_.Name -in @("DarwinLingua.Web.exe", "DarwinLingua.WebApi.exe")
        }

    foreach ($process in $processes) {
        Stop-Process -Id $process.ProcessId -Force -ErrorAction SilentlyContinue
    }
}

function Start-CloudflaredFromInstalledService {
    if (Get-Process cloudflared -ErrorAction SilentlyContinue) {
        return @($true, "already-running", "")
    }

    $service = Get-Service Cloudflared -ErrorAction SilentlyContinue
    if ($null -eq $service) {
        return @($false, "missing-service", "Cloudflared Windows service is not installed.")
    }

    try {
        Start-Service -Name Cloudflared -ErrorAction Stop
        Start-Sleep -Seconds 3
        if (Get-Process cloudflared -ErrorAction SilentlyContinue) {
            return @($true, "windows-service", "")
        }
    }
    catch {
        # Fall through to the user-process fallback. This keeps local public smoke possible
        # without requiring an elevated shell.
    }

    $serviceConfig = sc.exe qc Cloudflared
    $binaryLine = $serviceConfig |
        Where-Object { $_ -match "BINARY_PATH_NAME" } |
        Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($binaryLine)) {
        return @($false, "missing-service-command", "Cloudflared service command line was not found.")
    }

    $commandLine = ($binaryLine -split "BINARY_PATH_NAME\s*:", 2)[1].Trim()
    if ($commandLine -match '^"([^"]+)"\s+(.*)$') {
        $exe = $Matches[1]
        $arguments = $Matches[2]
    }
    else {
        $parts = $commandLine -split "\s+", 2
        $exe = $parts[0]
        $arguments = $parts[1]
    }

    if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
        return @($false, "missing-binary", "Cloudflared executable was not found.")
    }

    $cloudflaredOut = Resolve-RepositoryPath -Path "cloudflared-run.log"
    $cloudflaredErr = Resolve-RepositoryPath -Path "cloudflared-run.err.log"
    Remove-Item -LiteralPath $cloudflaredOut, $cloudflaredErr -ErrorAction SilentlyContinue
    Start-Process `
        -FilePath $exe `
        -ArgumentList $arguments `
        -WorkingDirectory (Get-Location) `
        -RedirectStandardOutput $cloudflaredOut `
        -RedirectStandardError $cloudflaredErr `
        -WindowStyle Hidden | Out-Null

    Start-Sleep -Seconds 4
    if (Get-Process cloudflared -ErrorAction SilentlyContinue) {
        return @($true, "user-process-fallback", "")
    }

    return @($false, "failed", "Cloudflared did not start.")
}

function Invoke-UrlSmoke {
    param([string[]]$Urls)

    $results = New-Object System.Collections.Generic.List[object]
    foreach ($url in $Urls) {
        $tmp = New-TemporaryFile
        try {
            $status = & curl.exe -k -L -s -o $tmp.FullName -w "%{http_code}" $url
            $length = (Get-Item -LiteralPath $tmp.FullName).Length
            $results.Add([ordered]@{
                url = $url
                statusCode = $status
                bytes = $length
                passed = $status -match '^2\d\d$|^3\d\d$'
            })
        }
        finally {
            Remove-Item -LiteralPath $tmp.FullName -Force -ErrorAction SilentlyContinue
        }
    }

    return $results
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $repoRoot

if ($StopExisting.IsPresent) {
    Stop-DarwinLinguaProcess
    Start-Sleep -Seconds 2
}

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$jsonPath = Join-Path $outputRoot "web-public-stack-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-public-stack-$timestamp.md"

Remove-Item -LiteralPath .\webapi-public-run.log,.\webapi-public-run.err.log,.\web-public-run.log,.\web-public-run.err.log -ErrorAction SilentlyContinue

$api = Start-Process `
    -FilePath dotnet `
    -ArgumentList @("run", "--project", "src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj", "--no-build", "--launch-profile", "DarwinLingua.WebApi") `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput (Join-Path $repoRoot "webapi-public-run.log") `
    -RedirectStandardError (Join-Path $repoRoot "webapi-public-run.err.log") `
    -WindowStyle Hidden `
    -PassThru

Start-Sleep -Seconds 4

$web = Start-Process `
    -FilePath dotnet `
    -ArgumentList @("run", "--project", "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj", "--no-build", "--launch-profile", "https") `
    -WorkingDirectory $repoRoot `
    -RedirectStandardOutput (Join-Path $repoRoot "web-public-run.log") `
    -RedirectStandardError (Join-Path $repoRoot "web-public-run.err.log") `
    -WindowStyle Hidden `
    -PassThru

$cloudflaredStarted = $false
$cloudflaredMode = "skipped"
$cloudflaredIssue = ""
if (-not $SkipCloudflared.IsPresent) {
    $cloudflaredResult = Start-CloudflaredFromInstalledService
    $cloudflaredStarted = [bool]$cloudflaredResult[0]
    $cloudflaredMode = [string]$cloudflaredResult[1]
    $cloudflaredIssue = [string]$cloudflaredResult[2]
}

Start-Sleep -Seconds 8

$urls = @(
    "http://localhost:5192",
    "http://localhost:5099/health",
    "http://localhost:53945/health",
    "https://darwinlingua.com",
    "https://darwinlingua.com/legal",
    "https://darwinlingua.com/privacy",
    "https://api.darwinlingua.com/health"
)

$smokeResults = if ($SkipSmoke.IsPresent) {
    @()
}
else {
    @(Invoke-UrlSmoke -Urls $urls)
}

$failed = @($smokeResults | Where-Object { -not $_.passed })
$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    webApiPid = $api.Id
    webPid = $web.Id
    cloudflaredStarted = $cloudflaredStarted
    cloudflaredMode = $cloudflaredMode
    cloudflaredIssue = $cloudflaredIssue
    smokeSkipped = $SkipSmoke.IsPresent
    smokePassed = (-not $SkipSmoke.IsPresent) -and $failed.Count -eq 0
    smokeResults = $smokeResults
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Web Public Dev Stack")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- WebApi PID: $($report.webApiPid)")
$lines.Add("- Web PID: $($report.webPid)")
$lines.Add("- Cloudflared mode: $($report.cloudflaredMode)")
$lines.Add("- Cloudflared issue: $($report.cloudflaredIssue)")
$lines.Add("- Smoke passed: $($report.smokePassed)")
$lines.Add("")
$lines.Add("| URL | Status | Bytes | Passed |")
$lines.Add("| --- | --- | ---: | --- |")
foreach ($result in $smokeResults) {
    $lines.Add("| $($result.url) | $($result.statusCode) | $($result.bytes) | $($result.passed) |")
}
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web public stack report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "WebApi PID: $($api.Id)"
Write-Host "Web PID: $($web.Id)"
Write-Host "Cloudflared mode: $cloudflaredMode"
Write-Host "Smoke passed: $($report.smokePassed)"

if (-not $SkipSmoke.IsPresent -and $failed.Count -gt 0) {
    exit 1
}
