param(
    [string]$ApiBaseUrl = "http://localhost:5099",
    [string]$SeedPath = "tools\Web\WebReadinessSeedFixtureManifest.json",
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
        $errorDetailsMessage = $null
        if ($_.PSObject.Properties["ErrorDetails"] -and $null -ne $_.ErrorDetails) {
            $errorDetailsMessageProperty = $_.ErrorDetails.PSObject.Properties["Message"]
            if ($null -ne $errorDetailsMessageProperty) {
                $errorDetailsMessage = $errorDetailsMessageProperty.Value
            }
        }

        if (-not [string]::IsNullOrWhiteSpace($errorDetailsMessage)) {
            $errorBody = $errorDetailsMessage
        }
        elseif ($response.GetType().FullName -eq "System.Net.Http.HttpResponseMessage") {
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

function Get-FirstSeedItems {
    param(
        [object]$Seed,
        [string[]]$PropertyNames
    )

    foreach ($propertyName in $PropertyNames) {
        $items = @(Get-SeedItems -Seed $Seed -PropertyName $propertyName)
        if ($items.Count -gt 0) {
            return $items
        }
    }

    return @()
}

function Get-SeedValue {
    param(
        [object]$Item,
        [string]$PropertyName,
        [object]$Default = $null,
        [switch]$Required
    )

    $property = $Item.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or $null -eq $property.Value) {
        if ($Required.IsPresent) {
            throw "Seed item is missing required property '$PropertyName'."
        }

        return $Default
    }

    if ($property.Value -is [string] -and [string]::IsNullOrWhiteSpace($property.Value)) {
        if ($Required.IsPresent) {
            throw "Seed item property '$PropertyName' cannot be empty."
        }

        return $Default
    }

    return $property.Value
}

function Get-SeedValueFromAny {
    param(
        [object]$Item,
        [string[]]$PropertyNames,
        [switch]$Required
    )

    foreach ($propertyName in $PropertyNames) {
        $property = $Item.PSObject.Properties[$propertyName]
        if ($null -eq $property -or $null -eq $property.Value) {
            continue
        }

        if ($property.Value -is [string] -and [string]::IsNullOrWhiteSpace($property.Value)) {
            continue
        }

        return $property.Value
    }

    if ($Required.IsPresent) {
        throw "Seed item is missing one of the required properties: $($PropertyNames -join ', ')."
    }

    return $null
}

function Get-SeedArray {
    param(
        [object]$Item,
        [string]$PropertyName
    )

    $property = $Item.PSObject.Properties[$PropertyName]
    if ($null -eq $property -or $null -eq $property.Value) {
        return ,@()
    }

    return ,@($property.Value)
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

function Get-RequiredProfileEmailByKey {
    param(
        [hashtable]$EmailsByKey,
        [string]$ProfileKey
    )

    if (-not $EmailsByKey.ContainsKey($ProfileKey)) {
        throw "Learner profile email not found for key '$ProfileKey'."
    }

    return $EmailsByKey[$ProfileKey]
}

function Get-RequiredProfileIdByKey {
    param(
        [hashtable]$ProfileIdsByKey,
        [string]$ProfileKey
    )

    if (-not $ProfileIdsByKey.ContainsKey($ProfileKey)) {
        throw "Learner profile ID not found for key '$ProfileKey'."
    }

    return $ProfileIdsByKey[$ProfileKey]
}

function Get-RequiredPartnerRequestIdByKey {
    param(
        [hashtable]$PartnerRequestIdsByKey,
        [string]$PartnerRequestKey
    )

    if (-not $PartnerRequestIdsByKey.ContainsKey($PartnerRequestKey)) {
        throw "Partner request ID not found for key '$PartnerRequestKey'."
    }

    return $PartnerRequestIdsByKey[$PartnerRequestKey]
}

function Get-RequiredReportIdByKey {
    param(
        [hashtable]$ReportIdsByKey,
        [string]$ReportKey
    )

    if (-not $ReportIdsByKey.ContainsKey($ReportKey)) {
        throw "User report ID not found for key '$ReportKey'."
    }

    return $ReportIdsByKey[$ReportKey]
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
            slug = Get-SeedValue -Item $profile -PropertyName "slug" -Required
            displayName = Get-SeedValue -Item $profile -PropertyName "displayName" -Required
            organizerType = Get-SeedValue -Item $profile -PropertyName "organizerType" -Required
            description = Get-SeedValue -Item $profile -PropertyName "description" -Required
            cityRegion = Get-SeedValue -Item $profile -PropertyName "cityRegion"
            isOnlineAvailable = Get-SeedValue -Item $profile -PropertyName "isOnlineAvailable" -Default $false
            supportedLearnerLevels = Get-SeedArray -Item $profile -PropertyName "supportedLearnerLevels"
            helperLanguageCodes = Get-SeedArray -Item $profile -PropertyName "helperLanguageCodes"
            websiteUrl = Get-SeedValue -Item $profile -PropertyName "websiteUrl"
            publicContactMethod = Get-SeedValue -Item $profile -PropertyName "publicContactMethod"
            verificationStatus = Get-SeedValue -Item $profile -PropertyName "verificationStatus" -Required
            planKey = Get-SeedValue -Item $profile -PropertyName "planKey" -Required
            historicalEventCount = Get-SeedValue -Item $profile -PropertyName "historicalEventCount" -Default 0
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-profiles" -Method Post -Body $body | Out-Null
        $counts.OrganizerProfiles++
    }

    foreach ($event in (Get-SeedItems -Seed $seed -PropertyName "conversationEvents")) {
        $body = @{
            slug = Get-SeedValue -Item $event -PropertyName "slug" -Required
            name = Get-SeedValue -Item $event -PropertyName "name" -Required
            description = Get-SeedValue -Item $event -PropertyName "description" -Required
            city = Get-SeedValue -Item $event -PropertyName "city"
            countryRegion = Get-SeedValue -Item $event -PropertyName "countryRegion" -Required
            approximateLocation = Get-SeedValue -Item $event -PropertyName "approximateLocation"
            isOnline = Get-SeedValue -Item $event -PropertyName "isOnline" -Default $false
            category = Get-SeedValue -Item $event -PropertyName "category" -Required
            supportedLearnerLevels = Get-SeedArray -Item $event -PropertyName "supportedLearnerLevels"
            helperLanguageCodes = Get-SeedArray -Item $event -PropertyName "helperLanguageCodes"
            organizerName = Get-SeedValue -Item $event -PropertyName "organizerName" -Required
            organizerProfileSlug = Get-SeedValue -Item $event -PropertyName "organizerProfileSlug"
            externalLink = Get-SeedValue -Item $event -PropertyName "externalLink"
            contactMethod = Get-SeedValue -Item $event -PropertyName "contactMethod"
            scheduleText = Get-SeedValue -Item $event -PropertyName "scheduleText" -Required
            startsAtUtc = Get-SeedValue -Item $event -PropertyName "startsAtUtc"
            endsAtUtc = Get-SeedValue -Item $event -PropertyName "endsAtUtc"
            priceType = Get-SeedValue -Item $event -PropertyName "priceType" -Required
            verificationStatus = Get-SeedValue -Item $event -PropertyName "verificationStatus" -Required
            sourceName = Get-SeedValue -Item $event -PropertyName "sourceName"
            sourceUrl = Get-SeedValue -Item $event -PropertyName "sourceUrl"
            lastVerifiedAtUtc = Get-SeedValue -Item $event -PropertyName "lastVerifiedAtUtc"
            linkedEventPreparationPackSlugs = Get-SeedArray -Item $event -PropertyName "linkedEventPreparationPackSlugs"
            recurrenceRule = Get-SeedValue -Item $event -PropertyName "recurrenceRule"
            capacity = Get-SeedValue -Item $event -PropertyName "capacity"
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/conversation-events" -Method Post -Body $body | Out-Null
        $counts.ConversationEvents++
    }

    foreach ($owner in (Get-SeedItems -Seed $seed -PropertyName "organizerProfileOwners")) {
        $body = @{
            organizerProfileSlug = Get-SeedValue -Item $owner -PropertyName "organizerProfileSlug" -Required
            ownerEmail = Get-SeedValue -Item $owner -PropertyName "ownerEmail" -Required
            assignedBy = Get-SeedValue -Item $owner -PropertyName "assignedBy" -Required
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-profile-owners" -Method Post -Body $body | Out-Null
        $counts.OrganizerOwners++
    }

    $existingClaims = @((Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-claim-requests" -Method Get) | ForEach-Object { $_ })
    foreach ($claim in (Get-FirstSeedItems -Seed $seed -PropertyNames @("organizerClaimRequests", "organizerClaims"))) {
        $organizerProfileSlug = Get-SeedValue -Item $claim -PropertyName "organizerProfileSlug" -Required
        $requesterEmail = Get-SeedValue -Item $claim -PropertyName "requesterEmail" -Required
        $relationshipToOrganizer = Get-SeedValue -Item $claim -PropertyName "relationshipToOrganizer" -Required
        $alreadyExists = @($existingClaims | Where-Object {
                $_.organizerProfileSlug -eq $organizerProfileSlug -and
                $_.requesterEmail -eq $requesterEmail -and
                $_.relationshipToOrganizer -eq $relationshipToOrganizer
            }).Count -gt 0

        $claimId = $null

        if ($alreadyExists) {
            $claimId = @($existingClaims | Where-Object {
                    $_.organizerProfileSlug -eq $organizerProfileSlug -and
                    $_.requesterEmail -eq $requesterEmail -and
                    $_.relationshipToOrganizer -eq $relationshipToOrganizer
                } | Select-Object -First 1)[0].id
        }
        else {
            $body = @{
                requesterName = Get-SeedValue -Item $claim -PropertyName "requesterName" -Required
                requesterEmail = $requesterEmail
                relationshipToOrganizer = $relationshipToOrganizer
                evidenceText = Get-SeedValue -Item $claim -PropertyName "evidenceText" -Required
            }

            $savedClaim = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/organizer-profiles/$(Escape-Url $organizerProfileSlug)/claim" -Method Post -Body $body
            $claimId = $savedClaim.id
            $counts.OrganizerClaims++
        }

        $claimStatus = Get-SeedValue -Item $claim -PropertyName "status" -Default "submitted"
        if ($claimStatus -ne "submitted") {
            Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/organizer-claim-requests/$claimId/status" -Method Post -Body @{ status = $claimStatus } | Out-Null
        }
    }

    foreach ($rsvp in (Get-SeedItems -Seed $seed -PropertyName "eventRsvps")) {
        $eventSlug = Get-SeedValueFromAny -Item $rsvp -PropertyNames @("conversationEventSlug", "eventSlug") -Required
        $body = @{
            participantName = Get-SeedValue -Item $rsvp -PropertyName "participantName" -Required
            participantEmail = Get-SeedValue -Item $rsvp -PropertyName "participantEmail" -Required
            status = Get-SeedValue -Item $rsvp -PropertyName "status" -Required
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/conversation-events/$(Escape-Url $eventSlug)/rsvps" -Method Post -Body $body | Out-Null
        $counts.EventRsvps++
    }

    $profileIdsByEmail = @{}
    $profileIdsByKey = @{}
    $emailsByProfileKey = @{}
    foreach ($learner in (Get-FirstSeedItems -Seed $seed -PropertyNames @("learnerConversationProfiles", "learnerProfiles"))) {
        $ownerEmail = Get-SeedValue -Item $learner -PropertyName "ownerEmail" -Required
        $profileKey = Get-SeedValue -Item $learner -PropertyName "key" -Default $ownerEmail
        $body = @{
            displayName = Get-SeedValue -Item $learner -PropertyName "displayName" -Required
            cityRegion = Get-SeedValue -Item $learner -PropertyName "cityRegion" -Default (Get-SeedValue -Item $learner -PropertyName "city")
            interactionPreference = Get-SeedValue -Item $learner -PropertyName "interactionPreference" -Default "both"
            learningLevel = Get-SeedValue -Item $learner -PropertyName "learningLevel" -Default "B1"
            helperLanguageCodes = Get-SeedArray -Item $learner -PropertyName "helperLanguageCodes"
            conversationGoals = Get-SeedValue -Item $learner -PropertyName "conversationGoals" -Default ((Get-SeedArray -Item $learner -PropertyName "practiceGoals") -join ", ")
            availabilityNotes = Get-SeedValue -Item $learner -PropertyName "availabilityNotes"
            visibility = Get-SeedValue -Item $learner -PropertyName "visibility" -Default ($(if ((Get-SeedValue -Item $learner -PropertyName "isPublic" -Default $true)) { "public" } else { "request-only" }))
            hasConfirmedAdult = Get-SeedValue -Item $learner -PropertyName "hasConfirmedAdult" -Default $true
        }

        $savedProfile = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/learner-conversation-profiles/me?ownerEmail=$(Escape-Url $ownerEmail)" -Method Post -Body $body
        $normalizedOwnerEmail = $ownerEmail.Trim().ToLowerInvariant()
        $profileIdsByEmail[$normalizedOwnerEmail] = $savedProfile.id
        $profileIdsByKey[$profileKey] = $savedProfile.id
        $emailsByProfileKey[$profileKey] = $normalizedOwnerEmail
        $counts.LearnerProfiles++
    }

    $partnerRequestIdsByKey = @{}
    foreach ($request in (Get-SeedItems -Seed $seed -PropertyName "partnerRequests")) {
        $requesterEmail = Get-SeedValue -Item $request -PropertyName "requesterEmail"
        if ([string]::IsNullOrWhiteSpace($requesterEmail)) {
            $requesterEmail = Get-RequiredProfileEmailByKey -EmailsByKey $emailsByProfileKey -ProfileKey (Get-SeedValue -Item $request -PropertyName "requesterProfileKey" -Required)
        }

        $targetLearnerEmail = Get-SeedValue -Item $request -PropertyName "targetLearnerEmail"
        if ([string]::IsNullOrWhiteSpace($targetLearnerEmail)) {
            $targetLearnerEmail = Get-RequiredProfileEmailByKey -EmailsByKey $emailsByProfileKey -ProfileKey (Get-SeedValue -Item $request -PropertyName "targetProfileKey" -Required)
        }

        $targetProfileId = Get-SeedValue -Item $request -PropertyName "targetLearnerProfileId"
        if ([string]::IsNullOrWhiteSpace($targetProfileId)) {
            $targetProfileId = Get-RequiredProfileIdByKey -ProfileIdsByKey $profileIdsByKey -ProfileKey (Get-SeedValue -Item $request -PropertyName "targetProfileKey" -Required)
        }
        $openerTemplateKey = Get-SeedValue -Item $request -PropertyName "openerTemplateKey" -Default (Get-SeedValue -Item $request -PropertyName "practiceGoal" -Default "practice-goals")
        $note = Get-SeedValue -Item $request -PropertyName "note" -Default (Get-SeedValue -Item $request -PropertyName "practiceGoal")
        $existingRequests = @((Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests?ownerEmail=$(Escape-Url $requesterEmail)" -Method Get) | ForEach-Object { $_ })
        $alreadyExists = @($existingRequests | Where-Object {
                $_.openerTemplateKey -eq $openerTemplateKey -and
                $_.note -eq $note
            }).Count -gt 0

        if ($alreadyExists) {
            $requestKey = Get-SeedValue -Item $request -PropertyName "key"
            if (-not [string]::IsNullOrWhiteSpace($requestKey)) {
                $partnerRequestIdsByKey[$requestKey] = @($existingRequests | Where-Object {
                        $_.openerTemplateKey -eq $openerTemplateKey -and
                        $_.note -eq $note
                    } | Select-Object -First 1)[0].id
            }
            continue
        }

        $body = @{
            targetLearnerProfileId = $targetProfileId
            openerTemplateKey = $openerTemplateKey
            note = $note
        }

        $savedRequest = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests?ownerEmail=$(Escape-Url $requesterEmail)" -Method Post -Body $body
        $requestKey = Get-SeedValue -Item $request -PropertyName "key"
        if (-not [string]::IsNullOrWhiteSpace($requestKey)) {
            $partnerRequestIdsByKey[$requestKey] = $savedRequest.id
        }

        switch (Get-SeedValue -Item $request -PropertyName "status" -Default "pending") {
            "accepted" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $targetLearnerEmail)" -Method Post -Body @{ action = "accept" } | Out-Null
            }
            "declined" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $targetLearnerEmail)" -Method Post -Body @{ action = "decline" } | Out-Null
            }
            "cancelled" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $requesterEmail)" -Method Post -Body @{ action = "cancel" } | Out-Null
            }
            "blocked" {
                Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/partner-requests/$($savedRequest.id)/state?ownerEmail=$(Escape-Url $targetLearnerEmail)" -Method Post -Body @{ action = "block" } | Out-Null
            }
        }

        $counts.PartnerRequests++
    }

    $existingReports = @((Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/moderation/reports" -Method Get) | ForEach-Object { $_ })
    $reportIdsByKey = @{}
    $reportsByKey = @{}
    foreach ($report in (Get-SeedItems -Seed $seed -PropertyName "userReports")) {
        $reporterEmail = Get-SeedValue -Item $report -PropertyName "reporterEmail" -Required
        $targetType = Get-SeedValue -Item $report -PropertyName "targetType" -Required
        $targetKey = Get-SeedValue -Item $report -PropertyName "targetKey" -Required
        $reason = Get-SeedValue -Item $report -PropertyName "reason" -Required
        $existingReport = @($existingReports | Where-Object {
                $_.reporterEmail -eq $reporterEmail -and
                $_.targetType -eq $targetType -and
                $_.targetKey -eq $targetKey -and
                $_.reason -eq $reason
            } | Select-Object -First 1)

        if ($existingReport.Count -gt 0) {
            $reportKey = Get-SeedValue -Item $report -PropertyName "key"
            if (-not [string]::IsNullOrWhiteSpace($reportKey)) {
                $reportIdsByKey[$reportKey] = $existingReport[0].id
                $reportsByKey[$reportKey] = $existingReport[0]
            }
            continue
        }

        $body = @{
            targetType = $targetType
            targetKey = $targetKey
            reportedUserEmail = Get-SeedValue -Item $report -PropertyName "reportedUserEmail"
            reason = $reason
            details = Get-SeedValue -Item $report -PropertyName "details" -Required
        }

        $savedReport = Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/moderation/reports?reporterEmail=$(Escape-Url $reporterEmail)" -Method Post -Body $body
        $reportKey = Get-SeedValue -Item $report -PropertyName "key"
        if (-not [string]::IsNullOrWhiteSpace($reportKey)) {
            $reportIdsByKey[$reportKey] = $savedReport.id
            $reportsByKey[$reportKey] = $savedReport
        }

        $counts.UserReports++
    }

    foreach ($audit in (Get-SeedItems -Seed $seed -PropertyName "moderationAudits")) {
        $reportKey = Get-SeedValue -Item $audit -PropertyName "userReportKey" -Required
        $decisionStatus = Get-SeedValue -Item $audit -PropertyName "decisionStatus" -Required
        $reportId = Get-RequiredReportIdByKey -ReportIdsByKey $reportIdsByKey -ReportKey $reportKey
        $reportForAudit = $reportsByKey[$reportKey]
        if ($null -ne $reportForAudit -and $reportForAudit.status -eq $decisionStatus -and -not [string]::IsNullOrWhiteSpace($reportForAudit.decidedBy)) {
            continue
        }

        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/admin/catalog/moderation/reports/$reportId/decision" -Method Post -Body @{
            status = $decisionStatus
            decisionNote = Get-SeedValue -Item $audit -PropertyName "decisionNote" -Required
            decidedBy = Get-SeedValue -Item $audit -PropertyName "decidedBy" -Required
        } | Out-Null
    }

    foreach ($block in (Get-SeedItems -Seed $seed -PropertyName "userBlocks")) {
        $sourcePartnerRequestKey = Get-SeedValue -Item $block -PropertyName "sourcePartnerRequestKey"
        $targetLearnerProfileId = $null
        if ([string]::IsNullOrWhiteSpace($sourcePartnerRequestKey)) {
            $targetLearnerProfileId = Get-RequiredProfileIdByKey -ProfileIdsByKey $profileIdsByKey -ProfileKey (Get-SeedValue -Item $block -PropertyName "blockedProfileKey" -Required)
        }

        $body = @{
            blockedEmail = Get-SeedValue -Item $block -PropertyName "blockedEmail"
            reason = Get-SeedValue -Item $block -PropertyName "reason"
            sourcePartnerRequestId = $(if ([string]::IsNullOrWhiteSpace($sourcePartnerRequestKey)) { $null } else { Get-RequiredPartnerRequestIdByKey -PartnerRequestIdsByKey $partnerRequestIdsByKey -PartnerRequestKey $sourcePartnerRequestKey })
            targetLearnerProfileId = $targetLearnerProfileId
        }

        $blockerEmail = Get-SeedValue -Item $block -PropertyName "blockerEmail" -Default (Get-RequiredProfileEmailByKey -EmailsByKey $emailsByProfileKey -ProfileKey (Get-SeedValue -Item $block -PropertyName "blockerProfileKey" -Required))
        Invoke-JsonRequest -Uri "$ApiBaseUrl/api/catalog/moderation/blocks?blockerEmail=$(Escape-Url $blockerEmail)" -Method Post -Body $body | Out-Null
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
