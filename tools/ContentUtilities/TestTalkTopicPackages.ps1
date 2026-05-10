param(
    [Parameter(Mandatory = $true)]
    [string]$ContentPath
)

$ErrorActionPreference = "Stop"

$ranges = @{
    "A1" = @(900, 1100, 3, 2, 12, 18)
    "A2" = @(1400, 1600, 3, 2, 15, 22)
    "B1" = @(1900, 2100, 3, 2, 18, 26)
    "B2" = @(2400, 2600, 4, 3, 22, 32)
    "C1" = @(2900, 3100, 4, 3, 26, 38)
    "C2" = @(3400, 3600, 4, 3, 30, 45)
}
$allowedContentTypes = @("article", "book-summary", "movie-summary", "story", "fact-sheet", "opinion-text", "interview", "debate-text")
$allowedQuestionTypes = @("opinion", "imagination", "prediction", "comparison")
$allowedSpeakingGoals = @("express-opinion", "give-reasons", "agree-disagree", "ask-follow-up-questions", "compare-options", "make-predictions", "describe-experiences", "imagine-possibilities", "debate-politely", "summarize-position")
$internalArticleFragments = @(
    "category ",
    "contentType",
    "Talk Topic",
    "artificial-intelligence",
    "city-life",
    "social-media",
    "germany-and-integration",
    "language-learning"
)
$asciiGermanSpellingPattern = "\b(fuer|ueber|koennte|koenntest|wuerde|wuerdest|aendern|frueher|Laendern|Loesung|saehe|ungewoehnliche|begruenden)\b"
$repeatedTitleEndingPattern = "im Alltag im Alltag|in der Familie in der Familie|in der Zukunft in der Zukunft|in Deutschland in Deutschland|in der Schule in der Schule|am Arbeitsplatz am Arbeitsplatz|in den Medien in den Medien|für junge Menschen für junge Menschen|für die Umwelt für die Umwelt"

$files = Get-ChildItem -Path $ContentPath -Filter "*.json" -File | Sort-Object Name
$errors = New-Object System.Collections.Generic.List[string]
$slugs = New-Object System.Collections.Generic.HashSet[string]
$groups = New-Object System.Collections.Generic.HashSet[string]
$articleHashes = New-Object System.Collections.Generic.HashSet[string]
$levelCounts = @{}
$contentTypeCounts = @{}
$topicCount = 0

foreach ($file in $files) {
    $package = Get-Content $file.FullName -Raw | ConvertFrom-Json
    foreach ($topic in @($package.talkTopics)) {
        $topicCount++
        if (-not $slugs.Add([string]$topic.slug)) {
            $errors.Add("Duplicate TalkTopic slug '$($topic.slug)' in $($file.Name).")
        }
        $title = [string]$topic.title
        if ([string]::IsNullOrWhiteSpace($title)) {
            $errors.Add("'$($topic.slug)' must have a title.")
        }
        elseif ($title.Substring(0, 1) -cmatch "[a-zäöü]") {
            $errors.Add("'$($topic.slug)' title should start with an uppercase character.")
        }
        if ($title -match $repeatedTitleEndingPattern) {
            $errors.Add("'$($topic.slug)' title contains a repeated mechanical ending.")
        }
        if ($title -match $asciiGermanSpellingPattern) {
            $errors.Add("'$($topic.slug)' title contains ASCII German spelling; use canonical umlauts.")
        }
        $description = [string]$topic.description
        if ($description -match $asciiGermanSpellingPattern) {
            $errors.Add("'$($topic.slug)' description contains ASCII German spelling; use canonical umlauts.")
        }
        [void]$groups.Add([string]$topic.topicGroupKey)
        $level = [string]$topic.cefrLevel
        if (-not $ranges.ContainsKey($level)) {
            $errors.Add("Unsupported CEFR '$level' in '$($topic.slug)'.")
            continue
        }
        $range = $ranges[$level]
        $article = ([string]$topic.article.baseText).Trim()
        if ($article -match "Ã|Â|�") {
            $errors.Add("'$($topic.slug)' article contains mojibake or replacement characters.")
        }
        foreach ($fragment in $internalArticleFragments) {
            if ($article.Contains($fragment, [System.StringComparison]::Ordinal)) {
                $errors.Add("'$($topic.slug)' article contains internal/template fragment '$fragment'.")
            }
        }
        if ($article -match $asciiGermanSpellingPattern) {
            $errors.Add("'$($topic.slug)' article contains ASCII German spelling; use canonical umlauts.")
        }
        if ($article.Length -lt $range[0] -or $article.Length -gt $range[1]) {
            $errors.Add("'$($topic.slug)' article length $($article.Length) is outside $($range[0])-$($range[1]).")
        }
        $hashInput = "$($topic.topicGroupKey)|$article"
        if (-not $articleHashes.Add($hashInput)) {
            $errors.Add("'$($topic.slug)' duplicates an article in its topic group.")
        }
        if ($null -ne $topic.article.translations -and @($topic.article.translations).Count -gt 0) {
            $errors.Add("'$($topic.slug)' contains article translations.")
        }
        if ($allowedContentTypes -notcontains [string]$topic.contentType) {
            $errors.Add("'$($topic.slug)' has unsupported contentType '$($topic.contentType)'.")
        }
        if (@($topic.warmupQuestions).Count -lt $range[2]) {
            $errors.Add("'$($topic.slug)' has too few warmup questions.")
        }
        foreach ($question in @($topic.warmupQuestions)) {
            if ([string]$question.prompt -match "Ã|Â|�") {
                $errors.Add("'$($topic.slug)' contains a warmup question with mojibake or replacement characters.")
            }
            if ([string]$question.prompt -match $asciiGermanSpellingPattern) {
                $errors.Add("'$($topic.slug)' contains a warmup question with ASCII German spelling; use canonical umlauts.")
            }
            if ($null -ne $question.translations -and @($question.translations).Count -gt 0) {
                $errors.Add("'$($topic.slug)' contains warmup question translations.")
            }
        }
        foreach ($type in $allowedQuestionTypes) {
            $count = @($topic.discussionQuestions | Where-Object { $_.questionType -eq $type }).Count
            if ($count -lt $range[3]) {
                $errors.Add("'$($topic.slug)' has $count '$type' questions; expected at least $($range[3]).")
            }
        }
        foreach ($question in @($topic.discussionQuestions)) {
            if ([string]$question.prompt -match "Ã|Â|�") {
                $errors.Add("'$($topic.slug)' contains a discussion question with mojibake or replacement characters.")
            }
            if ([string]$question.prompt -match $asciiGermanSpellingPattern) {
                $errors.Add("'$($topic.slug)' contains a discussion question with ASCII German spelling; use canonical umlauts.")
            }
            if ($allowedQuestionTypes -notcontains [string]$question.questionType) {
                $errors.Add("'$($topic.slug)' has unsupported questionType '$($question.questionType)'.")
            }
            if ($null -ne $question.translations -and @($question.translations).Count -gt 0) {
                $errors.Add("'$($topic.slug)' contains discussion question translations.")
            }
        }
        $vocabularyCount = @($topic.vocabularyItems).Count
        if ($vocabularyCount -lt $range[4] -or $vocabularyCount -gt $range[5]) {
            $errors.Add("'$($topic.slug)' has $vocabularyCount vocabulary items; expected $($range[4])-$($range[5]).")
        }
        foreach ($vocabularyItem in @($topic.vocabularyItems)) {
            if ([string]$vocabularyItem.lemma -match "Ã|Â|�") {
                $errors.Add("'$($topic.slug)' contains a vocabulary lemma with mojibake or replacement characters.")
            }
            if ([string]$vocabularyItem.lemma -match $asciiGermanSpellingPattern) {
                $errors.Add("'$($topic.slug)' contains a vocabulary lemma with ASCII German spelling; use canonical umlauts.")
            }
        }
        $speakingGoals = @($topic.speakingGoals)
        if ($speakingGoals.Count -lt 2 -or $speakingGoals.Count -gt 5) {
            $errors.Add("'$($topic.slug)' must have 2-5 speaking goals.")
        }
        foreach ($goal in $speakingGoals) {
            if ($allowedSpeakingGoals -notcontains [string]$goal) {
                $errors.Add("'$($topic.slug)' has unsupported speaking goal '$goal'.")
            }
        }
        if (-not $levelCounts.ContainsKey($level)) {
            $levelCounts[$level] = 0
        }
        $levelCounts[$level] = 1 + $levelCounts[$level]

        $contentType = [string]$topic.contentType
        if (-not $contentTypeCounts.ContainsKey($contentType)) {
            $contentTypeCounts[$contentType] = 0
        }
        $contentTypeCounts[$contentType] = 1 + $contentTypeCounts[$contentType]
    }
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

[ordered]@{
    validationOk = $true
    jsonFiles = $files.Count
    topicGroups = $groups.Count
    talkTopics = $topicCount
    cefrDistribution = $levelCounts
    contentTypeDistribution = $contentTypeCounts
} | ConvertTo-Json -Depth 5
