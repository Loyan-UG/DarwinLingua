[CmdletBinding()]
param(
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$ProjectPath = "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj",
    [string]$OutputDirectory = "artifacts/validation/brevo-webhook-configuration-check",
    [string]$ExpectedWebhookUrl = "https://darwinlingua.com/webhooks/brevo/transactional-email",
    [string[]]$ExpectedEvents = @(
        "request",
        "delivered",
        "deferred",
        "softBounce",
        "hardBounce",
        "blocked",
        "invalid",
        "spam",
        "error",
        "opened",
        "uniqueOpened",
        "proxyOpen",
        "uniqueProxyOpen",
        "click",
        "unsubscribed"
    ),
    [switch]$AllowMissingOptionalTelemetryEvents
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

function Test-ConfiguredSecret {
    param([object]$Value)

    if ($null -eq $Value) {
        return $false
    }

    $text = $Value.ToString().Trim()
    if ([string]::IsNullOrWhiteSpace($text)) {
        return $false
    }

    return -not ($text.StartsWith("<", [StringComparison]::Ordinal) -or
        $text.Contains("secret-from-brevo", [StringComparison]::OrdinalIgnoreCase) -or
        $text.Contains("strong-random-secret", [StringComparison]::OrdinalIgnoreCase) -or
        $text.Contains("example", [StringComparison]::OrdinalIgnoreCase))
}

function ConvertTo-EventKey {
    param([string]$EventName)

    if ([string]::IsNullOrWhiteSpace($EventName)) {
        return ""
    }

    $trimmed = $EventName.Trim()
    $withoutHyphen = $trimmed.Replace("-", "", [StringComparison]::Ordinal)
    $withoutUnderscore = $withoutHyphen.Replace("_", "", [StringComparison]::Ordinal)
    return $withoutUnderscore.ToLowerInvariant()
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
$brevoWebhookSecret = [string](Get-ConfigurationValue -SectionObject $transactionalEmail -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoWebhookSecret")

if ([string]::IsNullOrWhiteSpace($brevoApiBaseUrl)) {
    $brevoApiBaseUrl = "https://api.brevo.com"
}

if (-not (Test-ConfiguredSecret -Value $brevoApiKey)) {
    throw "Brevo API key is not configured. The key value was not printed."
}

$configuredWebhookSecretPresent = (Test-ConfiguredSecret -Value $brevoWebhookSecret) -and $brevoWebhookSecret.Length -ge 32
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$jsonPath = Join-Path $outputRoot "brevo-webhook-configuration-check-$timestamp.json"
$markdownPath = Join-Path $outputRoot "brevo-webhook-configuration-check-$timestamp.md"

$headers = @{
    "api-key" = $brevoApiKey
    "accept" = "application/json"
}

$endpoint = "$($brevoApiBaseUrl.TrimEnd('/'))/v3/webhooks"
$response = Invoke-RestMethod -Uri $endpoint -Method Get -Headers $headers -TimeoutSec 30
$webhooks = @()
if ($response -is [array]) {
    $webhooks = @($response)
}
elseif ($null -ne $response.PSObject.Properties["webhooks"]) {
    $webhooks = @($response.webhooks)
}
elseif ($null -ne $response.PSObject.Properties["items"]) {
    $webhooks = @($response.items)
}

$targetWebhook = $webhooks |
    Where-Object { [string](Get-PropertyValue -Object $_ -Name "url") -eq $ExpectedWebhookUrl } |
    Select-Object -First 1

$targetFound = $null -ne $targetWebhook
$targetEvents = @()
$targetAuthType = $null
$targetType = $null
$targetId = $null
if ($targetFound) {
    $targetId = Get-PropertyValue -Object $targetWebhook -Name "id"
    $targetType = [string](Get-PropertyValue -Object $targetWebhook -Name "type")
    $eventsValue = Get-PropertyValue -Object $targetWebhook -Name "events"
    $targetEvents = @($eventsValue | ForEach-Object { [string]$_ } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    $auth = Get-PropertyValue -Object $targetWebhook -Name "auth"
    if ($null -ne $auth) {
        $targetAuthType = [string](Get-PropertyValue -Object $auth -Name "type")
    }
}

$expectedEventKeys = @($ExpectedEvents | ForEach-Object { ConvertTo-EventKey -EventName $_ } | Where-Object { $_ } | Select-Object -Unique)
$targetEventKeys = @($targetEvents | ForEach-Object { ConvertTo-EventKey -EventName $_ } | Where-Object { $_ } | Select-Object -Unique)
$optionalTelemetryEventKeys = @("proxyopen", "uniqueproxyopen", "error")
$missingExpectedEventKeys = @($expectedEventKeys | Where-Object {
        $candidate = $_
        -not ($targetEventKeys | Where-Object { $_ -eq $candidate })
    })
if ($AllowMissingOptionalTelemetryEvents.IsPresent) {
    $missingExpectedEventKeys = @($missingExpectedEventKeys | Where-Object {
            $candidate = $_
            -not ($optionalTelemetryEventKeys | Where-Object { $_ -eq $candidate })
        })
}

$unexpectedEventKeys = @($targetEventKeys | Where-Object {
        $candidate = $_
        -not ($expectedEventKeys | Where-Object { $_ -eq $candidate })
    })

$targetTypeOk = $targetFound -and [string]::Equals($targetType, "transactional", [StringComparison]::OrdinalIgnoreCase)
$targetAuthOk = $targetFound -and [string]::Equals($targetAuthType, "bearer", [StringComparison]::OrdinalIgnoreCase)
$passed = $targetFound -and
    $targetTypeOk -and
    $targetAuthOk -and
    $configuredWebhookSecretPresent -and
    $missingExpectedEventKeys.Count -eq 0

$report = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow
    brevoApiBaseUrl = $brevoApiBaseUrl
    expectedWebhookUrl = $ExpectedWebhookUrl
    webhookCount = $webhooks.Count
    targetWebhookFound = $targetFound
    targetWebhookId = $targetId
    targetWebhookType = $targetType
    targetWebhookAuthType = $targetAuthType
    configuredWebhookSecretPresent = $configuredWebhookSecretPresent
    targetEvents = @($targetEvents | Sort-Object)
    expectedEvents = @($ExpectedEvents | Sort-Object)
    missingExpectedEventKeys = @($missingExpectedEventKeys | Sort-Object)
    unexpectedEventKeys = @($unexpectedEventKeys | Sort-Object)
    allowMissingOptionalTelemetryEvents = $AllowMissingOptionalTelemetryEvents.IsPresent
    passed = $passed
    officialCreateWebhookReference = "https://developers.brevo.com/reference/create-webhook"
    officialSecuredWebhookReference = "https://developers.brevo.com/docs/secured-webhooks"
    officialTransactionalWebhookReference = "https://developers.brevo.com/docs/transactional-webhooks"
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Brevo Webhook Configuration Check")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Expected webhook URL: $ExpectedWebhookUrl")
$lines.Add("- Webhook count: $($report.webhookCount)")
$lines.Add("- Target webhook found: $targetFound")
$lines.Add("- Target webhook id: $targetId")
$lines.Add("- Target webhook type: $targetType")
$lines.Add("- Target webhook auth type: $targetAuthType")
$lines.Add("- Configured webhook secret present: $configuredWebhookSecretPresent")
$lines.Add("- Missing expected event keys: $($missingExpectedEventKeys.Count)")
$lines.Add("- Unexpected event keys: $($unexpectedEventKeys.Count)")
$lines.Add("- Passed: $passed")
$lines.Add("")
$lines.Add("## Target Events")
$lines.Add("")
foreach ($eventName in @($targetEvents | Sort-Object)) {
    $lines.Add(('- `{0}`' -f $eventName))
}
$lines.Add("")
$lines.Add("## Missing Expected Event Keys")
$lines.Add("")
if ($missingExpectedEventKeys.Count -eq 0) {
    $lines.Add("- none")
}
else {
    foreach ($eventKey in @($missingExpectedEventKeys | Sort-Object)) {
        $lines.Add(('- `{0}`' -f $eventKey))
    }
}
$lines.Add("")
$lines.Add("The report intentionally stores only webhook metadata, event names, auth type, and boolean secret presence. It does not store the Brevo API key, webhook bearer token, custom header values, or raw webhook response.")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Brevo webhook configuration check report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
