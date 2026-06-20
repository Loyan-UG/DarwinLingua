param(
    [Parameter(Mandatory = $true)]
    [string]$TesterCsvPath,
    [string]$DatabaseName = "darwinlingua_shared",
    [string]$PostgresContainer = "darwinlingua-postgres",
    [string]$PostgresUser = "postgres",
    [string]$UpdatedBy = "web-tester-operator",
    [datetime]$PremiumEndsAtUtc,
    [switch]$WhatIf
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

function Invoke-PostgresScalar {
    param([string]$Sql)

    $result = docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -t -A -v ON_ERROR_STOP=1 -c $Sql
    if ($LASTEXITCODE -ne 0) {
        throw "psql failed for query: $Sql"
    }

    return ($result | Out-String).Trim()
}

function Invoke-PostgresCommand {
    param([string]$Sql)

    docker exec $PostgresContainer psql -U $PostgresUser -d $DatabaseName -v ON_ERROR_STOP=1 -c $Sql | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "psql failed for command: $Sql"
    }
}

$resolvedCsvPath = Resolve-RepositoryPath -Path $TesterCsvPath
if (-not (Test-Path -LiteralPath $resolvedCsvPath -PathType Leaf)) {
    throw "Tester CSV was not found: $resolvedCsvPath"
}

$rows = @(Import-Csv -LiteralPath $resolvedCsvPath)
if ($rows.Count -eq 0) {
    throw "Tester CSV has no rows: $resolvedCsvPath"
}

$requiredColumns = @("Email")
foreach ($column in $requiredColumns) {
    if (-not ($rows[0].PSObject.Properties.Name -contains $column)) {
        throw "Tester CSV must contain a '$column' column."
    }
}

$expiresSql = "NULL"
if ($PSBoundParameters.ContainsKey("PremiumEndsAtUtc")) {
    $expiresSql = "'$($PremiumEndsAtUtc.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"))'::timestamp with time zone"
}

$updatedBySql = Escape-SqlLiteral -Value $UpdatedBy
$nowSql = "now()"
$changed = 0
$skipped = 0
$missing = New-Object System.Collections.Generic.List[string]

foreach ($row in $rows) {
    $emailProperty = $row.PSObject.Properties["Email"]
    $email = if ($null -eq $emailProperty -or $null -eq $emailProperty.Value) { "" } else { $emailProperty.Value.ToString().Trim() }
    if ([string]::IsNullOrWhiteSpace($email)) {
        continue
    }

    $normalizedEmail = $email.ToUpperInvariant()
    $normalizedEmailSql = Escape-SqlLiteral -Value $normalizedEmail
    $userId = Invoke-PostgresScalar -Sql "select ""Id"" from ""AspNetUsers"" where ""NormalizedEmail"" = '$normalizedEmailSql' limit 1;"

    if ([string]::IsNullOrWhiteSpace($userId)) {
        $missing.Add($email)
        continue
    }

    $userIdSql = Escape-SqlLiteral -Value $userId
    $previousTier = Invoke-PostgresScalar -Sql "select coalesce((select ""Tier"" from ""UserEntitlementStates"" where ""UserId"" = '$userIdSql'), 'Free');"
    $emailConfirmed = Invoke-PostgresScalar -Sql "select ""EmailConfirmed"" from ""AspNetUsers"" where ""Id"" = '$userIdSql' limit 1;"
    $previousPremiumEndsAt = Invoke-PostgresScalar -Sql "select coalesce((select to_char(""PremiumEndsAtUtc"", 'YYYY-MM-DD""T""HH24:MI:SS.US""Z""') from ""UserEntitlementStates"" where ""UserId"" = '$userIdSql'), '');"
    $previousPremiumEndsAtSql = if ([string]::IsNullOrWhiteSpace($previousPremiumEndsAt)) { "NULL" } else { "'$(Escape-SqlLiteral -Value $previousPremiumEndsAt)'::timestamp with time zone" }
    $auditId = [Guid]::NewGuid().ToString()

    if (-not $PSBoundParameters.ContainsKey("PremiumEndsAtUtc") -and
        $emailConfirmed -eq "t" -and
        $previousTier -eq "Premium" -and
        [string]::IsNullOrWhiteSpace($previousPremiumEndsAt)) {
        Write-Host "Already confirmed with non-expiring Premium: $email ($userId)." -ForegroundColor DarkGreen
        $skipped++
        continue
    }

    $sql = @"
begin;
update "AspNetUsers"
set "EmailConfirmed" = true
where "Id" = '$userIdSql';

insert into "UserEntitlementStates" (
    "UserId",
    "Tier",
    "TrialStartedAtUtc",
    "TrialEndsAtUtc",
    "PremiumStartedAtUtc",
    "PremiumEndsAtUtc",
    "CreatedAtUtc",
    "UpdatedAtUtc",
    "LastUpdatedBy"
)
values (
    '$userIdSql',
    'Premium',
    null,
    null,
    $nowSql,
    $expiresSql,
    $nowSql,
    $nowSql,
    '$updatedBySql'
)
on conflict ("UserId") do update
set
    "Tier" = 'Premium',
    "PremiumStartedAtUtc" = coalesce("UserEntitlementStates"."PremiumStartedAtUtc", $nowSql),
    "PremiumEndsAtUtc" = $expiresSql,
    "UpdatedAtUtc" = $nowSql,
    "LastUpdatedBy" = '$updatedBySql';

insert into "UserEntitlementAuditEvents" (
    "Id",
    "UserId",
    "EventType",
    "PreviousTier",
    "NewTier",
    "PreviousTrialEndsAtUtc",
    "NewTrialEndsAtUtc",
    "PreviousPremiumEndsAtUtc",
    "NewPremiumEndsAtUtc",
    "UpdatedBy",
    "CreatedAtUtc"
)
values (
    '$auditId',
    '$userIdSql',
    'tier-changed',
    '$(Escape-SqlLiteral -Value $previousTier)',
    'Premium',
    null,
    null,
    $previousPremiumEndsAtSql,
    $expiresSql,
    '$updatedBySql',
    $nowSql
);
commit;
"@

    if ($WhatIf.IsPresent) {
        Write-Host "Would confirm and grant Premium to $email ($userId)." -ForegroundColor Yellow
        continue
    }

    Invoke-PostgresCommand -Sql $sql
    Write-Host "Confirmed and granted Premium to $email ($userId)." -ForegroundColor Green
    $changed++
}

Write-Host ""
Write-Host "Tester premium access update complete." -ForegroundColor Cyan
Write-Host "Processed rows: $($rows.Count)"
Write-Host "Updated users: $changed"
Write-Host "Skipped unchanged users: $skipped"

if ($missing.Count -gt 0) {
    Write-Host "Missing users: $($missing.Count)" -ForegroundColor Yellow
    foreach ($email in $missing) {
        Write-Host "  - $email"
    }
}
