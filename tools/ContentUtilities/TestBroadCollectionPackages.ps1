param(
    [string]$ContentPath = "content/generated/broad-collections-20260510",
    [int]$MinWordBatchSize = 25,
    [int]$MaxWordBatchSize = 35,
    [int]$ExpectedCollectionWordCount = 100
)

$ErrorActionPreference = "Stop"

$validTopics = @(
    "everyday-life",
    "housing",
    "shopping",
    "work-and-jobs",
    "appointments-and-health"
)

if (-not (Test-Path -LiteralPath $ContentPath)) {
    throw "Content path was not found: $ContentPath"
}

$errors = [System.Collections.Generic.List[string]]::new()
$collectionSlugs = [System.Collections.Generic.List[string]]::new()
$wordKeys = [System.Collections.Generic.List[string]]::new()
$wordBatchSizes = @()

$jsonFiles = @(Get-ChildItem -LiteralPath $ContentPath -Filter "*.json" -File | Sort-Object Name)
if ($jsonFiles.Count -eq 0) {
    throw "No JSON content packages were found in $ContentPath."
}

foreach ($file in $jsonFiles) {
    try {
        $json = Get-Content -Raw -LiteralPath $file.FullName | ConvertFrom-Json
    }
    catch {
        $errors.Add("invalid-json:$($file.Name):$($_.Exception.Message)")
        continue
    }

    if ($null -eq $json.packageVersion) {
        $errors.Add("missing-packageVersion:$($file.Name)")
    }

    if ($null -eq $json.entries) {
        $errors.Add("missing-entries:$($file.Name)")
    }

    $entries = @($json.entries)
    if ($file.Name -like "*-words-*.json") {
        $wordBatchSizes += $entries.Count
        if ($entries.Count -lt $MinWordBatchSize -or $entries.Count -gt $MaxWordBatchSize) {
            $errors.Add("batch-size:$($file.Name):$($entries.Count)")
        }
    }

    foreach ($entry in $entries) {
        $key = "{0}|{1}|{2}" -f $entry.word, $entry.partOfSpeech, $entry.cefrLevel
        $wordKeys.Add($key)

        foreach ($topic in @($entry.topics)) {
            if ($validTopics -notcontains $topic) {
                $errors.Add("invalid-topic:$($file.Name):$($entry.word):$topic")
            }
        }

        foreach ($label in @($entry.contextLabels + $entry.usageLabels)) {
            if ($label -and $label -cnotmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
                $errors.Add("bad-label:$($file.Name):$($entry.word):$label")
            }
        }
    }

    foreach ($collection in @($json.collections)) {
        $collectionSlugs.Add($collection.slug)

        if ($collection.slug -cnotmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
            $errors.Add("bad-collection-slug:$($collection.slug)")
        }

        $refs = @($collection.words | ForEach-Object {
            "{0}|{1}|{2}" -f $_.word, $_.partOfSpeech, $_.cefrLevel
        })

        if ($refs.Count -ne $ExpectedCollectionWordCount) {
            $errors.Add("collection-count:$($collection.slug):$($refs.Count)")
        }

        foreach ($duplicate in ($refs | Group-Object | Where-Object Count -gt 1)) {
            $errors.Add("duplicate-ref:$($collection.slug):$($duplicate.Name)")
        }
    }
}

foreach ($duplicate in ($collectionSlugs | Group-Object | Where-Object Count -gt 1)) {
    $errors.Add("duplicate-collection-slug:$($duplicate.Name)")
}

foreach ($duplicate in ($wordKeys | Group-Object | Where-Object Count -gt 1)) {
    $errors.Add("duplicate-generated-word-key:$($duplicate.Name)")
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

$summary = [ordered]@{
    validationOk = $true
    jsonFiles = $jsonFiles.Count
    wordBatches = $wordBatchSizes.Count
    minBatchSize = if ($wordBatchSizes.Count -gt 0) { ($wordBatchSizes | Measure-Object -Minimum).Minimum } else { 0 }
    maxBatchSize = if ($wordBatchSizes.Count -gt 0) { ($wordBatchSizes | Measure-Object -Maximum).Maximum } else { 0 }
    collectionSlugs = $collectionSlugs.Count
    uniqueWordKeys = @($wordKeys | Sort-Object -Unique).Count
}

$summary | ConvertTo-Json
