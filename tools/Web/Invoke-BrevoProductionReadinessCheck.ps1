[CmdletBinding()]
param(
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$ProjectPath = "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj",
    [string]$OutputDirectory = "artifacts/validation/brevo-readiness",
    [string]$ExpectedPublicBaseUrl = "https://darwinlingua.com",
    [string]$SendingDomain,
    [switch]$SenderVerified,
    [switch]$DnsAuthenticated,
    [switch]$WebhookConfigured,
    [switch]$DpaAccepted,
    [switch]$RequireRealDelivery,
    [switch]$VerifyBrevoApi,
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

function New-Check {
    param(
        [string]$Key,
        [string]$Status,
        [string]$Message,
        [string]$Evidence = ""
    )

    [ordered]@{
        key = $Key
        status = $Status
        message = $Message
        evidence = $Evidence
    }
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

function Test-Email {
    param([object]$Value)

    if ($null -eq $Value) {
        return $false
    }

    return $Value.ToString().Trim() -match '^[^@\s]+@[^@\s]+\.[^@\s]+$'
}

function Get-EmailDomain {
    param([object]$Value)

    if (-not (Test-Email -Value $Value)) {
        return ""
    }

    return ($Value.ToString().Trim() -split '@', 2)[1].ToLowerInvariant()
}

$resolvedConfigPath = Resolve-RepositoryPath -Path $ConfigPath
if (-not (Test-Path -LiteralPath $resolvedConfigPath -PathType Leaf)) {
    throw "Config file was not found: $resolvedConfigPath"
}

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null

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

$checks = New-Object System.Collections.Generic.List[object]

if ($null -eq $email) {
    $checks.Add((New-Check -Key "config.section" -Status "blocker" -Message "TransactionalEmail section is missing." -Evidence $resolvedConfigPath))
}
else {
    $mode = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "Mode")
    $publicBaseUrl = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "PublicBaseUrl")
    $fromEmail = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "FromEmail")
    $replyToEmail = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "ReplyToEmail")
    $supportEmail = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "SupportEmail")
    $brevoApiBaseUrl = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoApiBaseUrl")
    $brevoApiKey = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoApiKey")
    $brevoWebhookSecret = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoWebhookSecret")
    $brevoSandboxMode = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoSandboxMode")
    $brevoAllowQuerySecretFallback = (Get-ConfigurationValue -SectionObject $email -Secrets $userSecrets -Section "TransactionalEmail" -Name "BrevoAllowQuerySecretFallback")

    if ($mode -eq "BrevoApi") {
        $checks.Add((New-Check -Key "config.mode" -Status "pass" -Message "TransactionalEmail mode is BrevoApi."))
    }
    else {
        $checks.Add((New-Check -Key "config.mode" -Status "blocker" -Message "TransactionalEmail:Mode must be BrevoApi before Brevo production sending." -Evidence "Current value: $mode"))
    }

    $parsedPublicBaseUri = $null
    if ([Uri]::TryCreate([string]$publicBaseUrl, [UriKind]::Absolute, [ref]$parsedPublicBaseUri) -and $publicBaseUrl.ToString().StartsWith("https://", [StringComparison]::OrdinalIgnoreCase)) {
        $status = if ($publicBaseUrl -eq $ExpectedPublicBaseUrl) { "pass" } else { "warning" }
        $message = if ($status -eq "pass") { "PublicBaseUrl matches the expected public web origin." } else { "PublicBaseUrl is HTTPS but does not match the expected public web origin; confirm this was intentional." }
        $checks.Add((New-Check -Key "config.publicBaseUrl" -Status $status -Message $message -Evidence "Current value: $publicBaseUrl"))
    }
    else {
        $checks.Add((New-Check -Key "config.publicBaseUrl" -Status "blocker" -Message "TransactionalEmail:PublicBaseUrl must be an HTTPS absolute URL." -Evidence "Current value: $publicBaseUrl"))
    }

    $parsedBrevoApiBaseUri = $null
    if ([Uri]::TryCreate([string]$brevoApiBaseUrl, [UriKind]::Absolute, [ref]$parsedBrevoApiBaseUri) -and $brevoApiBaseUrl.ToString().StartsWith("https://", [StringComparison]::OrdinalIgnoreCase)) {
        $checks.Add((New-Check -Key "config.brevoApiBaseUrl" -Status "pass" -Message "Brevo API base URL is HTTPS." -Evidence "Current value: $brevoApiBaseUrl"))
    }
    else {
        $checks.Add((New-Check -Key "config.brevoApiBaseUrl" -Status "blocker" -Message "TransactionalEmail:BrevoApiBaseUrl must be HTTPS." -Evidence "Current value: $brevoApiBaseUrl"))
    }

    foreach ($emailCheck in @(
            @{ Key = "config.fromEmail"; Name = "FromEmail"; Value = $fromEmail },
            @{ Key = "config.replyToEmail"; Name = "ReplyToEmail"; Value = $replyToEmail },
            @{ Key = "config.supportEmail"; Name = "SupportEmail"; Value = $supportEmail }
        )) {
        if (Test-Email -Value $emailCheck.Value) {
            $checks.Add((New-Check -Key $emailCheck.Key -Status "pass" -Message "$($emailCheck.Name) is a syntactically valid email address."))
        }
        else {
            $checks.Add((New-Check -Key $emailCheck.Key -Status "blocker" -Message "$($emailCheck.Name) must be configured as a real email address." -Evidence "Current value: $($emailCheck.Value)"))
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($SendingDomain)) {
        $fromDomain = Get-EmailDomain -Value $fromEmail
        if ($fromDomain -eq $SendingDomain.ToLowerInvariant()) {
            $checks.Add((New-Check -Key "config.fromDomain" -Status "pass" -Message "FromEmail uses the expected sending domain." -Evidence $SendingDomain))
        }
        else {
            $checks.Add((New-Check -Key "config.fromDomain" -Status "blocker" -Message "FromEmail must use the Brevo-verified sending domain." -Evidence "Expected: $SendingDomain; current: $fromDomain"))
        }
    }

    if (Test-ConfiguredSecret -Value $brevoApiKey) {
        $checks.Add((New-Check -Key "secret.apiKey" -Status "pass" -Message "Brevo API key appears configured. Secret value was not printed."))
    }
    else {
        $checks.Add((New-Check -Key "secret.apiKey" -Status "blocker" -Message "Brevo API key is missing or still a placeholder."))
    }

    if ($VerifyBrevoApi.IsPresent) {
        if ((Test-ConfiguredSecret -Value $brevoApiKey) -and
            [Uri]::TryCreate([string]$brevoApiBaseUrl, [UriKind]::Absolute, [ref]$parsedBrevoApiBaseUri) -and
            $brevoApiBaseUrl.ToString().StartsWith("https://", [StringComparison]::OrdinalIgnoreCase)) {
            try {
                $accountUri = "$($brevoApiBaseUrl.ToString().TrimEnd('/'))/v3/account"
                $response = Invoke-WebRequest -Uri $accountUri -Method Get -Headers @{
                    "api-key" = $brevoApiKey.ToString()
                    "accept" = "application/json"
                } -UseBasicParsing -TimeoutSec 30
                $checks.Add((New-Check -Key "brevo.accountApi" -Status "pass" -Message "Brevo account API is reachable from this host with the configured API key." -Evidence "HTTP $($response.StatusCode)"))
            }
            catch {
                $statusCode = ""
                $body = ""
                if ($null -ne $_.Exception.Response) {
                    $statusCode = [int]$_.Exception.Response.StatusCode
                    if (-not [string]::IsNullOrWhiteSpace($_.ErrorDetails.Message)) {
                        $body = $_.ErrorDetails.Message
                    }
                    elseif ($_.Exception.Response -is [System.Net.Http.HttpResponseMessage]) {
                        try {
                            $body = $_.Exception.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                        }
                        catch {
                            $body = ""
                        }
                    }
                    else {
                        $stream = $_.Exception.Response.GetResponseStream()
                        if ($null -ne $stream) {
                            $reader = [System.IO.StreamReader]::new($stream)
                            $body = $reader.ReadToEnd()
                        }
                    }
                }

                $evidence = if ([string]::IsNullOrWhiteSpace($body)) {
                    $_.Exception.Message
                }
                else {
                    "HTTP $statusCode. $body"
                }

                $message = if ($evidence.Contains("unrecognised IP address", [StringComparison]::OrdinalIgnoreCase)) {
                    "Brevo rejected this host because its IP address is not authorized. Add the current server/operator IP in Brevo Authorized IPs at https://app.brevo.com/security/authorised_ips before real delivery smoke."
                }
                else {
                    "Brevo account API could not be reached with the configured API key from this host."
                }

                $checks.Add((New-Check -Key "brevo.accountApi" -Status "blocker" -Message $message -Evidence $evidence))
            }
        }
        else {
            $checks.Add((New-Check -Key "brevo.accountApi" -Status "blocker" -Message "Brevo API live verification was requested, but BrevoApiBaseUrl or BrevoApiKey is not configured."))
        }
    }
    else {
        $checks.Add((New-Check -Key "brevo.accountApi" -Status "warning" -Message "Brevo live API verification was skipped; rerun with -VerifyBrevoApi before real inbox/webhook smoke."))
    }

    if ((Test-ConfiguredSecret -Value $brevoWebhookSecret) -and $brevoWebhookSecret.ToString().Length -ge 32) {
        $checks.Add((New-Check -Key "secret.webhookSecret" -Status "pass" -Message "Brevo webhook secret appears configured and long enough. Secret value was not printed."))
    }
    else {
        $checks.Add((New-Check -Key "secret.webhookSecret" -Status "blocker" -Message "Brevo webhook secret is missing, a placeholder, or shorter than 32 characters."))
    }

    if ($RequireRealDelivery.IsPresent) {
        if ($brevoSandboxMode -eq $false) {
            $checks.Add((New-Check -Key "config.sandbox" -Status "pass" -Message "Brevo sandbox mode is disabled for real delivery."))
        }
        else {
            $checks.Add((New-Check -Key "config.sandbox" -Status "blocker" -Message "BrevoSandboxMode must be false before real delivery."))
        }
    }
    else {
        $checks.Add((New-Check -Key "config.sandbox" -Status "warning" -Message "Real delivery was not required for this check; rerun with -RequireRealDelivery before sending real emails."))
    }

    if ($brevoAllowQuerySecretFallback -eq $false) {
        $checks.Add((New-Check -Key "config.querySecretFallback" -Status "pass" -Message "Brevo query-string secret fallback is disabled."))
    }
    else {
        $checks.Add((New-Check -Key "config.querySecretFallback" -Status "blocker" -Message "BrevoAllowQuerySecretFallback must be false outside local diagnostics."))
    }
}

foreach ($operatorGate in @(
        @{ Key = "operator.senderVerified"; Value = $SenderVerified.IsPresent; Message = "Brevo sender identity is verified." },
        @{ Key = "operator.dnsAuthenticated"; Value = $DnsAuthenticated.IsPresent; Message = "Brevo SPF/DKIM/DMARC domain authentication is complete." },
        @{ Key = "operator.webhookConfigured"; Value = $WebhookConfigured.IsPresent; Message = "Brevo transactional webhook is configured with the Darwin Lingua secret." },
        @{ Key = "operator.dpaAccepted"; Value = $DpaAccepted.IsPresent; Message = "Brevo DPA/data-processing terms have been reviewed and accepted." }
    )) {
    if ($operatorGate.Value) {
        $checks.Add((New-Check -Key $operatorGate.Key -Status "pass" -Message $operatorGate.Message))
    }
    else {
        $checks.Add((New-Check -Key $operatorGate.Key -Status "blocker" -Message "$($operatorGate.Message) This must be confirmed by the operator in Brevo before production sending."))
    }
}

if (-not $SkipDnsLookup.IsPresent -and -not [string]::IsNullOrWhiteSpace($SendingDomain)) {
    try {
        $txtRecords = @(Resolve-DnsName -Name $SendingDomain -Type TXT -ErrorAction Stop)
        $dmarcRecords = @(Resolve-DnsName -Name "_dmarc.$SendingDomain" -Type TXT -ErrorAction Stop)
        $spfFound = $false
        foreach ($record in $txtRecords) {
            $text = ($record.Strings -join "")
            if ($text.StartsWith("v=spf1", [StringComparison]::OrdinalIgnoreCase)) {
                $spfFound = $true
            }
        }

        $dmarcFound = $false
        foreach ($record in $dmarcRecords) {
            $text = ($record.Strings -join "")
            if ($text.StartsWith("v=DMARC1", [StringComparison]::OrdinalIgnoreCase)) {
                $dmarcFound = $true
            }
        }

        $checks.Add((New-Check -Key "dns.spf" -Status $(if ($spfFound) { "pass" } else { "warning" }) -Message $(if ($spfFound) { "SPF TXT record was found." } else { "No SPF TXT record was found by local DNS lookup; verify Brevo DNS status directly." })))
        $checks.Add((New-Check -Key "dns.dmarc" -Status $(if ($dmarcFound) { "pass" } else { "warning" }) -Message $(if ($dmarcFound) { "DMARC TXT record was found." } else { "No DMARC TXT record was found by local DNS lookup; verify Brevo DNS status directly." })))
    }
    catch {
        $checks.Add((New-Check -Key "dns.lookup" -Status "warning" -Message "DNS lookup could not be completed locally; verify DNS in Brevo and with your DNS provider." -Evidence $_.Exception.Message))
    }
}
elseif ($SkipDnsLookup.IsPresent) {
    $checks.Add((New-Check -Key "dns.lookup" -Status "warning" -Message "DNS lookup was skipped; rely on Brevo verified-domain status and DNS-provider records."))
}

$blockers = @($checks | Where-Object { $_.status -eq "blocker" })
$warnings = @($checks | Where-Object { $_.status -eq "warning" })
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$jsonPath = Join-Path $outputRoot "brevo-production-readiness-$timestamp.json"
$markdownPath = Join-Path $outputRoot "brevo-production-readiness-$timestamp.md"

$report = [ordered]@{
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("O")
    configPath = $resolvedConfigPath
    userSecretsPath = $resolvedUserSecretsPath
    userSecretsLoaded = $null -ne $userSecrets
    expectedPublicBaseUrl = $ExpectedPublicBaseUrl
    sendingDomain = $SendingDomain
    requireRealDelivery = $RequireRealDelivery.IsPresent
    verifyBrevoApi = $VerifyBrevoApi.IsPresent
    blockerCount = $blockers.Count
    warningCount = $warnings.Count
    checks = $checks
}

$report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Brevo Production Readiness Check")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Config: $resolvedConfigPath")
$lines.Add("- User secrets loaded: $($report.userSecretsLoaded)")
$lines.Add("- Expected public base URL: $ExpectedPublicBaseUrl")
$lines.Add("- Sending domain: $SendingDomain")
$lines.Add("- Blockers: $($blockers.Count)")
$lines.Add("- Warnings: $($warnings.Count)")
$lines.Add("")
$lines.Add("| Status | Check | Message |")
$lines.Add("| --- | --- | --- |")
foreach ($check in $checks) {
    $lines.Add("| $($check.status) | $($check.key) | $($check.message.Replace('|', '/')) |")
}

$lines.Add("")
if ($blockers.Count -gt 0) {
    $lines.Add("## Result")
    $lines.Add("")
    $lines.Add("Do not enable real Brevo delivery yet. Resolve all blocker checks first.")
}
else {
    $lines.Add("## Result")
    $lines.Add("")
    $lines.Add("No blockers were found. Complete the manual inbox/webhook smoke before inviting testers to self-register.")
}

$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Brevo readiness report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Blockers: $($blockers.Count)"
Write-Host "Warnings: $($warnings.Count)"

if ($blockers.Count -gt 0) {
    exit 1
}
