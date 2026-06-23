[CmdletBinding()]
param(
    [string]$WebBaseUrl = "https://darwinlingua.com",
    [string]$RegistrationEmailPrefix = "shahramvafadar+darwinlingua-link-smoke",
    [string]$RegistrationEmailDomain = "gmail.com",
    [string]$OutputDirectory = "artifacts/validation/web-account-email-link-smoke",
    [string]$ConfigPath = "src\Apps\DarwinLingua.Web\appsettings.Development.Local.json",
    [string]$ProjectPath = "src\Apps\DarwinLingua.Web\DarwinLingua.Web.csproj",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresUser = "postgres"
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

function Get-AntiForgeryToken {
    param([string]$Html)

    $match = [regex]::Match(
        $Html,
        'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"|type="hidden"\s+value="([^"]+)"\s+name="__RequestVerificationToken"',
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if (-not $match.Success) {
        throw "Anti-forgery token was not found in the page."
    }

    if ($match.Groups[1].Success) {
        return $match.Groups[1].Value
    }

    return $match.Groups[2].Value
}

function Invoke-FormPost {
    param(
        [Microsoft.PowerShell.Commands.WebRequestSession]$Session,
        [string]$Url,
        [hashtable]$Body
    )

    try {
        return Invoke-WebRequest `
            -Uri $Url `
            -Method Post `
            -WebSession $Session `
            -Body $Body `
            -MaximumRedirection 5 `
            -SkipHttpErrorCheck
    }
    catch {
        $responseProperty = $_.Exception.PSObject.Properties["Response"]
        if ($null -ne $responseProperty -and $null -ne $responseProperty.Value) {
            return $responseProperty.Value
        }

        throw
    }
}

function Invoke-ScalarPsql {
    param([string]$Sql)

    $result = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql scalar query failed."
    }

    return ($result | Out-String).Trim()
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

function ConvertTo-PostgresSqlLiteral {
    param([string]$Value)

    return "'" + $Value.Replace("'", "''") + "'"
}

function ConvertTo-Sha256 {
    param([string]$Value)

    return [Convert]::ToHexString([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($Value))).ToLowerInvariant()
}

function Get-MessageIdPreview {
    param([string]$MessageId)

    if ([string]::IsNullOrWhiteSpace($MessageId)) {
        return ""
    }

    if ($MessageId.Length -le 20) {
        return $MessageId
    }

    return "$($MessageId.Substring(0, 10))...$($MessageId.Substring($MessageId.Length - 6))"
}

function Get-BrevoSentEmailContent {
    param(
        [string]$MessageId,
        [string]$RecipientEmail,
        [string]$BrevoApiBaseUrl,
        [hashtable]$Headers
    )

    $normalizedMessageId = $MessageId.Trim()
    $encodedMessageId = [System.Uri]::EscapeDataString($normalizedMessageId)
    $encodedRecipientEmail = if ([string]::IsNullOrWhiteSpace($RecipientEmail)) { "" } else { [System.Uri]::EscapeDataString($RecipientEmail.Trim()) }
    $uuid = $null
    $lastShape = ""
    for ($attempt = 1; $attempt -le 12 -and [string]::IsNullOrWhiteSpace($uuid); $attempt++) {
        if ($attempt -gt 1) {
            Start-Sleep -Seconds 5
        }

        $listUri = "$($BrevoApiBaseUrl.TrimEnd('/'))/v3/smtp/emails?messageId=$encodedMessageId&limit=1"
        $listResponse = Invoke-WebRequest -Uri $listUri -Headers $Headers -Method Get -TimeoutSec 30
        $listBody = [string]$listResponse.Content
        if (-not [string]::IsNullOrWhiteSpace($listBody)) {
            $list = $listBody | ConvertFrom-Json
            $topProperties = @($list.PSObject.Properties | ForEach-Object { $_.Name })
            $first = $null
            if ($list.PSObject.Properties["transactionalEmails"] -and $null -ne $list.transactionalEmails) {
                $first = @($list.transactionalEmails) | Select-Object -First 1
            }
            elseif ($list.PSObject.Properties["emails"] -and $null -ne $list.emails) {
                $first = @($list.emails) | Select-Object -First 1
            }

            $firstProperties = if ($null -ne $first) { @($first.PSObject.Properties | ForEach-Object { $_.Name }) } else { @() }
            $countValue = Get-PropertyValue -Object $list -Name "count"
            $lastShape = "filter=messageId; http=$($listResponse.StatusCode); bodyLength=$($listBody.Length); count=$countValue; top=$($topProperties -join ','); first=$($firstProperties -join ',')"
            $uuid = if ($null -ne $first) { [string](Get-PropertyValue -Object $first -Name "uuid") } else { [string](Get-PropertyValue -Object $list -Name "uuid") }
        }
        else {
            $lastShape = "filter=messageId; http=$($listResponse.StatusCode); bodyLength=0"
        }

        if (-not [string]::IsNullOrWhiteSpace($uuid) -or [string]::IsNullOrWhiteSpace($encodedRecipientEmail)) {
            continue
        }

        $emailResponse = Invoke-WebRequest -Uri "$($BrevoApiBaseUrl.TrimEnd('/'))/v3/smtp/emails?email=$encodedRecipientEmail&limit=20" -Headers $Headers -Method Get -TimeoutSec 30
        $emailBody = [string]$emailResponse.Content
        if ([string]::IsNullOrWhiteSpace($emailBody)) {
            $lastShape = "$lastShape; fallback=email; http=$($emailResponse.StatusCode); bodyLength=0"
            continue
        }

        $list = $emailBody | ConvertFrom-Json
        $topProperties = @($list.PSObject.Properties | ForEach-Object { $_.Name })
        $first = $null
        if ($list.PSObject.Properties["transactionalEmails"] -and $null -ne $list.transactionalEmails) {
            $emails = @($list.transactionalEmails)
            $first = $emails | Where-Object {
                ([string](Get-PropertyValue -Object $_ -Name "messageId")).Trim() -eq $normalizedMessageId
            } | Select-Object -First 1
        }
        elseif ($list.PSObject.Properties["emails"] -and $null -ne $list.emails) {
            $emails = @($list.emails)
            $first = $emails | Where-Object {
                ([string](Get-PropertyValue -Object $_ -Name "messageId")).Trim() -eq $normalizedMessageId
            } | Select-Object -First 1
        }

        $firstProperties = if ($null -ne $first) { @($first.PSObject.Properties | ForEach-Object { $_.Name }) } else { @() }
        $countValue = Get-PropertyValue -Object $list -Name "count"
        $lastShape = "$lastShape; fallback=email; http=$($emailResponse.StatusCode); bodyLength=$($emailBody.Length); count=$countValue; top=$($topProperties -join ','); matched=$($null -ne $first); first=$($firstProperties -join ',')"
        $uuid = if ($null -ne $first) { [string](Get-PropertyValue -Object $first -Name "uuid") } else { $null }
    }

    if ([string]::IsNullOrWhiteSpace($uuid)) {
        throw "Brevo did not return a sent-email UUID for the provider message id. Response shape: $lastShape"
    }

    $encodedUuid = [System.Uri]::EscapeDataString($uuid)
    $content = Invoke-RestMethod -Uri "$($BrevoApiBaseUrl.TrimEnd('/'))/v3/smtp/emails/$encodedUuid" -Headers $Headers -Method Get -TimeoutSec 30
    return [ordered]@{
        uuid = $uuid
        body = [string]$content.body
        subject = [string]$content.subject
    }
}

function Get-LinkFromEmailBody {
    param(
        [string]$Body,
        [string]$PathFragment
    )

    $decodedBody = [System.Net.WebUtility]::HtmlDecode($Body)
    $matches = [regex]::Matches($decodedBody, 'href\s*=\s*["'']([^"'']+)["'']', [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    foreach ($match in $matches) {
        $href = [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
        if ($href.Contains($PathFragment, [StringComparison]::OrdinalIgnoreCase)) {
            return $href
        }

        $handler = [System.Net.Http.HttpClientHandler]::new()
        $handler.AllowAutoRedirect = $false
        $client = [System.Net.Http.HttpClient]::new($handler)
        try {
            $client.Timeout = [TimeSpan]::FromSeconds(30)
            $redirectResponse = $client.GetAsync($href).GetAwaiter().GetResult()
            $location = if ($null -ne $redirectResponse.Headers.Location) { [string]$redirectResponse.Headers.Location } else { "" }
            if (-not [string]::IsNullOrWhiteSpace($location) -and $location.Contains($PathFragment, [StringComparison]::OrdinalIgnoreCase)) {
                return $location
            }
        }
        catch {
            continue
        }
        finally {
            $client.Dispose()
            $handler.Dispose()
        }
    }

    $pathPattern = [regex]::Escape($PathFragment)
    $plainUrlMatches = [regex]::Matches(
        $decodedBody,
        "https?://[^\s<>`"'']*$pathPattern[^\s<>`"'']*",
        [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    foreach ($match in $plainUrlMatches) {
        $candidate = [System.Net.WebUtility]::HtmlDecode($match.Value).Trim()
        if ($candidate.Contains($PathFragment, [StringComparison]::OrdinalIgnoreCase)) {
            return $candidate
        }
    }

    throw "Expected email action link containing '$PathFragment' was not found."
}

function Get-LatestDeliveryLog {
    param(
        [string]$ScenarioKey,
        [string]$RunStartedAtUtc
    )

    $scenarioSql = ConvertTo-PostgresSqlLiteral -Value $ScenarioKey
    $startedSql = ConvertTo-PostgresSqlLiteral -Value $RunStartedAtUtc
    $sql = @"
select coalesce(json_agg(row_to_json(t)), '[]'::json)
from (
    select "Id", "ScenarioKey", "Status", "ProviderName", "ProviderMessageId", "ProviderLastEvent", "CreatedAtUtc"
    from "WebEmailDeliveryLogs"
    where "CreatedAtUtc" >= ${startedSql}::timestamptz
      and "ScenarioKey" = $scenarioSql
    order by "CreatedAtUtc" desc
    limit 1
) t;
"@
    $logs = @(Invoke-JsonPsql -Sql $sql)
    if ($logs.Count -eq 0) {
        throw "No delivery log was found for scenario $ScenarioKey."
    }

    if ([string]::IsNullOrWhiteSpace([string]$logs[0].ProviderMessageId)) {
        throw "Delivery log for scenario $ScenarioKey does not have a provider message id."
    }

    return $logs[0]
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

if ([string]::IsNullOrWhiteSpace($brevoApiKey) -or $brevoApiKey.Contains("CHANGE_ME", [StringComparison]::OrdinalIgnoreCase)) {
    throw "Brevo API key is not configured. The key value was not printed."
}

$outputRoot = Resolve-RepositoryPath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputRoot -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$jsonPath = Join-Path $outputRoot "web-account-email-link-smoke-$timestamp.json"
$markdownPath = Join-Path $outputRoot "web-account-email-link-smoke-$timestamp.md"

$normalizedWebBaseUrl = $WebBaseUrl.TrimEnd("/")
$runStartedAtUtc = (Get-Date).ToUniversalTime()
$runStartedAtText = $runStartedAtUtc.ToString("O")
$safeRunId = $timestamp.ToLowerInvariant()
$registrationEmail = "$RegistrationEmailPrefix-$safeRunId@$RegistrationEmailDomain"
$changedEmail = "$RegistrationEmailPrefix-$safeRunId-changed@$RegistrationEmailDomain"
$initialPassword = "DarwinLinkSmoke-$timestamp!"
$resetPassword = "DarwinLinkSmoke-Reset-$timestamp!"
$headers = @{
    "api-key" = $brevoApiKey
    "accept" = "application/json"
}

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$registerPage = Invoke-WebRequest -Uri "$normalizedWebBaseUrl/Identity/Account/Register" -WebSession $session -MaximumRedirection 5
$registerToken = Get-AntiForgeryToken -Html $registerPage.Content
$registerResponse = Invoke-FormPost -Session $session -Url "$normalizedWebBaseUrl/Identity/Account/Register" -Body @{
    "__RequestVerificationToken" = $registerToken
    "Input.Email" = $registrationEmail
    "Input.Password" = $initialPassword
    "Input.ConfirmPassword" = $initialPassword
    "Input.AcceptTermsOfUse" = "true"
    "Input.AcknowledgePrivacyNotice" = "true"
    "ReturnUrl" = ""
}

Start-Sleep -Seconds 3
$registrationLog = Get-LatestDeliveryLog -ScenarioKey "Account.EmailConfirmation" -RunStartedAtUtc $runStartedAtText
$resendStartedAtUtc = (Get-Date).ToUniversalTime()
$resendStartedAtText = $resendStartedAtUtc.ToString("O")
$resendPage = Invoke-WebRequest -Uri "$normalizedWebBaseUrl/Identity/Account/ResendEmailConfirmation" -WebSession $session -MaximumRedirection 5
$resendToken = Get-AntiForgeryToken -Html $resendPage.Content
$resendResponse = Invoke-FormPost -Session $session -Url "$normalizedWebBaseUrl/Identity/Account/ResendEmailConfirmation" -Body @{
    "__RequestVerificationToken" = $resendToken
    "Input.Email" = $registrationEmail
}

Start-Sleep -Seconds 3
$resendConfirmationLog = Get-LatestDeliveryLog -ScenarioKey "Account.EmailConfirmation" -RunStartedAtUtc $resendStartedAtText
$resendConfirmationContent = Get-BrevoSentEmailContent -MessageId ([string]$resendConfirmationLog.ProviderMessageId) -RecipientEmail $registrationEmail -BrevoApiBaseUrl $brevoApiBaseUrl -Headers $headers
$confirmationLink = Get-LinkFromEmailBody -Body $resendConfirmationContent.body -PathFragment "/Identity/Account/ConfirmEmail"
$confirmResponse = Invoke-WebRequest -Uri $confirmationLink -WebSession $session -MaximumRedirection 5 -SkipHttpErrorCheck

$registrationEmailSql = ConvertTo-PostgresSqlLiteral -Value $registrationEmail
$emailConfirmed = Invoke-ScalarPsql -Sql "select coalesce((select ""EmailConfirmed""::text from ""AspNetUsers"" where lower(""Email"") = lower($registrationEmailSql) limit 1), 'false');"

$forgotPage = Invoke-WebRequest -Uri "$normalizedWebBaseUrl/Identity/Account/ForgotPassword" -WebSession $session -MaximumRedirection 5
$forgotToken = Get-AntiForgeryToken -Html $forgotPage.Content
$forgotResponse = Invoke-FormPost -Session $session -Url "$normalizedWebBaseUrl/Identity/Account/ForgotPassword" -Body @{
    "__RequestVerificationToken" = $forgotToken
    "Input.Email" = $registrationEmail
}

Start-Sleep -Seconds 3
$resetLog = Get-LatestDeliveryLog -ScenarioKey "Account.PasswordReset" -RunStartedAtUtc $runStartedAtText
$resetContent = Get-BrevoSentEmailContent -MessageId ([string]$resetLog.ProviderMessageId) -RecipientEmail $registrationEmail -BrevoApiBaseUrl $brevoApiBaseUrl -Headers $headers
$resetLink = Get-LinkFromEmailBody -Body $resetContent.body -PathFragment "/Identity/Account/ResetPassword"
$resetPage = Invoke-WebRequest -Uri $resetLink -WebSession $session -MaximumRedirection 5
$resetToken = Get-AntiForgeryToken -Html $resetPage.Content
$resetUri = [Uri]$resetLink
$resetQuery = [System.Web.HttpUtility]::ParseQueryString($resetUri.Query)
$resetCode = $resetQuery["code"]
$resetEmail = $resetQuery["email"]
if ([string]::IsNullOrWhiteSpace($resetCode)) {
    throw "Reset link did not contain a code query parameter."
}

$resetResponse = Invoke-FormPost -Session $session -Url "$normalizedWebBaseUrl/Identity/Account/ResetPassword" -Body @{
    "__RequestVerificationToken" = $resetToken
    "Input.Email" = if ([string]::IsNullOrWhiteSpace($resetEmail)) { $registrationEmail } else { $resetEmail }
    "Input.Password" = $resetPassword
    "Input.ConfirmPassword" = $resetPassword
    "Input.Code" = $resetCode
}

Start-Sleep -Seconds 3
$resetCompletedLog = Get-LatestDeliveryLog -ScenarioKey "Account.PasswordResetCompleted" -RunStartedAtUtc $runStartedAtText

$loginPage = Invoke-WebRequest -Uri "$normalizedWebBaseUrl/Identity/Account/Login" -WebSession $session -MaximumRedirection 5
$loginToken = Get-AntiForgeryToken -Html $loginPage.Content
$loginResponse = Invoke-FormPost -Session $session -Url "$normalizedWebBaseUrl/Identity/Account/Login" -Body @{
    "__RequestVerificationToken" = $loginToken
    "Input.Email" = $registrationEmail
    "Input.Password" = $resetPassword
    "Input.RememberMe" = "false"
    "ReturnUrl" = "/"
}

$manageEmailPage = Invoke-WebRequest -Uri "$normalizedWebBaseUrl/Identity/Account/Manage/Email" -WebSession $session -MaximumRedirection 5
$manageEmailToken = Get-AntiForgeryToken -Html $manageEmailPage.Content
$changeEmailResponse = Invoke-FormPost -Session $session -Url "$normalizedWebBaseUrl/Identity/Account/Manage/Email" -Body @{
    "__RequestVerificationToken" = $manageEmailToken
    "Input.NewEmail" = $changedEmail
    "Input.CurrentPassword" = $resetPassword
}

Start-Sleep -Seconds 3
$changeConfirmationLog = Get-LatestDeliveryLog -ScenarioKey "Account.EmailChangeConfirmation" -RunStartedAtUtc $runStartedAtText
$changeContent = Get-BrevoSentEmailContent -MessageId ([string]$changeConfirmationLog.ProviderMessageId) -RecipientEmail $changedEmail -BrevoApiBaseUrl $brevoApiBaseUrl -Headers $headers
$changeLink = Get-LinkFromEmailBody -Body $changeContent.body -PathFragment "/Identity/Account/ConfirmEmailChange"
$changeConfirmResponse = Invoke-WebRequest -Uri $changeLink -WebSession $session -MaximumRedirection 5 -SkipHttpErrorCheck

Start-Sleep -Seconds 3
$changedEmailSql = ConvertTo-PostgresSqlLiteral -Value $changedEmail
$emailChanged = Invoke-ScalarPsql -Sql "select count(*) from ""AspNetUsers"" where lower(""Email"") = lower($changedEmailSql) and ""EmailConfirmed"" = true;"
$emailChangedNotificationLog = Get-LatestDeliveryLog -ScenarioKey "Account.EmailChangedNotification" -RunStartedAtUtc $runStartedAtText

$linkHashes = [ordered]@{
    confirmation = (ConvertTo-Sha256 -Value $confirmationLink).Substring(0, 16)
    passwordReset = (ConvertTo-Sha256 -Value $resetLink).Substring(0, 16)
    emailChange = (ConvertTo-Sha256 -Value $changeLink).Substring(0, 16)
}

$checks = [ordered]@{
    registrationHttpStatus = [int]$registerResponse.StatusCode
    resendConfirmationHttpStatus = [int]$resendResponse.StatusCode
    resendConfirmationLogged = -not [string]::IsNullOrWhiteSpace([string]$resendConfirmationLog.ProviderMessageId)
    confirmationLinkResolved = $confirmResponse.StatusCode -ge 200 -and $confirmResponse.StatusCode -lt 400
    emailConfirmed = $emailConfirmed -eq "true"
    forgotPasswordHttpStatus = [int]$forgotResponse.StatusCode
    passwordResetLinkResolved = $resetResponse.StatusCode -ge 200 -and $resetResponse.StatusCode -lt 400
    passwordResetCompletedLogged = -not [string]::IsNullOrWhiteSpace([string]$resetCompletedLog.ProviderMessageId)
    loginAfterResetHttpStatus = [int]$loginResponse.StatusCode
    changeEmailRequestHttpStatus = [int]$changeEmailResponse.StatusCode
    emailChangeLinkResolved = $changeConfirmResponse.StatusCode -ge 200 -and $changeConfirmResponse.StatusCode -lt 400
    emailChanged = $emailChanged -eq "1"
    emailChangedNotificationLogged = -not [string]::IsNullOrWhiteSpace([string]$emailChangedNotificationLog.ProviderMessageId)
}

$logs = @(
    [ordered]@{ scenario = "Account.EmailConfirmation.Register"; messageIdPreview = Get-MessageIdPreview -MessageId ([string]$registrationLog.ProviderMessageId); messageIdSha256 = (ConvertTo-Sha256 -Value ([string]$registrationLog.ProviderMessageId)).Substring(0, 16) },
    [ordered]@{ scenario = "Account.EmailConfirmation.Resend"; messageIdPreview = Get-MessageIdPreview -MessageId ([string]$resendConfirmationLog.ProviderMessageId); messageIdSha256 = (ConvertTo-Sha256 -Value ([string]$resendConfirmationLog.ProviderMessageId)).Substring(0, 16) },
    [ordered]@{ scenario = "Account.PasswordReset"; messageIdPreview = Get-MessageIdPreview -MessageId ([string]$resetLog.ProviderMessageId); messageIdSha256 = (ConvertTo-Sha256 -Value ([string]$resetLog.ProviderMessageId)).Substring(0, 16) },
    [ordered]@{ scenario = "Account.PasswordResetCompleted"; messageIdPreview = Get-MessageIdPreview -MessageId ([string]$resetCompletedLog.ProviderMessageId); messageIdSha256 = (ConvertTo-Sha256 -Value ([string]$resetCompletedLog.ProviderMessageId)).Substring(0, 16) },
    [ordered]@{ scenario = "Account.EmailChangeConfirmation"; messageIdPreview = Get-MessageIdPreview -MessageId ([string]$changeConfirmationLog.ProviderMessageId); messageIdSha256 = (ConvertTo-Sha256 -Value ([string]$changeConfirmationLog.ProviderMessageId)).Substring(0, 16) },
    [ordered]@{ scenario = "Account.EmailChangedNotification"; messageIdPreview = Get-MessageIdPreview -MessageId ([string]$emailChangedNotificationLog.ProviderMessageId); messageIdSha256 = (ConvertTo-Sha256 -Value ([string]$emailChangedNotificationLog.ProviderMessageId)).Substring(0, 16) }
)

$passed = $true
foreach ($check in $checks.GetEnumerator()) {
    if ($check.Value -ne $true -and $check.Key -notlike "*HttpStatus") {
        $passed = $false
    }
}

$report = [ordered]@{
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    webBaseUrl = $normalizedWebBaseUrl
    testAccountHash = (ConvertTo-Sha256 -Value $registrationEmail).Substring(0, 16)
    changedAccountHash = (ConvertTo-Sha256 -Value $changedEmail).Substring(0, 16)
    passed = $passed
    checks = $checks
    linkHashes = $linkHashes
    logs = $logs
    officialBrevoEmailContentReference = "https://developers.brevo.com/reference/get-transac-email-content"
}

$report | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $jsonPath -Encoding UTF8

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add("# Web Account Email Link Smoke")
$lines.Add("")
$lines.Add("- Generated: $($report.generatedAtUtc)")
$lines.Add("- Web base URL: $normalizedWebBaseUrl")
$lines.Add("- Test account hash: $($report.testAccountHash)")
$lines.Add("- Changed account hash: $($report.changedAccountHash)")
$lines.Add("- Passed: $passed")
$lines.Add("- Official Brevo sent-email content API: $($report.officialBrevoEmailContentReference)")
$lines.Add("")
$lines.Add("| Check | Value |")
$lines.Add("| --- | --- |")
foreach ($check in $checks.GetEnumerator()) {
    $lines.Add("| $($check.Key) | $($check.Value) |")
}
$lines.Add("")
$lines.Add("| Scenario | Provider message id hash |")
$lines.Add("| --- | --- |")
foreach ($log in $logs) {
    $lines.Add("| $($log.scenario) | $($log.messageIdSha256)... |")
}
$lines.Add("")
$lines.Add("No full action URLs, email tokens, reset codes, Brevo API keys, webhook secrets, or full provider message ids are written to this report.")
$lines | Set-Content -LiteralPath $markdownPath -Encoding UTF8

Write-Host "Web account email link smoke report: $markdownPath"
Write-Host "JSON report: $jsonPath"
Write-Host "Passed: $passed"

if (-not $passed) {
    exit 1
}
