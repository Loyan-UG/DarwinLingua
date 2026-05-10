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
elseif ($TopicSet -eq "third") {
    $topicSpecs = @(
        @{ title = "Der Wecker klingelt"; category = "everyday-life"; place = "am Morgen"; action = "aufstehen"; object = "der Wecker" },
        @{ title = "Brot und Käse"; category = "food"; place = "in der Küche"; action = "essen"; object = "das Brot" },
        @{ title = "Zur Haltestelle gehen"; category = "travel"; place = "auf der Straße"; action = "gehen"; object = "die Haltestelle" },
        @{ title = "Milch kaufen"; category = "shopping"; place = "im Supermarkt"; action = "kaufen"; object = "die Milch" },
        @{ title = "Mein Bruder"; category = "family"; place = "zu Hause"; action = "sprechen"; object = "der Bruder" },
        @{ title = "Eine kurze Frage"; category = "everyday-life"; place = "im Kurs"; action = "fragen"; object = "die Frage" },
        @{ title = "Husten haben"; category = "health"; place = "zu Hause"; action = "sich ausruhen"; object = "der Husten" },
        @{ title = "Das Wohnzimmer"; category = "housing"; place = "in der Wohnung"; action = "sitzen"; object = "das Sofa" },
        @{ title = "Die Nachbarin grüßen"; category = "housing"; place = "im Haus"; action = "grüßen"; object = "die Nachbarin" },
        @{ title = "Brötchen holen"; category = "shopping"; place = "in der Bäckerei"; action = "holen"; object = "das Brötchen" },
        @{ title = "Arbeitsbeginn"; category = "work"; place = "bei der Arbeit"; action = "beginnen"; object = "die Arbeit" },
        @{ title = "Der Stundenplan"; category = "education"; place = "im Kurs"; action = "lesen"; object = "der Plan" },
        @{ title = "Ein Glas Wasser"; category = "health"; place = "zu Hause"; action = "trinken"; object = "das Glas" },
        @{ title = "Fernsehen am Abend"; category = "everyday-life"; place = "zu Hause"; action = "fernsehen"; object = "der Fernseher" },
        @{ title = "Eine Hose kaufen"; category = "shopping"; place = "im Geschäft"; action = "anprobieren"; object = "die Hose" },
        @{ title = "Kuchen im Café"; category = "food"; place = "im Café"; action = "bestellen"; object = "der Kuchen" },
        @{ title = "Am Gleis warten"; category = "travel"; place = "am Bahnhof"; action = "warten"; object = "das Gleis" },
        @{ title = "Freitagabend"; category = "everyday-life"; place = "am Abend"; action = "planen"; object = "der Freitag" },
        @{ title = "Eine Freundin treffen"; category = "family"; place = "in der Stadt"; action = "treffen"; object = "die Freundin" },
        @{ title = "Eine Antwort schreiben"; category = "work"; place = "am Computer"; action = "schreiben"; object = "die Antwort" },
        @{ title = "Bauchschmerzen haben"; category = "health"; place = "zu Hause"; action = "warten"; object = "der Bauch" },
        @{ title = "Ein Satz im Kurs"; category = "education"; place = "im Unterricht"; action = "sprechen"; object = "der Satz" },
        @{ title = "Eine Karte kaufen"; category = "shopping"; place = "im Laden"; action = "kaufen"; object = "die Karte" },
        @{ title = "Nudeln kochen"; category = "food"; place = "in der Küche"; action = "kochen"; object = "die Nudeln" },
        @{ title = "Den Tisch putzen"; category = "housing"; place = "zu Hause"; action = "putzen"; object = "der Tisch" },
        @{ title = "Ein Platz im Bus"; category = "travel"; place = "im Bus"; action = "sitzen"; object = "der Platz" },
        @{ title = "Die Telefonnummer sagen"; category = "everyday-life"; place = "im Büro"; action = "sagen"; object = "die Nummer" },
        @{ title = "Eine Kollegin hilft"; category = "work"; place = "bei der Arbeit"; action = "helfen"; object = "die Kollegin" },
        @{ title = "Laut lesen"; category = "education"; place = "im Kurs"; action = "lesen"; object = "der Text" },
        @{ title = "Die Speisekarte lesen"; category = "food"; place = "im Restaurant"; action = "lesen"; object = "die Speisekarte" },
        @{ title = "Äpfel kaufen"; category = "shopping"; place = "auf dem Markt"; action = "kaufen"; object = "der Apfel" },
        @{ title = "Der Computer startet"; category = "work"; place = "im Büro"; action = "starten"; object = "der Computer" },
        @{ title = "Schnee am Morgen"; category = "everyday-life"; place = "draußen"; action = "gehen"; object = "der Schnee" },
        @{ title = "Eltern anrufen"; category = "family"; place = "zu Hause"; action = "anrufen"; object = "die Eltern" },
        @{ title = "Reis mit Gemüse"; category = "food"; place = "zu Hause"; action = "kochen"; object = "der Reis" },
        @{ title = "Ein Hotelzimmer"; category = "travel"; place = "im Hotel"; action = "schlafen"; object = "das Zimmer" },
        @{ title = "Der Schlüssel fehlt"; category = "housing"; place = "zu Hause"; action = "suchen"; object = "der Schlüssel" },
        @{ title = "Fieber messen"; category = "health"; place = "zu Hause"; action = "messen"; object = "das Fieber" },
        @{ title = "Vokabeln üben"; category = "education"; place = "zu Hause"; action = "üben"; object = "die Vokabel" },
        @{ title = "Ein Paket bringen"; category = "everyday-life"; place = "an der Tür"; action = "bringen"; object = "das Paket" },
        @{ title = "Eine Mütze kaufen"; category = "shopping"; place = "im Geschäft"; action = "kaufen"; object = "die Mütze" },
        @{ title = "Der Dienstplan"; category = "work"; place = "bei der Arbeit"; action = "lesen"; object = "der Dienstplan" },
        @{ title = "Musik hören"; category = "everyday-life"; place = "zu Hause"; action = "hören"; object = "die Musik" },
        @{ title = "Mit der Mutter sprechen"; category = "family"; place = "am Telefon"; action = "sprechen"; object = "die Mutter" },
        @{ title = "Tomaten schneiden"; category = "food"; place = "in der Küche"; action = "schneiden"; object = "die Tomate" },
        @{ title = "Eine Linie suchen"; category = "travel"; place = "in der Stadt"; action = "suchen"; object = "die Linie" },
        @{ title = "Das Fenster öffnen"; category = "housing"; place = "in der Wohnung"; action = "öffnen"; object = "das Fenster" },
        @{ title = "Leise sprechen"; category = "health"; place = "in der Praxis"; action = "sprechen"; object = "die Stimme" },
        @{ title = "Zu spät kommen"; category = "work"; place = "zum Termin"; action = "kommen"; object = "die Zeit" },
        @{ title = "Ein Wort erklären"; category = "education"; place = "im Kurs"; action = "erklären"; object = "das Wort" }
    )
}
elseif ($TopicSet -eq "fourth") {
    $topicSpecs = @(
        @{ title = "Die Tasche packen"; category = "everyday-life"; place = "zu Hause"; action = "packen"; object = "die Tasche" },
        @{ title = "Joghurt und Obst"; category = "food"; place = "in der Küche"; action = "essen"; object = "der Joghurt" },
        @{ title = "Zum Arzt fahren"; category = "travel"; place = "in der Stadt"; action = "fahren"; object = "die Praxis" },
        @{ title = "Batterien kaufen"; category = "shopping"; place = "im Laden"; action = "kaufen"; object = "die Batterie" },
        @{ title = "Mein Vater"; category = "family"; place = "zu Hause"; action = "reden"; object = "der Vater" },
        @{ title = "Eine Uhr kaufen"; category = "shopping"; place = "im Geschäft"; action = "kaufen"; object = "die Uhr" },
        @{ title = "Die Tür öffnen"; category = "housing"; place = "in der Wohnung"; action = "öffnen"; object = "die Tür" },
        @{ title = "Die Hände waschen"; category = "health"; place = "im Bad"; action = "waschen"; object = "die Hand" },
        @{ title = "Eine Pause brauchen"; category = "work"; place = "bei der Arbeit"; action = "Pause machen"; object = "die Pause" },
        @{ title = "Den Text hören"; category = "education"; place = "im Kurs"; action = "hören"; object = "der Text" },
        @{ title = "Zur Arbeit laufen"; category = "travel"; place = "auf der Straße"; action = "laufen"; object = "der Weg" },
        @{ title = "Kaffee ohne Zucker"; category = "food"; place = "im Café"; action = "bestellen"; object = "der Zucker" },
        @{ title = "Die Lampe ist kaputt"; category = "housing"; place = "zu Hause"; action = "reparieren"; object = "die Lampe" },
        @{ title = "Eine Liste schreiben"; category = "everyday-life"; place = "zu Hause"; action = "schreiben"; object = "die Liste" },
        @{ title = "Bananen kaufen"; category = "shopping"; place = "im Supermarkt"; action = "kaufen"; object = "die Banane" },
        @{ title = "Die Tante besuchen"; category = "family"; place = "bei der Familie"; action = "besuchen"; object = "die Tante" },
        @{ title = "Ein Rezept lesen"; category = "food"; place = "in der Küche"; action = "lesen"; object = "das Rezept" },
        @{ title = "An der Ampel warten"; category = "travel"; place = "in der Stadt"; action = "warten"; object = "die Ampel" },
        @{ title = "Ein Pflaster holen"; category = "health"; place = "in der Apotheke"; action = "holen"; object = "das Pflaster" },
        @{ title = "Die Hausnummer suchen"; category = "everyday-life"; place = "in der Straße"; action = "suchen"; object = "die Hausnummer" },
        @{ title = "Der Kalender im Büro"; category = "work"; place = "im Büro"; action = "planen"; object = "der Kalender" },
        @{ title = "Das Bild zeigen"; category = "education"; place = "im Kurs"; action = "zeigen"; object = "das Bild" },
        @{ title = "Ein Kilo Kartoffeln"; category = "shopping"; place = "auf dem Markt"; action = "kaufen"; object = "die Kartoffel" },
        @{ title = "Der Balkon"; category = "housing"; place = "zu Hause"; action = "sitzen"; object = "der Balkon" },
        @{ title = "Eine Suppe essen"; category = "food"; place = "zu Hause"; action = "essen"; object = "die Suppe" },
        @{ title = "Die Oma anrufen"; category = "family"; place = "am Telefon"; action = "anrufen"; object = "die Oma" },
        @{ title = "Zum Kurs laufen"; category = "education"; place = "zur Schule"; action = "laufen"; object = "der Kurs" },
        @{ title = "Den Namen buchstabieren"; category = "everyday-life"; place = "im Büro"; action = "buchstabieren"; object = "der Name" },
        @{ title = "Eine Tablette nehmen"; category = "health"; place = "zu Hause"; action = "nehmen"; object = "die Tablette" },
        @{ title = "Der Bus ist voll"; category = "travel"; place = "im Bus"; action = "stehen"; object = "der Bus" },
        @{ title = "Der Drucker geht nicht"; category = "work"; place = "bei der Arbeit"; action = "fragen"; object = "der Drucker" },
        @{ title = "Ein kleines Geschenk"; category = "shopping"; place = "im Laden"; action = "finden"; object = "das Geschenk" },
        @{ title = "Müsli am Morgen"; category = "food"; place = "in der Küche"; action = "essen"; object = "das Müsli" },
        @{ title = "Das Kinderzimmer"; category = "housing"; place = "in der Wohnung"; action = "aufräumen"; object = "das Zimmer" },
        @{ title = "Mit dem Onkel sprechen"; category = "family"; place = "zu Hause"; action = "sprechen"; object = "der Onkel" },
        @{ title = "Die Hausaufgabe zeigen"; category = "education"; place = "im Kurs"; action = "zeigen"; object = "die Hausaufgabe" },
        @{ title = "Frische Luft"; category = "health"; place = "draußen"; action = "gehen"; object = "die Luft" },
        @{ title = "Ein Ticket kaufen"; category = "travel"; place = "am Automaten"; action = "kaufen"; object = "das Ticket" },
        @{ title = "Die E-Mail öffnen"; category = "work"; place = "am Computer"; action = "öffnen"; object = "die E-Mail" },
        @{ title = "Den Müll rausbringen"; category = "housing"; place = "zu Hause"; action = "rausbringen"; object = "der Müll" },
        @{ title = "Ein Glas Saft"; category = "food"; place = "im Café"; action = "trinken"; object = "der Saft" },
        @{ title = "Die Farbe wählen"; category = "shopping"; place = "im Geschäft"; action = "wählen"; object = "die Farbe" },
        @{ title = "Eine Nachricht sprechen"; category = "everyday-life"; place = "am Handy"; action = "sprechen"; object = "die Nachricht" },
        @{ title = "Der Fahrstuhl"; category = "housing"; place = "im Haus"; action = "fahren"; object = "der Fahrstuhl" },
        @{ title = "Die Rechnung teilen"; category = "food"; place = "im Restaurant"; action = "bezahlen"; object = "die Rechnung" },
        @{ title = "Eine Frage verstehen"; category = "education"; place = "im Kurs"; action = "verstehen"; object = "die Frage" },
        @{ title = "Ein Bild vom Urlaub"; category = "travel"; place = "im Urlaub"; action = "zeigen"; object = "das Foto" },
        @{ title = "Der Arbeitsplatz"; category = "work"; place = "im Büro"; action = "arbeiten"; object = "der Tisch" },
        @{ title = "Ein warmer Tee"; category = "health"; place = "zu Hause"; action = "trinken"; object = "der Tee" },
        @{ title = "Der kleine Einkauf"; category = "shopping"; place = "im Supermarkt"; action = "einkaufen"; object = "der Einkauf" }
    )
}
elseif ($TopicSet -eq "fifth") {
    $topicSpecs = @(
        @{ title = "Der erste Kaffee"; category = "food"; place = "in der Küche"; action = "trinken"; object = "der Kaffee" },
        @{ title = "Die Tasche finden"; category = "everyday-life"; place = "zu Hause"; action = "suchen"; object = "die Tasche" },
        @{ title = "Zum Supermarkt laufen"; category = "shopping"; place = "in der Straße"; action = "laufen"; object = "der Supermarkt" },
        @{ title = "Ein Brot schneiden"; category = "food"; place = "in der Küche"; action = "schneiden"; object = "das Brot" },
        @{ title = "Die Tochter abholen"; category = "family"; place = "in der Schule"; action = "abholen"; object = "die Tochter" },
        @{ title = "Ein Formular lesen"; category = "everyday-life"; place = "im Büro"; action = "lesen"; object = "das Formular" },
        @{ title = "Ein Pflaster brauchen"; category = "health"; place = "zu Hause"; action = "brauchen"; object = "das Pflaster" },
        @{ title = "Das Fenster putzen"; category = "housing"; place = "in der Wohnung"; action = "putzen"; object = "das Fenster" },
        @{ title = "Eine Treppe steigen"; category = "housing"; place = "im Haus"; action = "steigen"; object = "die Treppe" },
        @{ title = "Im Kiosk"; category = "shopping"; place = "im Kiosk"; action = "bezahlen"; object = "die Zeitung" },
        @{ title = "Ein kurzer Arbeitstag"; category = "work"; place = "bei der Arbeit"; action = "arbeiten"; object = "der Tag" },
        @{ title = "Das Wörterbuch benutzen"; category = "education"; place = "im Kurs"; action = "benutzen"; object = "das Wörterbuch" },
        @{ title = "Der Hals tut weh"; category = "health"; place = "zu Hause"; action = "trinken"; object = "der Hals" },
        @{ title = "Radio hören"; category = "everyday-life"; place = "zu Hause"; action = "hören"; object = "das Radio" },
        @{ title = "Eine Bluse kaufen"; category = "shopping"; place = "im Geschäft"; action = "kaufen"; object = "die Bluse" },
        @{ title = "Ein Wasser bestellen"; category = "food"; place = "im Café"; action = "bestellen"; object = "das Wasser" },
        @{ title = "Den Bus verpassen"; category = "travel"; place = "an der Haltestelle"; action = "warten"; object = "der Bus" },
        @{ title = "Sonntag zu Hause"; category = "everyday-life"; place = "zu Hause"; action = "sich ausruhen"; object = "der Sonntag" },
        @{ title = "Die Familie einladen"; category = "family"; place = "zu Hause"; action = "einladen"; object = "die Familie" },
        @{ title = "Eine Datei öffnen"; category = "work"; place = "am Computer"; action = "öffnen"; object = "die Datei" },
        @{ title = "Ein Rezept holen"; category = "health"; place = "in der Praxis"; action = "holen"; object = "das Rezept" },
        @{ title = "Eine Antwort üben"; category = "education"; place = "im Kurs"; action = "üben"; object = "die Antwort" },
        @{ title = "Karten bezahlen"; category = "shopping"; place = "im Laden"; action = "bezahlen"; object = "die Karte" },
        @{ title = "Kartoffeln kochen"; category = "food"; place = "in der Küche"; action = "kochen"; object = "die Kartoffel" },
        @{ title = "Das Regal aufräumen"; category = "housing"; place = "zu Hause"; action = "aufräumen"; object = "das Regal" },
        @{ title = "Neben dem Fahrer sitzen"; category = "travel"; place = "im Auto"; action = "sitzen"; object = "der Fahrer" },
        @{ title = "Die Adresse schreiben"; category = "everyday-life"; place = "im Büro"; action = "schreiben"; object = "die Adresse" },
        @{ title = "Die Chefin fragt"; category = "work"; place = "bei der Arbeit"; action = "antworten"; object = "die Chefin" },
        @{ title = "Ein Wort markieren"; category = "education"; place = "im Heft"; action = "markieren"; object = "das Wort" },
        @{ title = "Eine Cola trinken"; category = "food"; place = "im Restaurant"; action = "trinken"; object = "die Cola" },
        @{ title = "Birnen kaufen"; category = "shopping"; place = "auf dem Markt"; action = "kaufen"; object = "die Birne" },
        @{ title = "Der Bildschirm ist dunkel"; category = "work"; place = "im Büro"; action = "fragen"; object = "der Bildschirm" },
        @{ title = "Nebel am Morgen"; category = "everyday-life"; place = "draußen"; action = "gehen"; object = "der Nebel" },
        @{ title = "Den Cousin besuchen"; category = "family"; place = "bei der Familie"; action = "besuchen"; object = "der Cousin" },
        @{ title = "Fisch mit Reis"; category = "food"; place = "zu Hause"; action = "essen"; object = "der Fisch" },
        @{ title = "Eine Postkarte schreiben"; category = "travel"; place = "im Urlaub"; action = "schreiben"; object = "die Postkarte" },
        @{ title = "Das Licht ausmachen"; category = "housing"; place = "zu Hause"; action = "ausmachen"; object = "das Licht" },
        @{ title = "Die Nase läuft"; category = "health"; place = "zu Hause"; action = "warten"; object = "die Nase" },
        @{ title = "Einen Dialog üben"; category = "education"; place = "im Kurs"; action = "sprechen"; object = "der Dialog" },
        @{ title = "Einen Umschlag kaufen"; category = "everyday-life"; place = "bei der Post"; action = "kaufen"; object = "der Umschlag" },
        @{ title = "Socken suchen"; category = "shopping"; place = "im Geschäft"; action = "suchen"; object = "die Socke" },
        @{ title = "Ein Telefonat bei der Arbeit"; category = "work"; place = "im Büro"; action = "telefonieren"; object = "das Telefon" },
        @{ title = "Eine ruhige Stunde"; category = "everyday-life"; place = "zu Hause"; action = "lesen"; object = "die Stunde" },
        @{ title = "Mit dem Sohn spielen"; category = "family"; place = "im Park"; action = "spielen"; object = "der Sohn" },
        @{ title = "Gurken schneiden"; category = "food"; place = "in der Küche"; action = "schneiden"; object = "die Gurke" },
        @{ title = "Die Station suchen"; category = "travel"; place = "in der Stadt"; action = "suchen"; object = "die Station" },
        @{ title = "Das Bad lüften"; category = "housing"; place = "zu Hause"; action = "lüften"; object = "das Bad" },
        @{ title = "Warm anziehen"; category = "health"; place = "draußen"; action = "anziehen"; object = "die Jacke" },
        @{ title = "Den Termin notieren"; category = "work"; place = "im Kalender"; action = "notieren"; object = "der Termin" },
        @{ title = "Eine Regel verstehen"; category = "education"; place = "im Kurs"; action = "verstehen"; object = "die Regel" }
    )
}
elseif ($TopicSet -ne "first") {
    throw "Unsupported TopicSet '$TopicSet'. Use 'first', 'second', 'third', 'fourth', or 'fifth'."
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
