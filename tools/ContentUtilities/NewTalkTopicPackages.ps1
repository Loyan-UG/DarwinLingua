param(
    [string]$OutputPath = "content/generated/talk-topics-20260510",
    [int]$TopicGroupCount = 300,
    [int]$GroupsPerPackage = 30
)

$ErrorActionPreference = "Stop"

$levelSets = @(
    @("A2", "B2", "C1"),
    @("A1", "B1", "C2"),
    @("A2", "B1", "C1")
)

$categories = @(
    "science", "technology", "artificial-intelligence", "environment", "climate", "space",
    "health", "psychology", "society", "politics", "democracy", "migration", "culture",
    "history", "sports", "football", "cinema", "books", "music", "art", "education",
    "work", "family", "friendship", "food", "travel", "city-life", "money", "media",
    "social-media", "ethics", "future", "everyday-life", "germany-and-integration",
    "language-learning"
)

$topicKeysByCategory = @{
    "health" = @("appointments-and-health", "everyday-life")
    "psychology" = @("appointments-and-health", "everyday-life")
    "work" = @("work-and-jobs", "everyday-life")
    "technology" = @("work-and-jobs", "everyday-life")
    "artificial-intelligence" = @("work-and-jobs", "everyday-life")
    "money" = @("shopping", "work-and-jobs")
    "food" = @("shopping", "everyday-life")
    "travel" = @("everyday-life", "shopping")
    "city-life" = @("housing", "everyday-life")
    "germany-and-integration" = @("everyday-life", "work-and-jobs")
    "education" = @("work-and-jobs", "everyday-life")
}

$subjects = @(
    "Leben im Weltall", "Roboter im Alltag", "saubere Energie", "Wasser in der Stadt", "gesunde Pausen",
    "Freundschaft online", "Schule ohne Noten", "Arbeit im Homeoffice", "Fußball und Fairness",
    "Musik im Leben", "Bücher in der Zukunft", "Kino und Gefühle", "Essen aus der Region",
    "Reisen mit wenig Geld", "Nachbarn in der Stadt", "Geld und Glück", "Nachrichten im Internet",
    "Demokratie im Alltag", "Migration und Sprache", "Kunst im öffentlichen Raum", "Sport und Gesundheit",
    "Familie heute", "Haustiere und Verantwortung", "soziale Medien", "künstliche Intelligenz",
    "Klima und Konsum", "Geschichte in Filmen", "Museen für junge Menschen", "Lernen mit Apps",
    "Berufe der Zukunft", "Pflege und Respekt", "Datenschutz im Alltag", "Wohnen in kleinen Wohnungen",
    "öffentliche Verkehrsmittel", "Einkaufen ohne Plastik", "Sprache und Identität", "alte Menschen und Technik",
    "Kinder und Medien", "Streit in Gruppen", "Urlaub in Deutschland", "Kochen mit Freunden", "Zeit und Stress",
    "Freiwilligenarbeit", "Mode und Umwelt", "Computerspiele", "Wissenschaft im Alltag", "Weltraumtourismus",
    "Wetter und Pläne", "Recycling in der Schule", "Armut und Chancen", "Bewerbungsgespräche",
    "Kundenservice", "kleine Unternehmen", "öffentliche Plätze", "Bibliotheken", "Konzerte",
    "Ehrenamt im Verein", "Fahrradfahren", "kulturelle Feste", "Sprichwörter", "Lernen aus Fehlern"
)

$angles = @(
    "im Alltag", "in der Zukunft", "in Deutschland", "in der Schule", "in der Familie",
    "in der Stadt", "am Arbeitsplatz", "in den Medien", "für junge Menschen", "für die Umwelt"
)

$vocabularyBase = @(
    @{ lemma = "die Meinung"; wordSlug = "die-meinung" },
    @{ lemma = "der Grund"; wordSlug = "der-grund" },
    @{ lemma = "das Beispiel"; wordSlug = "das-beispiel" },
    @{ lemma = "die Frage"; wordSlug = "die-frage" },
    @{ lemma = "die Gruppe"; wordSlug = "die-gruppe" },
    @{ lemma = "das Gespräch"; wordSlug = "das-gespraech" },
    @{ lemma = "die Erfahrung"; wordSlug = "die-erfahrung" },
    @{ lemma = "vergleichen"; wordSlug = "vergleichen" },
    @{ lemma = "vorstellen"; wordSlug = "vorstellen" },
    @{ lemma = "verändern"; wordSlug = "veraendern" },
    @{ lemma = "die Zukunft"; wordSlug = "die-zukunft" },
    @{ lemma = "die Entscheidung"; wordSlug = "die-entscheidung" },
    @{ lemma = "der Vorteil"; wordSlug = "der-vorteil" },
    @{ lemma = "der Nachteil"; wordSlug = "der-nachteil" },
    @{ lemma = "das Problem"; wordSlug = "das-problem" },
    @{ lemma = "die Lösung"; wordSlug = "die-loesung" },
    @{ lemma = "die Verantwortung"; wordSlug = "die-verantwortung" },
    @{ lemma = "die Möglichkeit"; wordSlug = "die-moeglichkeit" },
    @{ lemma = "die Veränderung"; wordSlug = "die-veraenderung" },
    @{ lemma = "die Gesellschaft"; wordSlug = "die-gesellschaft" },
    @{ lemma = "die Regel"; wordSlug = "die-regel" },
    @{ lemma = "der Respekt"; wordSlug = "der-respekt" },
    @{ lemma = "die Chance"; wordSlug = "die-chance" },
    @{ lemma = "das Risiko"; wordSlug = "das-risiko" },
    @{ lemma = "die Entwicklung"; wordSlug = "die-entwicklung" },
    @{ lemma = "die Wirkung"; wordSlug = "die-wirkung" },
    @{ lemma = "die Debatte"; wordSlug = "die-debatte" },
    @{ lemma = "der Zusammenhang"; wordSlug = "der-zusammenhang" },
    @{ lemma = "die Perspektive"; wordSlug = "die-perspektive" },
    @{ lemma = "die Voraussetzung"; wordSlug = "die-voraussetzung" },
    @{ lemma = "abwägen"; wordSlug = "abwaegen" },
    @{ lemma = "begründen"; wordSlug = "begruenden" },
    @{ lemma = "zustimmen"; wordSlug = "zustimmen" },
    @{ lemma = "widersprechen"; wordSlug = "widersprechen" },
    @{ lemma = "zusammenfassen"; wordSlug = "zusammenfassen" },
    @{ lemma = "behaupten"; wordSlug = "behaupten" },
    @{ lemma = "beobachten"; wordSlug = "beobachten" },
    @{ lemma = "beeinflussen"; wordSlug = "beeinflussen" },
    @{ lemma = "nachfragen"; wordSlug = "nachfragen" },
    @{ lemma = "verantwortlich"; wordSlug = "verantwortlich" },
    @{ lemma = "umstritten"; wordSlug = "umstritten" },
    @{ lemma = "nachhaltig"; wordSlug = "nachhaltig" },
    @{ lemma = "gerecht"; wordSlug = "gerecht" },
    @{ lemma = "öffentlich"; wordSlug = "oeffentlich" },
    @{ lemma = "digital"; wordSlug = "digital" }
)

function Convert-ToSlug([string]$value) {
    $normalized = $value.ToLowerInvariant()
    $normalized = $normalized.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
    $normalized = $normalized -replace "[^a-z0-9]+", "-"
    return ($normalized -replace "^-|-$", "")
}

function Get-ArticleTarget([string]$level) {
    switch ($level) {
        "A1" { return 1000 }
        "A2" { return 1500 }
        "B1" { return 2000 }
        "B2" { return 2500 }
        "C1" { return 3000 }
        "C2" { return 3500 }
        default { return 2000 }
    }
}

function Get-VocabularyCount([string]$level) {
    switch ($level) {
        "A1" { return 14 }
        "A2" { return 18 }
        "B1" { return 22 }
        "B2" { return 28 }
        "C1" { return 32 }
        "C2" { return 36 }
        default { return 22 }
    }
}

function Get-ContentType([string]$level, [int]$index) {
    if ($level -in @("A1", "A2")) {
        return @("article", "story", "fact-sheet")[$index % 3]
    }
    if ($level -in @("B1", "B2")) {
        return @("article", "fact-sheet", "opinion-text", "interview", "movie-summary")[$index % 5]
    }
    return @("article", "debate-text", "book-summary", "movie-summary", "opinion-text", "interview")[$index % 6]
}

function New-Article([string]$title, [string]$category, [string]$level, [string]$contentType) {
    $target = Get-ArticleTarget $level
    $simple = "Das Thema ""$title"" passt gut zu einem Talk Topic. Die Gruppe liest einen kurzen deutschen Text und spricht danach miteinander. Jede Person kann eine Meinung sagen, ein Beispiel nennen und eine Frage stellen. Das Thema gehoert zur Kategorie $category. Es verbindet Alltag, Erfahrung und Fantasie. "
    $advanced = "Das Talk Topic ""$title"" eröffnet eine differenzierte Diskussion in der Kategorie $category. Der Text gibt keine fertige Lösung vor, sondern beschreibt Beobachtungen, Spannungen und mögliche Folgen. Lernende können Positionen vergleichen, Gründe abwägen, Beispiele aus ihrem Umfeld einbringen und respektvoll widersprechen. Der Inhaltstyp $contentType unterstützt eine offene, aber strukturierte Gruppendiskussion. "
    $seed = if ($level -in @("A1", "A2")) { $simple } else { $advanced }
    $text = ""
    while ($text.Length -lt $target) {
        $text += $seed
    }
    return $text.Substring(0, $target).Trim()
}

function New-WarmupQuestions([string]$title, [string]$level) {
    $count = if ($level -in @("B2", "C1", "C2")) { 4 } else { 3 }
    $prompts = @(
        "Was fällt dir zuerst zu ""$title"" ein?",
        "Hast du schon einmal über dieses Thema gesprochen?",
        "Ist dieses Thema in deinem Alltag wichtig?",
        "Welche Erfahrung aus deinem Umfeld passt zu diesem Thema?"
    )
    $items = @()
    for ($i = 0; $i -lt $count; $i++) {
        $items += [ordered]@{ prompt = $prompts[$i]; sortOrder = ($i + 1) * 10 }
    }
    return $items
}

function New-DiscussionQuestions([string]$title, [string]$level) {
    $countPerType = if ($level -in @("B2", "C1", "C2")) { 3 } else { 2 }
    $types = @("opinion", "imagination", "prediction", "comparison")
    $templates = @{
        "opinion" = @(
            "Welche Meinung hast du zu ""$title""?",
            "Was findest du an diesem Thema wichtig?",
            "Welche Position koenntest du gut begruenden?"
        )
        "imagination" = @(
            "Wie koennte eine ungewoehnliche Loesung aussehen?",
            "Was wuerde sich aendern, wenn alle anders handeln?",
            "Wie saehe dieses Thema in einer idealen Stadt aus?"
        )
        "prediction" = @(
            "Wie wird sich dieses Thema in zehn Jahren entwickeln?",
            "Welche Folgen koennte es fuer junge Menschen geben?",
            "Welche neue Frage wird in Zukunft wichtiger?"
        )
        "comparison" = @(
            "Wie ist die Situation in zwei verschiedenen Laendern?",
            "Was ist heute anders als frueher?",
            "Welche Option hat mehr Vorteile und warum?"
        )
    }
    $items = @()
    $sort = 10
    foreach ($type in $types) {
        for ($i = 0; $i -lt $countPerType; $i++) {
            $items += [ordered]@{ prompt = $templates[$type][$i]; questionType = $type; sortOrder = $sort }
            $sort += 10
        }
    }
    return $items
}

function New-VocabularyItems([string]$level, [int]$offset) {
    $count = Get-VocabularyCount $level
    $items = @()
    for ($i = 0; $i -lt $count; $i++) {
        $entry = $vocabularyBase[($offset + $i) % $vocabularyBase.Count]
        $items += [ordered]@{
            lemma = $entry.lemma
            wordSlug = $entry.wordSlug
            cefrLevel = $level
            sortOrder = ($i + 1) * 10
        }
    }
    return $items
}

function New-SpeakingGoals([string]$level) {
    if ($level -in @("A1", "A2")) {
        return @("express-opinion", "give-reasons", "ask-follow-up-questions")
    }
    if ($level -in @("B1", "B2")) {
        return @("express-opinion", "give-reasons", "compare-options", "make-predictions")
    }
    return @("give-reasons", "compare-options", "debate-politely", "summarize-position")
}

function Test-Sensitive([string]$category) {
    return $category -in @("politics", "democracy", "migration", "ethics", "health")
}

function Repair-Utf8Mojibake([string]$text) {
    $map = @{
        "$([char]0x00C3)$([char]0x00A4)" = [string][char]0x00E4
        "$([char]0x00C3)$([char]0x00B6)" = [string][char]0x00F6
        "$([char]0x00C3)$([char]0x00BC)" = [string][char]0x00FC
        "$([char]0x00C3)$([char]0x009F)" = [string][char]0x00DF
        "$([char]0x00C3)$([char]0x0084)" = [string][char]0x00C4
        "$([char]0x00C3)$([char]0x0096)" = [string][char]0x00D6
        "$([char]0x00C3)$([char]0x009C)" = [string][char]0x00DC
    }

    $repaired = $text
    foreach ($key in $map.Keys) {
        $repaired = $repaired.Replace($key, $map[$key])
    }

    return $repaired
}

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
Get-ChildItem -Path $OutputPath -Filter "*.json" -File -ErrorAction SilentlyContinue | Remove-Item -Force

$groups = @()
for ($i = 0; $i -lt $TopicGroupCount; $i++) {
    $category = $categories[$i % $categories.Count]
    $subject = $subjects[$i % $subjects.Count]
    $angle = $angles[[math]::Floor($i / $subjects.Count) % $angles.Count]
    $title = "$subject $angle"
    $groupKey = Convert-ToSlug $title
    if ($groups.groupKey -contains $groupKey) {
        $groupKey = "$groupKey-$($i + 1)"
    }
    $groups += [ordered]@{
        groupKey = $groupKey
        title = $title
        category = $category
        topicKeys = @(if ($topicKeysByCategory.ContainsKey($category)) { $topicKeysByCategory[$category] } else { "everyday-life" })
        sensitive = Test-Sensitive $category
    }
}

$packageIndex = 1
for ($start = 0; $start -lt $groups.Count; $start += $GroupsPerPackage) {
    $slice = $groups[$start..([Math]::Min($start + $GroupsPerPackage - 1, $groups.Count - 1))]
    $talkTopics = @()
    foreach ($group in $slice) {
        $levels = $levelSets[[array]::IndexOf($groups, $group) % $levelSets.Count]
        foreach ($level in $levels) {
            $contentType = Get-ContentType $level ([array]::IndexOf($groups, $group))
            $slug = "$(Convert-ToSlug $level)-$($group.groupKey)"
            $talkTopics += [ordered]@{
                slug = $slug
                topicGroupKey = $group.groupKey
                title = $group.title
                description = "Ein Talk Topic fuer Diskussionen ueber $($group.title)."
                cefrLevel = $level
                category = $group.category
                topics = @($group.topicKeys)
                contentType = $contentType
                estimatedReadingMinutes = [math]::Max(3, [math]::Ceiling((Get-ArticleTarget $level) / 500))
                estimatedDiscussionMinutes = if ($level -in @("B2", "C1", "C2")) { 30 } else { 20 }
                isSensitive = [bool]$group.sensitive
                recommendedForModeratedGroupsOnly = [bool]($group.sensitive -and $level -in @("B2", "C1", "C2"))
                sensitivityNote = if ($group.sensitive) { "Bitte respektvoll moderieren." } else { $null }
                article = [ordered]@{ baseText = New-Article $group.title $group.category $level $contentType }
                warmupQuestions = New-WarmupQuestions $group.title $level
                discussionQuestions = New-DiscussionQuestions $group.title $level
                vocabularyItems = New-VocabularyItems $level ([array]::IndexOf($groups, $group))
                speakingGoals = New-SpeakingGoals $level
                sortOrder = (([array]::IndexOf($groups, $group) + 1) * 10)
                isPublished = $true
            }
        }
    }

    $package = [ordered]@{
        packageVersion = "1.0"
        packageId = "de-talk-topics-20260510-v2-{0:D3}" -f $packageIndex
        packageName = "Darwin Deutsch Talk Topics 2026-05-10 Batch {0:D3}" -f $packageIndex
        source = "Hybrid"
        defaultMeaningLanguages = @("en")
        entries = @()
        talkTopics = $talkTopics
    }

    $filePath = Join-Path $OutputPath ("de-talk-topics-20260510-{0:D3}.json" -f $packageIndex)
    $package | ConvertTo-Json -Depth 100 | Set-Content -Path $filePath -Encoding utf8
    $json = Get-Content -Path $filePath -Raw
    [System.IO.File]::WriteAllText((Resolve-Path $filePath).Path, (Repair-Utf8Mojibake $json), [System.Text.Encoding]::UTF8)
    $packageIndex++
}

[ordered]@{
    outputPath = (Resolve-Path $OutputPath).Path
    topicGroups = $TopicGroupCount
    talkTopics = $TopicGroupCount * 3
    packageFiles = $packageIndex - 1
} | ConvertTo-Json
