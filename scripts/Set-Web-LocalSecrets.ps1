param(
    [string] $ProjectPath = "src/Apps/DarwinLingua.Web/DarwinLingua.Web.csproj",
    [switch] $ConfigureBrevo,
    [switch] $ConfigureStripe,
    [switch] $UseEnvironment,
    [string] $PublicBaseUrl = "http://localhost:5192",
    [bool] $BrevoSandboxMode = $true,
    [bool] $BrevoAllowQuerySecretFallback = $false
)

$ErrorActionPreference = "Stop"

function Read-SecretValue {
    param(
        [Parameter(Mandatory = $true)][string] $Prompt,
        [string] $EnvironmentVariable
    )

    if ($UseEnvironment -and -not [string]::IsNullOrWhiteSpace($EnvironmentVariable)) {
        $value = [Environment]::GetEnvironmentVariable($EnvironmentVariable, "Process")
        if ([string]::IsNullOrWhiteSpace($value)) {
            $value = [Environment]::GetEnvironmentVariable($EnvironmentVariable, "User")
        }

        if (-not [string]::IsNullOrWhiteSpace($value)) {
            return $value
        }

        Write-Host "Environment variable $EnvironmentVariable was not set; prompting instead."
    }

    $secure = Read-Host -Prompt $Prompt -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    }
    finally {
        if ($bstr -ne [IntPtr]::Zero) {
            [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
        }
    }
}

function Set-UserSecret {
    param(
        [Parameter(Mandatory = $true)][string] $Key,
        [Parameter(Mandatory = $true)][string] $Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        Write-Host "Skipped $Key because no value was entered."
        return
    }

    if (-not (Test-Path -LiteralPath $script:SecretsDirectory)) {
        New-Item -ItemType Directory -Path $script:SecretsDirectory -Force | Out-Null
    }

    $json = [ordered]@{}
    if (Test-Path -LiteralPath $script:SecretsPath) {
        $existing = Get-Content -LiteralPath $script:SecretsPath -Raw
        if (-not [string]::IsNullOrWhiteSpace($existing)) {
            $parsed = $existing | ConvertFrom-Json
            foreach ($property in $parsed.PSObject.Properties) {
                $json[$property.Name] = $property.Value
            }
        }
    }

    $json[$Key] = $Value
    $json | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $script:SecretsPath -Encoding UTF8
    Write-Host "Set $Key in local user-secrets."
}

if (-not (Test-Path -LiteralPath $ProjectPath)) {
    throw "Project file not found: $ProjectPath"
}

[xml] $projectXml = Get-Content -LiteralPath $ProjectPath
$userSecretsId = ($projectXml.Project.PropertyGroup | ForEach-Object { $_.UserSecretsId } | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -First 1)
if ([string]::IsNullOrWhiteSpace($userSecretsId)) {
    throw "Project does not define UserSecretsId: $ProjectPath"
}

$appData = [Environment]::GetFolderPath("ApplicationData")
if ([string]::IsNullOrWhiteSpace($appData)) {
    throw "Could not resolve the user application-data directory for ASP.NET Core user-secrets."
}

$script:SecretsDirectory = Join-Path $appData "Microsoft/UserSecrets/$userSecretsId"
$script:SecretsPath = Join-Path $script:SecretsDirectory "secrets.json"

if (-not $ConfigureBrevo -and -not $ConfigureStripe) {
    Write-Host "Nothing selected. Use -ConfigureBrevo and/or -ConfigureStripe."
    exit 0
}

if ($ConfigureBrevo) {
    Set-UserSecret "TransactionalEmail:Mode" "BrevoApi"
    Set-UserSecret "TransactionalEmail:PublicBaseUrl" $PublicBaseUrl
    Set-UserSecret "TransactionalEmail:BrevoApiBaseUrl" "https://api.brevo.com"
    Set-UserSecret "TransactionalEmail:BrevoApiKey" (Read-SecretValue "Brevo API key" "DARWINLINGUA_BREVO_API_KEY")
    Set-UserSecret "TransactionalEmail:BrevoWebhookSecret" (Read-SecretValue "Brevo webhook secret" "DARWINLINGUA_BREVO_WEBHOOK_SECRET")
    Set-UserSecret "TransactionalEmail:BrevoSandboxMode" $BrevoSandboxMode.ToString().ToLowerInvariant()
    Set-UserSecret "TransactionalEmail:BrevoAllowQuerySecretFallback" $BrevoAllowQuerySecretFallback.ToString().ToLowerInvariant()
}

if ($ConfigureStripe) {
    Set-UserSecret "Billing:PublicBaseUrl" $PublicBaseUrl
    Set-UserSecret "Billing:StripeApiBaseUrl" "https://api.stripe.com"
    Set-UserSecret "Billing:StripeSecretKey" (Read-SecretValue "Stripe secret key" "DARWINLINGUA_STRIPE_SECRET_KEY")
    Set-UserSecret "Billing:StripeWebhookSecret" (Read-SecretValue "Stripe webhook secret" "DARWINLINGUA_STRIPE_WEBHOOK_SECRET")
    Set-UserSecret "Billing:StripePremiumMonthlyPriceId" (Read-SecretValue "Stripe premium monthly price id" "DARWINLINGUA_STRIPE_PRICE_ID")
    Set-UserSecret "Billing:StripeWebhookToleranceMinutes" "5"
    Set-UserSecret "Billing:PremiumPlanKey" "premium-monthly"
    Set-UserSecret "Billing:EnableStripe" "true"
}

Write-Host "Local Web secrets updated. Restart DarwinLingua.Web for changes to take effect."
