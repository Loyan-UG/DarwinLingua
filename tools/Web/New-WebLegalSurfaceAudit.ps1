[CmdletBinding()]
param(
    [string]$BaseUrl = "https://darwinlingua.com",
    [string]$OutputDirectory = "artifacts/validation/web-legal-surface-audit",
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

function New-Check {
    param(
        [string]$Key,
        [string]$Status,
        [string]$Message,
        [string]$Path = ""
    )

    [ordered]@{
        key = $Key
        status = $Status
        message = $Message
        path = $Path
    }
}

function Get-PublicPage {
    param([string]$Url)

    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -MaximumRedirection 5 -TimeoutSec 30
        return [ordered]@{
            ok = $true
            statusCode = [int]$response.StatusCode
            content = [string]$response.Content
            error = ""
        }
    }
    catch {
        $statusCode = 0
        if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
            $statusCode = [int]$_.Exception.Response.StatusCode
        }

        return [ordered]@{
            ok = $false
            statusCode = $statusCode
            content = ""
            error = $_.Exception.Message
        }
    }
}

function Decode-CloudflareEmail {
    param([string]$Encoded)

    if ([string]::IsNullOrWhiteSpace($Encoded) -or $Encoded.Length -lt 4 -or ($Encoded.Length % 2) -ne 0) {
        return ""
    }

    try {
        $key = [Convert]::ToInt32($Encoded.Substring(0, 2), 16)
        $characters = New-Object System.Collections.Generic.List[char]
        for ($index = 2; $index -lt $Encoded.Length; $index += 2) {
            $value = [Convert]::ToInt32($Encoded.Substring($index, 2), 16) -bxor $key
            $characters.Add([char]$value)
        }

        return -join $characters
    }
    catch {
        return ""
    }
}

function Get-SearchablePageContent {
    param([string]$Content)

    $decodedEmails = New-Object System.Collections.Generic.List[string]
    foreach ($match in [regex]::Matches($Content, 'data-cfemail="(?<value>[0-9a-fA-F]+)"')) {
        $decoded = Decode-CloudflareEmail -Encoded $match.Groups["value"].Value
        if (-not [string]::IsNullOrWhiteSpace($decoded)) {
            $decodedEmails.Add($decoded)
        }
    }

    if ($decodedEmails.Count -eq 0) {
        return $Content
    }

    return $Content + "`n" + ($decodedEmails.ToArray() -join "`n")
}

$normalizedBaseUrl = $BaseUrl.TrimEnd("/")
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$markdownPath = Join-Path $outputRoot "web-legal-surface-audit-$timestamp.md"
$jsonPath = Join-Path $outputRoot "web-legal-surface-audit-$timestamp.json"

$requiredPages = @(
    @{
        path = "/legal"
        label = "Legal Notice"
        requiredText = @("Shahram Vafadar", "37154 Northeim", "info@darwinlingua.com", "Darwin Lingua")
    },
    @{
        path = "/impressum"
        label = "Impressum Alias"
        requiredText = @("Shahram Vafadar", "37154 Northeim", "info@darwinlingua.com")
    },
    @{
        path = "/privacy"
        label = "Privacy Policy"
        requiredText = @("Privacy", "account", "delete", "export", "Landesbeauftragte fuer den Datenschutz Niedersachsen", "info@darwinlingua.com")
    },
    @{
        path = "/terms"
        label = "Terms"
        requiredText = @("Terms", "educational", "not legal, medical, immigration, or financial advice", "illegal", "security-abuse", "Darwin Lingua")
    },
    @{
        path = "/cookies"
        label = "Cookie / Storage Notice"
        requiredText = @("Cookie", "Storage", "no cookie banner", "marketing cookies", "third-party analytics")
    },
    @{
        path = "/contact"
        label = "Contact"
        requiredText = @("Contact", "info@darwinlingua.com", "support@darwinlingua.com", "Security, abuse, and illegal-content reports")
    }
)

$forbiddenPatterns = @(
    @{ pattern = "Not configured"; reason = "missing operator configuration warning" },
    @{ pattern = "TODO"; reason = "TODO marker" },
    @{ pattern = "TBD"; reason = "TBD marker" },
    @{ pattern = "lingua.vafadar.pro"; reason = "old temporary domain" },
    @{ pattern = "www.darwinlingua.com"; reason = "unconfigured www host" },
    @{ pattern = ("xkeysib" + "-"); reason = "Brevo API key prefix" },
    @{ pattern = "BrevoWebhookSecret"; reason = "webhook secret configuration key" },
    @{ pattern = "BrevoApiKey"; reason = "Brevo API key configuration key" }
)

$checks = New-Object System.Collections.Generic.List[object]
$pageReports = New-Object System.Collections.Generic.List[object]

foreach ($page in $requiredPages) {
    $url = "$normalizedBaseUrl$($page.path)"
    $result = Get-PublicPage -Url $url
    $pageReports.Add([ordered]@{
        path = $page.path
        label = $page.label
        url = $url
        statusCode = $result.statusCode
        bytes = $result.content.Length
        error = $result.error
    })

    if (-not $result.ok -or $result.statusCode -lt 200 -or $result.statusCode -ge 300) {
        $checks.Add((New-Check -Key "page.status$($page.path)" -Status "blocker" -Path $page.path -Message "$($page.label) did not return HTTP 2xx. Status: $($result.statusCode). $($result.error)"))
        continue
    }

    $checks.Add((New-Check -Key "page.status$($page.path)" -Status "passed" -Path $page.path -Message "$($page.label) returned HTTP $($result.statusCode)."))
    $searchableContent = Get-SearchablePageContent -Content $result.content

    foreach ($requiredText in $page.requiredText) {
        if ($searchableContent.IndexOf($requiredText, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            $checks.Add((New-Check -Key "page.requiredText$($page.path)" -Status "blocker" -Path $page.path -Message "$($page.label) is missing required text: $requiredText"))
        }
    }

    foreach ($forbidden in $forbiddenPatterns) {
        if ($searchableContent.IndexOf($forbidden.pattern, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $checks.Add((New-Check -Key "page.forbiddenText$($page.path)" -Status "blocker" -Path $page.path -Message "$($page.label) contains $($forbidden.reason)."))
        }
    }
}

$blockers = @($checks | Where-Object { $_.status -eq "blocker" })
$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    baseUrl = $normalizedBaseUrl
    requiredWwwHost = $false
    pages = @($pageReports.ToArray())
    checks = @($checks.ToArray())
    blockerCount = $blockers.Count
    passed = ($blockers.Count -eq 0)
    disclaimer = "Engineering surface audit only. This does not replace operator/legal review before broad public launch."
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Web Legal Surface Audit")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Base URL: $($report.baseUrl)")
$lines.Add("- Required `www` host: false")
$lines.Add("- Passed: $($report.passed)")
$lines.Add("- Blockers: $($report.blockerCount)")
$lines.Add("")
$lines.Add("This is an engineering surface audit. It verifies that the public legal/support pages render with the expected configured operator and contact data, without placeholders, old domains, `www`, or obvious secret leaks. It does not replace final operator/legal review before broad public launch.")
$lines.Add("")
$lines.Add("## Pages")
$lines.Add("")
$lines.Add("| Page | Status | Bytes |")
$lines.Add("| --- | ---: | ---: |")
foreach ($pageReport in $pageReports) {
    $pagePath = [string]$pageReport["path"]
    $pageStatusCode = [string]$pageReport["statusCode"]
    $pageBytes = [string]$pageReport["bytes"]
    $lines.Add("| ``$pagePath`` | $pageStatusCode | $pageBytes |")
}
$lines.Add("")
$lines.Add("## Checks")
$lines.Add("")
$lines.Add("| Status | Key | Page | Message |")
$lines.Add("| --- | --- | --- | --- |")
foreach ($check in $checks) {
    $message = ([string]$check["message"]).Replace("|", "\|")
    $checkStatus = [string]$check["status"]
    $checkKey = [string]$check["key"]
    $checkPath = [string]$check["path"]
    $lines.Add("| $checkStatus | ``$checkKey`` | ``$checkPath`` | $message |")
}

Set-Content -LiteralPath $markdownPath -Value $lines -Encoding UTF8

Write-Host "Web legal surface audit Markdown: $markdownPath"
Write-Host "Web legal surface audit JSON: $jsonPath"
Write-Host "Passed: $($report.passed)"
Write-Host "Blockers: $($report.blockerCount)"

if ($FailOnIssue -and $blockers.Count -gt 0) {
    throw "Web legal surface audit found $($blockers.Count) blocker(s)."
}
