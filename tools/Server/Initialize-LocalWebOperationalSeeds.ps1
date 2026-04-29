param(
    [string]$ApiBaseUrl = "http://localhost:5099",
    [string]$SeedPath = "content\generated\de-event-directory-seeds-001.json",
    [string]$WebApiProjectPath = "src\Apps\DarwinLingua.WebApi\DarwinLingua.WebApi.csproj",
    [string]$LogDirectory = "artifacts\logs",
    [string]$AdminApiKey = $env:DARWINLINGUA_ADMIN_API_KEY,
    [switch]$StartWebApi,
    [int]$StartupTimeoutSeconds = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$AdminApiHeaderName = "X-DarwinLingua-Admin-Key"

if ([string]::IsNullOrWhiteSpace($AdminApiKey)) {
    $AdminApiKey = "local-dev-admin-api-key-change-me"
}

function Write-Step {
    param([string]$Message)
    Write-Host "[DarwinLingua] $Message" -ForegroundColor Cyan
}

function Wait-ForHealth {
    param(
        [string]$HealthUrl,
        [int]$TimeoutSeconds,
        [System.Diagnostics.Process]$Process = $null,
        [string]$StandardOutputPath = $null,
        [string]$StandardErrorPath = $null
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        if ($null -ne $Process -and $Process.HasExited) {
            $diagnostics = @("Web API process exited before becoming healthy. Exit code: $($Process.ExitCode).")
            if (-not [string]::IsNullOrWhiteSpace($StandardOutputPath) -and (Test-Path -LiteralPath $StandardOutputPath)) {
                $diagnostics += "stdout:"
                $diagnostics += (Get-Content -LiteralPath $StandardOutputPath -Raw)
            }

            if (-not [string]::IsNullOrWhiteSpace($StandardErrorPath) -and (Test-Path -LiteralPath $StandardErrorPath)) {
                $diagnostics += "stderr:"
                $diagnostics += (Get-Content -LiteralPath $StandardErrorPath -Raw)
            }

            throw ($diagnostics -join [Environment]::NewLine)
        }

        try {
            $response = Invoke-RestMethod -Uri $HealthUrl -Method Get -TimeoutSec 5
            if ($response.status -in @("Healthy", "ok", "OK")) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    } while ((Get-Date) -lt $deadline)

    throw "Web API did not become healthy within $TimeoutSeconds seconds. Checked: $HealthUrl"
}

function ConvertTo-SeedJson {
    param([object]$Value)
    return ($Value | ConvertTo-Json -Depth 20)
}

function Invoke-JsonRequest {
    param(
        [string]$Uri,
        [string]$Method,
        [object]$Body = $null
    )

    try {
        if ($null -eq $Body) {
            return Invoke-RestMethod -Uri $Uri -Method $Method -Headers @{ $AdminApiHeaderName = $AdminApiKey } -TimeoutSec 30
        }

        return Invoke-RestMethod -Uri $Uri -Method $Method -ContentType "application/json" -Headers @{ $AdminApiHeaderName = $AdminApiKey } -Body (ConvertTo-SeedJson -Value $Body) -TimeoutSec 30
    }
    catch {
        $response = $_.Exception.Response
        if ($null -eq $response) {
            throw
        }

        $errorBody = $null
        if (-not [string]::IsNullOrWhiteSpace($_.ErrorDetails.Message)) {
            $errorBody = $_.ErrorDetails.Message
        }
        elseif ($response -is [System.Net.Http.HttpResponseMessage]) {
            try {
                $errorBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            }
            catch {
                $errorBody = $_.Exception.Message
            }
        }
        elseif ($response.PSObject.Methods["GetResponseStream"]) {
            $reader = New-Object System.IO.StreamReader($response.GetResponseStream())
            $errorBody = $reader.ReadToEnd()
        }

        $statusCode = try { [int]$response.StatusCode } catch { 0 }

        if (-not [string]::IsNullOrWhiteSpace($errorBody)) {
            throw "Request to $Uri failed with status $statusCode. Body: $errorBody"
        }

        throw "Request to $Uri failed with status $statusCode."
    }
}

function Escape-Url {
    param([string]$Value)
    return [System.Uri]::EscapeDataString($Value)
}

function Get-SeedItems {
    param(
        [object]$Seed,
        [string]$PropertyName
    )

    $property = $Seed.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or $null -eq $property.Value) {
        return @()
    }

    return @($property.Value)
}

function Get-PublicProfileIdByEmail {
    param(
        [hashtable]$ProfileIdsByEmail,
        [string]$OwnerEmail
    )

    $key = $OwnerEmail.Trim().ToLowerInvariant()
    if (-not $ProfileIdsByEmail.ContainsKey($key)) {
        throw "Learner profile ID not found for $OwnerEmail. The learner profile must be seeded before partner requests."
    }

    return $ProfileIdsByEmail[$key]
}

$workspaceRoot = (Resolve-Path ".").Path
$seedFullPath = Resolve-Path -LiteralPath (Join-Path $workspaceRoot $SeedPath)
$webApiProjectFullPath = Join-Path $workspaceRoot $WebApiProjectPath
if (-not (Test-Path -LiteralPath $webApiProjectFullPath)) {
    throw "Web API project not found: $webApiProjectFullPath"
}

$webApiProcess = $null
$webApiStdOutPath = $null
$webApiStdErrPath = $null

try {
    if ($StartWebApi.IsPresent) {
        Write-Step "Starting Web API in a separate process"
        $resolvedLogDirectory = Join-Path $workspaceRoot $LogDirectory
        New-Item -ItemType Directory -Path $resolvedLogDirectory -Force | Out-Null
        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $webApiStdOutPath = Join-Path $resolvedLogDirectory "webapi-operational-seed-$timestamp.out.log"
        $webApiStdErrPath = Join-Path $resolvedLogDirectory "webapi-operational-seed-$timestamp.err.log"
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        $env:DOTNET_ENVIRONMENT = "Development"
        $env:ASPNETCORE_URLS = $ApiBaseUrl
        $webApiProcess = Start-Process dotnet `
            -ArgumentList @(
                "run",
                "--project", $webApiProjectFullPath,
                "--no-launch-profile"
            ) `
            -WorkingDirectory $workspaceRoot `
            -WindowStyle Hidden `
            -RedirectStandardOutput $webApiStdOutPath `
            -RedirectStandardError $webApiStdErrPath `
            -PassThru
    }

    Write-Step "Waiting for Web API health endpoint"
    Wait-ForHealth `
        -HealthUrl "$ApiBaseUrl/health" `
        -TimeoutSeconds $StartupTimeoutSeconds `
        -Process $webApiProcess `
        -StandardOutputPath $webApiStdOutPath `
        -StandardErrorPath $webApiStdErrPath

    Write-Step "Reading operational seed file: $seedFullPath"
    $seed = Get-Content -LiteralPath $seedFullPath -Raw | ConvertFrom-Json

    $counts = [ordered]@{
        OrganizerProfiles = 0
        ConversationEvents = 0
        OrganizerOwners = 0
        OrganizerClaims = 0
        EventRsvps = 0
        LearnerProfiles = 0
        PartnerRequests = 0
        UserReports = 0
        UserBlocks = 0
    }

    foreach ($profile in (Get-SeedItems -Seed $seed -PropertyName "organizerProfiles")) {
        $body = @{
            slug = $profile.slug
            displayName = $profile.displayName
            organizerType = $profile.organizerType
            description = $profile.description
            cityRegion = $profile.cityRegion
            isOnlineAvailable = $profile.isOnlineAvailable
            supportedLearnerLevels = @($profile.supportedLearnerLevels)
            helperLanguageCodes = @($profile.helperLanguageCodes)
            websiteUrl = $profile.websiteUrl
            publicContactMethod = $profile.publicContactMethod
            verificationStatus = $profile.verificationStatus
            planKey = $profile.planKey
            historicalEventCount = $profile.historicalEventCount
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-profiles" -Method Post -Body $body | Out-Null
        $counts.OrganizerProfiles++
    }

    foreach ($event in (Get-SeedItems -Seed $seed -PropertyName "conversationEvents")) {
        $body = @{
            slug = $event.slug
            name = $event.name
            description = $event.description
            city = $event.city
            countryRegion = $event.countryRegion
            approximateLocation = $event.approximateLocation
            isOnline = $event.isOnline
            category = $event.category
            supportedLearnerLevels = @($event.supportedLearnerLevels)
            helperLanguageCodes = @($event.helperLanguageCodes)
            organizerName = $event.organizerName
            organizerProfileSlug = $event.organizerProfileSlug
            externalLink = $event.externalLink
            contactMethod = $event.contactMethod
            scheduleText = $event.scheduleText
            priceType = $event.priceType
            verificationStatus = $event.verificationStatus
            sourceName = $event.sourceName
            sourceUrl = $event.sourceUrl
            lastVerifiedAtUtc = $event.lastVerifiedAtUtc
            linkedEventPreparationPackSlugs = @($event.linkedEventPreparationPackSlugs)
            recurrenceRule = $event.recurrenceRule
            capacity = $event.capacity
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/conversation-events" -Method Post -Body $body | Out-Null
        $counts.ConversationEvents++
    }

    foreach ($owner in (Get-SeedItems -Seed $seed -PropertyName "organizerProfileOwners")) {
        $body = @{
            organizerProfileSlug = $owner.organizerProfileSlug
            ownerEmail = $owner.ownerEmail
            assignedBy = $owner.assignedBy
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-profile-owners" -Method Post -Body $body | Out-Null
        $counts.OrganizerOwners++
    }

    $existingClaims = @((Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-claim-requests" -Method Get) | ForEach-Object { $_ })
    foreach ($claim in (Get-SeedItems -Seed $seed -PropertyName "organizerClaimRequests")) {
        $alreadyExists = @($existingClaims | Where-Object {
                $_.organizerProfileSlug -eq $claim.organizerProfileSlug -and
                $_.requesterEmail -eq $claim.requesterEmail -and
                $_.relationshipToOrganizer -eq $claim.relationshipToOrganizer
            }).Count -gt 0

        if ($alreadyExists) {
            continue
        }

        $body = @{
            requesterName = $claim.requesterName
            requesterEmail = $claim.requesterEmail
            relationshipToOrganizer = $claim.relationshipToOrganizer
            evidenceText = $claim.evidenceText
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/organizer-profiles/$(Escape-Url $claim.organizerProfileSlug)/claim" -Method Post -Body $body | Out-Null
        $counts.OrganizerClaims++
    }

    foreach ($rsvp in (Get-SeedItems -Seed $seed -PropertyName "eventRsvps")) {
        $body = @{
            participantName = $rsvp.participantName
            participantEmail = $rsvp.participantEmail
            status = $rsvp.status
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/conversation-events/$(Escape-Url $rsvp.conversationEventSlug)/rsvps" -Method Post -Body $body | Out-Null
        $counts.EventRsvps++
    }

    $profileIdsByEmail = @{}
    foreach ($learner in (Get-SeedItems -Seed $seed -PropertyName "learnerConversationProfiles")) {
        $body = @{
            displayName = $learner.displayName
            cityRegion = $learner.cityRegion
            interactionPreference = $learner.interactionPreference
            germanLevel = $learner.germanLevel
            helperLanguageCodes = @($learner.helperLanguageCodes)
            conversationGoals = $learner.conversationGoals
            availabilityNotes = $learner.availabilityNotes
            visibility = $learner.visibility
            hasConfirmedAdult = $learner.hasConfirmedAdult
        }

        $savedProfile = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/learner-conversation-profiles/me?ownerEmail=$(Escape-Url $learner.ownerEmail)" -Method Post -Body $body
        $profileIdsByEmail[$learner.ownerEmail.Trim().ToLowerInvariant()] = $savedProfile.id
        $counts.LearnerProfiles++
    }

    foreach ($request in (Get-SeedItems -Seed $seed -PropertyName "partnerRequests")) {
        $existingRequests = @((Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests?ownerEmail=$(Escape-Url $request.requesterEmail)" -Method Get) | ForEach-Object { $_ })
        $alreadyExists = @($existingRequests | Where-Object {
                $_.openerTemplateKey -eq $request.openerTemplateKey -and
                $_.note -eq $request.note
            }).Count -gt 0

        if ($alreadyExists) {
            continue
        }

        $targetProfileId = Get-PublicProfileIdByEmail -ProfileIdsByEmail $profileIdsByEmail -OwnerEmail $request.targetLearnerEmail
        $body = @{
            targetLearnerProfileId = $targetProfileId
            openerTemplateKey = $request.openerTemplateKey
            note = $request.note
        }

        $savedRequest = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests?ownerEmail=$(Escape-Url $request.requesterEmail)" -Method Post -Body $body
        switch ($request.status) {
            "accepted" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $request.targetLearnerEmail)" -Method Post -Body @{ action = "accept" } | Out-Null
            }
            "declined" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $request.targetLearnerEmail)" -Method Post -Body @{ action = "decline" } | Out-Null
            }
            "cancelled" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $request.requesterEmail)" -Method Post -Body @{ action = "cancel" } | Out-Null
            }
            "blocked" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $request.targetLearnerEmail)" -Method Post -Body @{ action = "block" } | Out-Null
            }
        }

        $counts.PartnerRequests++
    }

    $existingReports = @((Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/moderation/reports" -Method Get) | ForEach-Object { $_ })
    foreach ($report in (Get-SeedItems -Seed $seed -PropertyName "userReports")) {
        $existingReport = @($existingReports | Where-Object {
                $_.reporterEmail -eq $report.reporterEmail -and
                $_.targetType -eq $report.targetType -and
                $_.targetKey -eq $report.targetKey -and
                $_.reason -eq $report.reason
            } | Select-Object -First 1)

        if ($existingReport.Count -gt 0) {
            continue
        }

        $body = @{
            targetType = $report.targetType
            targetKey = $report.targetKey
            reportedUserEmail = $report.reportedUserEmail
            reason = $report.reason
            details = $report.details
        }

        $savedReport = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/moderation/reports?reporterEmail=$(Escape-Url $report.reporterEmail)" -Method Post -Body $body
        if ($report.status -ne "pending") {
            Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/moderation/reports/$($savedReport.id)/decision" -Method Post -Body @{
                status = $report.status
                decisionNote = $report.decisionNote
                decidedBy = $report.decidedBy
            } | Out-Null
        }

        $counts.UserReports++
    }

    foreach ($block in (Get-SeedItems -Seed $seed -PropertyName "userBlocks")) {
        $body = @{
            blockedEmail = $block.blockedEmail
            reason = $block.reason
            sourcePartnerRequestId = $null
            targetLearnerProfileId = $null
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/moderation/blocks?blockerEmail=$(Escape-Url $block.blockerEmail)" -Method Post -Body $body | Out-Null
        $counts.UserBlocks++
    }

    Write-Host ""
    Write-Host "Local Web operational seed completed." -ForegroundColor Green
    foreach ($item in $counts.GetEnumerator()) {
        Write-Host ("{0}: {1}" -f $item.Key, $item.Value)
    }
}
finally {
    if ($null -ne $webApiProcess -and -not $webApiProcess.HasExited) {
        Write-Step "Stopping Web API process"
        Stop-Process -Id $webApiProcess.Id -Force
    }
}
