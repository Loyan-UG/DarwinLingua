param(
    [string]$OutputPath = "content/generated/talk-topics-20260510-b1-c1-expansion",
    [int]$TopicsPerPackage = 30,
    [string]$TopicSet = "first",
    [string]$PackageIdPrefix = "de-talk-topics-20260510-b1-c1-expansion-v2",
    [string]$PackageNamePrefix = "Darwin Deutsch Talk Topics B1-C1 Expansion 2026-05-10 v2",
    [string]$GroupPrefix = "mittelstufe",
    [string]$B2GroupPrefix = "b2-vertiefung"
)

$ErrorActionPreference = "Stop"

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

$categoryVocabulary = @{
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
    "sports" = @("der Sport", "die Bewegung", "die Gesundheit", "das Training", "die Gruppe", "der Wettbewerb")
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
    "language-learning" = @("das Lernen", "die Sprache", "der Fehler", "die Übung", "das Gespräch", "sprechen")
    "everyday-life" = @("der Alltag", "die Zeit", "die Familie", "die Stadt", "die Entscheidung", "das Gespräch")
}

$vocabularyBase = @(
    "die Meinung", "der Grund", "das Beispiel", "die Frage", "die Gruppe", "das Gespräch",
    "die Erfahrung", "vergleichen", "vorstellen", "verändern", "die Zukunft", "die Entscheidung",
    "der Vorteil", "der Nachteil", "das Problem", "die Lösung", "die Verantwortung",
    "die Möglichkeit", "die Veränderung", "die Gesellschaft", "die Regel", "der Respekt",
    "die Chance", "das Risiko", "die Entwicklung", "die Wirkung", "die Debatte",
    "der Zusammenhang", "die Perspektive", "die Voraussetzung", "abwägen", "begründen",
    "zustimmen", "widersprechen", "zusammenfassen", "behaupten", "beobachten",
    "beeinflussen", "nachfragen", "verantwortlich", "umstritten", "nachhaltig",
    "gerecht", "öffentlich", "digital"
)

$themeSpecs = @(
    @{ title = "Digitale Weiterbildung im Beruf"; category = "education" },
    @{ title = "Künstliche Intelligenz in kleinen Firmen"; category = "artificial-intelligence" },
    @{ title = "Datenschutz bei Gesundheitsapps"; category = "technology" },
    @{ title = "Homeoffice und soziale Nähe"; category = "work" },
    @{ title = "Mehrsprachige Teams am Arbeitsplatz"; category = "work" },
    @{ title = "Nachhaltige Kantinen"; category = "food" },
    @{ title = "Reisen mit Nachtzügen"; category = "travel" },
    @{ title = "Mietpreise und Nachbarschaft"; category = "city-life" },
    @{ title = "Stadtparks in heißen Sommern"; category = "climate" },
    @{ title = "Reparieren statt Wegwerfen"; category = "environment" },
    @{ title = "Sportvereine und Integration"; category = "sports" },
    @{ title = "Fußball ohne Diskriminierung"; category = "football" },
    @{ title = "Museen als Lernorte"; category = "art" },
    @{ title = "Bibliotheken als Treffpunkte"; category = "books" },
    @{ title = "Konzerte und Lärmschutz"; category = "music" },
    @{ title = "Filme über Geschichte"; category = "cinema" },
    @{ title = "Nachrichten und Vertrauen"; category = "media" },
    @{ title = "Soziale Medien und Privatsphäre"; category = "social-media" },
    @{ title = "Digitale Freundschaften"; category = "friendship" },
    @{ title = "Familienmodelle heute"; category = "family" },
    @{ title = "Pflegekräfte und Anerkennung"; category = "health" },
    @{ title = "Stress durch ständige Erreichbarkeit"; category = "psychology" },
    @{ title = "Ehrenamt in der Stadt"; category = "society" },
    @{ title = "Demokratie in Vereinen"; category = "democracy" },
    @{ title = "Migration und berufliche Chancen"; category = "migration" },
    @{ title = "Geldsorgen junger Menschen"; category = "money" },
    @{ title = "Faire Preise für Lebensmittel"; category = "ethics" },
    @{ title = "Berufe nach der Automatisierung"; category = "future" },
    @{ title = "Deutschlernen mit Serien"; category = "language-learning" },
    @{ title = "Kulturelle Feste im Stadtteil"; category = "germany-and-integration" },
    @{ title = "Wissenschaft in Podcasts"; category = "science" },
    @{ title = "Weltraumforschung und Alltag"; category = "space" },
    @{ title = "Klimaschutz im Büro"; category = "climate" },
    @{ title = "Wasserverbrauch in der Stadt"; category = "environment" },
    @{ title = "Digitale Ausweise"; category = "technology" },
    @{ title = "Algorithmen bei Bewerbungen"; category = "artificial-intelligence" },
    @{ title = "Schule und praktische Fähigkeiten"; category = "education" },
    @{ title = "Kundenservice mit Chatbots"; category = "work" },
    @{ title = "Lokale Märkte und Gemeinschaft"; category = "food" },
    @{ title = "Tourismus und Wohnraum"; category = "travel" },
    @{ title = "Öffentliche Plätze für Jugendliche"; category = "city-life" },
    @{ title = "Online-Debatten und Respekt"; category = "social-media" },
    @{ title = "Musikunterricht für Erwachsene"; category = "music" },
    @{ title = "Kunst als Protest"; category = "art" },
    @{ title = "Bücherclubs im Internet"; category = "books" },
    @{ title = "Sport und mentale Gesundheit"; category = "health" },
    @{ title = "Erinnerungskultur in Familien"; category = "history" },
    @{ title = "Sprachbarrieren bei Behörden"; category = "germany-and-integration" },
    @{ title = "Einkaufen mit kleinem Budget"; category = "money" },
    @{ title = "Zukunft der Innenstädte"; category = "future" }
)

if ($TopicSet -eq "second") {
    $themeSpecs = @(
        @{ title = "Digitale Sprechstunden beim Arzt"; category = "health" },
        @{ title = "Lernen mit kurzen Videos"; category = "education" },
        @{ title = "Künstliche Intelligenz und Hausaufgaben"; category = "artificial-intelligence" },
        @{ title = "Roboter in Pflegeheimen"; category = "technology" },
        @{ title = "Fahrradstraßen in der Stadt"; category = "city-life" },
        @{ title = "Regionale Küche und Identität"; category = "food" },
        @{ title = "Reisen ohne Flugzeug"; category = "travel" },
        @{ title = "Secondhand-Mode und Konsum"; category = "environment" },
        @{ title = "Klimaanpassung in Mietwohnungen"; category = "climate" },
        @{ title = "Psychische Gesundheit im Studium"; category = "psychology" },
        @{ title = "Bewerbungen ohne Foto"; category = "work" },
        @{ title = "Vier-Tage-Woche im Unternehmen"; category = "work" },
        @{ title = "Geld teilen in Beziehungen"; category = "money" },
        @{ title = "Nachrichten auf TikTok"; category = "social-media" },
        @{ title = "Podcasts statt Radio"; category = "media" },
        @{ title = "Ehrenamt im Sportverein"; category = "sports" },
        @{ title = "Frauenfußball in den Medien"; category = "football" },
        @{ title = "Serien als Kulturspiegel"; category = "cinema" },
        @{ title = "Bücher in einfacher Sprache"; category = "books" },
        @{ title = "Straßenmusik und öffentlicher Raum"; category = "music" },
        @{ title = "Kunst im Krankenhaus"; category = "art" },
        @{ title = "Freundschaft nach einem Umzug"; category = "friendship" },
        @{ title = "Alleinerziehende Familien"; category = "family" },
        @{ title = "Digitale Behörden und Barrieren"; category = "germany-and-integration" },
        @{ title = "Migration und Vereinsleben"; category = "migration" },
        @{ title = "Bürgerbeteiligung im Stadtteil"; category = "democracy" },
        @{ title = "Erinnerung an die eigene Schulzeit"; category = "history" },
        @{ title = "Wissenschaftliche Mythen im Alltag"; category = "science" },
        @{ title = "Leben auf einer Mondstation"; category = "space" },
        @{ title = "Faire Entscheidungen in Gruppen"; category = "ethics" },
        @{ title = "Zukunft des Bargelds"; category = "future" },
        @{ title = "Deutsch sprechen im Alltag"; category = "language-learning" },
        @{ title = "Digitale Kalender und Stress"; category = "technology" },
        @{ title = "Gesundes Essen in Schulen"; category = "food" },
        @{ title = "Tourismus in kleinen Städten"; category = "travel" },
        @{ title = "Grüne Dächer gegen Hitze"; category = "climate" },
        @{ title = "Reparaturcafés im Stadtteil"; category = "environment" },
        @{ title = "Online-Therapie und Vertrauen"; category = "health" },
        @{ title = "Konflikte in Wohngemeinschaften"; category = "psychology" },
        @{ title = "Meetings mit internationalen Teams"; category = "work" },
        @{ title = "Preise im Supermarkt verstehen"; category = "money" },
        @{ title = "Kommentare im Internet moderieren"; category = "social-media" },
        @{ title = "Lokale Zeitungen und Demokratie"; category = "media" },
        @{ title = "Training ohne Leistungsdruck"; category = "sports" },
        @{ title = "Fußballfans und Verantwortung"; category = "football" },
        @{ title = "Filmabende in Sprachgruppen"; category = "cinema" },
        @{ title = "Bibliotheken für neue Nachbarn"; category = "books" },
        @{ title = "Musikgeschmack zwischen Generationen"; category = "music" },
        @{ title = "Museen und digitale Führungen"; category = "art" },
        @{ title = "Sprachenlernen mit Tandempartnern"; category = "language-learning" }
    )
}
elseif ($TopicSet -eq "third") {
    $themeSpecs = @(
        @{ title = "Digitale Rechnungen im Alltag"; category = "technology" },
        @{ title = "Kundenbewertungen im Internet"; category = "media" },
        @{ title = "Künstliche Intelligenz im Kundenservice"; category = "artificial-intelligence" },
        @{ title = "Automatische Übersetzungen beim Lernen"; category = "language-learning" },
        @{ title = "Datenschutz in sozialen Netzwerken"; category = "social-media" },
        @{ title = "Gesunde Pausen am Arbeitsplatz"; category = "health" },
        @{ title = "Achtsamkeit ohne Leistungsdruck"; category = "psychology" },
        @{ title = "Pendeln zwischen Stadt und Land"; category = "city-life" },
        @{ title = "Carsharing und persönliche Freiheit"; category = "travel" },
        @{ title = "Lebensmittelverschwendung vermeiden"; category = "food" },
        @{ title = "Mehrwegverpackungen im Supermarkt"; category = "environment" },
        @{ title = "Hitzeschutz für ältere Menschen"; category = "climate" },
        @{ title = "Weiterbildung während der Arbeitszeit"; category = "education" },
        @{ title = "Probezeit und Erwartungen"; category = "work" },
        @{ title = "Kritikgespräche im Team"; category = "work" },
        @{ title = "Transparente Gehälter"; category = "money" },
        @{ title = "Schulden und Scham"; category = "psychology" },
        @{ title = "Digitale Wahlen im Verein"; category = "democracy" },
        @{ title = "Mehrsprachigkeit in Behörden"; category = "germany-and-integration" },
        @{ title = "Einbürgerung und Zugehörigkeit"; category = "migration" },
        @{ title = "Historische Denkmäler im Alltag"; category = "history" },
        @{ title = "Wissenschaft gegen Falschinformationen"; category = "science" },
        @{ title = "Satelliten und Wettervorhersagen"; category = "space" },
        @{ title = "Fairness bei Gruppenarbeiten"; category = "ethics" },
        @{ title = "Zukunft der beruflichen Ausbildung"; category = "future" },
        @{ title = "Elternabende und Mitbestimmung"; category = "family" },
        @{ title = "Freundschaften über Generationen"; category = "friendship" },
        @{ title = "Sport als zweite Chance"; category = "sports" },
        @{ title = "Fußball und soziale Medien"; category = "football" },
        @{ title = "Dokumentarfilme und Wirklichkeit"; category = "cinema" },
        @{ title = "Hörbücher statt gedruckter Bücher"; category = "books" },
        @{ title = "Musikfestivals und Nachhaltigkeit"; category = "music" },
        @{ title = "Streetart und Stadtbild"; category = "art" },
        @{ title = "Onlinekurse mit Zertifikat"; category = "education" },
        @{ title = "Digitale Akten in Arztpraxen"; category = "health" },
        @{ title = "Lernen durch Fehler"; category = "language-learning" },
        @{ title = "Homeoffice in kleinen Wohnungen"; category = "city-life" },
        @{ title = "Nachbarschaftshilfe bei Hitze"; category = "climate" },
        @{ title = "Reisen zu Familienfesten"; category = "travel" },
        @{ title = "Traditionelle Rezepte verändern"; category = "food" },
        @{ title = "Gebrauchte Technik kaufen"; category = "technology" },
        @{ title = "Algorithmen und Musikempfehlungen"; category = "artificial-intelligence" },
        @{ title = "Kurze Nachrichten und lange Analysen"; category = "media" },
        @{ title = "Private Fotos online teilen"; category = "social-media" },
        @{ title = "Arbeitszeugnisse verstehen"; category = "work" },
        @{ title = "Sparen für unsichere Zeiten"; category = "money" },
        @{ title = "Politische Gespräche in Familien"; category = "democracy" },
        @{ title = "Sprachkurse und Kinderbetreuung"; category = "germany-and-integration" },
        @{ title = "Museen über Migration"; category = "migration" },
        @{ title = "Berufe, die es noch nicht gibt"; category = "future" }
    )
}
elseif ($TopicSet -eq "fourth") {
    $themeSpecs = @(
        @{ title = "Digitale Quittungen und Kontrolle"; category = "technology" },
        @{ title = "Künstliche Intelligenz in der Medizin"; category = "artificial-intelligence" },
        @{ title = "Wasser sparen im Haushalt"; category = "environment" },
        @{ title = "Klimafreundliche Schulwege"; category = "climate" },
        @{ title = "Gesundheitsinformationen im Internet"; category = "health" },
        @{ title = "Einsamkeit in Großstädten"; category = "psychology" },
        @{ title = "Nachbarschaftsräte und Mitbestimmung"; category = "democracy" },
        @{ title = "Migration und berufliche Netzwerke"; category = "migration" },
        @{ title = "Familiengeschichten aufbewahren"; category = "history" },
        @{ title = "Amateursport und Fairness"; category = "sports" },
        @{ title = "Fußballvereine als soziale Orte"; category = "football" },
        @{ title = "Kino ohne große Budgets"; category = "cinema" },
        @{ title = "Bücher tauschen statt kaufen"; category = "books" },
        @{ title = "Musik beim Arbeiten"; category = "music" },
        @{ title = "Kunst in Schulen"; category = "art" },
        @{ title = "Praktika und echte Verantwortung"; category = "education" },
        @{ title = "Berufliche Grenzen setzen"; category = "work" },
        @{ title = "Freundschaft am Arbeitsplatz"; category = "friendship" },
        @{ title = "Pflege in der Familie"; category = "family" },
        @{ title = "Vegetarisches Essen in Kantinen"; category = "food" },
        @{ title = "Reisen mit wenig Gepäck"; category = "travel" },
        @{ title = "Wohnen in geteilten Häusern"; category = "city-life" },
        @{ title = "Taschengeld und Verantwortung"; category = "money" },
        @{ title = "Newsletter und Informationsflut"; category = "media" },
        @{ title = "Influencer und Vertrauen"; category = "social-media" },
        @{ title = "Ethische Kaufentscheidungen"; category = "ethics" },
        @{ title = "Zukunft der öffentlichen Bibliotheken"; category = "future" },
        @{ title = "Fehler beim Deutschsprechen"; category = "language-learning" },
        @{ title = "Willkommensfeste im Stadtteil"; category = "germany-and-integration" },
        @{ title = "Citizen Science im Alltag"; category = "science" },
        @{ title = "Weltraummüll und Verantwortung"; category = "space" },
        @{ title = "Digitale Geräte länger nutzen"; category = "technology" },
        @{ title = "Automatisierte Entscheidungen im Alltag"; category = "artificial-intelligence" },
        @{ title = "Saubere Flüsse in der Stadt"; category = "environment" },
        @{ title = "Kühle Orte im Sommer"; category = "climate" },
        @{ title = "Vorsorgeuntersuchungen verstehen"; category = "health" },
        @{ title = "Schlaf und digitale Medien"; category = "psychology" },
        @{ title = "Jugendparlamente und echte Macht"; category = "democracy" },
        @{ title = "Anerkennung ausländischer Abschlüsse"; category = "migration" },
        @{ title = "Sportliche Vorbilder"; category = "sports" },
        @{ title = "Fußball und lokale Identität"; category = "football" },
        @{ title = "Filme in Originalsprache"; category = "cinema" },
        @{ title = "Lesen auf dem Smartphone"; category = "books" },
        @{ title = "Chöre und Gemeinschaft"; category = "music" },
        @{ title = "Ausstellungen im öffentlichen Raum"; category = "art" },
        @{ title = "Lebenslanges Lernen ohne Druck"; category = "education" },
        @{ title = "Arbeitszeiten und Familienleben"; category = "work" },
        @{ title = "Freundschaft trotz unterschiedlicher Meinungen"; category = "friendship" },
        @{ title = "Essenspläne in Familien"; category = "food" },
        @{ title = "Deutschland verstehen durch Alltagssprache"; category = "germany-and-integration" }
    )
}
elseif ($TopicSet -eq "fifth") {
    $themeSpecs = @(
        @{ title = "Digitale Verträge im Alltag"; category = "technology" },
        @{ title = "Künstliche Intelligenz und Kreativität"; category = "artificial-intelligence" },
        @{ title = "Recycling in Mehrfamilienhäusern"; category = "environment" },
        @{ title = "Starkregen und Stadtplanung"; category = "climate" },
        @{ title = "Gesundheit am Schreibtisch"; category = "health" },
        @{ title = "Selbstvertrauen beim Sprechen"; category = "psychology" },
        @{ title = "Bürgerräte und Kompromisse"; category = "democracy" },
        @{ title = "Migration und Unternehmensgründung"; category = "migration" },
        @{ title = "Geschichte von Alltagsgegenständen"; category = "history" },
        @{ title = "Breitensport und Integration"; category = "sports" },
        @{ title = "Fußball und Geld"; category = "football" },
        @{ title = "Kinobesuche als Gemeinschaftserlebnis"; category = "cinema" },
        @{ title = "Buchhandlungen als Kulturorte"; category = "books" },
        @{ title = "Musik und politische Botschaften"; category = "music" },
        @{ title = "Kunstmärkte und faire Bezahlung"; category = "art" },
        @{ title = "Prüfungsangst und Lernstrategien"; category = "education" },
        @{ title = "Feedbackkultur im Büro"; category = "work" },
        @{ title = "Freundschaften nach der Ausbildung"; category = "friendship" },
        @{ title = "Familienrat und gemeinsame Entscheidungen"; category = "family" },
        @{ title = "Esskultur in internationalen Teams"; category = "food" },
        @{ title = "Reisen mit Sprachbarrieren"; category = "travel" },
        @{ title = "Leben in Hochhäusern"; category = "city-life" },
        @{ title = "Geldgeschenke und Erwartungen"; category = "money" },
        @{ title = "Lokaljournalismus im Internet"; category = "media" },
        @{ title = "Selbstdarstellung in sozialen Medien"; category = "social-media" },
        @{ title = "Verantwortung beim Teilen von Informationen"; category = "ethics" },
        @{ title = "Zukunft kleiner Geschäfte"; category = "future" },
        @{ title = "Dialekte und Standardsprache"; category = "language-learning" },
        @{ title = "Alltagsregeln in Deutschland"; category = "germany-and-integration" },
        @{ title = "Forschung im Alltag erklären"; category = "science" },
        @{ title = "Marsmissionen und Zusammenleben"; category = "space" },
        @{ title = "Digitale Sicherheit für Familien"; category = "technology" },
        @{ title = "Automatisierung in Supermärkten"; category = "artificial-intelligence" },
        @{ title = "Weniger Plastik im Badezimmer"; category = "environment" },
        @{ title = "Klimaschutz in Vereinen"; category = "climate" },
        @{ title = "Apothekenberatung verstehen"; category = "health" },
        @{ title = "Streitkultur in Gruppen"; category = "psychology" },
        @{ title = "Demokratie in der Schule"; category = "democracy" },
        @{ title = "Mehrsprachige Kinder in der Kita"; category = "germany-and-integration" },
        @{ title = "Ankommen in einem neuen Beruf"; category = "migration" },
        @{ title = "Sportliche Ziele im Alltag"; category = "sports" },
        @{ title = "Fußballturniere im Stadtteil"; category = "football" },
        @{ title = "Serienmarathons und Freizeit"; category = "cinema" },
        @{ title = "Lesekreise für Erwachsene"; category = "books" },
        @{ title = "Musikstreaming und Künstler"; category = "music" },
        @{ title = "Kunstkurse für Berufstätige"; category = "art" },
        @{ title = "Weiterbildung mit wenig Zeit"; category = "education" },
        @{ title = "Remote Work und Vertrauen"; category = "work" },
        @{ title = "Freundschaft und Geld"; category = "friendship" },
        @{ title = "Familienfeste planen"; category = "family" }
    )
}
elseif ($TopicSet -eq "sixth") {
    $themeSpecs = @(
        @{ title = "Digitale Belege für Steuererklärungen"; category = "technology" },
        @{ title = "Künstliche Intelligenz in Bewerbungen"; category = "artificial-intelligence" },
        @{ title = "Kompostieren in der Stadt"; category = "environment" },
        @{ title = "Klimafreundliche Urlaubsplanung"; category = "climate" },
        @{ title = "Sprechstunden gut vorbereiten"; category = "health" },
        @{ title = "Motivation nach Rückschlägen"; category = "psychology" },
        @{ title = "Transparenz in politischen Entscheidungen"; category = "democracy" },
        @{ title = "Migration und Wohnungssuche"; category = "migration" },
        @{ title = "Oral History in Familien"; category = "history" },
        @{ title = "Sport nach langer Pause"; category = "sports" },
        @{ title = "Fußball und Ehrenamt"; category = "football" },
        @{ title = "Filmfestivals in kleinen Städten"; category = "cinema" },
        @{ title = "Sachbücher und Meinungsbildung"; category = "books" },
        @{ title = "Musikunterricht online"; category = "music" },
        @{ title = "Kunst und künstliche Intelligenz"; category = "art" },
        @{ title = "Lernen in kleinen Gruppen"; category = "education" },
        @{ title = "Konflikte mit Vorgesetzten"; category = "work" },
        @{ title = "Freundschaft über große Entfernungen"; category = "friendship" },
        @{ title = "Familie und berufliche Mobilität"; category = "family" },
        @{ title = "Gemeinsam kochen im Büro"; category = "food" },
        @{ title = "Barrierefreies Reisen"; category = "travel" },
        @{ title = "Mehr Grün auf Balkonen"; category = "city-life" },
        @{ title = "Finanzielle Bildung in der Schule"; category = "money" },
        @{ title = "Mediatheken und klassische Sender"; category = "media" },
        @{ title = "Kurzvideos und Aufmerksamkeit"; category = "social-media" },
        @{ title = "Fairness bei Wartelisten"; category = "ethics" },
        @{ title = "Zukunft der Arztpraxen"; category = "future" },
        @{ title = "Aussprachetraining mit Apps"; category = "language-learning" },
        @{ title = "Formulare in einfacher Sprache"; category = "germany-and-integration" },
        @{ title = "Wissenschaftliche Studien verstehen"; category = "science" },
        @{ title = "Leben mit Satellitennavigation"; category = "space" },
        @{ title = "Passwörter und digitale Gewohnheiten"; category = "technology" },
        @{ title = "Chatbots als Lernpartner"; category = "artificial-intelligence" },
        @{ title = "Regenwasser nutzen"; category = "environment" },
        @{ title = "Hitzefrei in Schulen und Betrieben"; category = "climate" },
        @{ title = "Gesund schlafen trotz Schichtarbeit"; category = "health" },
        @{ title = "Perfektionismus beim Lernen"; category = "psychology" },
        @{ title = "Beteiligung bei Bauprojekten"; category = "democracy" },
        @{ title = "Sprachmittlung im Alltag"; category = "germany-and-integration" },
        @{ title = "Berufsabschlüsse aus dem Ausland"; category = "migration" },
        @{ title = "Sport und Körperbild"; category = "sports" },
        @{ title = "Derbyspiele und Sicherheit"; category = "football" },
        @{ title = "Filmkritiken und persönliche Meinung"; category = "cinema" },
        @{ title = "Bibliotheken und digitale Medien"; category = "books" },
        @{ title = "Musik und Erinnerung"; category = "music" },
        @{ title = "Kunsttherapie und Wohlbefinden"; category = "art" },
        @{ title = "Lernen am Arbeitsplatz dokumentieren"; category = "education" },
        @{ title = "Arbeitswege produktiv nutzen"; category = "work" },
        @{ title = "Freundschaften in Online-Communities"; category = "friendship" },
        @{ title = "Familienbudgets gemeinsam planen"; category = "money" }
    )
}
elseif ($TopicSet -eq "seventh") {
    $themeSpecs = @(
        @{ title = "Digitale Unterschriften im Berufsalltag"; category = "technology" },
        @{ title = "Künstliche Intelligenz und faire Chancen"; category = "artificial-intelligence" },
        @{ title = "Leihen statt Kaufen"; category = "environment" },
        @{ title = "Klimaschutz bei Veranstaltungen"; category = "climate" },
        @{ title = "Gesundheitschecks in der Firma"; category = "health" },
        @{ title = "Umgang mit Kritik beim Lernen"; category = "psychology" },
        @{ title = "Mehr Demokratie in Wohnprojekten"; category = "democracy" },
        @{ title = "Migration und digitale Communities"; category = "migration" },
        @{ title = "Geschichte von Nachbarschaften"; category = "history" },
        @{ title = "Vereinssport und Lebensbalance"; category = "sports" },
        @{ title = "Fußballschulen für Kinder"; category = "football" },
        @{ title = "Kurzfilme als Gesprächsanlass"; category = "cinema" },
        @{ title = "Lieblingsbücher neu lesen"; category = "books" },
        @{ title = "Musikproben und Disziplin"; category = "music" },
        @{ title = "Kunstpreise und Öffentlichkeit"; category = "art" },
        @{ title = "Noten ohne Druck"; category = "education" },
        @{ title = "Rollenwechsel im Team"; category = "work" },
        @{ title = "Freundschaften in Sprachkursen"; category = "friendship" },
        @{ title = "Generationen unter einem Dach"; category = "family" },
        @{ title = "Pausenbrot und Essgewohnheiten"; category = "food" },
        @{ title = "Allein reisen und Sicherheit"; category = "travel" },
        @{ title = "Stadtviertel ohne Autos"; category = "city-life" },
        @{ title = "Gemeinsame Kassen in Wohngemeinschaften"; category = "money" },
        @{ title = "Medienpausen im Alltag"; category = "media" },
        @{ title = "Anonyme Profile und Verantwortung"; category = "social-media" },
        @{ title = "Ehrlichkeit bei Online-Bewertungen"; category = "ethics" },
        @{ title = "Zukunft der Weiterbildung"; category = "future" },
        @{ title = "Grammatik im Gespräch üben"; category = "language-learning" },
        @{ title = "Deutsch am Arbeitsplatz verstehen"; category = "germany-and-integration" },
        @{ title = "Forschung im Museum"; category = "science" },
        @{ title = "Weltraumtourismus und Gerechtigkeit"; category = "space" },
        @{ title = "Digitale Erinnerungen sichern"; category = "technology" },
        @{ title = "Automatische Texte und Urheberrecht"; category = "artificial-intelligence" },
        @{ title = "Reparatur statt Neukauf im Büro"; category = "environment" },
        @{ title = "Klimadaten verständlich erklären"; category = "climate" },
        @{ title = "Hausarzt oder Notaufnahme"; category = "health" },
        @{ title = "Mut zum Nachfragen"; category = "psychology" },
        @{ title = "Kommunalpolitik im Alltag"; category = "democracy" },
        @{ title = "Mehrsprachige Beratungsstellen"; category = "germany-and-integration" },
        @{ title = "Ankommen in einem neuen Verein"; category = "migration" },
        @{ title = "Sport und soziale Regeln"; category = "sports" },
        @{ title = "Fußballreisen und Umwelt"; category = "football" },
        @{ title = "Filmfiguren als Vorbilder"; category = "cinema" },
        @{ title = "Bücherregale und Persönlichkeit"; category = "books" },
        @{ title = "Musik in öffentlichen Verkehrsmitteln"; category = "music" },
        @{ title = "Kunst im digitalen Raum"; category = "art" },
        @{ title = "Prüfungen fair gestalten"; category = "education" },
        @{ title = "Neue Mitarbeitende einarbeiten"; category = "work" },
        @{ title = "Freundschaft und Distanz"; category = "friendship" },
        @{ title = "Familienregeln neu verhandeln"; category = "family" }
    )
}
elseif ($TopicSet -eq "eighth") {
    $themeSpecs = @(
        @{ title = "Digitale Terminplanung im Team"; category = "technology" },
        @{ title = "Künstliche Intelligenz und persönliche Beratung"; category = "artificial-intelligence" },
        @{ title = "Gemeinschaftsgärten im Viertel"; category = "environment" },
        @{ title = "Klimaschutz und kleine Unternehmen"; category = "climate" },
        @{ title = "Gesundheitskurse im Betrieb"; category = "health" },
        @{ title = "Geduld beim Sprachenlernen"; category = "psychology" },
        @{ title = "Demokratische Entscheidungen im Alltag"; category = "democracy" },
        @{ title = "Migration und Nachbarschaftshilfe"; category = "migration" },
        @{ title = "Alte Fotos und neue Geschichten"; category = "history" },
        @{ title = "Sportangebote für Anfänger"; category = "sports" },
        @{ title = "Fußballtraining und Teamgeist"; category = "football" },
        @{ title = "Dokumentarserien und Diskussionen"; category = "cinema" },
        @{ title = "Bücher aus anderen Ländern"; category = "books" },
        @{ title = "Musik im Sprachunterricht"; category = "music" },
        @{ title = "Kunsthandwerk und moderne Märkte"; category = "art" },
        @{ title = "Berufsschulen und Praxisnähe"; category = "education" },
        @{ title = "Fehlerkultur in Projekten"; category = "work" },
        @{ title = "Freundschaften nach Konflikten"; category = "friendship" },
        @{ title = "Familie und digitale Kommunikation"; category = "family" },
        @{ title = "Essen bestellen oder selbst kochen"; category = "food" },
        @{ title = "Reisen mit begrenztem Budget"; category = "travel" },
        @{ title = "Ruhige Orte in lauten Städten"; category = "city-life" },
        @{ title = "Geld sparen durch Teilen"; category = "money" },
        @{ title = "Nachrichten verstehen ohne Überforderung"; category = "media" },
        @{ title = "Soziale Medien und echte Nähe"; category = "social-media" },
        @{ title = "Gerechte Aufgabenverteilung"; category = "ethics" },
        @{ title = "Zukunft der Mobilität im Alltag"; category = "future" },
        @{ title = "Wortschatz durch Gespräche erweitern"; category = "language-learning" },
        @{ title = "Behördentermine gut vorbereiten"; category = "germany-and-integration" },
        @{ title = "Experimente zu Hause erklären"; category = "science" },
        @{ title = "Sternenhimmel in der Stadt"; category = "space" },
        @{ title = "Digitale Notizen organisieren"; category = "technology" },
        @{ title = "Automatisierte Empfehlungen im Alltag"; category = "artificial-intelligence" },
        @{ title = "Kleidung länger tragen"; category = "environment" },
        @{ title = "Klimaschutz in Mietshäusern"; category = "climate" },
        @{ title = "Gesundheitsfragen im Familienkreis"; category = "health" },
        @{ title = "Selbstkritik und Fortschritt"; category = "psychology" },
        @{ title = "Abstimmungen in Online-Gruppen"; category = "democracy" },
        @{ title = "Sprachcafés als Begegnungsorte"; category = "germany-and-integration" },
        @{ title = "Neue Heimat und alte Gewohnheiten"; category = "migration" },
        @{ title = "Sport im Park"; category = "sports" },
        @{ title = "Fußball und Vorurteile"; category = "football" },
        @{ title = "Lieblingsfilme im Freundeskreis"; category = "cinema" },
        @{ title = "Bücher verschenken"; category = "books" },
        @{ title = "Konzerte im Freien"; category = "music" },
        @{ title = "Kunst im Alltag entdecken"; category = "art" },
        @{ title = "Lernziele realistisch setzen"; category = "education" },
        @{ title = "Kommunikation bei Stress"; category = "work" },
        @{ title = "Freundschaft und neue Lebensphasen"; category = "friendship" },
        @{ title = "Familienzeit ohne Bildschirm"; category = "family" }
    )
}
elseif ($TopicSet -ne "first") {
    throw "Unsupported TopicSet '$TopicSet'. Use 'first', 'second', 'third', 'fourth', 'fifth', 'sixth', 'seventh', or 'eighth'."
}

$b2Contexts = @("in der Praxis", "in der Zukunft")

function Convert-ToSlug([string]$value) {
    $normalized = $value.ToLowerInvariant()
    $normalized = $normalized.Replace("ä", "ae").Replace("ö", "oe").Replace("ü", "ue").Replace("ß", "ss")
    $normalized = $normalized -replace "[^a-z0-9]+", "-"
    return ($normalized -replace "^-|-$", "")
}

function Get-ArticleTarget([string]$level) {
    switch ($level) {
        "B1" { return 2000 }
        "B2" { return 2500 }
        "C1" { return 3000 }
        default { throw "Unsupported level '$level'." }
    }
}

function Get-VocabularyCount([string]$level) {
    switch ($level) {
        "B1" { return 22 }
        "B2" { return 28 }
        "C1" { return 32 }
        default { throw "Unsupported level '$level'." }
    }
}

function Get-ContentType([string]$level, [int]$index) {
    if ($level -eq "B1") {
        return @("article", "fact-sheet", "opinion-text", "interview", "movie-summary")[$index % 5]
    }
    if ($level -eq "B2") {
        return @("article", "fact-sheet", "opinion-text", "interview", "movie-summary")[$index % 5]
    }
    return @("article", "debate-text", "book-summary", "movie-summary", "opinion-text", "interview")[$index % 6]
}

function Get-CategoryDisplay([string]$category) {
    switch ($category) {
        "technology" { return "Technik" }
        "artificial-intelligence" { return "künstliche Intelligenz" }
        "environment" { return "Umwelt" }
        "climate" { return "Klima" }
        "space" { return "Weltraum" }
        "health" { return "Gesundheit" }
        "psychology" { return "Psychologie" }
        "society" { return "Gesellschaft" }
        "democracy" { return "Demokratie" }
        "migration" { return "Migration" }
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
        "science" { return "Wissenschaft" }
        default { return "Alltag" }
    }
}

function Get-Topics([string]$category) {
    if ($topicKeysByCategory.ContainsKey($category)) {
        return $topicKeysByCategory[$category]
    }

    return "everyday-life"
}

function Test-IsSensitive([string]$category, [string]$title) {
    return $category -in @("health", "psychology", "migration", "democracy", "money", "ethics") -or
        $title -match "Diskriminierung|Behörden|Geldsorgen|Pflege|Protest"
}

function Fit-ArticleLength([string]$text, [string]$level) {
    $target = Get-ArticleTarget $level
    $minimum = $target - 100
    $maximum = $target + 100
    $normalized = ($text -replace "\s+", " ").Trim()
    $bridges = @(
        "Diese Beobachtung hilft der Gruppe, eigene Beispiele zu sammeln und genauer nachzufragen.",
        "So entsteht ein Gespräch, in dem verschiedene Erfahrungen nebeneinanderstehen können.",
        "Am Ende müssen nicht alle dieselbe Meinung haben, aber alle sollten ihre Gründe nennen.",
        "Wer zuhört und nachfragt, kann das Thema aus einer neuen Perspektive sehen."
    )

    $index = 0
    while ($normalized.Length -lt $minimum) {
        $normalized = "$normalized $($bridges[$index % $bridges.Count])"
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
    if ($level -eq "B1") {
        $sentences = @(
            "$title ist ein Thema aus dem Bereich $categoryDisplay und passt gut zu Gesprächen im Kurs oder in einer Sprachgruppe.",
            "Viele Menschen können dazu eigene Erfahrungen nennen, auch wenn sie nicht jeden Fachbegriff kennen.",
            "Wichtig ist, konkrete Situationen zu beschreiben und einfache Gründe zu geben.",
            "Eine Person kann erzählen, was sie im Alltag beobachtet hat.",
            "Eine andere Person kann vergleichen, wie die Situation in einer Familie, einer Schule, einer Firma oder einer Stadt wirkt.",
            "Das Gespräch wird besser, wenn man nicht nur sagt, ob etwas gut oder schlecht ist.",
            "Hilfreich sind Beispiele, kurze Erklärungen und freundliche Nachfragen.",
            "Man kann auch überlegen, welche Vorteile und Nachteile verschiedene Lösungen haben.",
            "Manche Fragen bleiben offen, weil Menschen unterschiedliche Wünsche und Grenzen haben.",
            "Gerade deshalb eignet sich das Thema zum Üben: Die Gruppe kann Meinungen austauschen, Gründe sammeln und am Ende die wichtigsten Punkte zusammenfassen."
        )
    }
    elseif ($level -eq "B2") {
        $sentences = @(
            "$title verbindet persönliche Erfahrungen mit gesellschaftlichen Entwicklungen im Bereich $categoryDisplay.",
            "Auf den ersten Blick scheint das Thema leicht zugänglich, doch bei genauerem Hinsehen entstehen mehrere Spannungen.",
            "Einige Menschen betonen praktische Vorteile, andere weisen auf Risiken, Kosten oder fehlende Teilhabe hin.",
            "Für eine gute Diskussion reicht es deshalb nicht, nur Zustimmung oder Ablehnung zu äußern.",
            "Teilnehmende sollten erklären, welche Werte ihnen wichtig sind und welche Folgen sie erwarten.",
            "Interessant ist auch, wer von einer Veränderung profitiert und wer zusätzliche Unterstützung braucht.",
            "In vielen Situationen spielen Gewohnheiten, technische Möglichkeiten, Geld, Zeit und Vertrauen zusammen.",
            "Die Gruppe kann verschiedene Beispiele vergleichen: Was funktioniert im privaten Alltag, was am Arbeitsplatz und was in öffentlichen Einrichtungen?",
            "Auch ein Blick in die Zukunft ist sinnvoll, denn kleine Entscheidungen von heute können später größere Wirkungen haben.",
            "Das Ziel der Diskussion ist nicht ein schneller Konsens, sondern eine begründete Einschätzung mit klaren Beispielen und fairen Gegenfragen."
        )
    }
    else {
        $sentences = @(
            "$title eröffnet im Bereich $categoryDisplay eine anspruchsvolle Debatte, weil individuelle Freiheit, soziale Verantwortung und institutionelle Rahmenbedingungen ineinandergreifen.",
            "Wer darüber spricht, muss häufig zwischen kurzfristigem Nutzen, langfristigen Folgen und der Frage nach gerechter Verteilung abwägen.",
            "Gerade für fortgeschrittene Lernende ist das Thema ergiebig, weil es präzise Begriffe, differenzierte Beispiele und eine klare Argumentationsstruktur verlangt.",
            "Eine überzeugende Position sollte nicht nur behaupten, was richtig wäre, sondern auch mögliche Einwände vorwegnehmen.",
            "Dabei lohnt es sich, zwischen persönlichen Erfahrungen, medialen Darstellungen und empirisch beobachtbaren Entwicklungen zu unterscheiden.",
            "Je nach Perspektive kann dieselbe Veränderung als Chance, Belastung oder notwendiger Kompromiss erscheinen.",
            "Eine moderierte Diskussion kann fragen, welche Interessen sichtbar werden, welche Stimmen fehlen und welche Regeln Vertrauen schaffen könnten.",
            "Besonders produktiv wird das Gespräch, wenn Teilnehmende Beispiele aus verschiedenen Lebensbereichen vergleichen.",
            "So lässt sich prüfen, ob eine Lösung nur in einem bestimmten Umfeld funktioniert oder auch auf andere Situationen übertragbar ist.",
            "Am Ende steht idealerweise kein einfacher Slogan, sondern eine nuancierte Zusammenfassung mit begründeten Prioritäten."
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
    $count = if ($level -in @("B2", "C1")) { 4 } else { 3 }
    $prompts = @(
        "Was fällt dir zuerst zu ""$title"" ein?",
        "Wo begegnet dir dieses Thema im Alltag?",
        "Welche Erfahrung aus deinem Umfeld passt dazu?",
        "Welche Frage sollte die Gruppe zuerst klären?"
    )

    $items = @()
    for ($i = 0; $i -lt $count; $i++) {
        $items += [ordered]@{ prompt = $prompts[$i]; sortOrder = ($i + 1) * 10 }
    }

    return $items
}

function New-DiscussionQuestions([string]$title, [string]$level) {
    $countPerType = if ($level -in @("B2", "C1")) { 3 } else { 2 }
    $templates = @{
        "opinion" = @(
            "Welche Position hast du zu ""$title"" und warum?",
            "Welche Werte sind bei diesem Thema besonders wichtig?",
            "Welches Argument findest du am stärksten?"
        )
        "imagination" = @(
            "Wie könnte eine mutige, aber realistische Lösung aussehen?",
            "Was würde sich ändern, wenn viele Menschen anders handeln würden?",
            "Wie sähe eine besonders faire Situation aus?"
        )
        "prediction" = @(
            "Wie könnte sich dieses Thema in den nächsten Jahren entwickeln?",
            "Welche Folgen wären für junge Menschen wahrscheinlich?",
            "Welche neue Konfliktlinie könnte entstehen?"
        )
        "comparison" = @(
            "Wie unterscheidet sich die Situation im privaten Alltag und am Arbeitsplatz?",
            "Was ist heute anders als vor einigen Jahren?",
            "Welche Lösung hat langfristig mehr Vorteile?"
        )
    }

    $items = @()
    $sort = 10
    foreach ($type in @("opinion", "imagination", "prediction", "comparison")) {
        for ($i = 0; $i -lt $countPerType; $i++) {
            $items += [ordered]@{ prompt = $templates[$type][$i]; questionType = $type; sortOrder = $sort }
            $sort += 10
        }
    }

    return $items
}

function New-VocabularyItems([string]$level, [string]$category) {
    $count = Get-VocabularyCount $level
    $lemmas = @()
    if ($categoryVocabulary.ContainsKey($category)) {
        $lemmas += $categoryVocabulary[$category]
    }
    $lemmas += $vocabularyBase

    $items = @()
    $seen = New-Object System.Collections.Generic.HashSet[string]
    $sort = 10
    foreach ($lemma in $lemmas) {
        $key = $lemma.ToLowerInvariant()
        if (-not $seen.Add($key)) {
            continue
        }

        $items += [ordered]@{
            lemma = $lemma
            wordSlug = Convert-ToSlug $lemma
            cefrLevel = $level
            sortOrder = $sort
        }
        $sort += 10

        if ($items.Count -ge $count) {
            break
        }
    }

    return $items
}

function New-SpeakingGoals([string]$level, [string]$contentType) {
    if ($level -eq "B1") {
        return @("express-opinion", "give-reasons", "compare-options")
    }
    if ($level -eq "B2") {
        return @("express-opinion", "give-reasons", "compare-options", "make-predictions")
    }
    if ($contentType -eq "debate-text") {
        return @("give-reasons", "agree-disagree", "debate-politely", "summarize-position")
    }
    return @("give-reasons", "compare-options", "debate-politely", "summarize-position")
}

function New-TalkTopic([hashtable]$spec, [string]$level, [string]$groupPrefix, [int]$index) {
    $title = [string]$spec.title
    $category = [string]$spec.category
    $baseSlug = Convert-ToSlug $title
    $slug = "$(Convert-ToSlug $level)-$groupPrefix-$baseSlug"
    $groupKey = "$groupPrefix-$baseSlug"
    $contentType = Get-ContentType $level $index
    $sensitive = Test-IsSensitive $category $title

    return [ordered]@{
        slug = $slug
        topicGroupKey = $groupKey
        title = $title
        description = "Ein Gesprächsthema über $title mit Fokus auf persönliche Beispiele, begründete Meinungen und respektvolle Diskussion."
        cefrLevel = $level
        category = $category
        topics = @(Get-Topics $category)
        contentType = $contentType
        estimatedReadingMinutes = if ($level -eq "B1") { 4 } elseif ($level -eq "B2") { 5 } else { 6 }
        estimatedDiscussionMinutes = if ($level -eq "B1") { 25 } else { 30 }
        isSensitive = $sensitive
        recommendedForModeratedGroupsOnly = $sensitive
        sensitivityNote = if ($sensitive) { "Kann persönliche Erfahrungen oder gesellschaftliche Konflikte berühren." } else { $null }
        article = [ordered]@{ baseText = New-Article $title $category $level $contentType }
        warmupQuestions = New-WarmupQuestions $title $level
        discussionQuestions = New-DiscussionQuestions $title $level
        vocabularyItems = New-VocabularyItems $level $category
        speakingGoals = New-SpeakingGoals $level $contentType
        sortOrder = 20000 + $index
        isPublished = $true
    }
}

if (Test-Path $OutputPath) {
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath | Out-Null

$talkTopics = @()
$index = 0
foreach ($spec in $themeSpecs) {
    foreach ($level in @("B1", "B2", "C1")) {
        $talkTopics += New-TalkTopic $spec $level $GroupPrefix $index
        $index++
    }
}

foreach ($spec in $themeSpecs) {
    foreach ($context in $b2Contexts) {
        $b2Spec = @{
            title = "$($spec.title) $context"
            category = $spec.category
        }
        $talkTopics += New-TalkTopic $b2Spec "B2" $B2GroupPrefix $index
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

    $filePath = Join-Path $OutputPath ("de-talk-topics-20260510-b1-c1-expansion-{0:D3}.json" -f $packageIndex)
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
