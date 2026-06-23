[CmdletBinding()]
param(
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$ProjectPath = "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj",
    [string]$OutputDirectory = "artifacts/validation/brevo-transactional-log-check",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres",
    [int]$Days = 7,
    [int]$MaxMessages = 10,
    [switch]$RequireEvents
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

function Get-UserSecretsPath {
    param([string]$ProjectFile)

    $resolvedProjectPath = Resolve-RepositoryPath -Path $ProjectFile
    if (-not (Test-Path -LiteralPath $resolvedProjectPath -PathType Leaf)) {
        return $null
    }

    [xml]$projectXml = Get-Content -LiteralPath $resolvedProjectPath
    $userSecretsId = ($projectXml.Project.PropertyGroup |
        ForEach-Object { $_.UserSecretsId } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) } |
        Select-Object -First 1)

    if ([string]::IsNullOrWhiteSpace($userSecretsId)) {
        return $null
    }

    $appData = [Environment]::GetFolderPath("ApplicationData")
    if ([string]::IsNullOrWhiteSpace($appData)) {
        return $null
    }

    return Join-Path $appData "Microsoft\UserSecrets\$userSecretsId\secrets.json"
}

function Get-SecretOverride {
    param(
        [object]$Secrets,
        [string]$Section,
        [string]$Name
    )

    if ($null -eq $Secrets) {
        return $null
    }

    $flatName = "$Section`:$Name"
    $flatProperty = $Secrets.PSObject.Properties[$flatName]
    if ($null -ne $flatProperty) {
        return $flatProperty.Value
    }

    $sectionObject = Get-PropertyValue -Object $Secrets -Name $Section
    return Get-PropertyValue -Object $sectionObject -Name $Name
}

function Get-ConfigurationValue {
    param(
        [object]$SectionObject,
        [object]$Secrets,
        [string]$Section,
        [string]$Name
    )

    $secretValue = Get-SecretOverride -Secrets $Secrets -Section $Section -Name $Name
    if ($null -ne $secretValue) {
        return $secretValue
    }

    return Get-PropertyValue -Object $SectionObject -Name $Name
}

function Invoke-JsonPsql {
    param([string]$Sql)

    $raw = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql JSON query failed."
    }

    $text = ($raw | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return @()
    }

    return $text | ConvertFrom-Json
}

function ConvertTo-SafeReportMessageId {
    param([string]$MessageId)

    if ([string]::IsNullOrWhiteSpace($MessageId)) {
        return ""
    }

    if ($MessageId.Length -le 24) {
        return $MessageId
    }

    return "$($MessageId.Substring(0, 12))...$($MessageId.Substring($MessageId.Length - 8))"
}

$resolvedConfigPath = Resolve-RepositoryPath -Path $ConfigPath
if (-not (Test-Path -LiteralPath $resolvedConfigPath -PathType Leaf)) {
    throw "Config file was not found: $resolvedConfigPath"
}

$config = Get-Content -LiteralPath $resolvedConfigPath -Raw | ConvertFrom-Json
$resolvedUserSecretsPath = Get-UserSecretsPath -ProjectFile $ProjectPath
$userSecrets = $null
if (-not [string]::IsNullOrWhiteSpace($resolvedUserSecretsPath) -and
    (Test-Path -LiteralPath $resolvedUserSecretsPath -PathType Leaf)) {
    $userSecretsContent = Get-Content -LiteralPath $resolvedUserSecretsPath -Raw
    if (-not [string]::IsNullOrWhiteSpace($userSecretsContent)) {
        $userSecrets = $userSecretsContent | ConvertFrom-Json
    }
}

$transactionalEmail = Get-PropertyValue -Object $config -Name "TransactionalEmail"
$brevoApiBaseUrl = [string](Get-ConfigurationValue -SectionObject $transactionalEmail -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoApiBaseUrl")
$brevoApiKey = [string](Get-ConfigurationValue -SectionObject $transactionalEmail -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoApiKey")

if ([string]::IsNullOrWhiteSpace($brevoApiBaseUrl)) {
    $brevoApiBaseUrl = "https://api.brevo.com"
}

if ([string]::IsNullOrWhiteSpace($brevoApiKey) -or
    $brevoApiKey.StartsWith("<", [StringComparison]::Ordinal) -or
    $brevoApiKey.Contains("secret-from-brevo", [StringComparison]::OrdinalIgnoreCase)) {
    throw "Brevo API key is not configured. The key value was not printed."
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "brevo-transactional-log-check-$timestamp.json"
$markdownPath = Join-Path $outputRoot "brevo-transactional-log-check-$timestamp.md"

$safeDays = [Math]::Max(1, [Math]::Min(90, $Days))
$safeMaxMessages = [Math]::Max(1, [Math]::Min(50, $MaxMessages))
$sinceSql = [DateTimeOffset]::UtcNow.AddDays(-$safeDays).UtcDateTime.ToString("O")

$messageSql = @"
select coalesce(json_agg(t), '[]'::json)
from (
    select
        "ScenarioKey",
        "Status"::text as "Status",
        "ProviderMessageId",
        "ProviderLastEvent",
        "ProviderLastEventAtUtc",
        "FailureCode",
        "CreatedAtUtc"
    from "WebEmailDeliveryLogs"
    where "ProviderName" = 'brevo-api'
      and "ProviderMessageId" is not null
      and "ProviderMessageId" <> ''
      and "CreatedAtUtc" >= '$sinceSql'::timestamptz
    order by "CreatedAtUtc" desc
    limit $safeMaxMessages
) t;
"@

$messages = @(Invoke-JsonPsql -Sql $messageSql)
$headers = @{
    "api-key" = $brevoApiKey
    "accept" = "application/json"
}

$results = [System.Collections.Generic.List[object]]::new()
foreach ($message in $messages) {
    $providerMessageId = [string]$message.ProviderMessageId
    $encodedMessageId = [System.Uri]::EscapeDataString($providerMessageId)
    $endpoint = "$($brevoApiBaseUrl.TrimEnd('/'))/v3/smtp/statistics/events?days=$safeDays&limit=50&sort=desc&messageId=$encodedMessageId"
    $eventNames = @()
    $eventCount = 0
    $apiStatus = "ok"
    $apiError = $null

    try {
        $response = Invoke-RestMethod -Uri $endpoint -Method Get -Headers $headers -TimeoutSec 30
        $eventsProperty = $response.PSObject.Properties["events"]
        $events = if ($null -ne $eventsProperty) { @($eventsProperty.Value) } else { @() }
        $eventCount = @($events).Count
        $eventNames = @($events | ForEach-Object { [string]$_.event } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)
    }
    catch {
        $apiStatus = "error"
        $apiError = $_.Exception.Message
    }

    $results.Add([ordered]@{
        scenarioKey = $message.ScenarioKey
        deliveryStatus = $message.Status
        providerMessageIdPreview = ConvertTo-SafeReportMessageId -MessageId $providerMessageId
        providerMessageIdSha256 = [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($providerMessageId))).ToLowerInvariant()
        dbProviderLastEvent = $message.ProviderLastEvent
        dbProviderLastEventAtUtc = $message.ProviderLastEventAtUtc
        dbFailureCode = $message.FailureCode
        dbCreatedAtUtc = $message.CreatedAtUtc
        brevoApiStatus = $apiStatus
        brevoEventCount = $eventCount
        brevoEvents = $eventNames
        brevoApiError = $apiError
    }) | Out-Null
}

$matchedCount = @($results | Where-Object { $_.brevoApiStatus -eq "ok" -and $_.brevoEventCount -gt 0 }).Count
$errorCount = @($results | Where-Object { $_.brevoApiStatus -ne "ok" }).Count
$passed = $messages.Count -gt 0 -and $errorCount -eq 0 -and (-not $RequireEvents.IsPresent -or $matchedCount -gt 0)

$report = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow
    days = $safeDays
    maxMessages = $safeMaxMessages
    messageCount = $messages.Count
    matchedBrevoEventCount = $matchedCount
    apiErrorCount = $errorCount
    requireEvents = $RequireEvents.IsPresent
    passed = $passed
    officialBrevoEventReportReference = "https://developers.brevo.com/reference/get-email-event-report"
    officialBrevoEmailContentReference = "https://developers.brevo.com/reference/get-transac-email-content"
    results = $results
}

$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Brevo Transactional Log Check")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Days: $safeDays")
$lines.Add("- Messages checked: $($report.messageCount)")
$lines.Add("- Messages with Brevo API events: $matchedCount")
$lines.Add("- API errors: $errorCount")
$lines.Add("- Passed: $passed")
$lines.Add("- Official event report API: $($report.officialBrevoEventReportReference)")
$lines.Add("- Official sent-email content API: $($report.officialBrevoEmailContentReference)")
$lines.Add("")
$lines.Add("| Scenario | DB status | DB event | Brevo events | Message id hash |")
$lines.Add("| --- | --- | --- | --- | --- |")
foreach ($result in $results) {
    $eventsText = if ($result.brevoEvents.Count -gt 0) { $result.brevoEvents -join ", " } else { "(none)" }
    $lines.Add("| $($result.scenarioKey) | $($result.deliveryStatus) | $($result.dbProviderLastEvent) | $eventsText | $($result.providerMessageIdSha256.Substring(0, 16))... |")
}
$lines.Add("")
$lines.Add("The report uses Brevo's official transactional event report endpoint and never prints the Brevo API key or full provider message ids.")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Brevo transactional log check report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
