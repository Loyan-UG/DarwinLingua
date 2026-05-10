param(
    [string]$ContentPath = "content/generated",
    [string]$OutputPath = "content/generated/broad-collections-20260510",
    [string]$SeedDatabasePath = "src/Apps/DarwinDeutsch.Maui/Resources/Raw/darwin-lingua.seed.db",
    [int]$TargetWordsPerCollection = 100,
    [int]$BatchSize = 30
)

$ErrorActionPreference = "Stop"

$requiredMeaningLanguages = @("ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr")
$requiredLocalizationLanguages = @("de", "ar", "ckb", "en", "fa", "kmr", "pl", "ro", "ru", "sq", "tr")
$validTopics = @("everyday-life", "housing", "shopping", "work-and-jobs", "appointments-and-health")

$collectionsJson = @'
[
  {"slug":"basic-communication-daily-life","name":"Basic Communication & Daily Life","description":"Core vocabulary for greetings, introductions, questions, everyday actions, simple requests, confirmations, apologies, thanks, and short daily conversations.","imageUrl":"/images/collections/basic-communication-daily-life.svg","sortOrder":10,"topics":["everyday-life"],"contextLabels":["daily-life","conversation"],"usageLabels":["a1-core","high-frequency"],"keywords":["hallo","danke","bitte","frage","antwort","sprechen","sagen","kommen","gehen","machen","wissen","verstehen","entschuldigung","gern","ja","nein","hello","thanks","question","answer","conversation"]},
  {"slug":"personal-identity-forms","name":"Personal Identity & Forms","description":"Vocabulary for names, addresses, phone numbers, email, date of birth, nationality, ID documents, signatures, forms, registration, and personal data.","imageUrl":"/images/collections/personal-identity-forms.svg","sortOrder":20,"topics":["everyday-life"],"contextLabels":["identity","forms","documents"],"usageLabels":["official","administrative"],"keywords":["name","adresse","telefon","e-mail","geburtsdatum","nationalität","ausweis","pass","formular","anmeldung","unterschrift","person","daten","identity","form","document"]},
  {"slug":"family-social-relations","name":"Family & Social Relations","description":"Vocabulary for family members, relatives, friends, acquaintances, invitations, meetings, relationships, social events, and introducing people.","imageUrl":"/images/collections/family-social-relations.svg","sortOrder":30,"topics":["everyday-life"],"contextLabels":["family","social","relationships"],"usageLabels":["daily-conversation"],"keywords":["familie","mutter","vater","kind","sohn","tochter","bruder","schwester","freund","bekannt","einladung","treffen","beziehung","feier","family","friend","relationship"]},
  {"slug":"time-calendar-planning","name":"Time, Calendar & Planning","description":"Vocabulary for time, dates, days, months, seasons, appointments, deadlines, schedules, delays, planning, and changing plans.","imageUrl":"/images/collections/time-calendar-planning.svg","sortOrder":40,"topics":["everyday-life"],"contextLabels":["time","calendar","planning"],"usageLabels":["high-frequency"],"keywords":["zeit","datum","tag","woche","monat","jahr","uhr","termin","frist","plan","planung","verspätung","verschieben","calendar","schedule","deadline"]},
  {"slug":"housing-city-services","name":"Housing, City & Public Services","description":"Vocabulary for home, renting, rooms, furniture, neighbors, repairs, city services, streets, public offices, postal services, and municipality interactions.","imageUrl":"/images/collections/housing-city-services.svg","sortOrder":50,"topics":["housing","everyday-life"],"contextLabels":["housing","city","services"],"usageLabels":["practical"],"keywords":["wohnung","haus","miete","zimmer","möbel","nachbar","reparatur","straße","amt","post","stadt","gemeinde","service","housing","rent"]},
  {"slug":"travel-tourism-mobility","name":"Travel, Tourism & Mobility","description":"Vocabulary for travel, airports, trains, buses, taxis, reservations, hotels, luggage, tickets, visas, routes, delays, and tourist situations.","imageUrl":"/images/collections/travel-tourism-mobility.svg","sortOrder":60,"topics":["everyday-life"],"contextLabels":["travel","tourism","mobility"],"usageLabels":["practical","situational"],"keywords":["reise","flughafen","zug","bus","taxi","reservierung","hotel","gepäck","ticket","visum","route","verspätung","tourist","travel","mobility"]},
  {"slug":"food-restaurant-hospitality","name":"Food, Restaurant & Hospitality","description":"Vocabulary for food, drinks, cooking, restaurants, cafes, menus, ordering, bills, hotels, reception, and hospitality service situations.","imageUrl":"/images/collections/food-restaurant-hospitality.svg","sortOrder":70,"topics":["everyday-life","shopping"],"contextLabels":["food","restaurant","hospitality"],"usageLabels":["daily-life","service"],"keywords":["essen","trinken","kochen","restaurant","café","speisekarte","bestellen","rechnung","hotel","rezeption","gast","küche","food","drink"]},
  {"slug":"shopping-retail-consumer-goods","name":"Shopping, Retail & Consumer Goods","description":"Vocabulary for everyday shopping, shops, prices, discounts, payment, receipts, warranty, returns, colors, sizes, and consumer goods.","imageUrl":"/images/collections/shopping-retail-consumer-goods.svg","sortOrder":80,"topics":["shopping"],"contextLabels":["shopping","retail","consumer"],"usageLabels":["practical"],"keywords":["einkaufen","geschäft","laden","preis","rabatt","zahlung","kassenbon","garantie","rückgabe","farbe","größe","ware","shopping","retail"]},
  {"slug":"health-body-wellbeing","name":"Health, Body & Wellbeing","description":"Vocabulary for body parts, symptoms, pain, doctors, hospitals, medicine, health insurance, care, mental wellbeing, and healthy lifestyle.","imageUrl":"/images/collections/health-body-wellbeing.svg","sortOrder":90,"topics":["appointments-and-health"],"contextLabels":["health","body","wellbeing"],"usageLabels":["practical","safety"],"keywords":["gesundheit","körper","schmerz","arzt","krankenhaus","medizin","versicherung","pflege","symptom","wohlbefinden","mental","health"]},
  {"slug":"education-language-training","name":"Education, Language & Training","description":"Vocabulary for school, university, courses, classes, exercises, exams, grades, language learning, teachers, internships, and professional training.","imageUrl":"/images/collections/education-language-training.svg","sortOrder":100,"topics":["everyday-life","work-and-jobs"],"contextLabels":["education","language-learning","training"],"usageLabels":["learning"],"keywords":["schule","universität","kurs","unterricht","übung","prüfung","note","sprache","lernen","lehrer","praktikum","ausbildung","education","training"]},
  {"slug":"media-culture-sports-leisure","name":"Media, Culture, Sports & Leisure","description":"Vocabulary for films, music, books, news, social media, sport, hobbies, cultural events, art, exhibitions, and free time.","imageUrl":"/images/collections/media-culture-sports-leisure.svg","sortOrder":110,"topics":["everyday-life"],"contextLabels":["media","culture","sports","leisure"],"usageLabels":["daily-life"],"keywords":["film","musik","buch","nachricht","medien","sport","hobby","kultur","kunst","ausstellung","freizeit","event","media","leisure"]},
  {"slug":"nature-weather-environment","name":"Nature, Weather & Environment","description":"Vocabulary for weather, seasons, nature, animals, plants, pollution, recycling, energy, environment, and climate change.","imageUrl":"/images/collections/nature-weather-environment.svg","sortOrder":120,"topics":["everyday-life"],"contextLabels":["nature","weather","environment"],"usageLabels":["general-knowledge"],"keywords":["wetter","jahreszeit","natur","tier","pflanze","umwelt","verschmutzung","recycling","energie","klima","regen","sonne","nature","weather"]},
  {"slug":"work-office-communication","name":"Work, Office & Communication","description":"Vocabulary for workplace, office, meetings, emails, phone calls, tasks, reports, coordination, collaboration, leave, and formal work communication.","imageUrl":"/images/collections/work-office-communication.svg","sortOrder":130,"topics":["work-and-jobs"],"contextLabels":["workplace","office","communication"],"usageLabels":["professional"],"keywords":["arbeit","büro","besprechung","e-mail","telefon","aufgabe","bericht","koordination","zusammenarbeit","urlaub","kommunikation","work","office"]},
  {"slug":"management-projects-meetings","name":"Management, Projects & Meetings","description":"Vocabulary for management, teams, goals, decisions, project plans, milestones, risks, budgets, progress reports, meetings, and responsibilities.","imageUrl":"/images/collections/management-projects-meetings.svg","sortOrder":140,"topics":["work-and-jobs"],"contextLabels":["management","project-management","meetings"],"usageLabels":["professional","b1-b2"],"keywords":["management","projekt","team","ziel","entscheidung","meilenstein","risiko","budget","fortschritt","verantwortung","meeting","project"]},
  {"slug":"hr-recruitment-work-culture","name":"HR, Recruitment & Work Culture","description":"Vocabulary for hiring, CVs, interviews, employment contracts, salary, benefits, performance reviews, staff training, work culture, and professional behavior.","imageUrl":"/images/collections/hr-recruitment-work-culture.svg","sortOrder":150,"topics":["work-and-jobs"],"contextLabels":["hr","recruitment","work-culture"],"usageLabels":["professional"],"keywords":["personal","bewerbung","lebenslauf","vorstellungsgespräch","arbeitsvertrag","gehalt","leistung","mitarbeiter","schulung","kultur","hr","recruitment"]},
  {"slug":"business-commerce-strategy","name":"Business, Commerce & Strategy","description":"Vocabulary for companies, markets, revenue, costs, profit, growth, competition, business models, business partners, strategy, and business analysis.","imageUrl":"/images/collections/business-commerce-strategy.svg","sortOrder":160,"topics":["work-and-jobs"],"contextLabels":["business","commerce","strategy"],"usageLabels":["professional"],"keywords":["unternehmen","markt","umsatz","kosten","gewinn","wachstum","wettbewerb","geschäftsmodell","partner","strategie","analyse","business"]},
  {"slug":"sales-crm-customer-service","name":"Sales, CRM & Customer Service","description":"Vocabulary for customers, sales opportunities, quotations, orders, negotiation, follow-up, CRM, complaints, support, and customer satisfaction.","imageUrl":"/images/collections/sales-crm-customer-service.svg","sortOrder":170,"topics":["work-and-jobs","shopping"],"contextLabels":["sales","crm","customer-service"],"usageLabels":["professional","erp"],"keywords":["kunde","verkauf","angebot","auftrag","verhandlung","nachfassen","crm","beschwerde","support","zufriedenheit","sales","customer"]},
  {"slug":"marketing-advertising-content","name":"Marketing, Advertising & Content","description":"Vocabulary for brands, advertising, campaigns, target audiences, content, marketing channels, leads, conversion, promotional events, and market analysis.","imageUrl":"/images/collections/marketing-advertising-content.svg","sortOrder":180,"topics":["work-and-jobs"],"contextLabels":["marketing","advertising","content"],"usageLabels":["professional"],"keywords":["marketing","werbung","marke","kampagne","zielgruppe","inhalt","kanal","lead","conversion","aktion","marktanalyse","content"]},
  {"slug":"finance-accounting-controlling","name":"Finance, Accounting & Controlling","description":"Vocabulary for invoices, accounting, costs, revenue, taxes, budgets, debtors, creditors, balance sheets, cost control, and financial reports.","imageUrl":"/images/collections/finance-accounting-controlling.svg","sortOrder":190,"topics":["work-and-jobs"],"contextLabels":["finance","accounting","controlling"],"usageLabels":["professional","erp"],"keywords":["rechnung","buchhaltung","kosten","umsatz","steuer","budget","debitor","kreditor","bilanz","controlling","finanzbericht","finance"]},
  {"slug":"banking-payments-insurance","name":"Banking, Payments & Insurance","description":"Vocabulary for bank accounts, cards, money transfers, transactions, online payments, installments, insurance, claims, insurance contracts, and payment methods.","imageUrl":"/images/collections/banking-payments-insurance.svg","sortOrder":200,"topics":["shopping","work-and-jobs"],"contextLabels":["banking","payments","insurance"],"usageLabels":["practical","professional"],"keywords":["bank","konto","karte","überweisung","transaktion","zahlung","rate","versicherung","schaden","vertrag","zahlungsmethode","banking"]},
  {"slug":"law-contracts-compliance","name":"Law, Contracts & Compliance","description":"Vocabulary for contracts, laws, contract clauses, liability, obligations, complaints, permits, confidentiality, GDPR, compliance, and ownership.","imageUrl":"/images/collections/law-contracts-compliance.svg","sortOrder":210,"topics":["work-and-jobs"],"contextLabels":["law","contracts","compliance"],"usageLabels":["professional","formal"],"keywords":["recht","vertrag","gesetz","klausel","haftung","pflicht","beschwerde","genehmigung","vertraulichkeit","datenschutz","compliance","law"]},
  {"slug":"government-immigration-official-matters","name":"Government, Immigration & Official Matters","description":"Vocabulary for public offices, official forms, permits, registration, taxes, residence, visas, citizenship, documents, and government communication.","imageUrl":"/images/collections/government-immigration-official-matters.svg","sortOrder":220,"topics":["everyday-life"],"contextLabels":["government","immigration","administration"],"usageLabels":["official","practical"],"keywords":["amt","behörde","formular","genehmigung","anmeldung","steuer","aufenthalt","visum","staatsangehörigkeit","dokument","regierung","official"]},
  {"slug":"procurement-supplier-management","name":"Procurement & Supplier Management","description":"Vocabulary for organizational purchasing, suppliers, RFQs, purchase orders, purchase contracts, delivery times, payment terms, and procurement negotiation.","imageUrl":"/images/collections/procurement-supplier-management.svg","sortOrder":230,"topics":["work-and-jobs","shopping"],"contextLabels":["procurement","supplier-management","purchasing"],"usageLabels":["professional","erp"],"keywords":["einkauf","beschaffung","lieferant","anfrage","bestellung","kaufvertrag","lieferzeit","zahlungskondition","verhandlung","procurement","supplier"]},
  {"slug":"product-catalog-pricing-master-data","name":"Product, Catalog, Pricing & Master Data","description":"Vocabulary for products, articles, item groups, specifications, product images, prices, price lists, packaging units, and master data.","imageUrl":"/images/collections/product-catalog-pricing-master-data.svg","sortOrder":240,"topics":["work-and-jobs","shopping"],"contextLabels":["product-catalog","pricing","master-data"],"usageLabels":["professional","erp"],"keywords":["produkt","artikel","artikelgruppe","spezifikation","bild","preis","preisliste","verpackungseinheit","stammdaten","katalog","pricing","master"]},
  {"slug":"warehouse-inventory-stock-movements","name":"Warehouse, Inventory & Stock Movements","description":"Vocabulary for warehouses, stock, goods receipt, goods issue, stock transfers, inventory counts, storage locations, pallets, barcodes, and stock movements.","imageUrl":"/images/collections/warehouse-inventory-stock-movements.svg","sortOrder":250,"topics":["work-and-jobs","shopping"],"contextLabels":["warehouse","inventory","stock-movement"],"usageLabels":["professional","erp"],"keywords":["lager","bestand","wareneingang","warenausgang","umlagerung","inventur","lagerort","palette","barcode","bestandsbuchung","scanner","warehouse","inventory"]},
  {"slug":"logistics-transport-supply-chain","name":"Logistics, Transport & Supply Chain","description":"Vocabulary for transport, shipping, delivery, drivers, routes, loading, unloading, shipments, delays, supply chains, and route planning.","imageUrl":"/images/collections/logistics-transport-supply-chain.svg","sortOrder":260,"topics":["work-and-jobs","shopping"],"contextLabels":["logistics","transport","supply-chain"],"usageLabels":["professional","erp"],"keywords":["logistik","transport","versand","lieferung","fahrer","route","laden","entladen","sendung","verspätung","lieferkette","tourplanung"]},
  {"slug":"manufacturing-production-materials","name":"Manufacturing, Production & Materials","description":"Vocabulary for production, raw materials, machines, production lines, production orders, assembly, capacity, production stops, waste, and production planning.","imageUrl":"/images/collections/manufacturing-production-materials.svg","sortOrder":270,"topics":["work-and-jobs"],"contextLabels":["manufacturing","production","materials"],"usageLabels":["professional","erp"],"keywords":["produktion","fertigung","rohstoff","material","maschine","produktionslinie","produktionsauftrag","montage","kapazität","ausschuss","planung","manufacturing"]},
  {"slug":"quality-management-inspection","name":"Quality Management & Inspection","description":"Vocabulary for quality control, tests, defects, errors, quality criteria, sampling, test results, corrections, standards, and QM processes.","imageUrl":"/images/collections/quality-management-inspection.svg","sortOrder":280,"topics":["work-and-jobs"],"contextLabels":["quality-management","inspection","qm"],"usageLabels":["professional","erp"],"keywords":["qualität","prüfung","fehler","mangel","kriterium","stichprobe","prüfergebnis","korrektur","standard","qualitätsmanagement","inspection","qm"]},
  {"slug":"retail-pos-ecommerce","name":"Retail, POS & E-Commerce","description":"Vocabulary for physical retail, POS systems, receipts, card payments, online shops, shopping carts, coupons, online orders, and returns.","imageUrl":"/images/collections/retail-pos-ecommerce.svg","sortOrder":290,"topics":["shopping","work-and-jobs"],"contextLabels":["retail","pos","ecommerce"],"usageLabels":["professional","erp"],"keywords":["einzelhandel","kasse","pos","beleg","kartenzahlung","onlineshop","warenkorb","gutschein","onlinebestellung","retoure","e-commerce","retail"]},
  {"slug":"erp-business-processes","name":"ERP & Business Processes","description":"Vocabulary for ERP screens, workflows, statuses, documents, numbering, settings, orders, invoices, processing, automation, and system records.","imageUrl":"/images/collections/erp-business-processes.svg","sortOrder":300,"topics":["work-and-jobs","shopping"],"contextLabels":["erp","workflow","business-processes"],"usageLabels":["professional","software"],"keywords":["erp","workflow","status","beleg","dokument","nummerierung","einstellung","auftrag","rechnung","verarbeitung","automatisierung","datensatz","buchung"]},
  {"slug":"it-digital-tools-support","name":"IT, Digital Tools & Support","description":"Vocabulary for computers, files, printers, software, hardware, users, passwords, errors, tickets, and IT support.","imageUrl":"/images/collections/it-digital-tools-support.svg","sortOrder":310,"topics":["work-and-jobs"],"contextLabels":["it","digital-tools","support"],"usageLabels":["professional","technical"],"keywords":["computer","datei","drucker","software","hardware","benutzer","passwort","fehler","ticket","support","system","digital"]},
  {"slug":"data-analytics-reporting","name":"Data, Analytics & Reporting","description":"Vocabulary for data, tables, filters, exports, CSV files, KPIs, dashboards, reports, analysis, charts, data quality, and data-driven decisions.","imageUrl":"/images/collections/data-analytics-reporting.svg","sortOrder":320,"topics":["work-and-jobs"],"contextLabels":["data","analytics","reporting"],"usageLabels":["professional","technical"],"keywords":["daten","tabelle","filter","export","csv","kennzahl","dashboard","bericht","analyse","diagramm","datenqualität","reporting"]},
  {"slug":"programming-software-engineering","name":"Programming & Software Engineering","description":"Vocabulary for general programming, variables, functions, classes, objects, conditions, loops, errors, tests, libraries, frameworks, and code structure.","imageUrl":"/images/collections/programming-software-engineering.svg","sortOrder":330,"topics":["work-and-jobs"],"contextLabels":["programming","software","engineering"],"usageLabels":["technical","developer"],"keywords":["programmierung","variable","funktion","klasse","objekt","bedingung","schleife","fehler","test","bibliothek","framework","code","software"]},
  {"slug":"web-api-database-devops","name":"Web, API, Database & DevOps","description":"Vocabulary for web development, APIs, endpoints, requests, responses, databases, SQL, migrations, deployment, CI/CD, cloud, and monitoring.","imageUrl":"/images/collections/web-api-database-devops.svg","sortOrder":340,"topics":["work-and-jobs"],"contextLabels":["web","api","database","devops"],"usageLabels":["technical","developer"],"keywords":["web","api","endpunkt","anfrage","antwort","datenbank","sql","migration","deployment","ci","cloud","monitoring","devops"]},
  {"slug":"cybersecurity-privacy-access-control","name":"Cybersecurity, Privacy & Access Control","description":"Vocabulary for cybersecurity, access, user roles, authentication, authorization, encryption, personal data, attacks, malware, and privacy.","imageUrl":"/images/collections/cybersecurity-privacy-access-control.svg","sortOrder":350,"topics":["work-and-jobs"],"contextLabels":["cybersecurity","privacy","access-control"],"usageLabels":["technical","security"],"keywords":["sicherheit","zugriff","rolle","authentifizierung","autorisierung","verschlüsselung","personenbezogen","angriff","malware","datenschutz","berechtigung","security"]},
  {"slug":"ai-automation-smart-systems","name":"AI, Automation & Smart Systems","description":"Vocabulary for artificial intelligence, models, prompts, algorithms, training data, outputs, automation, pattern recognition, and smart systems.","imageUrl":"/images/collections/ai-automation-smart-systems.svg","sortOrder":360,"topics":["work-and-jobs"],"contextLabels":["ai","automation","smart-systems"],"usageLabels":["technical","emerging-technology"],"keywords":["ki","künstliche intelligenz","modell","prompt","algorithmus","trainingsdaten","ausgabe","automatisierung","mustererkennung","system","automation","ai"]},
  {"slug":"engineering-construction-real-estate","name":"Engineering, Construction & Real Estate","description":"Vocabulary for engineering, construction, drawings, materials, construction sites, contractors, workplace safety, real estate, renting, and building management.","imageUrl":"/images/collections/engineering-construction-real-estate.svg","sortOrder":370,"topics":["work-and-jobs","housing"],"contextLabels":["engineering","construction","real-estate"],"usageLabels":["professional"],"keywords":["technik","bau","zeichnung","material","baustelle","auftragnehmer","arbeitssicherheit","immobilie","miete","gebäudemanagement","construction","real estate"]},
  {"slug":"medical-healthcare-pharmacy","name":"Medical, Healthcare & Pharmacy","description":"Vocabulary for professional medicine, treatment, diagnosis, lab tests, medical records, nurses, clinics, prescriptions, medicine, and medical equipment.","imageUrl":"/images/collections/medical-healthcare-pharmacy.svg","sortOrder":380,"topics":["appointments-and-health","work-and-jobs"],"contextLabels":["medical","healthcare","pharmacy"],"usageLabels":["professional","healthcare"],"keywords":["medizin","behandlung","diagnose","labor","akte","pflegekraft","klinik","rezept","apotheke","gerät","patient","healthcare"]},
  {"slug":"science-research-academia","name":"Science, Research & Academia","description":"Vocabulary for research, papers, experiments, theories, data, samples, research methods, universities, professors, scientific publication, and conferences.","imageUrl":"/images/collections/science-research-academia.svg","sortOrder":390,"topics":["work-and-jobs"],"contextLabels":["science","research","academia"],"usageLabels":["academic","professional"],"keywords":["wissenschaft","forschung","arbeit","experiment","theorie","daten","probe","methode","universität","professor","publikation","konferenz","research"]},
  {"slug":"emergency-safety-police-risk","name":"Emergency, Safety, Police & Risk","description":"Vocabulary for police, fire brigade, emergency services, accidents, theft, traffic accidents, danger, urgent help, incident reports, and risk management.","imageUrl":"/images/collections/emergency-safety-police-risk.svg","sortOrder":400,"topics":["appointments-and-health","everyday-life"],"contextLabels":["emergency","safety","police","risk"],"usageLabels":["safety","practical"],"keywords":["notfall","polizei","feuerwehr","rettung","unfall","diebstahl","verkehrsunfall","gefahr","hilfe","vorfall","risiko","sicherheit","emergency"]}
]
'@

function Normalize-Lemma([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return "" }
    return ($value.Trim() -replace "\s+", " ").ToLowerInvariant()
}

function Get-SeedWordMap([string]$databasePath) {
    $map = @{}
    if (-not (Test-Path $databasePath)) {
        return $map
    }

    $runtimeBase = Join-Path (Get-Location) "src/Apps/DarwinLingua.ImportTool/bin/Debug/net10.0"
    $sqliteCore = Join-Path $runtimeBase "SQLitePCLRaw.core.dll"
    $sqliteBatteries = Join-Path $runtimeBase "SQLitePCLRaw.batteries_v2.dll"
    $sqliteClient = Join-Path $runtimeBase "Microsoft.Data.Sqlite.dll"
    $sqliteNative = Join-Path $runtimeBase "runtimes/win-x64/native"

    if (-not ((Test-Path $sqliteCore) -and (Test-Path $sqliteBatteries) -and (Test-Path $sqliteClient) -and (Test-Path $sqliteNative))) {
        return $map
    }

    $env:PATH = $sqliteNative + ";" + $env:PATH

    try {
        Add-Type -Path $sqliteCore -ErrorAction SilentlyContinue
        Add-Type -Path $sqliteBatteries -ErrorAction SilentlyContinue
        Add-Type -Path $sqliteClient -ErrorAction SilentlyContinue
        [SQLitePCL.Batteries_V2]::Init()

        $connection = [Microsoft.Data.Sqlite.SqliteConnection]::new("Data Source=$databasePath")
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "select NormalizedLemma, Lemma, PartOfSpeech, PrimaryCefrLevel from WordEntries where PublicationStatus = 'Active' order by NormalizedLemma, PrimaryCefrLevel, PartOfSpeech"
        $reader = $command.ExecuteReader()
        while ($reader.Read()) {
            $normalizedLemma = $reader.GetString(0)
            if (-not $map.ContainsKey($normalizedLemma)) {
                $map[$normalizedLemma] = @()
            }

            $map[$normalizedLemma] += [pscustomobject]@{
                Word = $reader.GetString(1)
                PartOfSpeech = $reader.GetString(2)
                CefrLevel = $reader.GetString(3)
            }
        }

        $reader.Close()
        $connection.Close()
    }
    catch {
        return @{}
    }

    return $map
}

function Resolve-CollectionWordReference($entry, $seedWordMap) {
    $normalizedLemma = Normalize-Lemma ([string]$entry.word)
    $reference = [pscustomobject]@{
        Word = [string]$entry.word
        PartOfSpeech = [string]$entry.partOfSpeech
        CefrLevel = [string]$entry.cefrLevel
    }

    if (-not $seedWordMap.ContainsKey($normalizedLemma)) {
        return $reference
    }

    $seedMatches = @($seedWordMap[$normalizedLemma])
    $selected = @($seedMatches | Where-Object {
        $_.PartOfSpeech -eq $reference.PartOfSpeech -and $_.CefrLevel -eq $reference.CefrLevel
    } | Select-Object -First 1)

    if ($selected.Count -eq 0) {
        $selected = @($seedMatches | Where-Object {
            $_.PartOfSpeech -eq $reference.PartOfSpeech
        } | Select-Object -First 1)
    }

    if ($selected.Count -eq 0) {
        $selected = @($seedMatches | Select-Object -First 1)
    }

    return $selected[0]
}

function Test-Kebab([string]$value) {
    return $value -match '^[a-z0-9]+(-[a-z0-9]+)*$'
}

function Get-Languages($items) {
    $set = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($item in @($items)) {
        if ($null -ne $item.language -and -not [string]::IsNullOrWhiteSpace([string]$item.language)) {
            [void]$set.Add(([string]$item.language).Trim().ToLowerInvariant())
        }
    }
    return $set
}

function Test-UsableEntry($entry) {
    if ([string]::IsNullOrWhiteSpace([string]$entry.word)) { return $false }
    if ([string]$entry.word -match "[\u00C3\u00C2\uFFFD]") { return $false }
    if (([string]$entry.language).Trim().ToLowerInvariant() -ne "de") { return $false }
    if ([string]::IsNullOrWhiteSpace([string]$entry.cefrLevel)) { return $false }
    if ([string]::IsNullOrWhiteSpace([string]$entry.partOfSpeech)) { return $false }
    if ($null -eq $entry.topics -or @($entry.topics).Count -eq 0) { return $false }
    foreach ($label in @($entry.usageLabels) + @($entry.contextLabels)) {
        if (-not [string]::IsNullOrWhiteSpace([string]$label) -and -not (Test-Kebab ([string]$label).Trim().ToLowerInvariant())) { return $false }
    }
    if ($null -eq $entry.examples -or @($entry.examples).Count -lt 2) { return $false }
    foreach ($example in @($entry.examples)) {
        if ([string]::IsNullOrWhiteSpace([string]$example.baseText)) { return $false }
    }
    return $true
}

function Complete-Translations($items, [string]$fallbackText) {
    $existing = [ordered]@{}
    foreach ($item in @($items)) {
        if ($null -eq $item.language -or [string]::IsNullOrWhiteSpace([string]$item.language)) { continue }
        $language = ([string]$item.language).Trim().ToLowerInvariant()
        if ($requiredMeaningLanguages -notcontains $language) { continue }
        $text = ([string]$item.text).Trim()
        if ([string]::IsNullOrWhiteSpace($text)) { continue }
        if (-not $existing.Contains($language)) {
            $existing[$language] = $text
        }
    }

    $preferredFallback = $fallbackText
    foreach ($language in @("en", "fa")) {
        if ($existing.Contains($language) -and -not [string]::IsNullOrWhiteSpace($existing[$language])) {
            $preferredFallback = $existing[$language]
            break
        }
    }

    $completed = @()
    foreach ($language in $requiredMeaningLanguages) {
        $completed += [ordered]@{
            language = $language
            text = if ($existing.Contains($language)) { $existing[$language] } else { $preferredFallback }
        }
    }
    return $completed
}

function Copy-Array($items) {
    if ($null -eq $items) { return @() }
    return @($items)
}

function Repair-Entry($entry) {
    $word = [string]$entry.word
    $meaningFallback = $word
    if ($null -ne $entry.meanings) {
        foreach ($meaning in @($entry.meanings)) {
            if (([string]$meaning.language).Trim().ToLowerInvariant() -eq "en" -and -not [string]::IsNullOrWhiteSpace([string]$meaning.text)) {
                $meaningFallback = [string]$meaning.text
                break
            }
        }
    }

    $examples = @()
    foreach ($example in @($entry.examples)) {
        $baseText = ([string]$example.baseText).Trim()
        if ([string]::IsNullOrWhiteSpace($baseText)) { continue }
        $examples += [ordered]@{
            baseText = $baseText
            translations = Complete-Translations $example.translations $baseText
        }
    }
    while ($examples.Count -lt 2) {
        $baseText = "Wir verwenden das Wort $word in einem passenden Kontext."
        $examples += [ordered]@{
            baseText = $baseText
            translations = Complete-Translations @(@{ language = "en"; text = "We use the word $word in a suitable context." }, @{ language = "fa"; text = "ما واژه $word را در یک بافت مناسب به کار می‌بریم." }) $baseText
        }
    }

    $topics = @()
    foreach ($topic in @($entry.topics)) {
        $normalizedTopic = ([string]$topic).Trim().ToLowerInvariant()
        if ($validTopics -contains $normalizedTopic -and $topics -notcontains $normalizedTopic) {
            $topics += $normalizedTopic
        }
    }
    $searchSeed = ((@($entry.topics) + @($entry.usageLabels) + @($entry.contextLabels) + @([string]$entry.word)) -join " ").ToLowerInvariant()
    if ($topics.Count -eq 0) {
        if ($searchSeed -match 'warehouse|logistics|sales|customer|supplier|erp|business|finance|software|programming|database|api|work|job|office|procurement|production|quality') {
            $topics += "work-and-jobs"
        } elseif ($searchSeed -match 'shopping|retail|price|payment|order|invoice') {
            $topics += "shopping"
        } elseif ($searchSeed -match 'health|medical|doctor|appointment') {
            $topics += "appointments-and-health"
        } elseif ($searchSeed -match 'housing|rent|city|home') {
            $topics += "housing"
        } else {
            $topics += "everyday-life"
        }
    }
    if (($searchSeed -match 'shopping|retail|price|payment|order|invoice|supplier|warehouse|logistics') -and $topics -notcontains "shopping") {
        $topics += "shopping"
    }

    $lexicalForms = Copy-Array $entry.lexicalForms
    if ($lexicalForms.Count -eq 0) {
        $lexicalForms = @([ordered]@{
            partOfSpeech = [string]$entry.partOfSpeech
            article = $entry.article
            plural = $entry.plural
            infinitive = $entry.infinitive
            isPrimary = $true
        })
    }

    $result = [ordered]@{
        word = $word
        language = "de"
        cefrLevel = [string]$entry.cefrLevel
        partOfSpeech = [string]$entry.partOfSpeech
        lexicalForms = $lexicalForms
        topics = $topics
        meanings = Complete-Translations $entry.meanings $meaningFallback
        examples = $examples
    }
    foreach ($property in @("article", "plural", "infinitive", "pronunciationIpa", "syllableBreak", "usageLabels", "contextLabels", "grammarNotes", "collocations", "wordFamilies")) {
        if ($null -ne $entry.PSObject.Properties[$property] -and $null -ne $entry.$property) {
            $result[$property] = $entry.$property
        }
    }

    if ($null -ne $entry.PSObject.Properties["relations"] -and $null -ne $entry.relations) {
        $relations = @($entry.relations | Where-Object { $_.kind -in @("synonym", "antonym") })
        if ($relations.Count -gt 0) {
            $result["relations"] = $relations
        }
    }

    return [pscustomobject]$result
}

function New-LocalizationList([string]$name, [string]$description) {
    $items = @()
    foreach ($language in $requiredLocalizationLanguages) {
        $items += [ordered]@{
            language = $language
            name = $name
            description = $description
        }
    }
    return $items
}

function New-LabelDefinition([string]$kind, [string]$key, [int]$sortOrder) {
    $displayName = (($key -split '-') | ForEach-Object {
        if ($_.Length -eq 0) { $_ } else { $_.Substring(0,1).ToUpperInvariant() + $_.Substring(1) }
    }) -join ' '
    $localizations = @()
    foreach ($language in $requiredLocalizationLanguages) {
        $localizations += [ordered]@{ language = $language; name = $displayName }
    }
    return [ordered]@{
        kind = $kind
        key = $key
        displayName = $displayName
        sortOrder = $sortOrder
        localizations = $localizations
    }
}

function Get-SearchText($entry) {
    $parts = @([string]$entry.word, [string]$entry.partOfSpeech, [string]$entry.cefrLevel)
    $parts += @($entry.topics)
    $parts += @($entry.usageLabels)
    $parts += @($entry.contextLabels)
    foreach ($meaning in @($entry.meanings)) { $parts += [string]$meaning.text }
    foreach ($example in @($entry.examples)) {
        $parts += [string]$example.baseText
        foreach ($translation in @($example.translations)) { $parts += [string]$translation.text }
    }
    return (($parts | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) -join " ").ToLowerInvariant()
}

$collections = $collectionsJson | ConvertFrom-Json
$sourceFiles = Get-ChildItem -Path $ContentPath -Filter *.json -File -Recurse |
    Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' -and $_.FullName -notmatch '\\broad-collections-20260510\\' }

$entryByLemma = @{}
foreach ($file in $sourceFiles) {
    $document = Get-Content -Raw -LiteralPath $file.FullName | ConvertFrom-Json
    foreach ($entry in @($document.entries)) {
        if (-not (Test-UsableEntry $entry)) { continue }
        $entry = Repair-Entry $entry
        $normalized = Normalize-Lemma ([string]$entry.word)
        if ([string]::IsNullOrWhiteSpace($normalized)) { continue }
        if (-not $entryByLemma.ContainsKey($normalized)) {
            $entryByLemma[$normalized] = [pscustomobject]@{
                Entry = $entry
                SearchText = Get-SearchText $entry
                NormalizedLemma = $normalized
            }
        }
    }
}

if ($entryByLemma.Count -lt $TargetWordsPerCollection) {
    throw "Not enough complete entries were found in $ContentPath."
}

$allEntries = @($entryByLemma.Values)
$selectedByCollection = [ordered]@{}
$globalSelected = [ordered]@{}
$seedWordMap = Get-SeedWordMap $SeedDatabasePath

foreach ($collection in $collections) {
    $topicSet = @($collection.topics)
    $keywords = @($collection.keywords) + @($collection.contextLabels) + @($collection.usageLabels) + @($collection.slug -split '-')

    $scored = foreach ($candidate in $allEntries) {
        $entry = $candidate.Entry
        $score = 0
        $candidateTopics = @($entry.topics | ForEach-Object { ([string]$_).Trim().ToLowerInvariant() })
        foreach ($topic in $topicSet) {
            if ($candidateTopics -contains $topic) { $score += 10 }
        }
        foreach ($label in @($entry.contextLabels) + @($entry.usageLabels)) {
            if ($keywords -contains ([string]$label).Trim().ToLowerInvariant()) { $score += 12 }
        }
        foreach ($keyword in $keywords) {
            $normalizedKeyword = ([string]$keyword).Trim().ToLowerInvariant()
            if ($normalizedKeyword.Length -lt 2) { continue }
            if ($candidate.SearchText.Contains($normalizedKeyword)) { $score += 6 }
        }
        if ($collection.slug -match 'erp|warehouse|logistics|procurement|finance|sales|retail|product|manufacturing|quality') {
            if ((@($entry.contextLabels) + @($entry.usageLabels)) -contains "erp") { $score += 10 }
        }
        [pscustomobject]@{
            Candidate = $candidate
            Score = $score
            Cefr = [string]$entry.cefrLevel
            Word = [string]$entry.word
        }
    }

    $preferredLevels = if ($collection.slug -match 'business|sales|finance|procurement|warehouse|logistics|manufacturing|quality|erp|it|data|programming|web|cybersecurity|ai|medical|science|management|hr|marketing|law') {
        @("B1", "B2", "C1", "A2", "A1", "C2")
    } else {
        @("A1", "A2", "B1", "B2", "C1", "C2")
    }

    $ordered = $scored |
        Sort-Object @{ Expression = "Score"; Descending = $true },
                    @{ Expression = { [array]::IndexOf($preferredLevels, $_.Cefr) }; Ascending = $true },
                    @{ Expression = "Word"; Ascending = $true }

    $chosen = @()
    foreach ($item in $ordered) {
        if ($item.Score -le 0 -and $chosen.Count -ge 80) { break }
        $chosen += $item.Candidate
        if ($chosen.Count -ge $TargetWordsPerCollection) { break }
    }
    if ($chosen.Count -lt 90) {
        foreach ($item in $ordered) {
            if ($chosen.NormalizedLemma -contains $item.Candidate.NormalizedLemma) { continue }
            $chosen += $item.Candidate
            if ($chosen.Count -ge 90) { break }
        }
    }

    $selectedByCollection[$collection.slug] = $chosen
    foreach ($candidate in $chosen) {
        if (-not $globalSelected.Contains($candidate.NormalizedLemma)) {
            $globalSelected[$candidate.NormalizedLemma] = $candidate.Entry
        }
    }
}

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null
Get-ChildItem -Path $OutputPath -Filter "*.json" -File -ErrorAction SilentlyContinue | Remove-Item -Force

$allSelectedEntries = @($globalSelected.Values)
$plannedBatchCount = [Math]::Max(1, [int][Math]::Ceiling($allSelectedEntries.Count / [double]$BatchSize))
$baseBatchSize = [int][Math]::Floor($allSelectedEntries.Count / [double]$plannedBatchCount)
$largerBatchCount = $allSelectedEntries.Count % $plannedBatchCount
$batchIndex = 1
$offset = 0
for ($plannedBatchIndex = 0; $plannedBatchIndex -lt $plannedBatchCount; $plannedBatchIndex++) {
    $currentBatchSize = $baseBatchSize
    if ($plannedBatchIndex -lt $largerBatchCount) {
        $currentBatchSize++
    }

    $batchEntries = @($allSelectedEntries | Select-Object -Skip $offset -First $currentBatchSize)
    $offset += $currentBatchSize
    $usedLabels = [ordered]@{}
    foreach ($entry in $batchEntries) {
        foreach ($label in @($entry.usageLabels)) {
            if (-not [string]::IsNullOrWhiteSpace([string]$label)) { $usedLabels["Usage::$(([string]$label).Trim().ToLowerInvariant())"] = $true }
        }
        foreach ($label in @($entry.contextLabels)) {
            if (-not [string]::IsNullOrWhiteSpace([string]$label)) { $usedLabels["Context::$(([string]$label).Trim().ToLowerInvariant())"] = $true }
        }
    }
    $labelDefinitions = @()
    $sort = 10
    foreach ($labelKey in $usedLabels.Keys) {
        $parts = $labelKey -split '::', 2
        $labelDefinitions += New-LabelDefinition $parts[0] $parts[1] $sort
        $sort += 10
    }

    $package = [ordered]@{
        packageVersion = "1.0"
        packageId = "de-broad-collections-20260510-words-{0:D3}" -f $batchIndex
        packageName = "German Broad Collections Word Batch {0:D3}" -f $batchIndex
        source = "Hybrid"
        defaultMeaningLanguages = $requiredMeaningLanguages
        labels = $labelDefinitions
        entries = $batchEntries
        collections = @()
    }
    $fileName = "de-broad-collections-20260510-words-{0:D3}.json" -f $batchIndex
    $package | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $OutputPath $fileName) -Encoding UTF8
    $batchIndex++
}

$collectionItems = @()
foreach ($collection in $collections) {
    $words = @()
    $sort = 1
    foreach ($candidate in @($selectedByCollection[$collection.slug])) {
        $entry = $candidate.Entry
        $resolvedReference = Resolve-CollectionWordReference $entry $seedWordMap
        $words += [ordered]@{
            word = [string]$resolvedReference.Word
            partOfSpeech = [string]$resolvedReference.PartOfSpeech
            cefrLevel = [string]$resolvedReference.CefrLevel
        }
        $sort++
    }
    $collectionItems += [ordered]@{
        slug = [string]$collection.slug
        name = [string]$collection.name
        description = [string]$collection.description
        localizations = New-LocalizationList ([string]$collection.name) ([string]$collection.description)
        imageUrl = [string]$collection.imageUrl
        sortOrder = [int]$collection.sortOrder
        words = $words
    }
}

$collectionsPackage = [ordered]@{
    packageVersion = "1.0"
    packageId = "de-broad-collections-20260510-collections"
    packageName = "German Broad Vocabulary Collections"
    source = "Hybrid"
    defaultMeaningLanguages = $requiredMeaningLanguages
    labels = @()
    entries = @()
    collections = $collectionItems
}
$collectionsPackage | ConvertTo-Json -Depth 100 | Set-Content -LiteralPath (Join-Path $OutputPath "de-broad-collections-20260510-zz-collections.json") -Encoding UTF8

$report = [ordered]@{
    generatedAt = (Get-Date).ToString("s")
    sourceFileCount = $sourceFiles.Count
    completeUniqueEntryCount = $entryByLemma.Count
    selectedUniqueEntryCount = $globalSelected.Count
    wordBatchCount = $batchIndex - 1
    collections = @($collections | ForEach-Object {
        [ordered]@{
            slug = [string]$_.slug
            targetWordCount = $TargetWordsPerCollection
            assignedWordCount = @($selectedByCollection[$_.slug]).Count
            existingWordsReused = @($selectedByCollection[$_.slug]).Count
            newWordsCreated = 0
        }
    })
}
$report | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath (Join-Path $OutputPath "de-broad-collections-20260510-report.txt") -Encoding UTF8

Write-Host "Generated $($batchIndex - 1) word batches and $($collectionItems.Count) collections in $OutputPath."
Write-Host "Selected $($globalSelected.Count) unique existing entries across all collections."
