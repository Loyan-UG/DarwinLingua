param(
    [string]$OutputPath = "content/generated/talk-topics-20260510-a1-a2-expansion-1",
    [int]$TopicsPerPackage = 30,
    [string]$PackageIdPrefix = "de-talk-topics-20260510-a1-a2-expansion-1-v1",
    [string]$PackageNamePrefix = "Darwin Deutsch Talk Topics A1-A2 Expansion 1 2026-05-10 v1",
    [string]$GroupPrefix = "grundstufe-1",
    [string]$TopicSet = "first"
)

$ErrorActionPreference = "Stop"

$topicsByCategory = @{
    "everyday-life" = @("everyday-life")
    "family" = @("everyday-life")
    "food" = @("everyday-life", "shopping")
    "shopping" = @("shopping")
    "travel" = @("everyday-life")
    "housing" = @("housing", "everyday-life")
    "health" = @("appointments-and-health", "everyday-life")
    "work" = @("work-and-jobs", "everyday-life")
    "education" = @("everyday-life", "work-and-jobs")
}

$topicSpecs = @(
    @{ title = "Mein Morgen"; category = "everyday-life"; place = "zu Hause"; action = "den Tag beginnen"; object = "die Zeit" },
    @{ title = "Frühstück zu Hause"; category = "food"; place = "in der Küche"; action = "frühstücken"; object = "das Essen" },
    @{ title = "Der Weg zum Kurs"; category = "travel"; place = "in der Stadt"; action = "fahren"; object = "der Bus" },
    @{ title = "Im Supermarkt"; category = "shopping"; place = "im Supermarkt"; action = "einkaufen"; object = "der Preis" },
    @{ title = "Meine Familie"; category = "family"; place = "zu Hause"; action = "sprechen"; object = "die Familie" },
    @{ title = "Ein Anruf"; category = "everyday-life"; place = "am Telefon"; action = "anrufen"; object = "die Frage" },
    @{ title = "Ein Termin"; category = "health"; place = "in der Praxis"; action = "warten"; object = "der Termin" },
    @{ title = "Meine Wohnung"; category = "housing"; place = "in der Wohnung"; action = "aufräumen"; object = "das Zimmer" },
    @{ title = "Nachbarn treffen"; category = "housing"; place = "im Haus"; action = "grüßen"; object = "der Nachbar" },
    @{ title = "In der Bäckerei"; category = "shopping"; place = "in der Bäckerei"; action = "bezahlen"; object = "das Brot" },
    @{ title = "Mittagspause"; category = "work"; place = "bei der Arbeit"; action = "Pause machen"; object = "die Pause" },
    @{ title = "Deutsch im Kurs"; category = "education"; place = "im Kurs"; action = "lernen"; object = "die Sprache" },
    @{ title = "Ein kurzer Spaziergang"; category = "health"; place = "im Park"; action = "spazieren gehen"; object = "die Bewegung" },
    @{ title = "Am Abend"; category = "everyday-life"; place = "zu Hause"; action = "sich ausruhen"; object = "der Abend" },
    @{ title = "Kleidung kaufen"; category = "shopping"; place = "im Geschäft"; action = "anprobieren"; object = "die Größe" },
    @{ title = "Im Café"; category = "food"; place = "im Café"; action = "bestellen"; object = "der Kaffee" },
    @{ title = "Mit dem Zug fahren"; category = "travel"; place = "am Bahnhof"; action = "einsteigen"; object = "die Fahrkarte" },
    @{ title = "Das Wochenende planen"; category = "everyday-life"; place = "am Wochenende"; action = "planen"; object = "die Zeit" },
    @{ title = "Freunde besuchen"; category = "family"; place = "bei Freunden"; action = "besuchen"; object = "der Besuch" },
    @{ title = "Eine E-Mail schreiben"; category = "work"; place = "am Computer"; action = "schreiben"; object = "die Nachricht" },
    @{ title = "Beim Arzt"; category = "health"; place = "beim Arzt"; action = "fragen"; object = "der Schmerz" },
    @{ title = "Im Deutschkurs fragen"; category = "education"; place = "im Unterricht"; action = "nachfragen"; object = "die Antwort" },
    @{ title = "Ein Geschenk kaufen"; category = "shopping"; place = "im Laden"; action = "suchen"; object = "das Geschenk" },
    @{ title = "Kochen mit Freunden"; category = "food"; place = "in der Küche"; action = "kochen"; object = "das Gemüse" },
    @{ title = "Die Wohnung putzen"; category = "housing"; place = "zu Hause"; action = "putzen"; object = "die Wohnung" },
    @{ title = "Im Bus"; category = "travel"; place = "im Bus"; action = "sitzen"; object = "der Platz" },
    @{ title = "Ein Formular ausfüllen"; category = "everyday-life"; place = "im Büro"; action = "ausfüllen"; object = "der Name" },
    @{ title = "Meine Arbeit"; category = "work"; place = "bei der Arbeit"; action = "helfen"; object = "die Aufgabe" },
    @{ title = "Ein neues Wort"; category = "education"; place = "im Kurs"; action = "üben"; object = "das Wort" },
    @{ title = "Im Restaurant"; category = "food"; place = "im Restaurant"; action = "essen"; object = "die Rechnung" },
    @{ title = "Auf dem Markt"; category = "shopping"; place = "auf dem Markt"; action = "kaufen"; object = "das Obst" },
    @{ title = "Der erste Arbeitstag"; category = "work"; place = "im Büro"; action = "ankommen"; object = "der Kollege" },
    @{ title = "Ein Spaziergang im Regen"; category = "everyday-life"; place = "draußen"; action = "gehen"; object = "der Regen" },
    @{ title = "Zu Besuch bei der Familie"; category = "family"; place = "bei der Familie"; action = "erzählen"; object = "das Gespräch" },
    @{ title = "Mein Lieblingsessen"; category = "food"; place = "zu Hause"; action = "essen"; object = "der Geschmack" },
    @{ title = "Eine kleine Reise"; category = "travel"; place = "in einer anderen Stadt"; action = "reisen"; object = "die Tasche" },
    @{ title = "Im Treppenhaus"; category = "housing"; place = "im Haus"; action = "sprechen"; object = "die Tür" },
    @{ title = "Krank zu Hause"; category = "health"; place = "zu Hause"; action = "sich ausruhen"; object = "die Gesundheit" },
    @{ title = "Hausaufgaben machen"; category = "education"; place = "zu Hause"; action = "lernen"; object = "die Übung" },
    @{ title = "Ein Paket abholen"; category = "everyday-life"; place = "bei der Post"; action = "abholen"; object = "das Paket" },
    @{ title = "Im Schuhgeschäft"; category = "shopping"; place = "im Geschäft"; action = "probieren"; object = "der Schuh" },
    @{ title = "Ein Gespräch mit dem Chef"; category = "work"; place = "bei der Arbeit"; action = "sprechen"; object = "die Arbeit" },
    @{ title = "Ein ruhiger Sonntag"; category = "everyday-life"; place = "zu Hause"; action = "lesen"; object = "das Buch" },
    @{ title = "Mit Kindern spielen"; category = "family"; place = "im Park"; action = "spielen"; object = "das Kind" },
    @{ title = "Salat machen"; category = "food"; place = "in der Küche"; action = "schneiden"; object = "der Salat" },
    @{ title = "Eine Adresse suchen"; category = "travel"; place = "in der Stadt"; action = "suchen"; object = "die Adresse" },
    @{ title = "Die Miete bezahlen"; category = "housing"; place = "zu Hause"; action = "bezahlen"; object = "die Miete" },
    @{ title = "Wasser trinken"; category = "health"; place = "im Alltag"; action = "trinken"; object = "das Wasser" },
    @{ title = "Pünktlich kommen"; category = "work"; place = "zum Termin"; action = "kommen"; object = "die Uhrzeit" },
    @{ title = "Lernen mit Karten"; category = "education"; place = "zu Hause"; action = "wiederholen"; object = "der Wortschatz" }
)

if ($TopicSet -eq "second") {
    $topicSpecs = @(
        @{ title = "Mein Nachmittag"; category = "everyday-life"; place = "zu Hause"; action = "Pause machen"; object = "die Zeit" },
        @{ title = "Abendessen mit der Familie"; category = "food"; place = "am Tisch"; action = "essen"; object = "das Essen" },
        @{ title = "Zum Bahnhof gehen"; category = "travel"; place = "am Bahnhof"; action = "warten"; object = "der Zug" },
        @{ title = "Im Drogeriemarkt"; category = "shopping"; place = "im Geschäft"; action = "kaufen"; object = "die Seife" },
        @{ title = "Meine Schwester"; category = "family"; place = "zu Hause"; action = "erzählen"; object = "die Schwester" },
        @{ title = "Eine Nachricht lesen"; category = "everyday-life"; place = "am Handy"; action = "lesen"; object = "die Nachricht" },
        @{ title = "In der Apotheke"; category = "health"; place = "in der Apotheke"; action = "fragen"; object = "die Medizin" },
        @{ title = "Das Schlafzimmer"; category = "housing"; place = "in der Wohnung"; action = "schlafen"; object = "das Bett" },
        @{ title = "Hilfe im Haus"; category = "housing"; place = "im Haus"; action = "helfen"; object = "die Hilfe" },
        @{ title = "Beim Metzger"; category = "shopping"; place = "im Laden"; action = "bestellen"; object = "das Fleisch" },
        @{ title = "Der Pausenraum"; category = "work"; place = "bei der Arbeit"; action = "sitzen"; object = "die Pause" },
        @{ title = "Wörter im Heft"; category = "education"; place = "im Kurs"; action = "schreiben"; object = "das Heft" },
        @{ title = "Fünf Minuten Sport"; category = "health"; place = "zu Hause"; action = "üben"; object = "die Bewegung" },
        @{ title = "Vor dem Schlafen"; category = "everyday-life"; place = "am Abend"; action = "lesen"; object = "das Buch" },
        @{ title = "Eine Jacke suchen"; category = "shopping"; place = "im Geschäft"; action = "suchen"; object = "die Jacke" },
        @{ title = "Tee trinken"; category = "food"; place = "im Café"; action = "trinken"; object = "der Tee" },
        @{ title = "Mit der Straßenbahn fahren"; category = "travel"; place = "in der Stadt"; action = "fahren"; object = "die Haltestelle" },
        @{ title = "Samstag planen"; category = "everyday-life"; place = "am Wochenende"; action = "planen"; object = "der Samstag" },
        @{ title = "Freunde einladen"; category = "family"; place = "zu Hause"; action = "einladen"; object = "der Freund" },
        @{ title = "Eine SMS schreiben"; category = "work"; place = "am Handy"; action = "schreiben"; object = "der Text" },
        @{ title = "Kopfschmerzen haben"; category = "health"; place = "zu Hause"; action = "sich ausruhen"; object = "der Kopf" },
        @{ title = "Die Lehrerin fragen"; category = "education"; place = "im Kurs"; action = "fragen"; object = "die Lehrerin" },
        @{ title = "Blumen kaufen"; category = "shopping"; place = "im Laden"; action = "kaufen"; object = "die Blume" },
        @{ title = "Suppe kochen"; category = "food"; place = "in der Küche"; action = "kochen"; object = "die Suppe" },
        @{ title = "Das Bad putzen"; category = "housing"; place = "zu Hause"; action = "putzen"; object = "das Bad" },
        @{ title = "Im Taxi"; category = "travel"; place = "im Taxi"; action = "fahren"; object = "die Adresse" },
        @{ title = "Geburtsdatum sagen"; category = "everyday-life"; place = "im Büro"; action = "sagen"; object = "das Datum" },
        @{ title = "Eine Aufgabe bekommen"; category = "work"; place = "bei der Arbeit"; action = "arbeiten"; object = "die Aufgabe" },
        @{ title = "Ein Bild beschreiben"; category = "education"; place = "im Kurs"; action = "sprechen"; object = "das Bild" },
        @{ title = "Eine Pizza bestellen"; category = "food"; place = "im Restaurant"; action = "bestellen"; object = "die Pizza" },
        @{ title = "Gemüse auf dem Markt"; category = "shopping"; place = "auf dem Markt"; action = "kaufen"; object = "das Gemüse" },
        @{ title = "Ein neuer Kollege"; category = "work"; place = "im Büro"; action = "kennenlernen"; object = "der Kollege" },
        @{ title = "Sonne und Wind"; category = "everyday-life"; place = "draußen"; action = "gehen"; object = "das Wetter" },
        @{ title = "Großeltern besuchen"; category = "family"; place = "bei der Familie"; action = "besuchen"; object = "die Großeltern" },
        @{ title = "Lieblingsgetränk"; category = "food"; place = "zu Hause"; action = "trinken"; object = "das Getränk" },
        @{ title = "Ein Tagesausflug"; category = "travel"; place = "in einer Stadt"; action = "gehen"; object = "die Tasche" },
        @{ title = "Der Briefkasten"; category = "housing"; place = "im Haus"; action = "öffnen"; object = "der Brief" },
        @{ title = "Müde sein"; category = "health"; place = "zu Hause"; action = "schlafen"; object = "die Müdigkeit" },
        @{ title = "Eine Übung machen"; category = "education"; place = "zu Hause"; action = "üben"; object = "die Übung" },
        @{ title = "Einen Brief abholen"; category = "everyday-life"; place = "bei der Post"; action = "abholen"; object = "der Brief" },
        @{ title = "Schuhe anprobieren"; category = "shopping"; place = "im Geschäft"; action = "anprobieren"; object = "der Schuh" },
        @{ title = "Eine kurze Besprechung"; category = "work"; place = "bei der Arbeit"; action = "sprechen"; object = "die Besprechung" },
        @{ title = "Ein freier Tag"; category = "everyday-life"; place = "zu Hause"; action = "sich erholen"; object = "der Tag" },
        @{ title = "Mit dem Kind lesen"; category = "family"; place = "zu Hause"; action = "lesen"; object = "das Kind" },
        @{ title = "Obst schneiden"; category = "food"; place = "in der Küche"; action = "schneiden"; object = "das Obst" },
        @{ title = "Den Weg fragen"; category = "travel"; place = "in der Stadt"; action = "fragen"; object = "der Weg" },
        @{ title = "Die Küche aufräumen"; category = "housing"; place = "zu Hause"; action = "aufräumen"; object = "die Küche" },
        @{ title = "Täglich Wasser trinken"; category = "health"; place = "im Alltag"; action = "trinken"; object = "das Wasser" },
        @{ title = "Früh zur Arbeit kommen"; category = "work"; place = "bei der Arbeit"; action = "ankommen"; object = "die Uhrzeit" },
        @{ title = "Neue Wörter wiederholen"; category = "education"; place = "zu Hause"; action = "wiederholen"; object = "das Wort" }
    )
}
elseif ($TopicSet -ne "first") {
    throw "Unsupported TopicSet '$TopicSet'. Use 'first' or 'second'."
}

$vocabularyByLevel = @{
    "A1" = @("der Alltag", "die Zeit", "die Familie", "das Essen", "die Stadt", "die Arbeit", "die Schule", "die Sprache", "die Frage", "die Antwort", "das Geld", "der Preis", "das Gespräch", "die Wohnung")
    "A2" = @("der Alltag", "die Erfahrung", "die Familie", "die Wohnung", "die Arbeit", "die Aufgabe", "die Sprache", "das Gespräch", "die Frage", "die Antwort", "der Termin", "die Gesundheit", "die Reise", "der Einkauf", "die Entscheidung", "die Möglichkeit", "der Vorteil", "das Problem")
}

function Convert-ToSlug([string]$value) {
    $normalized = $value.ToLowerInvariant()
    $normalized = $normalized.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
    $normalized = $normalized -replace "[^a-z0-9]+", "-"
    return ($normalized -replace "^-|-$", "")
}

function Get-ArticleTarget([string]$level) {
    if ($level -eq "A1") { return 1000 }
    if ($level -eq "A2") { return 1500 }
    throw "Unsupported level '$level'."
}

function Fit-ArticleLength([string]$text, [string]$level) {
    $target = Get-ArticleTarget $level
    $minimum = $target - 100
    $maximum = $target + 100
    $normalized = ($text -replace "\s+", " ").Trim()
    $bridges = @(
        "So kann man einfache Sätze sagen und neue Wörter üben.",
        "Das Thema passt gut zu kleinen Gesprächen im Kurs.",
        "Jede Person kann ein kurzes Beispiel aus dem Alltag nennen.",
        "Am Ende kann die Gruppe die wichtigsten Wörter wiederholen."
    )
    $index = 0
    while ($normalized.Length -lt $minimum) {
        $normalized = "$normalized $($bridges[$index % $bridges.Count])"
        $index++
    }
    if ($normalized.Length -le $maximum) {
        return $normalized
    }
    $trimmed = $normalized.Substring(0, [Math]::Min($target, $normalized.Length))
    $lastStop = $trimmed.LastIndexOf(".")
    if ($lastStop -ge $minimum) {
        return $trimmed.Substring(0, $lastStop + 1).Trim()
    }
    $slice = $normalized.Substring(0, $maximum)
    $lastStop = $slice.LastIndexOf(".")
    if ($lastStop -ge $minimum) {
        return $slice.Substring(0, $lastStop + 1).Trim()
    }
    return $slice.Trim()
}

function New-Article([hashtable]$spec, [string]$level) {
    $title = [string]$spec.title
    $place = [string]$spec.place
    $action = [string]$spec.action
    $object = [string]$spec.object

    if ($level -eq "A1") {
        $sentences = @(
            "$title ist ein einfaches Thema aus dem Alltag.",
            "Viele Menschen kennen diese Situation.",
            "Man ist $place und möchte $action.",
            "Oft braucht man dafür einfache Wörter.",
            "$object ist dabei wichtig.",
            "Eine Person kann sagen, was sie macht.",
            "Eine andere Person kann eine kurze Frage stellen.",
            "Die Antwort kann sehr kurz sein.",
            "Zum Beispiel sagt man: Ich habe Zeit.",
            "Oder man sagt: Ich brauche Hilfe.",
            "Im Kurs kann die Gruppe langsam sprechen.",
            "Alle können einfache Sätze üben.",
            "Man kann auch fragen: Wie ist das bei dir?",
            "So entsteht ein kleines Gespräch.",
            "Das Thema ist gut für neue Lernende.",
            "Es hilft beim Hören und Sprechen."
        )
    }
    else {
        $sentences = @(
            "$title ist ein Thema aus dem Alltag und passt gut zu einem Gespräch im Deutschkurs.",
            "Viele Lernende kennen die Situation, auch wenn sie nicht immer die richtigen Wörter finden.",
            "Man ist $place und möchte $action.",
            "Dabei spielen Zeit, Geld, Hilfe und freundliche Fragen oft eine Rolle.",
            "$object ist für das Gespräch wichtig, weil man darüber etwas sagen oder fragen kann.",
            "Eine Person kann erzählen, was sie normalerweise macht.",
            "Eine andere Person kann vergleichen, ob es bei ihr ähnlich oder anders ist.",
            "Hilfreich sind einfache Beispiele: Wann passiert das, mit wem spricht man und was braucht man?",
            "Man kann auch sagen, was leicht ist und was manchmal schwer ist.",
            "Im Kurs üben die Teilnehmenden höfliche Fragen, kurze Erklärungen und passende Antworten.",
            "Das Gespräch muss nicht perfekt sein.",
            "Wichtig ist, dass alle langsam sprechen, nachfragen und neue Wörter benutzen.",
            "Am Ende kann die Gruppe drei gute Sätze sammeln und die wichtigsten Wörter wiederholen."
        )
    }

    $text = ""
    while ($text.Length -lt ((Get-ArticleTarget $level) + 80)) {
        foreach ($sentence in $sentences) {
            $text = "$text $sentence"
            if ($text.Length -ge ((Get-ArticleTarget $level) + 80)) {
                break
            }
        }
    }
    return Fit-ArticleLength $text $level
}

function New-WarmupQuestions([string]$title, [string]$level) {
    $items = @(
        "Kennst du diese Situation?",
        "Wo passiert das in deinem Alltag?",
        "Welche Wörter brauchst du dafür?"
    )
    return @($items | ForEach-Object -Begin { $i = 0 } -Process {
        $i++
        [ordered]@{ prompt = $_; sortOrder = $i * 10 }
    })
}

function New-DiscussionQuestions([string]$title, [string]$level) {
    if ($level -eq "A1") {
        $templates = @{
            "opinion" = @("Findest du das Thema leicht oder schwer?", "Magst du diese Situation?")
            "imagination" = @("Was sagst du in dieser Situation?", "Wer kann dir helfen?")
            "prediction" = @("Machst du das morgen oder nächste Woche?", "Wird das später leichter?")
            "comparison" = @("Ist das zu Hause anders als im Kurs?", "Ist das allein anders als mit Freunden?")
        }
    }
    else {
        $templates = @{
            "opinion" = @("Was findest du an dieser Situation wichtig?", "Was ist für dich dabei leicht oder schwer?")
            "imagination" = @("Wie kann man die Situation freundlicher machen?", "Welche Hilfe wäre in dieser Situation gut?")
            "prediction" = @("Wird diese Situation in Zukunft einfacher?", "Was möchtest du beim nächsten Mal anders machen?")
            "comparison" = @("Wie ist diese Situation in deinem Land?", "Was ist im Alltag anders als bei der Arbeit?")
        }
    }

    $items = @()
    $sortOrder = 10
    foreach ($type in @("opinion", "imagination", "prediction", "comparison")) {
        foreach ($prompt in $templates[$type]) {
            $items += [ordered]@{ prompt = $prompt; questionType = $type; sortOrder = $sortOrder }
            $sortOrder += 10
        }
    }
    return $items
}

function New-VocabularyItems([string]$level) {
    $lemmas = $vocabularyByLevel[$level]
    $items = @()
    for ($i = 0; $i -lt $lemmas.Count; $i++) {
        $lemma = $lemmas[$i]
        $slug = Convert-ToSlug $lemma
        $items += [ordered]@{
            lemma = $lemma
            wordSlug = $slug
            cefrLevel = $level
            sortOrder = ($i + 1) * 10
        }
    }
    return $items
}

function New-SpeakingGoals([string]$level) {
    if ($level -eq "A1") {
        return @("describe-experiences", "ask-follow-up-questions")
    }
    return @("describe-experiences", "express-opinion", "give-reasons")
}

function New-TalkTopic([hashtable]$spec, [string]$level, [int]$index) {
    $title = [string]$spec.title
    $category = [string]$spec.category
    $baseSlug = Convert-ToSlug $title
    $groupKey = "$GroupPrefix-$baseSlug"
    $slug = "$(Convert-ToSlug $level)-$groupKey"
    $contentType = if ($index % 3 -eq 0) { "story" } elseif ($index % 3 -eq 1) { "article" } else { "fact-sheet" }

    return [ordered]@{
        slug = $slug
        topicGroupKey = $groupKey
        title = $title
        description = "Ein einfaches Talk Topic über $title für kurze Gespräche im Alltag."
        cefrLevel = $level
        category = $category
        topics = @($topicsByCategory[$category])
        contentType = $contentType
        estimatedReadingMinutes = if ($level -eq "A1") { 3 } else { 4 }
        estimatedDiscussionMinutes = if ($level -eq "A1") { 15 } else { 20 }
        isSensitive = $false
        recommendedForModeratedGroupsOnly = $false
        sensitivityNote = $null
        article = [ordered]@{ baseText = New-Article $spec $level }
        warmupQuestions = New-WarmupQuestions $title $level
        discussionQuestions = New-DiscussionQuestions $title $level
        vocabularyItems = New-VocabularyItems $level
        speakingGoals = New-SpeakingGoals $level
        sortOrder = 30000 + $index
        isPublished = $true
    }
}

if (Test-Path $OutputPath) {
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null

$talkTopics = @()
$index = 0
foreach ($spec in $topicSpecs) {
    foreach ($level in @("A1", "A2")) {
        $talkTopics += New-TalkTopic $spec $level $index
        $index++
    }
}

$packageIndex = 1
for ($offset = 0; $offset -lt $talkTopics.Count; $offset += $TopicsPerPackage) {
    $batch = @($talkTopics | Select-Object -Skip $offset -First $TopicsPerPackage)
    $package = [ordered]@{
        packageVersion = "1.0"
        packageId = "$PackageIdPrefix-{0:D3}" -f $packageIndex
        packageName = "$PackageNamePrefix Batch {0:D3}" -f $packageIndex
        source = "Hybrid"
        defaultMeaningLanguages = @("en")
        entries = @()
        talkTopics = $batch
    }
    $filePath = Join-Path $OutputPath ("de-talk-topics-20260510-a1-a2-expansion-{0:D3}.json" -f $packageIndex)
    $json = $package | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $OutputPath).Path + [System.IO.Path]::DirectorySeparatorChar + [System.IO.Path]::GetFileName($filePath), $json, [System.Text.UTF8Encoding]::new($false))
    $packageIndex++
}

[ordered]@{
    outputPath = $OutputPath
    packageCount = $packageIndex - 1
    talkTopicCount = $talkTopics.Count
    cefrDistribution = @($talkTopics | Group-Object { $_.cefrLevel } | Sort-Object Name | ForEach-Object { [ordered]@{ level = $_.Name; count = $_.Count } })
} | ConvertTo-Json -Depth 5
