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

$anglesByCategory = @{
    "science" = @("im Alltag", "in der Schule", "in den Medien", "in der Zukunft", "in Deutschland")
    "technology" = @("im Alltag", "am Arbeitsplatz", "für junge Menschen", "in der Zukunft", "in der Familie")
    "artificial-intelligence" = @("im Alltag", "am Arbeitsplatz", "in der Schule", "in den Medien", "in der Zukunft")
    "environment" = @("im Alltag", "in der Stadt", "für die Umwelt", "in der Schule", "in Deutschland")
    "climate" = @("im Alltag", "in der Stadt", "für junge Menschen", "für die Umwelt", "in der Zukunft")
    "space" = @("in der Schule", "in den Medien", "für junge Menschen", "in der Zukunft", "in Deutschland")
    "health" = @("im Alltag", "am Arbeitsplatz", "in der Familie", "in der Schule", "in Deutschland")
    "psychology" = @("im Alltag", "in der Familie", "am Arbeitsplatz", "in der Schule", "in den Medien")
    "society" = @("im Alltag", "in der Stadt", "in Deutschland", "in den Medien", "für junge Menschen")
    "politics" = @("im Alltag", "in Deutschland", "in den Medien", "für junge Menschen", "in der Zukunft")
    "democracy" = @("im Alltag", "in Deutschland", "in der Schule", "in den Medien", "in der Zukunft")
    "migration" = @("im Alltag", "in Deutschland", "in der Stadt", "am Arbeitsplatz", "in der Schule")
    "culture" = @("im Alltag", "in der Stadt", "in Deutschland", "für junge Menschen", "in den Medien")
    "history" = @("in der Schule", "in den Medien", "in Deutschland", "in der Familie", "für junge Menschen")
    "sports" = @("im Alltag", "in der Schule", "in der Stadt", "für junge Menschen", "in den Medien")
    "football" = @("im Alltag", "in der Schule", "in den Medien", "in Deutschland", "für junge Menschen")
    "cinema" = @("in den Medien", "für junge Menschen", "in Deutschland", "in der Schule", "in der Familie")
    "books" = @("im Alltag", "in der Schule", "in der Familie", "in der Zukunft", "für junge Menschen")
    "music" = @("im Alltag", "in der Stadt", "in der Familie", "für junge Menschen", "in den Medien")
    "art" = @("im Alltag", "in der Stadt", "in der Schule", "in Deutschland", "für junge Menschen")
    "education" = @("in der Schule", "im Alltag", "in Deutschland", "in der Zukunft", "für junge Menschen")
    "work" = @("am Arbeitsplatz", "in Deutschland", "in der Zukunft", "in den Medien", "im Alltag")
    "family" = @("im Alltag", "in Deutschland", "in der Stadt", "in der Zukunft", "in den Medien")
    "friendship" = @("im Alltag", "in der Schule", "in der Stadt", "in den Medien", "in der Zukunft")
    "food" = @("im Alltag", "in der Familie", "in der Stadt", "für die Umwelt", "in Deutschland")
    "travel" = @("im Alltag", "in Deutschland", "für junge Menschen", "in der Zukunft", "für die Umwelt")
    "city-life" = @("in der Stadt", "im Alltag", "in Deutschland", "für junge Menschen", "in der Zukunft")
    "money" = @("im Alltag", "in der Familie", "am Arbeitsplatz", "in Deutschland", "für junge Menschen")
    "media" = @("im Alltag", "in den Medien", "in der Schule", "für junge Menschen", "in der Zukunft")
    "social-media" = @("im Alltag", "in der Schule", "in der Familie", "für junge Menschen", "in den Medien")
    "ethics" = @("im Alltag", "am Arbeitsplatz", "in der Schule", "in den Medien", "in der Zukunft")
    "future" = @("im Alltag", "in Deutschland", "am Arbeitsplatz", "in der Stadt", "für junge Menschen")
    "everyday-life" = @("im Alltag", "in der Familie", "in der Stadt", "in Deutschland", "für junge Menschen")
    "germany-and-integration" = @("in Deutschland", "im Alltag", "am Arbeitsplatz", "in der Stadt", "in der Schule")
    "language-learning" = @("im Alltag", "in der Schule", "in Deutschland", "am Arbeitsplatz", "in der Zukunft")
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

$categoryVocabulary = @{
    "space" = @("das Weltall", "der Planet", "der Stern", "die Forschung", "das Teleskop", "die Erde")
    "technology" = @("der Roboter", "das Gerät", "die Technik", "der Computer", "die Software", "der Datenschutz")
    "artificial-intelligence" = @("die künstliche Intelligenz", "der Algorithmus", "das Modell", "die Automatisierung", "die Daten", "der Prompt")
    "environment" = @("die Umwelt", "das Wasser", "die Energie", "das Recycling", "der Abfall", "nachhaltig")
    "climate" = @("das Klima", "das Wetter", "der Konsum", "die Veränderung", "die Verantwortung", "die Zukunft")
    "health" = @("die Gesundheit", "die Pause", "die Pflege", "der Körper", "die Bewegung", "der Respekt")
    "psychology" = @("der Stress", "das Gefühl", "der Streit", "die Gruppe", "die Erfahrung", "zuhören")
    "democracy" = @("die Demokratie", "die Abstimmung", "die Meinung", "die Regel", "die Verantwortung", "die Beteiligung")
    "migration" = @("die Migration", "die Sprache", "die Integration", "die Behörde", "der Aufenthalt", "die Gemeinschaft")
    "history" = @("die Geschichte", "der Film", "die Erinnerung", "die Quelle", "die Vergangenheit", "der Vergleich")
    "football" = @("der Fußball", "die Fairness", "die Mannschaft", "das Spiel", "der Verein", "der Schiedsrichter")
    "sports" = @("der Sport", "die Bewegung", "die Gesundheit", "das Training", "das Team", "der Wettbewerb")
    "cinema" = @("das Kino", "der Film", "die Szene", "die Figur", "das Gefühl", "die Geschichte")
    "books" = @("das Buch", "die Bibliothek", "die Geschichte", "die Figur", "das Lesen", "die Zukunft")
    "music" = @("die Musik", "das Konzert", "das Lied", "die Stimme", "die Bühne", "der Geschmack")
    "art" = @("die Kunst", "das Museum", "der öffentliche Raum", "die Ausstellung", "das Bild", "die Stadt")
    "education" = @("die Schule", "die Note", "das Lernen", "die App", "der Unterricht", "die Prüfung")
    "work" = @("die Arbeit", "das Homeoffice", "der Beruf", "die Bewerbung", "der Kunde", "das Unternehmen")
    "family" = @("die Familie", "das Haustier", "die Verantwortung", "der Alltag", "die Beziehung", "die Unterstützung")
    "friendship" = @("die Freundschaft", "der Kontakt", "online", "vertrauen", "die Nachricht", "die Nähe")
    "food" = @("das Essen", "die Region", "das Kochen", "die Mahlzeit", "der Geschmack", "die Verpackung")
    "travel" = @("die Reise", "der Urlaub", "das Geld", "der Verkehr", "das Fahrrad", "der Zug")
    "city-life" = @("die Stadt", "der Nachbar", "die Wohnung", "der öffentliche Platz", "die Miete", "der Verkehr")
    "money" = @("das Geld", "das Glück", "die Armut", "die Chance", "der Preis", "die Ausgabe")
    "media" = @("die Nachricht", "das Internet", "die Medien", "die Quelle", "der Bericht", "die Meinung")
    "social-media" = @("soziale Medien", "das Profil", "der Kommentar", "die Nachricht", "die Privatsphäre", "teilen")
    "ethics" = @("die Verantwortung", "die Fairness", "der Respekt", "gerecht", "die Entscheidung", "der Grund")
    "future" = @("die Zukunft", "die Veränderung", "die Möglichkeit", "die Entwicklung", "das Risiko", "die Chance")
    "germany-and-integration" = @("Deutschland", "die Integration", "das Fest", "das Sprichwort", "die Kultur", "die Sprache")
    "language-learning" = @("das Sprachenlernen", "die Sprache", "der Fehler", "die Übung", "der Wortschatz", "sprechen")
    "everyday-life" = @("der Alltag", "die Zeit", "die Familie", "die Stadt", "die Entscheidung", "das Gespräch")
}

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

function Get-CategoryForSubject([string]$subject) {
    switch -Regex ($subject) {
        "soziale Medien" { return "social-media" }
        "Weltall|Weltraum" { return "space" }
        "Roboter|Technik|Datenschutz|Computerspiele" { return "technology" }
        "künstliche Intelligenz|Apps" { return "artificial-intelligence" }
        "Energie|Wasser|Plastik|Recycling|Umwelt|Mode" { return "environment" }
        "Klima|Wetter" { return "climate" }
        "gesund|Pflege|Sport und Gesundheit" { return "health" }
        "Stress|Streit|Gefühle|Fehler" { return "psychology" }
        "Demokratie" { return "democracy" }
        "Migration|Integration" { return "migration" }
        "Geschichte" { return "history" }
        "Fußball" { return "football" }
        "Sport" { return "sports" }
        "Kino" { return "cinema" }
        "Bücher|Bibliotheken" { return "books" }
        "Musik|Konzerte" { return "music" }
        "Kunst|Museen" { return "art" }
        "Schule|Lernen|Sprache" { return "education" }
        "Arbeit|Berufe|Bewerbung|Kundenservice|Unternehmen" { return "work" }
        "Familie|Haustiere" { return "family" }
        "Freundschaft" { return "friendship" }
        "Essen|Kochen" { return "food" }
        "Reisen|Urlaub|Verkehrsmittel|Fahrrad" { return "travel" }
        "Nachbarn|Wohnungen|Plätze" { return "city-life" }
        "Geld|Armut" { return "money" }
        "Nachrichten|Medien" { return "media" }
        "soziale Medien" { return "social-media" }
        "Verantwortung|Fairness|Respekt" { return "ethics" }
        "Zukunft" { return "future" }
        "Deutschland|Feste|Sprichwörter" { return "germany-and-integration" }
        default { return "everyday-life" }
    }
}

function Convert-ToTitleStart([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) {
        return $value
    }

    return $value.Substring(0, 1).ToUpperInvariant() + $value.Substring(1)
}

function Join-TopicTitle([string]$subject, [string]$category, [int]$variantIndex) {
    $angles = if ($anglesByCategory.ContainsKey($category)) { $anglesByCategory[$category] } else { $anglesByCategory["everyday-life"] }
    $angle = $angles[$variantIndex % $angles.Count]

    $subjectLower = $subject.ToLowerInvariant()
    $angleLower = $angle.ToLowerInvariant()
    if ($subjectLower.EndsWith($angleLower) -or $subjectLower.Contains($angleLower)) {
        return Convert-ToTitleStart $subject
    }

    return Convert-ToTitleStart "$subject $angle"
}

function Get-CategoryDisplay([string]$category) {
    switch ($category) {
        "science" { return "Wissenschaft" }
        "technology" { return "Technik" }
        "artificial-intelligence" { return "künstliche Intelligenz" }
        "environment" { return "Umwelt" }
        "climate" { return "Klima" }
        "space" { return "Weltraum" }
        "health" { return "Gesundheit" }
        "psychology" { return "Psychologie" }
        "society" { return "Gesellschaft" }
        "politics" { return "Politik" }
        "democracy" { return "Demokratie" }
        "migration" { return "Migration" }
        "culture" { return "Kultur" }
        "history" { return "Geschichte" }
        "sports" { return "Sport" }
        "football" { return "Fußball" }
        "cinema" { return "Kino" }
        "books" { return "Bücher" }
        "music" { return "Musik" }
        "art" { return "Kunst" }
        "education" { return "Bildung" }
        "work" { return "Arbeit" }
        "family" { return "Familie" }
        "friendship" { return "Freundschaft" }
        "food" { return "Essen" }
        "travel" { return "Reisen" }
        "city-life" { return "Stadtleben" }
        "money" { return "Geld" }
        "media" { return "Medien" }
        "social-media" { return "soziale Medien" }
        "ethics" { return "Ethik" }
        "future" { return "Zukunft" }
        "germany-and-integration" { return "Deutschland und Integration" }
        "language-learning" { return "Sprachenlernen" }
        default { return "Alltag" }
    }
}

function Fit-ArticleLength([string]$text, [string]$level) {
    $target = Get-ArticleTarget $level
    $minimum = $target - 100
    $maximum = $target + 100
    $normalized = ($text -replace "\s+", " ").Trim()

    $bridgeSentences = @(
        "Diese Frage hilft der Gruppe, eigene Beispiele zu sammeln und genauer nachzufragen.",
        "So entsteht ein Gespräch, in dem verschiedene Erfahrungen nebeneinander stehen können.",
        "Am Ende müssen nicht alle die gleiche Meinung haben, aber alle sollten ihre Gründe nennen.",
        "Wer zuhört und nachfragt, kann das Thema aus einer neuen Perspektive sehen."
    )

    $index = 0
    while ($normalized.Length -lt $minimum) {
        $normalized = "$normalized $($bridgeSentences[$index % $bridgeSentences.Count])"
        $index++
    }

    if ($normalized.Length -gt $maximum) {
        $candidate = $normalized.Substring(0, $maximum)
        $lastSentenceEnd = $candidate.LastIndexOf(".")
        if ($lastSentenceEnd -ge ($minimum - 1)) {
            $normalized = $candidate.Substring(0, $lastSentenceEnd + 1).Trim()
        }
        else {
            $normalized = $candidate.Trim()
        }
    }

    return $normalized
}

function New-Article([string]$title, [string]$category, [string]$level, [string]$contentType) {
    $categoryDisplay = Get-CategoryDisplay $category
    if ($level -in @("A1", "A2")) {
        $sentences = @(
            "$title ist ein Thema aus dem Bereich $categoryDisplay.",
            "Viele Menschen kennen dazu kleine Situationen aus ihrem Alltag.",
            "Man kann zuerst sagen, was man sieht, hört oder erlebt.",
            "Dann kann jede Person eine einfache Meinung sagen.",
            "Eine Person sagt: Ich finde das gut.",
            "Eine andere Person sagt: Ich bin nicht sicher.",
            "Wichtig ist, langsam zu sprechen und freundlich zu fragen.",
            "Die Gruppe kann Beispiele sammeln und neue Wörter benutzen.",
            "Manchmal gibt es keine richtige Antwort.",
            "Das ist gut, denn so können alle weiterreden.",
            "Wer möchte, kann auch von der Familie, der Schule, der Arbeit oder der Stadt erzählen.",
            "So wird aus dem Text ein ruhiges Gespräch auf Deutsch."
        )
    }
    elseif ($level -in @("B1", "B2")) {
        $sentences = @(
            "$title gehört zum Bereich $categoryDisplay und eignet sich gut für eine offene Diskussion.",
            "Das Thema wirkt zuerst einfach, wird aber interessanter, wenn man konkrete Situationen betrachtet.",
            "Einige Menschen achten vor allem auf praktische Vorteile, andere denken stärker an mögliche Probleme.",
            "In einer Gesprächsgruppe können Lernende persönliche Erfahrungen, Beobachtungen aus den Medien und Beispiele aus ihrem Umfeld verbinden.",
            "Hilfreich ist es, nicht nur Zustimmung oder Ablehnung zu zeigen, sondern auch Gründe zu nennen.",
            "Man kann fragen, welche Regeln fair wären, welche Rolle Gewohnheiten spielen und was sich in Zukunft ändern könnte.",
            "Auch Vergleiche sind nützlich: Ist die Situation in einer Familie anders als am Arbeitsplatz oder in einer Stadt?",
            "Der Text soll keine fertige Lösung geben.",
            "Er soll helfen, genauer zu formulieren, nachzufragen und verschiedene Positionen respektvoll nebeneinanderzustellen.",
            "Am Ende kann die Gruppe zusammenfassen, welche Argumente besonders überzeugend waren."
        )
    }
    else {
        $sentences = @(
            "$title eröffnet im Bereich $categoryDisplay eine vielschichtige Debatte, weil persönliche Erfahrungen, gesellschaftliche Interessen und langfristige Folgen zusammenkommen.",
            "Wer darüber spricht, muss oft zwischen Bequemlichkeit, Verantwortung, Gerechtigkeit und realistischen Handlungsmöglichkeiten abwägen.",
            "Gerade deshalb eignet sich das Thema für fortgeschrittene Lernende, die nicht nur berichten, sondern Positionen präzise begründen möchten.",
            "Eine produktive Diskussion kann fragen, welche Annahmen hinter typischen Meinungen stehen und welche Gruppen von bestimmten Entscheidungen profitieren oder belastet werden.",
            "Dabei lohnt sich der Blick auf Alltag, Institutionen, Medien und wirtschaftliche Rahmenbedingungen.",
            "Je nach Perspektive kann dieselbe Entwicklung als Chance, Risiko oder notwendiger Kompromiss erscheinen.",
            "Der Inhaltstyp $contentType gibt der Gruppe einen Rahmen, aber die wichtigsten Fragen entstehen im Gespräch selbst.",
            "Teilnehmende können Beispiele vergleichen, Gegenargumente formulieren und prüfen, ob ihre erste Einschätzung stabil bleibt.",
            "Gute Moderation hilft, pauschale Aussagen zu vermeiden und unterschiedliche Erfahrungen ernst zu nehmen.",
            "Ziel ist nicht ein schneller Konsens, sondern eine klare, faire und begründete Auseinandersetzung."
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
            "Welche Position könntest du gut begründen?"
        )
        "imagination" = @(
            "Wie könnte eine ungewöhnliche Lösung aussehen?",
            "Was würde sich ändern, wenn alle anders handeln?",
            "Wie sähe dieses Thema in einer idealen Stadt aus?"
        )
        "prediction" = @(
            "Wie wird sich dieses Thema in zehn Jahren entwickeln?",
            "Welche Folgen könnte es für junge Menschen geben?",
            "Welche neue Frage wird in Zukunft wichtiger?"
        )
        "comparison" = @(
            "Wie ist die Situation in zwei verschiedenen Ländern?",
            "Was ist heute anders als früher?",
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

function New-VocabularyItems([string]$level, [int]$offset, [string]$category) {
    $count = Get-VocabularyCount $level
    $items = @()
    $seen = New-Object System.Collections.Generic.HashSet[string]
    $candidates = @()
    if ($categoryVocabulary.ContainsKey($category)) {
        foreach ($lemma in $categoryVocabulary[$category]) {
            $candidates += @{ lemma = $lemma; wordSlug = Convert-ToSlug $lemma }
        }
    }
    foreach ($entry in $vocabularyBase) {
        $candidates += $entry
    }

    $candidateIndex = 0
    while ($items.Count -lt $count) {
        $entry = if ($candidateIndex -lt $candidates.Count) {
            $candidates[$candidateIndex]
        }
        else {
            $vocabularyBase[($offset + $candidateIndex) % $vocabularyBase.Count]
        }
        $candidateIndex++
        $lemmaKey = ([string]$entry.lemma).Trim().ToLowerInvariant()
        if (-not $seen.Add($lemmaKey)) {
            continue
        }
        $items += [ordered]@{
            lemma = $entry.lemma
            wordSlug = $entry.wordSlug
            cefrLevel = $level
            sortOrder = $items.Count * 10 + 10
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
    $subject = $subjects[$i % $subjects.Count]
    $category = Get-CategoryForSubject $subject
    $variantIndex = [math]::Floor($i / $subjects.Count)
    $title = Join-TopicTitle $subject $category $variantIndex
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
                description = "Ein Talk Topic für Diskussionen über $($group.title)."
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
                vocabularyItems = New-VocabularyItems $level ([array]::IndexOf($groups, $group)) $group.category
                speakingGoals = New-SpeakingGoals $level
                sortOrder = (([array]::IndexOf($groups, $group) + 1) * 10)
                isPublished = $true
            }
        }
    }

    $package = [ordered]@{
        packageVersion = "1.0"
        packageId = "de-talk-topics-20260510-v6-{0:D3}" -f $packageIndex
        packageName = "Darwin Deutsch Talk Topics 2026-05-10 Vocabulary Diversity Refresh Batch {0:D3}" -f $packageIndex
        source = "Hybrid"
        defaultMeaningLanguages = @("en")
        entries = @()
        talkTopics = $talkTopics
    }

    $filePath = Join-Path $OutputPath ("de-talk-topics-20260510-{0:D3}.json" -f $packageIndex)
    $json = $package | ConvertTo-Json -Depth 100
    [System.IO.File]::WriteAllText((Join-Path (Resolve-Path $OutputPath).Path ([System.IO.Path]::GetFileName($filePath))), (Repair-Utf8Mojibake $json), [System.Text.Encoding]::UTF8)
    $packageIndex++
}

[ordered]@{
    outputPath = (Resolve-Path $OutputPath).Path
    topicGroups = $TopicGroupCount
    talkTopics = $TopicGroupCount * 3
    packageFiles = $packageIndex - 1
} | ConvertTo-Json
