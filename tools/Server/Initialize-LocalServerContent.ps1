param(
    [string]$ApiBaseUrl = "http://localhost:5099",
    [string]$ClientProductKey = "darwin-deutsch",
    [string]$ContentPath = "D:\_Projects\DarwinLingua.Content\A1.json",
    [string]$WebApiProjectPath = "src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj",
    [switch]$StartWebApi,
    [int]$StartupTimeoutSeconds = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host "[DarwinLingua] $Message" -ForegroundColor Cyan
}

function Assert-PathExists {
    param(
        [string]$Path,
        [string]$Description
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Description not found: $Path"
    }
}

function Wait-ForHealth {
    param(
        [string]$HealthUrl,
        [int]$TimeoutSeconds
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $response = Invoke-RestMethod -Uri $HealthUrl -Method Get -TimeoutSec 5
            if ($response.status -eq "Healthy") {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    } while ((Get-Date) -lt $deadline)

    throw "Web API did not become healthy within $TimeoutSeconds seconds. Checked: $HealthUrl"
}

function Get-JsonFiles {
    param([string]$TargetPath)

    if (Test-Path -LiteralPath $TargetPath -PathType Leaf) {
        if ([System.IO.Path]::GetExtension($TargetPath) -ne ".json") {
            throw "Content file must be a .json file: $TargetPath"
        }

        return ,(Resolve-Path -LiteralPath $TargetPath).Path
    }

    if (Test-Path -LiteralPath $TargetPath -PathType Container) {
        $jsonFiles = Get-ChildItem -LiteralPath $TargetPath -Filter *.json -File -Recurse |
            Sort-Object FullName |
            Select-Object -ExpandProperty FullName

        if ($jsonFiles.Count -eq 0) {
            throw "No .json files were found under: $TargetPath"
        }

        return $jsonFiles
    }

    throw "Content path does not exist: $TargetPath"
}

$workspaceRoot = (Resolve-Path ".").Path
$localSettingsPath = Join-Path $workspaceRoot "src\Apps\DarwinLingua.WebApi\appsettings.Development.Local.json"

Assert-PathExists -Path $localSettingsPath -Description "Local Web API settings"

$contentFiles = Get-JsonFiles -TargetPath $ContentPath
$webApiProjectFullPath = Join-Path $workspaceRoot $WebApiProjectPath
Assert-PathExists -Path $webApiProjectFullPath -Description "Web API project"

$webApiProcess = $null

try {
    if ($StartWebApi.IsPresent) {
        Write-Step "Starting Web API in a separate process"
        $webApiProcess = Start-Process powershell `
            -ArgumentList @(
                "-NoProfile",
                "-ExecutionPolicy", "Bypass",
                "-Command", "dotnet run --project `"$webApiProjectFullPath`" --no-launch-profile"
            ) `
            -WorkingDirectory $workspaceRoot `
            -PassThru
    }

    Write-Step "Waiting for Web API health endpoint"
    Wait-ForHealth -HealthUrl "$ApiBaseUrl/health" -TimeoutSeconds $StartupTimeoutSeconds

    $importResponses = New-Object System.Collections.Generic.List[object]
    foreach ($file in $contentFiles) {
        Write-Step "Importing $file"
        $importBody = @{
            filePath = $file
            clientProductKey = $ClientProductKey
        } | ConvertTo-Json

        $importResponse = Invoke-RestMethod `
            -Uri "$ApiBaseUrl/api/admin/content/catalog/import" `
            -Method Post `
            -ContentType "application/json" `
            -Body $importBody

        if (-not $importResponse.isSuccess) {
            $issues = ($importResponse.issueMessages -join "; ")
            throw "Import failed for $file. Issues: $issues"
        }

        $importResponses.Add($importResponse)
        Write-Host ("  Imported: {0} | Draft batch: {1}" -f $importResponse.importedEntries, $importResponse.draftPublicationBatchId)
    }

    $latestBatchId = $importResponses[$importResponses.Count - 1].draftPublicationBatchId
    Write-Step "Publishing draft batch $latestBatchId"
    $publishBody = @{
        clientProductKey = $ClientProductKey
        publicationBatchId = $latestBatchId
    } | ConvertTo-Json

    $publishResponse = Invoke-RestMethod `
        -Uri "$ApiBaseUrl/api/admin/content/catalog/publish" `
        -Method Post `
        -ContentType "application/json" `
        -Body $publishBody

    if (-not $publishResponse.isSuccess) {
        $issues = ($publishResponse.issueMessages -join "; ")
        throw "Publish failed. Issues: $issues"
    }

    $importedEntries = ($importResponses | Measure-Object -Property importedEntries -Sum).Sum
    $invalidEntries = ($importResponses | Measure-Object -Property invalidEntries -Sum).Sum
    $duplicateEntries = ($importResponses | Measure-Object -Property skippedDuplicateEntries -Sum).Sum

    Write-Host ""
    Write-Host "Local server bootstrap completed." -ForegroundColor Green
    Write-Host ("Client product: {0}" -f $ClientProductKey)
    Write-Host ("Files imported: {0}" -f $contentFiles.Count)
    Write-Host ("Entries imported: {0}" -f $importedEntries)
    Write-Host ("Duplicates skipped: {0}" -f $duplicateEntries)
    Write-Host ("Invalid entries: {0}" -f $invalidEntries)
    Write-Host ("Published batch: {0}" -f $publishResponse.publicationBatchId)
    Write-Host ("Published version: {0}" -f $publishResponse.publishedVersion)
    Write-Host ("Published packages: {0}" -f ($publishResponse.publishedPackageIds -join ", "))
}
finally {
    if ($null -ne $webApiProcess -and -not $webApiProcess.HasExited) {
        Write-Step "Stopping Web API process"
        Stop-Process -Id $webApiProcess.Id -Force
    }
}
