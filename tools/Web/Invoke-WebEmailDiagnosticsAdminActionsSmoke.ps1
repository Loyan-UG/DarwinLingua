[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$OutputDirectory = "artifacts/validation/web-email-diagnostics-admin-actions-smoke",
    [string]$AdminEmail = $env:DARWINLINGUA_WEB_ADMIN_EMAIL,
    [string]$AdminPassword = $env:DARWINLINGUA_WEB_ADMIN_PASSWORD,
    [switch]$UseLocalDevelopmentSeed,
    [string]$LocalSettingsPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
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

function Escape-SqlLiteral {
    param([string]$Value)

    return $Value.Replace("'", "''")
}

function Invoke-CheckedPsql {
    param([string]$Sql)

    docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -v ON_ERROR_STOP=1 -q -c $Sql | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "psql command failed."
    }
}

function Invoke-JsonPsql {
    param([string]$Sql)

    $raw = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql JSON query failed."
    }

    $text = ($raw | Out-String).Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $null
    }

    return $text | ConvertFrom-Json
}

function Get-Sha256Hex {
    param([string]$Value)

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Value)
    $hash = [System.Security.Cryptography.SHA256]::HashData($bytes)
    return ($hash | ForEach-Object { $_.ToString("X2") }) -join ""
}

function Get-HiddenInputValue {
    param(
        [string]$Html,
        [string]$Name
    )

    $pattern = '<input[^>]+name="' + [Regex]::Escape($Name) + '"[^>]*value="([^"]*)"'
    $match = [Regex]::Match($Html, $pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        return $null
    }

    return [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
}

function Read-LocalDevelopmentSeed {
    param([string]$Path)

    $resolvedPath = Resolve-RepositoryPath -Path $Path
    if (-not (Test-Path -LiteralPath $resolvedPath)) {
        throw "Local development settings file not found: $resolvedPath"
    }

    $settings = Get-Content -LiteralPath $resolvedPath -Raw | ConvertFrom-Json
    $bootstrap = $settings.PSObject.Properties["IdentityBootstrap"]?.Value
    if ($null -eq $bootstrap) {
        throw "IdentityBootstrap section was not found in $resolvedPath"
    }

    $email = $bootstrap.PSObject.Properties["SeedAdminEmail"]?.Value
    $password = $bootstrap.PSObject.Properties["SeedAdminPassword"]?.Value
    if ([string]::IsNullOrWhiteSpace($email) -or [string]::IsNullOrWhiteSpace($password)) {
        throw "Seed admin credentials are not configured in $resolvedPath"
    }

    return [pscustomobject]@{
        Email = [string]$email
        Password = [string]$password
    }
}

if ($UseLocalDevelopmentSeed.IsPresent) {
    $seed = Read-LocalDevelopmentSeed -Path $LocalSettingsPath
    $AdminEmail = $seed.Email
    $AdminPassword = $seed.Password
}

if ([string]::IsNullOrWhiteSpace($AdminEmail) -or [string]::IsNullOrWhiteSpace($AdminPassword)) {
    throw "Admin credentials are required. Set DARWINLINGUA_WEB_ADMIN_EMAIL and DARWINLINGUA_WEB_ADMIN_PASSWORD, pass -AdminEmail/-AdminPassword, or use -UseLocalDevelopmentSeed for local-only smoke."
}

$web = $WebBaseUrl.Trim().TrimEnd("/")
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$marker = "admin-actions-smoke-$timestamp"
$providerMessageId = "$marker@darwinlingua.local"
$deliveryHash = Get-Sha256Hex "$marker-delivery"
$removableSuppressionHash = Get-Sha256Hex "$marker-removable-suppression"
$deliveryId = [Guid]::NewGuid().ToString()
$suppressionId = [Guid]::NewGuid().ToString()
$nowIso = (Get-Date).ToUniversalTime().ToString("O")
$createdRows = [System.Collections.Generic.List[string]]::new()

try {
    $insertDeliverySql = @"
insert into "WebEmailDeliveryLogs" (
    "Id",
    "ScenarioKey",
    "RecipientEmailHash",
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
    '$deliveryId',
    'Smoke.AdminEmailDiagnostics.ProviderEvent',
    '$deliveryHash',
    'Smoke.AdminEmailDiagnostics.ProviderEvent',
    'en',
    'Admin diagnostics provider event smoke',
    'brevo-api',
    '$(Escape-SqlLiteral $providerMessageId)',
    'Sent',
    0,
    '$nowIso',
    '$nowIso',
    '$nowIso',
    'admin-actions-smoke:$timestamp'
);
"@
    Invoke-CheckedPsql -Sql $insertDeliverySql
    $createdRows.Add("delivery:$deliveryId") | Out-Null

    $insertSuppressionSql = @"
insert into "WebEmailSuppressions" (
    "Id",
    "RecipientEmailHash",
    "Reason",
    "ProviderName",
    "ProviderMessageId",
    "CreatedAtUtc",
    "LastSeenAtUtc"
) values (
    '$suppressionId',
    '$removableSuppressionHash',
    'brevo:hard_bounce',
    'brevo-api',
    '$(Escape-SqlLiteral $providerMessageId)',
    '$nowIso',
    '$nowIso'
);
"@
    Invoke-CheckedPsql -Sql $insertSuppressionSql
    $createdRows.Add("suppression:$suppressionId") | Out-Null

    $session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
    $loginUrl = "$web/Identity/Account/Login?ReturnUrl=%2Fadmin%2Femail-diagnostics"
    $loginPage = Invoke-WebRequest -Uri $loginUrl -WebSession $session -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
    $loginToken = Get-HiddenInputValue -Html $loginPage.Content -Name "__RequestVerificationToken"
    $returnUrl = Get-HiddenInputValue -Html $loginPage.Content -Name "ReturnUrl"
    if ([string]::IsNullOrWhiteSpace($loginToken)) {
        throw "Login antiforgery token was not found."
    }
    if ([string]::IsNullOrWhiteSpace($returnUrl)) {
        $returnUrl = "/admin/email-diagnostics"
    }

    $loginBody = @{
        "__RequestVerificationToken" = $loginToken
        "Input.Email" = $AdminEmail
        "Input.Password" = $AdminPassword
        "Input.RememberMe" = "false"
        "ReturnUrl" = $returnUrl
    }
    $loginResponse = Invoke-WebRequest -Uri "$web/Identity/Account/Login" -Method Post -WebSession $session -Body $loginBody -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
    if ($loginResponse.BaseResponse.RequestMessage.RequestUri.AbsoluteUri -like "*/Identity/Account/Login*") {
        throw "Admin login did not complete successfully."
    }

    $diagnosticsPage = Invoke-WebRequest -Uri "$web/admin/email-diagnostics?providerMessageId=$([System.Net.WebUtility]::UrlEncode($providerMessageId))&suppressionHashPrefix=$($removableSuppressionHash.Substring(0, 16))" -WebSession $session -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck
    $formToken = Get-HiddenInputValue -Html $diagnosticsPage.Content -Name "__RequestVerificationToken"
    if ([string]::IsNullOrWhiteSpace($formToken)) {
        throw "Email diagnostics antiforgery token was not found."
    }

    $providerEventBody = @{
        "__RequestVerificationToken" = $formToken
        "providerMessageId" = $providerMessageId
        "providerEvent" = "hard_bounce"
        "reason" = "Admin diagnostics actions smoke"
    }
    $providerEventResponse = Invoke-WebRequest -Uri "$web/admin/email-diagnostics/provider-events" -Method Post -WebSession $session -Body $providerEventBody -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck

    $removeSuppressionBody = @{
        "__RequestVerificationToken" = $formToken
        "recipientEmailHash" = $removableSuppressionHash
    }
    $removeSuppressionResponse = Invoke-WebRequest -Uri "$web/admin/email-diagnostics/suppressions/remove" -Method Post -WebSession $session -Body $removeSuppressionBody -TimeoutSec $TimeoutSeconds -MaximumRedirection 5 -SkipCertificateCheck

    $deliverySql = @"
select coalesce(row_to_json(t), '{}'::json)
from (
    select
        "Status"::text as "Status",
        "ProviderLastEvent",
        "FailureCode",
        "ProviderLastEventReason"
    from "WebEmailDeliveryLogs"
    where "Id" = '$deliveryId'
) t;
"@
    $delivery = Invoke-JsonPsql -Sql $deliverySql

    $providerSuppressionSql = @"
select coalesce(row_to_json(t), '{}'::json)
from (
    select
        "Reason",
        "ProviderName",
        "ProviderMessageId"
    from "WebEmailSuppressions"
    where "RecipientEmailHash" = '$deliveryHash'
) t;
"@
    $providerSuppression = Invoke-JsonPsql -Sql $providerSuppressionSql

    $removedSuppressionCountSql = @"
select json_build_object('count', count(*))
from "WebEmailSuppressions"
where "RecipientEmailHash" = '$removableSuppressionHash';
"@
    $removedSuppressionCount = Invoke-JsonPsql -Sql $removedSuppressionCountSql

    $controllerSource = Get-Content -Raw -LiteralPath (Resolve-RepositoryPath "src\Apps\DarwinLingua.Web\Areas\Admin\Controllers\EmailDiagnosticsController.cs")
    $viewSource = Get-Content -Raw -LiteralPath (Resolve-RepositoryPath "src\Apps\DarwinLingua.Web\Areas\Admin\Views\EmailDiagnostics\Index.cshtml")

    $checks = [ordered]@{
        providerEventPostHttpOk = [int]$providerEventResponse.StatusCode -eq 200
        removeSuppressionPostHttpOk = [int]$removeSuppressionResponse.StatusCode -eq 200
        providerEventUpdatedDelivery = $delivery.Status -eq "Failed" -and $delivery.ProviderLastEvent -eq "hard_bounce" -and $delivery.FailureCode -eq "brevo:hard_bounce"
        providerEventCreatedSuppression = $providerSuppression.Reason -eq "brevo:hard_bounce" -and $providerSuppression.ProviderName -eq "brevo-api"
        removeSuppressionDeletedRow = [int]$removedSuppressionCount.count -eq 0
        suppressionsAreAdminOnly = $controllerSource.Contains("[Authorize(Policy = ""Admin"")]") -and $controllerSource.Contains("RemoveSuppression")
        providerEventsAreAdminOnly = $controllerSource.Contains("[Authorize(Policy = ""Admin"")]") -and $controllerSource.Contains("RecordProviderEvent")
        cleanupIsAdminOnly = $controllerSource.Contains("Cleanup(") -and $controllerSource.Contains("DeleteOlderThanAsync") -and $controllerSource.Contains("[Authorize(Policy = ""Admin"")]")
        adminActionsLogInformation = $controllerSource.Contains("removed email suppression") -and $controllerSource.Contains("manually recorded provider event") -and $controllerSource.Contains("deleted {DeletedCount} email delivery log entries")
        adminOnlyControlsVisible = $viewSource.Contains("Unsuppress") -and $viewSource.Contains("Manual provider event") -and $viewSource.Contains("Clean up old delivery logs")
    }

    $failed = @($checks.GetEnumerator() | Where-Object { -not $_.Value })
    $passed = $failed.Count -eq 0

    $outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
    New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
    $jsonPath = Join-Path $outputRoot "web-email-diagnostics-admin-actions-smoke-$timestamp.json"
    $markdownPath = Join-Path $outputRoot "web-email-diagnostics-admin-actions-smoke-$timestamp.md"

    $report = [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
        webBaseUrl = $web
        credentialSource = if ($UseLocalDevelopmentSeed.IsPresent) { "local-development-seed" } else { "provided" }
        passed = $passed
        providerMessageIdDisplay = $providerMessageId
        deliveryId = $deliveryId
        removableSuppressionHashPrefix = $removableSuppressionHash.Substring(0, 16)
        providerEventSuppressionHashPrefix = $deliveryHash.Substring(0, 16)
        checks = $checks
        failedChecks = @($failed | ForEach-Object { $_.Key })
    }
    $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

    $lines = [System.Collections.Generic.List[string]]::new()
    $lines.Add("# Web Email Diagnostics Admin Actions Smoke")
    $lines.Add("")
    $lines.Add("- Generated: $($report.generatedAtUtc)")
    $lines.Add("- Web base URL: $web")
    $lines.Add("- Passed: $passed")
    $lines.Add("- Provider message id display: $providerMessageId")
    $lines.Add("- Delivery id: $deliveryId")
    $lines.Add("- Removable suppression hash prefix: $($report.removableSuppressionHashPrefix)")
    $lines.Add("- Provider-event suppression hash prefix: $($report.providerEventSuppressionHashPrefix)")
    $lines.Add("")
    $lines.Add("| Check | Value |")
    $lines.Add("| --- | --- |")
    foreach ($entry in $checks.GetEnumerator()) {
        $lines.Add("| $($entry.Key) | $($entry.Value) |")
    }
    $lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

    Write-Host "Web email diagnostics admin actions smoke report: $markdownPath"
    Write-Host "JSON report: $jsonPath"
    Write-Host "Passed: $passed"

    if (-not $passed) {
        exit 1
    }
}
finally {
    Invoke-CheckedPsql -Sql "delete from ""WebEmailSuppressions"" where ""RecipientEmailHash"" in ('$deliveryHash', '$removableSuppressionHash');"
    Invoke-CheckedPsql -Sql "delete from ""WebEmailDeliveryLogs"" where ""Id"" = '$deliveryId';"
}
