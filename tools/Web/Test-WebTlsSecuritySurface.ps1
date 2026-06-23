[CmdletBinding()]
param(
    [string]$OutputDirectory = "artifacts/validation/web-tls-security-surface",
    [switch]$FailOnIssue
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

function Invoke-CurlProbe {
    param(
        [ValidateSet("GET", "HEAD")]
        [string]$Method,
        [string]$Url,
        [switch]$NoFollowRedirects
    )

    $bodyPath = New-TemporaryFile
    $headersPath = New-TemporaryFile
    try {
        if ($Method -eq "HEAD") {
            $headerArgs = @("-I", "-s")
            $writeArgs = @("-I", "-s", "-o", $bodyPath.FullName, "-w", "%{http_code}|%{ssl_verify_result}|%{url_effective}|%{redirect_url}")
            if (-not $NoFollowRedirects.IsPresent) {
                $headerArgs += "-L"
                $writeArgs += "-L"
            }

            $headerArgs += $Url
            $writeArgs += $Url
            $headerText = (& curl.exe @headerArgs) -join "`n"
            $writeOut = & curl.exe @writeArgs
        }
        else {
            $writeArgs = @("-s", "-D", $headersPath.FullName, "-o", $bodyPath.FullName, "-w", "%{http_code}|%{ssl_verify_result}|%{url_effective}|%{redirect_url}")
            if (-not $NoFollowRedirects.IsPresent) {
                $writeArgs += "-L"
            }

            $writeArgs += $Url
            $writeOut = & curl.exe @writeArgs
            $headerText = Get-Content -LiteralPath $headersPath.FullName -Raw
        }

        $parts = ([string]$writeOut).Split("|")
        [ordered]@{
            method = $Method
            url = $Url
            statusCode = [int]$parts[0]
            sslVerifyResult = [int]$parts[1]
            effectiveUrl = $parts[2]
            redirectUrl = $parts[3]
            followsRedirects = -not $NoFollowRedirects.IsPresent
            bodyBytes = if (Test-Path -LiteralPath $bodyPath.FullName -PathType Leaf) { (Get-Item -LiteralPath $bodyPath.FullName).Length } else { 0 }
            headers = $headerText
        }
    }
    finally {
        Remove-Item -LiteralPath $bodyPath.FullName -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $headersPath.FullName -Force -ErrorAction SilentlyContinue
    }
}

function Get-CertificateSummary {
    param([string]$HostName)

    $tcp = [Net.Sockets.TcpClient]::new($HostName, 443)
    $ssl = [Net.Security.SslStream]::new($tcp.GetStream(), $false)
    try {
        $ssl.AuthenticateAsClient($HostName)
        $certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new($ssl.RemoteCertificate)
        $subjectAlternativeName = ($certificate.Extensions |
            Where-Object { $_.Oid.FriendlyName -eq "Subject Alternative Name" } |
            ForEach-Object { $_.Format($false) }) -join "; "

        [ordered]@{
            host = $HostName
            subject = $certificate.Subject
            issuer = $certificate.Issuer
            notBeforeUtc = $certificate.NotBefore.ToUniversalTime().ToString("O")
            notAfterUtc = $certificate.NotAfter.ToUniversalTime().ToString("O")
            thumbprint = $certificate.Thumbprint
            subjectAlternativeName = $subjectAlternativeName
            validNow = $certificate.NotBefore -le (Get-Date) -and $certificate.NotAfter -gt (Get-Date)
            handshakeSucceeded = $true
        }
    }
    finally {
        $ssl.Dispose()
        $tcp.Dispose()
    }
}

function Test-Header {
    param(
        [string]$Headers,
        [string]$Name
    )

    return $Headers -match ("(?im)^" + [regex]::Escape($Name) + "\s*:")
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "web-tls-security-surface-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-tls-security-surface-$timestamp.md"

$probes = @(
    (Invoke-CurlProbe -Method GET -Url "https://darwinlingua.com")
    (Invoke-CurlProbe -Method HEAD -Url "https://darwinlingua.com")
    (Invoke-CurlProbe -Method HEAD -Url "https://www.darwinlingua.com" -NoFollowRedirects)
    (Invoke-CurlProbe -Method GET -Url "https://api.darwinlingua.com/health")
    (Invoke-CurlProbe -Method HEAD -Url "https://api.darwinlingua.com/health")
)

$certificates = @(
    (Get-CertificateSummary -HostName "darwinlingua.com")
    (Get-CertificateSummary -HostName "api.darwinlingua.com")
)

$issues = [System.Collections.Generic.List[object]]::new()

foreach ($probe in $probes) {
    if ($probe.sslVerifyResult -ne 0) {
        $issues.Add([ordered]@{ area = "tls"; target = "$($probe.method) $($probe.url)"; message = "curl SSL verification failed with result $($probe.sslVerifyResult)." }) | Out-Null
    }

    if ($probe.statusCode -lt 200 -or $probe.statusCode -ge 400) {
        $issues.Add([ordered]@{ area = "http"; target = "$($probe.method) $($probe.url)"; message = "Expected HTTP 2xx/3xx, got $($probe.statusCode)." }) | Out-Null
    }
}

foreach ($certificate in $certificates) {
    if (-not $certificate.handshakeSucceeded -or -not $certificate.validNow) {
        $issues.Add([ordered]@{ area = "certificate"; target = $certificate.host; message = "Certificate handshake or validity window check failed." }) | Out-Null
    }
}

$webGet = $probes | Where-Object { $_.method -eq "GET" -and $_.url -eq "https://darwinlingua.com" } | Select-Object -First 1
$wwwHead = $probes | Where-Object { $_.method -eq "HEAD" -and $_.url -eq "https://www.darwinlingua.com" } | Select-Object -First 1
$apiHead = $probes | Where-Object { $_.method -eq "HEAD" -and $_.url -eq "https://api.darwinlingua.com/health" } | Select-Object -First 1

if ($wwwHead.statusCode -notin @(301, 308)) {
    $issues.Add([ordered]@{ area = "canonical-host"; target = "https://www.darwinlingua.com"; message = "Expected permanent redirect to https://darwinlingua.com, got $($wwwHead.statusCode)." }) | Out-Null
}

if (-not $wwwHead.redirectUrl.StartsWith("https://darwinlingua.com", [StringComparison]::OrdinalIgnoreCase)) {
    $issues.Add([ordered]@{ area = "canonical-host"; target = "https://www.darwinlingua.com"; message = "Expected redirect target to start with https://darwinlingua.com, got '$($wwwHead.redirectUrl)'." }) | Out-Null
}

$webRequiredHeaders = @(
    "Strict-Transport-Security",
    "Content-Security-Policy",
    "X-Content-Type-Options",
    "X-Frame-Options",
    "Referrer-Policy",
    "Permissions-Policy"
)

$apiRequiredHeaders = @(
    "Strict-Transport-Security",
    "X-Content-Type-Options",
    "X-Frame-Options",
    "Referrer-Policy",
    "Permissions-Policy"
)

foreach ($header in $webRequiredHeaders) {
    if (-not (Test-Header -Headers $webGet.headers -Name $header)) {
        $issues.Add([ordered]@{ area = "header"; target = "https://darwinlingua.com"; message = "Missing $header." }) | Out-Null
    }
}

foreach ($header in $apiRequiredHeaders) {
    if (-not (Test-Header -Headers $apiHead.headers -Name $header)) {
        $issues.Add([ordered]@{ area = "header"; target = "https://api.darwinlingua.com/health"; message = "Missing $header." }) | Out-Null
    }
}

$passed = $issues.Count -eq 0
$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    publicWeb = "https://darwinlingua.com"
    publicApiHealth = "https://api.darwinlingua.com/health"
    requiredWwwHost = $false
    wwwCanonicalRedirect = "https://darwinlingua.com"
    passed = $passed
    issueCount = $issues.Count
    probes = $probes | ForEach-Object {
        [ordered]@{
            method = $_.method
            url = $_.url
            statusCode = $_.statusCode
            sslVerifyResult = $_.sslVerifyResult
            effectiveUrl = $_.effectiveUrl
            redirectUrl = $_.redirectUrl
            followsRedirects = $_.followsRedirects
            bodyBytes = $_.bodyBytes
        }
    }
    certificates = $certificates
    issues = @($issues)
}

$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web TLS And Security Header Surface")
$lines.Add("")
$lines.Add(("- Generated: {0}" -f $report.generatedAtUtc))
$lines.Add(("- Passed: {0}" -f $passed))
$lines.Add(("- Issues: {0}" -f $issues.Count))
$lines.Add("- Required www host: false")
$lines.Add("- Canonical www redirect: https://darwinlingua.com")
$lines.Add("")
$lines.Add("## HTTP/TLS Probes")
$lines.Add("")
$lines.Add("| Method | URL | Status | SSL verify | Effective URL |")
$lines.Add("| --- | --- | --- | --- | --- |")
foreach ($probe in $report.probes) {
    $lines.Add(("| {0} | {1} | {2} | {3} | {4} |" -f $probe.method, $probe.url, $probe.statusCode, $probe.sslVerifyResult, $probe.effectiveUrl))
}
$lines.Add("")
$lines.Add("## Certificates")
$lines.Add("")
$lines.Add("| Host | Subject | Issuer | Valid until UTC |")
$lines.Add("| --- | --- | --- | --- |")
foreach ($certificate in $certificates) {
    $lines.Add(("| {0} | {1} | {2} | {3} |" -f $certificate.host, ($certificate.subject -replace "\|", "/"), ($certificate.issuer -replace "\|", "/"), $certificate.notAfterUtc))
}
if ($issues.Count -gt 0) {
    $lines.Add("")
    $lines.Add("## Issues")
    $lines.Add("")
    foreach ($issue in $issues) {
        $lines.Add(("- {0}: {1} - {2}" -f $issue.area, $issue.target, $issue.message))
    }
}
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web TLS/security surface Markdown: $markdownPath"
Write-Host "Web TLS/security surface JSON: $jsonPath"
Write-Host "Passed: $passed"
Write-Host "Issues: $($issues.Count)"

if ($FailOnIssue.IsPresent -and -not $passed) {
    exit 2
}
