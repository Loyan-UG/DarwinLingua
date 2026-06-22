[CmdletBinding()]
param(
    [string]$RecipientEmail,
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$ProjectPath = "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj",
    [string]$OutputDirectory = "artifacts/validation/brevo-real-delivery-smoke",
    [string]$ReadinessOutputDirectory = "artifacts/validation/brevo-readiness",
    [string]$ExpectedPublicBaseUrl = "https://darwinlingua.com",
    [string]$SendingDomain = "darwinlingua.com",
    [switch]$SenderVerified,
    [switch]$DnsAuthenticated,
    [switch]$WebhookConfigured,
    [switch]$DpaAccepted,
    [switch]$ConfirmSend,
    [switch]$PreflightOnly,
    [switch]$SkipDnsLookup
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

function Test-Email {
    param([object]$Value)

    if ($null -eq $Value) {
        return $false
    }

    return $Value.ToString().Trim() -match '^[^@\s]+@[^@\s]+\.[^@\s]+$'
}

function New-SmokeMessage {
    param(
        [string]$ScenarioKey,
        [string]$TemplateKey,
        [string]$Subject,
        [string]$ActionLabel,
        [string]$ActionUrl,
        [string]$PlainText
    )

    $safeSubject = [System.Net.WebUtility]::HtmlEncode($Subject)
    $safeActionLabel = [System.Net.WebUtility]::HtmlEncode($ActionLabel)
    $safeActionUrl = [System.Net.WebUtility]::HtmlEncode($ActionUrl)
    $safePlainText = [System.Net.WebUtility]::HtmlEncode($PlainText).Replace("`r`n", "<br>").Replace("`n", "<br>")

    $html = @"
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>$safeSubject</title>
</head>
<body style="margin:0;padding:0;background:#f3f4f6;color:#111827;font-family:Arial,'Helvetica Neue',Helvetica,sans-serif;">
  <div style="width:100%;background:#f3f4f6;padding:32px 12px;">
    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;">
      <tr>
        <td align="center">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border-collapse:collapse;max-width:640px;">
            <tr>
              <td style="padding:0 0 16px 0;text-align:left;">
                <div style="font-size:13px;line-height:20px;color:#4b5563;font-weight:700;letter-spacing:.08em;text-transform:uppercase;">Darwin Lingua</div>
              </td>
            </tr>
            <tr>
              <td style="background:#ffffff;border:1px solid #e5e7eb;border-radius:16px;padding:32px;box-shadow:0 14px 40px rgba(17,24,39,.08);">
                <h1 style="margin:0 0 20px 0;color:#111827;font-size:24px;line-height:32px;font-weight:800;">$safeSubject</h1>
                <p style="margin:0 0 20px 0;color:#1f2937;font-size:16px;line-height:26px;overflow-wrap:anywhere;">$safePlainText</p>
                <p style="margin:24px 0;">
                  <a href="$safeActionUrl" style="display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:700;border-radius:999px;padding:12px 20px;">$safeActionLabel</a>
                </p>
                <p style="margin:20px 0 0 0;color:#4b5563;font-size:14px;line-height:22px;overflow-wrap:anywhere;">If the button does not work, copy this URL: $safeActionUrl</p>
              </td>
            </tr>
            <tr>
              <td style="padding:16px 0 0 0;color:#6b7280;font-size:12px;line-height:18px;">
                This is a controlled Darwin Lingua real-delivery smoke email. Do not forward it.
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </div>
</body>
</html>
"@

    [ordered]@{
        scenarioKey = $ScenarioKey
        templateKey = $TemplateKey
        subject = $Subject
        textContent = "$PlainText`n`n$ActionUrl"
        htmlContent = $html
    }
}

function Write-SmokeReport {
    param(
        [string]$JsonPath,
        [string]$MarkdownPath,
        [object]$Report
    )

    $Report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $JsonPath -Encoding UTF8

    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# Brevo Real Delivery Smoke")
    $lines.Add("")
    $lines.Add("- Generated: $($Report.generatedAtUtc)")
    $lines.Add("- Recipient: $($Report.recipientEmail)")
    $lines.Add("- Expected public base URL: $($Report.expectedPublicBaseUrl)")
    $lines.Add("- Sending domain: $($Report.sendingDomain)")
    $lines.Add("- Preflight only: $($Report.preflightOnly)")
    $lines.Add("- Status: $($Report.status)")
    $lines.Add("- Readiness report: $($Report.readinessReport)")
    if (-not [string]::IsNullOrWhiteSpace($Report.blocker)) {
        $lines.Add("- Blocker: $($Report.blocker)")
    }
    $lines.Add("")
    $lines.Add("| Scenario | Sent | Provider message id |")
    $lines.Add("| --- | --- | --- |")
    foreach ($message in @($Report.messages)) {
        $lines.Add("| $($message.scenarioKey) | $($message.sent) | $($message.providerMessageId) |")
    }
    $lines.Add("")
    $lines.Add("## Follow-up")
    $lines.Add("")
    $lines.Add("- Confirm both messages in the real inbox.")
    $lines.Add("- Confirm Brevo transactional logs show the same provider message ids.")
    $lines.Add("- Confirm webhook or manual provider-event reconciliation appears in Admin Email Diagnostics.")

    $lines | Set-Content -LiteralPath $MarkdownPath -Encoding UTF8
}

$resolvedConfigPath = Resolve-RepositoryPath -Path $ConfigPath
if (-not (Test-Path -LiteralPath $resolvedConfigPath -PathType Leaf)) {
    throw "Config file was not found: $resolvedConfigPath"
}

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$readinessOutputRoot = Resolve-RepositoryPath -Path $ReadinessOutputDirectory
New-Item -ItemType Directory -Path $readinessOutputRoot -Force | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$jsonPath = Join-Path $outputRoot "brevo-real-delivery-smoke-$timestamp.json"
$markdownPath = Join-Path $outputRoot "brevo-real-delivery-smoke-$timestamp.md"
$readinessScript = Resolve-RepositoryPath -Path "tools\Web\Invoke-BrevoProductionReadinessCheck.ps1"

if (-not (Test-Path -LiteralPath $readinessScript -PathType Leaf)) {
    throw "Brevo readiness script was not found: $readinessScript"
}

$readinessArgs = @(
    "-ConfigPath", $resolvedConfigPath,
    "-ProjectPath", (Resolve-RepositoryPath -Path $ProjectPath),
    "-OutputDirectory", $readinessOutputRoot,
    "-ExpectedPublicBaseUrl", $ExpectedPublicBaseUrl,
    "-SendingDomain", $SendingDomain,
    "-RequireRealDelivery",
    "-VerifyBrevoApi"
)
if ($SenderVerified.IsPresent) { $readinessArgs += "-SenderVerified" }
if ($DnsAuthenticated.IsPresent) { $readinessArgs += "-DnsAuthenticated" }
if ($WebhookConfigured.IsPresent) { $readinessArgs += "-WebhookConfigured" }
if ($DpaAccepted.IsPresent) { $readinessArgs += "-DpaAccepted" }
if ($SkipDnsLookup.IsPresent) { $readinessArgs += "-SkipDnsLookup" }

$powerShellExecutable = (Get-Process -Id $PID).Path
if ([string]::IsNullOrWhiteSpace($powerShellExecutable)) {
    $powerShellExecutable = "pwsh"
}

& $powerShellExecutable -NoProfile -ExecutionPolicy Bypass -File $readinessScript @readinessArgs
$readinessExitCode = $LASTEXITCODE
$latestReadinessReport = Get-ChildItem -LiteralPath $readinessOutputRoot -Filter "brevo-production-readiness-*.json" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
$readinessReport = if ($null -eq $latestReadinessReport) { $null } else { Get-Content -LiteralPath $latestReadinessReport.FullName -Raw | ConvertFrom-Json }

if ($readinessExitCode -ne 0 -or ($null -ne $readinessReport -and $readinessReport.blockerCount -gt 0)) {
    $blocker = "Brevo readiness still has blockers. Resolve them before real delivery smoke."
    if ($null -ne $readinessReport) {
        $firstBlocker = @($readinessReport.checks | Where-Object { $_.status -eq "blocker" } | Select-Object -First 1)
        if ($firstBlocker.Count -gt 0) {
            $blocker = "$($firstBlocker[0].key): $($firstBlocker[0].message)"
        }
    }

    $report = [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
        status = "blocked"
        blocker = $blocker
        preflightOnly = $PreflightOnly.IsPresent
        recipientEmail = $RecipientEmail
        expectedPublicBaseUrl = $ExpectedPublicBaseUrl
        sendingDomain = $SendingDomain
        readinessReport = if ($null -eq $latestReadinessReport) { "" } else { $latestReadinessReport.FullName }
        messages = @()
    }
    Write-SmokeReport -JsonPath $jsonPath -MarkdownPath $markdownPath -Report $report
    Write-Host "Brevo real delivery smoke report: $markdownPath"
    Write-Host "JSON report: $jsonPath"
    Write-Host "Status: blocked"
    exit 1
}

if ($PreflightOnly.IsPresent) {
    $report = [ordered]@{
        generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
        status = "ready"
        blocker = ""
        preflightOnly = $true
        recipientEmail = $RecipientEmail
        expectedPublicBaseUrl = $ExpectedPublicBaseUrl
        sendingDomain = $SendingDomain
        readinessReport = if ($null -eq $latestReadinessReport) { "" } else { $latestReadinessReport.FullName }
        messages = @()
    }
    Write-SmokeReport -JsonPath $jsonPath -MarkdownPath $markdownPath -Report $report
    Write-Host "Brevo real delivery smoke report: $markdownPath"
    Write-Host "JSON report: $jsonPath"
    Write-Host "Status: ready"
    exit 0
}

if (-not $ConfirmSend.IsPresent) {
    throw "Real delivery smoke requires -ConfirmSend. This prevents accidental production email sends."
}

if (-not (Test-Email -Value $RecipientEmail)) {
    throw "RecipientEmail must be a syntactically valid real inbox address."
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

$email = Get-PropertyValue -Object $config -Name "TransactionalEmail"
if ($null -eq $email) {
    throw "TransactionalEmail section is missing."
}

$brevoApiBaseUrl = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoApiBaseUrl").ToString().TrimEnd("/")
$brevoApiKey = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoApiKey")
$fromEmail = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "FromEmail")
$fromName = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "FromName")
$replyToEmail = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "ReplyToEmail")
$publicBaseUrl = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "PublicBaseUrl").ToString().TrimEnd("/")

$messages = @(
    (New-SmokeMessage `
        -ScenarioKey "Account.EmailConfirmation" `
        -TemplateKey "account.email-confirmation" `
        -Subject "Darwin Lingua email confirmation smoke" `
        -ActionLabel "Open confirmation smoke link" `
        -ActionUrl "$publicBaseUrl/Identity/Account/ConfirmEmail?smoke=brevo-confirmation" `
        -PlainText "This controlled smoke message verifies real Brevo delivery for account confirmation."),
    (New-SmokeMessage `
        -ScenarioKey "Account.PasswordReset" `
        -TemplateKey "account.password-reset" `
        -Subject "Darwin Lingua password reset smoke" `
        -ActionLabel "Open password reset smoke link" `
        -ActionUrl "$publicBaseUrl/Identity/Account/ResetPassword?smoke=brevo-password-reset" `
        -PlainText "This controlled smoke message verifies real Brevo delivery for password reset.")
)

$sentMessages = New-Object System.Collections.Generic.List[object]
foreach ($message in $messages) {
    $headers = @{
        "api-key" = $brevoApiKey.ToString()
        "accept" = "application/json"
    }
    $payload = [ordered]@{
        sender = [ordered]@{
            email = $fromEmail
            name = $fromName
        }
        to = @(
            [ordered]@{
                email = $RecipientEmail
            }
        )
        replyTo = [ordered]@{
            email = $replyToEmail
        }
        subject = $message.subject
        textContent = $message.textContent
        htmlContent = $message.htmlContent
        tags = @("darwinlingua", "real-delivery-smoke", ($message.scenarioKey -replace '[^A-Za-z0-9]+', '-').Trim('-').ToLowerInvariant())
        headers = [ordered]@{
            "X-DarwinLingua-Scenario" = $message.scenarioKey
            "X-DarwinLingua-Template" = $message.templateKey
            "X-DarwinLingua-CorrelationId" = "brevo-smoke-$timestamp"
            "X-DarwinLingua-Smoke" = "true"
        }
    }

    $body = $payload | ConvertTo-Json -Depth 8
    $response = Invoke-RestMethod -Uri "$brevoApiBaseUrl/v3/smtp/email" -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 30
    $sentMessages.Add([ordered]@{
        scenarioKey = $message.scenarioKey
        templateKey = $message.templateKey
        sent = $true
        providerMessageId = if ($null -eq $response.messageId) { "" } else { $response.messageId }
    })
}

$finalReport = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    status = "sent"
    blocker = ""
    preflightOnly = $false
    recipientEmail = $RecipientEmail
    expectedPublicBaseUrl = $ExpectedPublicBaseUrl
    sendingDomain = $SendingDomain
    readinessReport = if ($null -eq $latestReadinessReport) { "" } else { $latestReadinessReport.FullName }
    messages = $sentMessages
}

Write-SmokeReport -JsonPath $jsonPath -MarkdownPath $markdownPath -Report $finalReport
Write-Host "Brevo real delivery smoke report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Status: sent"
