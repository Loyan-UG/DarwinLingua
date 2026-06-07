# A1 RoleplayScenario Title Candidates

Date: 2026-05-26

## Purpose

This file preserves the reviewed A1 standalone `RoleplayScenario` title backlog before full multilingual content generation. Use it to avoid duplicate titles, English-first metadata, ordinary filler tasks, and weak answer-choice design in future roleplay batches.

## Generation Rules

- Source `title`, `description`, and `learnerGoal` must be German-first.
- Add learner-language translations later for `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, and `sq`.
- Keep A1 scenarios short, deterministic, and practical.
- Do not generate AI chat, free-text evaluation, or unrestricted roleplay.
- Recommended learner turns should appear in the scripted turn sequence.
- Answer choices must include at least one correct deterministic choice for validation, but learner-facing UI should emphasize plausible weaker/not-recommended alternatives instead of repeating the correct answer.
- Incorrect choices must be plausible but insufficient: too short, missing context, too direct, missing politeness, or missing a follow-up question. Do not use irrelevant filler such as `Ich weiß nicht.`.
- Use only active topic keys, for example `everyday-life`, `social-and-relationships`, `shopping-and-services`, `appointments-and-health`, `public-services`, `transport-and-travel`, `education-and-training`, `customer-service`, `work-and-jobs`, and `housing`.

## Already Imported A1 Scenario

| Order | Slug | German Title | Category | Topic Keys | Notes |
| ---: | --- | --- | --- | --- | --- |
| 1 | `a1-sprachcafe-vorstellung` | Sich im Sprachcafe vorstellen | `conversation-cafe` | `social-and-relationships`, `everyday-life` | Already imported in `roleplays-a1-b2-pilot-v1`; do not duplicate. |

## Generated A1 Packages

| Package | Date | Orders Covered | Scenario Count | Database Status | Notes |
| --- | --- | --- | ---: | --- | --- |
| `roleplays-a1-core-01-v1.json` | 2026-05-26 | 2-5 | 4 | Imported into `darwinlingua_shared` | Covers course introduction, asking for a name, asking about language, and offering a seat. |
| `roleplays-a1-core-02-v1.json` | 2026-05-26 | 6-10 | 5 | Imported into `darwinlingua_shared` | Covers ordering in a cafe, bakery shopping, checkout payment, asking price, and asking for help in a shop. |
| `roleplays-a1-core-03-v1.json` | 2026-05-26 | 11-15 | 5 | Imported into `darwinlingua_shared` | Covers bus stops, train platforms, asking directions, greeting neighbors, and picking up a package from neighbors. |
| `roleplays-a1-core-04-v1.json` | 2026-05-26 | 16-20 | 5 | Imported into `darwinlingua_shared` | Covers doctor reception, waiting at a practice, pharmacy help, kindergarten notice, and asking for the school office. |
| `roleplays-a1-core-05-v1.json` | 2026-05-26 | 21-25 | 5 | Imported into `darwinlingua_shared` | Covers Buergeramt arrival, reception questions, phone appointment request/confirmation, and greeting a colleague in an office. |
| `roleplays-a1-core-06-v1.json` | 2026-05-26 | 26-31 | 6 | Imported into `darwinlingua_shared` | Covers workplace repetition, break small talk, neighbor introductions, supermarket product help, asking for a menu, and ordering a drink. |
| `roleplays-a1-core-07-v1.json` | 2026-05-26 | 32-40 | 9 | Imported into `darwinlingua_shared` | Covers restroom directions, slower speech at a language cafe, class help/apology, accepting/declining invitations, closing a conversation, submitting a form, and reception check-in. |

## Candidate Backlog

| Order | Suggested Slug | German Title | Category | Topic Keys | Interaction Mode | Register | Core Learner Task |
| ---: | --- | --- | --- | --- | --- | --- | --- |
| 2 | `a1-im-kurs-vorstellen` | Sich im Deutschkurs vorstellen | `classroom` | `education-and-training`, `social-and-relationships` | `classroom` | `neutral` | Say name, origin/city, and ask one simple question back. |
| 3 | `a1-nach-dem-namen-fragen` | Nach dem Namen fragen | `conversation-cafe` | `social-and-relationships`, `everyday-life` | `face-to-face` | `neutral` | Ask for a name and respond politely. |
| 4 | `a1-nach-der-sprache-fragen` | Nach der Sprache fragen | `conversation-cafe` | `social-and-relationships`, `everyday-life` | `face-to-face` | `neutral` | Ask which language someone speaks and answer simply. |
| 5 | `a1-einen-platz-anbieten` | Einen Platz anbieten | `conversation-cafe` | `social-and-relationships`, `everyday-life` | `face-to-face` | `neutral` | Offer a seat and respond to thanks. |
| 6 | `a1-im-cafe-etwas-bestellen` | Im Cafe etwas bestellen | `customer-service` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Order one drink politely and answer a short follow-up. |
| 7 | `a1-im-baeckerei-einkaufen` | In der Baeckerei einkaufen | `shopping-and-services` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Ask for one item and handle price/payment. |
| 8 | `a1-an-der-kasse-bezahlen` | An der Kasse bezahlen | `shopping-and-services` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Answer cash/card question and close politely. |
| 9 | `a1-nach-dem-preis-fragen` | Nach dem Preis fragen | `shopping-and-services` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Ask price and decide yes/no politely. |
| 10 | `a1-im-laden-um-hilfe-bitten` | Im Laden um Hilfe bitten | `shopping-and-services` | `shopping-and-services`, `customer-service` | `service-counter` | `formal` | Ask for help finding something. |
| 11 | `a1-im-bus-nach-der-haltestelle-fragen` | Im Bus nach der Haltestelle fragen | `transport` | `transport-and-travel`, `everyday-life` | `face-to-face` | `neutral` | Ask if this is the right stop and thank the person. |
| 12 | `a1-am-bahnhof-nach-dem-gleis-fragen` | Am Bahnhof nach dem Gleis fragen | `transport` | `transport-and-travel`, `everyday-life` | `service-counter` | `neutral` | Ask for a platform and confirm the answer. |
| 13 | `a1-nach-dem-weg-fragen` | Nach dem Weg fragen | `directions` | `transport-and-travel`, `everyday-life` | `face-to-face` | `neutral` | Ask for a simple direction and repeat the key point. |
| 14 | `a1-im-hausflur-nachbarn-gruessen` | Nachbarn im Hausflur gruessen | `housing` | `housing`, `social-and-relationships` | `face-to-face` | `neutral` | Greet a neighbor and exchange a short polite line. |
| 15 | `a1-paket-bei-nachbarn-abholen` | Ein Paket bei Nachbarn abholen | `housing` | `housing`, `everyday-life` | `face-to-face` | `neutral` | Ask for a package and thank politely. |
| 16 | `a1-beim-arzt-anmelden` | Sich beim Arzt anmelden | `appointments` | `appointments-and-health`, `everyday-life` | `doctor-office` | `formal` | Say name, appointment time, and answer a short question. |
| 17 | `a1-in-der-praxis-warten` | In der Praxis warten | `appointments` | `appointments-and-health`, `everyday-life` | `doctor-office` | `formal` | Ask whether to wait and confirm politely. |
| 18 | `a1-in-der-apotheke-fragen` | In der Apotheke fragen | `health` | `appointments-and-health`, `shopping-and-services` | `service-counter` | `formal` | Ask for simple help and follow staff guidance. |
| 19 | `a1-im-kindergarten-kurz-bescheid-sagen` | Im Kindergarten kurz Bescheid sagen | `school-kindergarten` | `education-and-training`, `everyday-life` | `school-kindergarten` | `formal` | Give a short simple notice and answer one follow-up. |
| 20 | `a1-in-der-schule-nach-dem-buero-fragen` | In der Schule nach dem Buero fragen | `school-kindergarten` | `education-and-training`, `public-services` | `school-kindergarten` | `formal` | Ask where the office is and confirm. |
| 21 | `a1-im-buergeramt-ankommen` | Im Buergeramt ankommen | `public-services` | `public-services`, `documents-and-administration` | `government-office` | `formal` | State name and appointment time at the counter. |
| 22 | `a1-am-empfang-nachfragen` | Am Empfang nachfragen | `public-services` | `public-services`, `documents-and-administration` | `service-counter` | `formal` | Ask where to go and respond to instructions. |
| 23 | `a1-telefonisch-nach-einem-termin-fragen` | Telefonisch nach einem Termin fragen | `appointments` | `appointments-and-health`, `everyday-life` | `phone` | `formal` | Ask for an appointment and provide a simple time preference. |
| 24 | `a1-einen-termin-bestaetigen` | Einen Termin bestaetigen | `appointments` | `appointments-and-health`, `documents-and-administration` | `phone` | `formal` | Confirm a day/time and repeat it. |
| 25 | `a1-im-buero-eine-kollegin-gruessen` | Eine Kollegin im Buero gruessen | `workplace` | `work-and-jobs`, `social-and-relationships` | `workplace` | `neutral` | Greet a colleague and answer a simple small-talk question. |
| 26 | `a1-im-buero-um-wiederholung-bitten` | Im Buero um Wiederholung bitten | `workplace` | `work-and-jobs`, `business-communication` | `workplace` | `neutral` | Ask someone to repeat slowly. |
| 27 | `a1-in-der-pause-smalltalk-machen` | In der Pause Smalltalk machen | `workplace` | `work-and-jobs`, `social-and-relationships` | `face-to-face` | `neutral` | Answer a simple personal question and ask one back. |
| 28 | `a1-im-haus-nach-dem-namen-fragen` | Im Haus nach dem Namen fragen | `housing` | `housing`, `social-and-relationships` | `face-to-face` | `neutral` | Ask a neighbor's name and introduce yourself. |
| 29 | `a1-im-supermarkt-nach-einem-produkt-fragen` | Im Supermarkt nach einem Produkt fragen | `shopping-and-services` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Ask where a product is and thank for directions. |
| 30 | `a1-im-restaurant-nach-der-karte-fragen` | Im Restaurant nach der Karte fragen | `customer-service` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Ask for the menu and answer a simple question. |
| 31 | `a1-ein-getraenk-bestellen` | Ein Getraenk bestellen | `customer-service` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | Order one drink with `bitte` and answer size/type. |
| 32 | `a1-nach-der-toilette-fragen` | Nach der Toilette fragen | `everyday-life` | `everyday-life`, `public-services` | `face-to-face` | `neutral` | Ask a simple location question politely. |
| 33 | `a1-im-sprachcafe-um-langsameres-sprechen-bitten` | Im Sprachcafe um langsameres Sprechen bitten | `conversation-cafe` | `social-and-relationships`, `education-and-training` | `face-to-face` | `neutral` | Ask someone to speak more slowly. |
| 34 | `a1-im-kurs-um-hilfe-bitten` | Im Kurs um Hilfe bitten | `classroom` | `education-and-training`, `customer-service` | `classroom` | `neutral` | Ask the teacher or partner for help. |
| 35 | `a1-eine-entschuldigung-im-kurs-formulieren` | Eine Entschuldigung im Kurs formulieren | `classroom` | `education-and-training`, `social-and-relationships` | `classroom` | `neutral` | Apologize briefly for being late or not understanding. |
| 36 | `a1-eine-einfache-einladung-annehmen` | Eine einfache Einladung annehmen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `informal` | Accept a simple invitation and confirm time/place. |
| 37 | `a1-eine-einfache-einladung-ablehnen` | Eine einfache Einladung freundlich ablehnen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `informal` | Decline briefly and politely without sounding rude. |
| 38 | `a1-sich-verabschieden-und-naechsten-kontakt-nennen` | Sich verabschieden und den naechsten Kontakt nennen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `neutral` | Say goodbye and mention a simple next contact. |
| 39 | `a1-im-amt-ein-formular-abgeben` | Im Amt ein Formular abgeben | `public-services` | `public-services`, `documents-and-administration` | `government-office` | `formal` | State that you want to submit a form and answer a document question. |
| 40 | `a1-an-der-rezeption-einchecken` | An der Rezeption einchecken | `travel` | `transport-and-travel`, `customer-service` | `service-counter` | `formal` | Say name and reservation/appointment in a controlled setting. |

## Suggested Small-Batch Order

1. Generate 8-10 scenarios from orders 2-11 after confirming the current corrected A1 pilot remains imported and smoke-tested.
2. Generate the next 8-10 scenarios from orders 12-21.
3. Generate the next 8-10 scenarios from orders 22-31.
4. Completed on 2026-05-26 with `roleplays-a1-core-07-v1.json`; all A1 candidate orders 2-40 are now generated, imported, and smoke-tested.
