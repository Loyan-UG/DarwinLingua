[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$ApiHealthUrl = "https://api.darwinlingua.com/health",
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$OutputDirectory = "artifacts/validation/web-operations-bootstrap",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres",
    [int]$TimeoutSeconds = 20
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

function Test-HttpOk {
    param([string]$Url)

    $started = Get-Date
    try {
        $response = Invoke-WebRequest -Uri $Url -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
        return [pscustomobject]@{
            Url = $Url
            StatusCode = [int]$response.StatusCode
            ContentLength = $response.Content.Length
            ElapsedMs = [int]((Get-Date) - $started).TotalMilliseconds
            Passed = [int]$response.StatusCode -eq 200
            Error = $null
        }
    }
    catch {
        return [pscustomobject]@{
            Url = $Url
            StatusCode = $null
            ContentLength = $null
            ElapsedMs = [int]((Get-Date) - $started).TotalMilliseconds
            Passed = $false
            Error = $_.Exception.Message
        }
    }
}

$resolvedConfigPath = Resolve-RepositoryPath -Path $ConfigPath
if (-not (Test-Path -LiteralPath $resolvedConfigPath)) {
    throw "Config file was not found: $resolvedConfigPath"
}

$config = Get-Content -Raw -LiteralPath $resolvedConfigPath | ConvertFrom-Json
$connectionStrings = $config.PSObject.Properties["ConnectionStrings"]?.Value
$webApi = $config.PSObject.Properties["WebApi"]?.Value
$identityBootstrap = $config.PSObject.Properties["IdentityBootstrap"]?.Value
$transactionalEmail = $config.PSObject.Properties["TransactionalEmail"]?.Value

$requiredTables = @(
    "AspNetUsers",
    "WebUserPreferences",
    "WebEmailDeliveryLogs",
    "WebEmailSuppressions",
    "WebPolicyAcceptances",
    "UserContentProgress"
)
$requiredTableValues = ($requiredTables | ForEach-Object { "'$($_)'" }) -join ","
$tableSql = @"
select coalesce(json_agg(table_name order by table_name), '[]'::json)
from information_schema.tables
where table_schema = 'public'
  and table_name in ($requiredTableValues);
"@
$existingTables = @(Invoke-JsonPsql -Sql $tableSql)

$requiredEmailColumns = @(
    "ProviderLastEvent",
    "ProviderLastEventAtUtc",
    "ProviderLastEventReason"
)
$requiredColumnValues = ($requiredEmailColumns | ForEach-Object { "'$($_)'" }) -join ","
$columnSql = @"
select coalesce(json_agg(column_name order by column_name), '[]'::json)
from information_schema.columns
where table_schema = 'public'
  and table_name = 'WebEmailDeliveryLogs'
  and column_name in ($requiredColumnValues);
"@
$existingEmailColumns = @(Invoke-JsonPsql -Sql $columnSql)

$webCheck = Test-HttpOk -Url $WebBaseUrl.TrimEnd("/")
$apiCheck = Test-HttpOk -Url $ApiHealthUrl

$adminRecipients = @()
if ($null -ne $transactionalEmail) {
    $property = $transactionalEmail.PSObject.Properties["AdminNotificationEmails"]
    if ($null -ne $property -and $null -ne $property.Value) {
        $adminRecipients = @($property.Value)
    }
}

$checks = [ordered]@{
    identityConnectionConfigured = $null -ne $connectionStrings -and -not [string]::IsNullOrWhiteSpace([string]$connectionStrings.Identity)
    identityAdminConnectionConfigured = $null -ne $connectionStrings -and -not [string]::IsNullOrWhiteSpace([string]$connectionStrings.IdentityAdmin)
    webApiBaseUrlConfigured = $null -ne $webApi -and -not [string]::IsNullOrWhiteSpace([string]$webApi.BaseUrl)
    webApiIgnoreSslErrorsFalse = $null -ne $webApi -and $webApi.IgnoreSslErrors -eq $false
    identityBootstrapConfigured = $null -ne $identityBootstrap -and $null -ne $identityBootstrap.RequireSeedAccounts
    seedCredentialsConfiguredWhenRequired = $null -ne $identityBootstrap -and (
        $identityBootstrap.RequireSeedAccounts -ne $true -or
        (-not [string]::IsNullOrWhiteSpace([string]$identityBootstrap.SeedAdminEmail) -and
            -not [string]::IsNullOrWhiteSpace([string]$identityBootstrap.SeedAdminPassword)))
    transactionalAdminRecipientConfigured = $adminRecipients.Count -gt 0
    webEndpointOk = $webCheck.Passed
    apiHealthOk = $apiCheck.Passed
    requiredTablesPresent = @($requiredTables | Where-Object { $existingTables -contains $_ }).Count -eq $requiredTables.Count
    emailEventColumnsPresent = @($requiredEmailColumns | Where-Object { $existingEmailColumns -contains $_ }).Count -eq $requiredEmailColumns.Count
}

$failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
$passed = $failed.Count -eq 0

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "web-operations-bootstrap-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-operations-bootstrap-$timestamp.md"

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    webBaseUrl = $WebBaseUrl.TrimEnd("/")
    apiHealthUrl = $ApiHealthUrl
    databaseName = $DatabaseName
    passed = $passed
    adminNotificationRecipientCount = $adminRecipients.Count
    existingTables = $existingTables
    existingEmailEventColumns = $existingEmailColumns
    webEndpoint = $webCheck
    apiHealth = $apiCheck
    checks = $checks
    failedChecks = @($failed | ForEach-Object { $_.Key })
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Operations Bootstrap Check")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Web base URL: $($report.webBaseUrl)")
$lines.Add("- API health URL: $ApiHealthUrl")
$lines.Add("- Database: $DatabaseName")
$lines.Add("- Passed: $passed")
$lines.Add("- Admin notification recipients: $($adminRecipients.Count)")
$lines.Add("")
$lines.Add("| Check | Value |")
$lines.Add("| --- | --- |")
foreach ($entry in $checks.GetEnumerator()) {
    $lines.Add("| $($entry.Key) | $($entry.Value) |")
}
$lines.Add("")
$lines.Add("## Runtime")
$lines.Add("")
$lines.Add("| URL | Status | Bytes | Passed |")
$lines.Add("| --- | --- | --- | --- |")
$lines.Add("| $($webCheck.Url) | $($webCheck.StatusCode) | $($webCheck.ContentLength) | $($webCheck.Passed) |")
$lines.Add("| $($apiCheck.Url) | $($apiCheck.StatusCode) | $($apiCheck.ContentLength) | $($apiCheck.Passed) |")
$lines.Add("")
$lines.Add("## Database")
$lines.Add("")
$lines.Add("- Required tables present: $($checks.requiredTablesPresent)")
$lines.Add("- Existing required tables: $($existingTables -join ', ')")
$lines.Add("- Email event columns present: $($checks.emailEventColumnsPresent)")
$lines.Add("- Existing email event columns: $($existingEmailColumns -join ', ')")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web operations bootstrap report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
