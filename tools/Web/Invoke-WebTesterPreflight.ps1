[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$ApiBaseUrl = "https://api.darwinlingua.com",
    [string]$OutputDirectory = "artifacts/validation/web-tester-preflight",
    [int]$TimeoutSeconds = 20
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $PSCommandPath
$repoRoot = Split-Path -Parent (Split-Path -Parent $scriptRoot)
Set-Location $repoRoot

function Normalize-BaseUrl {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "Base URL must not be empty."
    }

    return $Value.Trim().TrimEnd("/")
}

function New-CheckResult {
    param(
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus,
        [string]$RequiredText = $null
    )

    $started = Get-Date
    $result = [ordered]@{
        name = $Name
        url = $Url
        expectedStatus = $ExpectedStatus
        statusCode = $null
        elapsedMs = $null
        contentLength = $null
        passed = $false
        error = $null
    }

    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec $TimeoutSeconds -MaximumRedirection 5
        $result.statusCode = [int]$response.StatusCode
        $result.contentLength = $response.Content.Length
        $result.elapsedMs = [int]((Get-Date) - $started).TotalMilliseconds

        if ($result.statusCode -ne $ExpectedStatus) {
            $result.error = "Expected HTTP $ExpectedStatus but received HTTP $($result.statusCode)."
            return [pscustomobject]$result
        }

        if (-not [string]::IsNullOrWhiteSpace($RequiredText) -and $response.Content -notlike "*$RequiredText*") {
            $result.error = "Required text was not found: $RequiredText"
            return [pscustomobject]$result
        }

        $result.passed = $true
        return [pscustomobject]$result
    }
    catch {
        $result.elapsedMs = [int]((Get-Date) - $started).TotalMilliseconds
        $result.error = $_.Exception.Message

        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $result.statusCode = [int]$_.Exception.Response.StatusCode
        }

        return [pscustomobject]$result
    }
}

function Add-Check {
    param(
        [System.Collections.Generic.List[object]]$Checks,
        [string]$Name,
        [string]$Url,
        [int]$ExpectedStatus = 200,
        [string]$RequiredText = $null
    )

    $Checks.Add((New-CheckResult -Name $Name -Url $Url -ExpectedStatus $ExpectedStatus -RequiredText $RequiredText))
}

$web = Normalize-BaseUrl -Value $WebBaseUrl
$api = Normalize-BaseUrl -Value $ApiBaseUrl

$checks = [System.Collections.Generic.List[object]]::new()

Add-Check -Checks $checks -Name "Web home" -Url $web -RequiredText "Darwin Lingua"
Add-Check -Checks $checks -Name "Browse index" -Url "$web/browse"
Add-Check -Checks $checks -Name "Browse CEFR A1" -Url "$web/browse/cefr/A1"
Add-Check -Checks $checks -Name "Browse topic everyday life" -Url "$web/browse/topic/everyday-life"
Add-Check -Checks $checks -Name "Search query Termin" -Url "$web/search?q=Termin" -RequiredText "Termin"
Add-Check -Checks $checks -Name "Word detail" -Url "$web/words/das-haus" -RequiredText "das Haus"
Add-Check -Checks $checks -Name "Favorites page" -Url "$web/favorites"
Add-Check -Checks $checks -Name "Settings page" -Url "$web/settings"
Add-Check -Checks $checks -Name "Courses index" -Url "$web/courses"
Add-Check -Checks $checks -Name "A1 lesson detail" -Url "$web/courses/a1-einstieg-in-den-alltag/a1-begruessung-und-name"
Add-Check -Checks $checks -Name "C2 lesson detail" -Url "$web/courses/c2-stil-souveraenitaet-und-komplexer-diskurs/c2-abschluss-und-meisterschaftspflege"
Add-Check -Checks $checks -Name "Exercises index" -Url "$web/exercises"
Add-Check -Checks $checks -Name "Exam prep index" -Url "$web/exam-prep"
Add-Check -Checks $checks -Name "Writing templates index" -Url "$web/writing-templates"
Add-Check -Checks $checks -Name "Life in Germany index" -Url "$web/life-in-germany"
Add-Check -Checks $checks -Name "Search page" -Url "$web/search"
Add-Check -Checks $checks -Name "Recent page" -Url "$web/recent"
Add-Check -Checks $checks -Name "Admin dashboard requires login" -Url "$web/admin" -RequiredText "ReturnUrl"
Add-Check -Checks $checks -Name "Admin reports require login" -Url "$web/admin/reports" -RequiredText "ReturnUrl"
Add-Check -Checks $checks -Name "Admin email diagnostics require login" -Url "$web/admin/email-diagnostics" -RequiredText "ReturnUrl"

Add-Check -Checks $checks -Name "API health" -Url "$api/health" -RequiredText "ok"
Add-Check -Checks $checks -Name "API course lesson Persian projection" -Url "$api/api/catalog/course-lessons/c2-abschluss-und-meisterschaftspflege?primaryMeaningLanguageCode=fa" -RequiredText "activityBlocks"
Add-Check -Checks $checks -Name "API search course" -Url "$api/api/catalog/search?q=Termin&resultType=course-lesson" -RequiredText "Termin"
Add-Check -Checks $checks -Name "API search writing template" -Url "$api/api/catalog/search?q=Abschlussstatement&resultType=writing-template" -RequiredText "writing-template"
Add-Check -Checks $checks -Name "API search life in Germany" -Url "$api/api/catalog/search?q=Demokratie&resultType=cultural-note" -RequiredText "cultural-note"

$failed = @($checks | Where-Object { -not $_.passed })
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Join-Path $repoRoot $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$reportPath = Join-Path $outputRoot "web-tester-preflight-$timestamp.json"

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    webBaseUrl = $web
    apiBaseUrl = $api
    timeoutSeconds = $TimeoutSeconds
    totalChecks = $checks.Count
    passedChecks = $checks.Count - $failed.Count
    failedChecks = $failed.Count
    checks = @($checks)
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $reportPath -Encoding UTF8

foreach ($check in $checks) {
    $status = if ($check.passed) { "PASS" } else { "FAIL" }
    Write-Host ("[{0}] {1} ({2} ms) {3}" -f $status, $check.name, $check.elapsedMs, $check.url)
    if (-not $check.passed) {
        Write-Host ("      {0}" -f $check.error) -ForegroundColor Red
    }
}

Write-Host "Report: $reportPath"

if ($failed.Count -gt 0) {
    exit 1
}
