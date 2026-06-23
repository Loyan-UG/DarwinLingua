param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$OutputDirectory = "artifacts/validation/brevo-webhook-suppression-smoke",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres"
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

function Escape-SqlLiteral {
    param([string]$Value)

    return $Value.Replace("'", "''")
}

function Invoke-ScalarPsql {
    param([string]$Sql)

    $result = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql scalar query failed."
    }

    return ($result | Out-String).Trim()
}

function Invoke-CheckedPsql {
    param([string]$Sql)

    docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -v ON_ERROR_STOP=1 -c $Sql | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "psql command failed."
    }
}

function Get-Sha256Hex {
    param([string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value.Trim().ToUpperInvariant())
    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData($bytes))
}

$resolvedConfigPath = Resolve-RepositoryPath -Path $ConfigPath
if (-not (Test-Path -LiteralPath $resolvedConfigPath)) {
    throw "Config file was not found: $resolvedConfigPath"
}

$config = Get-Content -Raw -LiteralPath $resolvedConfigPath | ConvertFrom-Json
$emailOptions = $config.TransactionalEmail
if ($null -eq $emailOptions) {
    throw "TransactionalEmail section is missing in $resolvedConfigPath."
}

$webhookSecret = [string]$emailOptions.BrevoWebhookSecret
if ([string]::IsNullOrWhiteSpace($webhookSecret) -or $webhookSecret.Length -lt 32) {
    throw "TransactionalEmail:BrevoWebhookSecret is missing or too short. Secret value was not printed."
}

$baseUri = $WebBaseUrl.TrimEnd("/")
$webhookUri = "$baseUri/webhooks/brevo/transactional-email"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$nowUtc = [DateTimeOffset]::UtcNow
$providerMessageId = "<darwinlingua-webhook-smoke-$timestamp@smtp-relay.mailin.fr>"
$recipientEmail = "darwinlingua-webhook-smoke-$timestamp@example.invalid"
$recipientEmailHash = Get-Sha256Hex -Value $recipientEmail
$deliveryLogId = [Guid]::NewGuid().ToString()
$correlationId = "webhook-suppression-smoke-$timestamp"

$insertSql = @"
insert into "WebEmailDeliveryLogs" (
    "Id",
    "ScenarioKey",
    "RecipientEmailHash",
    "RecipientUserId",
    "TemplateKey",
    "Culture",
    "Subject",
    "ProviderName",
    "ProviderMessageId",
    "Status",
    "RetryCount",
    "CreatedAtUtc",
    "SentAtUtc",
    "LastAttemptAtUtc",
    "CorrelationId"
) values (
    '$deliveryLogId',
    'Smoke.BrevoWebhookSuppression',
    '$recipientEmailHash',
    null,
    'smoke.brevo-webhook-suppression',
    'en',
    'Brevo webhook suppression smoke',
    'brevo-api',
    '$(Escape-SqlLiteral -Value $providerMessageId)',
    'Sent',
    0,
    '$($nowUtc.UtcDateTime.ToString("O"))',
    '$($nowUtc.UtcDateTime.ToString("O"))',
    '$($nowUtc.UtcDateTime.ToString("O"))',
    '$correlationId'
);
"@

Invoke-CheckedPsql -Sql $insertSql

$eventTimestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
$payload = [ordered]@{
    event = "hardBounce"
    "message-id" = $providerMessageId
    ts_event = $eventTimestamp
    reason = "controlled smoke hard bounce; no real recipient was contacted"
    subject = "Brevo webhook suppression smoke"
} | ConvertTo-Json -Depth 5

$headers = @{
    Authorization = "Bearer $webhookSecret"
}

$response = Invoke-WebRequest `
    -Uri $webhookUri `
    -Method Post `
    -Headers $headers `
    -ContentType "application/json" `
    -Body $payload `
    -SkipHttpErrorCheck

$safeProviderMessageId = Escape-SqlLiteral -Value $providerMessageId
$deliverySql = @"
select
    "Status" || '|' ||
    coalesce("ProviderLastEvent", '') || '|' ||
    coalesce("FailureCode", '') || '|' ||
    coalesce("ProviderLastEventReason", '')
from "WebEmailDeliveryLogs"
where "ProviderMessageId" = '$safeProviderMessageId'
order by "CreatedAtUtc" desc
limit 1;
"@
$deliveryResult = Invoke-ScalarPsql -Sql $deliverySql

$suppressionSql = @"
select coalesce("Reason", '') || '|' || coalesce("ProviderName", '') || '|' || coalesce("ProviderMessageId", '')
from "WebEmailSuppressions"
where "ProviderMessageId" = '$safeProviderMessageId'
order by "CreatedAtUtc" desc
limit 1;
"@
$suppressionResult = Invoke-ScalarPsql -Sql $suppressionSql

$deliveryParts = $deliveryResult.Split("|", 4)
$suppressionParts = $suppressionResult.Split("|", 3)

$passed = $response.StatusCode -eq 200 -and
    $deliveryParts.Length -eq 4 -and
    $deliveryParts[0] -eq "Failed" -and
    $deliveryParts[1] -eq "hard_bounce" -and
    $deliveryParts[2] -eq "brevo:hard_bounce" -and
    $suppressionParts.Length -eq 3 -and
    $suppressionParts[0] -eq "brevo:hard_bounce" -and
    $suppressionParts[1] -eq "brevo-api" -and
    $suppressionParts[2] -eq $providerMessageId

$resolvedOutputDirectory = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force | Out-Null

$report = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow
    webBaseUrl = $baseUri
    webhookUrl = $webhookUri
    httpStatus = [int]$response.StatusCode
    passed = $passed
    deliveryLogId = $deliveryLogId
    providerMessageId = $providerMessageId
    recipientEmailHash = $recipientEmailHash
    providerLastEvent = $(if ($deliveryParts.Length -ge 2) { $deliveryParts[1] } else { $null })
    deliveryStatus = $(if ($deliveryParts.Length -ge 1) { $deliveryParts[0] } else { $null })
    failureCode = $(if ($deliveryParts.Length -ge 3) { $deliveryParts[2] } else { $null })
    suppressionReason = $(if ($suppressionParts.Length -ge 1) { $suppressionParts[0] } else { $null })
    suppressionProvider = $(if ($suppressionParts.Length -ge 2) { $suppressionParts[1] } else { $null })
}

$jsonPath = Join-Path $resolvedOutputDirectory "brevo-webhook-suppression-smoke-$timestamp.json"
$markdownPath = Join-Path $resolvedOutputDirectory "brevo-webhook-suppression-smoke-$timestamp.md"

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Brevo Webhook Suppression Smoke")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Webhook URL: $webhookUri")
$lines.Add("- HTTP status: $($report.httpStatus)")
$lines.Add("- Passed: $passed")
$lines.Add("- Provider message id: $providerMessageId")
$lines.Add("- Synthetic recipient hash: $recipientEmailHash")
$lines.Add("")
$lines.Add("| Check | Value |")
$lines.Add("| --- | --- |")
$lines.Add("| deliveryStatus | $($report.deliveryStatus) |")
$lines.Add("| providerLastEvent | $($report.providerLastEvent) |")
$lines.Add("| failureCode | $($report.failureCode) |")
$lines.Add("| suppressionReason | $($report.suppressionReason) |")
$lines.Add("| suppressionProvider | $($report.suppressionProvider) |")
$lines.Add("")
$lines.Add("The synthetic recipient address is not printed or reused. The smoke inserts one controlled delivery-log row, posts a `hardBounce` webhook with Bearer token authentication, and verifies that the public endpoint updates the log and creates an internal suppression.")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Brevo webhook suppression smoke report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
