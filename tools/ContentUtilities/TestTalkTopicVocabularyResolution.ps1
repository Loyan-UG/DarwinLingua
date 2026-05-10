param(
    [string]$ApiBaseUrl = "http://localhost:53945",
    [string]$PrimaryMeaningLanguageCode = "en",
    [string]$SecondaryMeaningLanguageCode = "fa",
    [decimal]$MinimumResolvedPercent = 0,
    [int]$UnresolvedSampleLimit = 50
)

$ErrorActionPreference = "Stop"

function Join-ApiPath([string]$baseUrl, [string]$path) {
    return "$($baseUrl.TrimEnd('/'))/$($path.TrimStart('/'))"
}

function Get-Json([string]$url) {
    return Invoke-RestMethod -Method Get -Uri $url -TimeoutSec 60
}

$topicsUrl = Join-ApiPath $ApiBaseUrl "api/catalog/talk-topics"
[object[]]$topics = Get-Json $topicsUrl

$totalVocabularyItems = 0
$resolvedVocabularyItems = 0
$unresolvedByLemma = @{}
$levelStats = @{}

foreach ($topic in $topics) {
    $slug = [Uri]::EscapeDataString([string]$topic.slug)
    $detailUrl = Join-ApiPath $ApiBaseUrl "api/catalog/talk-topics/$slug"
    $detailUrl = "$detailUrl`?primaryMeaningLanguageCode=$([Uri]::EscapeDataString($PrimaryMeaningLanguageCode))"

    if (-not [string]::IsNullOrWhiteSpace($SecondaryMeaningLanguageCode)) {
        $detailUrl = "$detailUrl&secondaryMeaningLanguageCode=$([Uri]::EscapeDataString($SecondaryMeaningLanguageCode))"
    }

    $detail = Get-Json $detailUrl
    $level = [string]$detail.cefrLevel

    if (-not $levelStats.ContainsKey($level)) {
        $levelStats[$level] = [ordered]@{
            topics = 0
            totalVocabularyItems = 0
            resolvedVocabularyItems = 0
            resolvedPercent = 0
        }
    }

    $levelStats[$level].topics++

    foreach ($item in @($detail.vocabularyItems)) {
        $totalVocabularyItems++
        $levelStats[$level].totalVocabularyItems++

        if ($item.isResolved) {
            $resolvedVocabularyItems++
            $levelStats[$level].resolvedVocabularyItems++
            continue
        }

        $lemma = ([string]$item.lemma).Trim()
        if (-not $unresolvedByLemma.ContainsKey($lemma)) {
            $unresolvedByLemma[$lemma] = [ordered]@{
                lemma = $lemma
                count = 0
                exampleSlugs = New-Object System.Collections.Generic.List[string]
            }
        }

        $unresolvedByLemma[$lemma].count++
        if ($unresolvedByLemma[$lemma].exampleSlugs.Count -lt 5) {
            $unresolvedByLemma[$lemma].exampleSlugs.Add([string]$detail.slug)
        }
    }
}

$resolvedPercent = 0
if ($totalVocabularyItems -gt 0) {
    $resolvedPercent = [math]::Round(($resolvedVocabularyItems * 100.0) / $totalVocabularyItems, 2)
}

foreach ($level in @($levelStats.Keys)) {
    $levelTotal = $levelStats[$level].totalVocabularyItems
    if ($levelTotal -gt 0) {
        $levelStats[$level].resolvedPercent = [math]::Round(($levelStats[$level].resolvedVocabularyItems * 100.0) / $levelTotal, 2)
    }
}

$unresolvedLemmas = @(
    $unresolvedByLemma.Values |
        Sort-Object @{ Expression = { $_.count }; Descending = $true }, @{ Expression = { $_.lemma }; Ascending = $true } |
        Select-Object -First $UnresolvedSampleLimit
)

$result = [ordered]@{
    apiBaseUrl = $ApiBaseUrl
    topicCount = $topics.Count
    totalVocabularyItems = $totalVocabularyItems
    resolvedVocabularyItems = $resolvedVocabularyItems
    unresolvedVocabularyItems = $totalVocabularyItems - $resolvedVocabularyItems
    resolvedPercent = $resolvedPercent
    distinctUnresolvedLemmas = $unresolvedByLemma.Count
    cefrDistribution = $levelStats
    unresolvedLemmaSample = $unresolvedLemmas
}

$result | ConvertTo-Json -Depth 8

if ($MinimumResolvedPercent -gt 0 -and $resolvedPercent -lt $MinimumResolvedPercent) {
    Write-Error "Talk Topic vocabulary resolution is $resolvedPercent%; expected at least $MinimumResolvedPercent%."
    exit 1
}
