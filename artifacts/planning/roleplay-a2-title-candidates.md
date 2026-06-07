# A2 RoleplayScenario Title Candidates

Date: 2026-05-26

## Purpose

This file preserves the reviewed A2 standalone `RoleplayScenario` title backlog before full multilingual content generation. Use it to avoid duplicate titles, A1-level simplicity, English-first metadata, weak answer-choice design, and unsupported controlled values in future roleplay batches.

## A2 Generation Rules

- Source `title`, `description`, and `learnerGoal` must be German-first.
- Add learner-language translations later for `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, and `sq`.
- Keep A2 scenarios deterministic and scripted: no AI chat, no free-text scoring, and no learner-generated text sent to a model.
- A2 scenarios should usually have 6-8 turns, not only the 4-turn A1 pattern.
- A2 learner turns should include at least one extra function beyond a direct answer: a short reason, a time preference, an alternative, a confirmation, a clarification question, or a polite repair.
- Each scenario should include at least 2 answer-choice moments; use 3 when the task has a planning, problem-solving, or phone-call sequence.
- Recommended learner turns should appear in the scripted turn sequence.
- Answer choices must include at least one correct deterministic choice for validation, but learner-facing UI should emphasize plausible weaker/not-recommended alternatives instead of repeating the correct answer.
- Incorrect choices must be plausible but insufficient: too short, missing a reason, too direct for the register, missing a time/date, missing a document, unclear next step, or incomplete for the scenario.
- Do not use irrelevant filler such as `Ich weiß nicht.`.
- Use only importer-supported controlled values for `taskType`, `skillFocus`, `interactionMode`, and `register`.
- Use active topic keys, for example `everyday-life`, `social-and-relationships`, `shopping-and-services`, `appointments-and-health`, `public-services`, `transport-and-travel`, `education-and-training`, `customer-service`, `work-and-jobs`, `housing`, `business-communication`, `documents-and-administration`, and `meetings-and-presentations`.

## Existing Imported A2 Scenario

| Order | Slug | German Title | Category | Topic Keys | Notes |
| ---: | --- | --- | --- | --- | --- |
| 1 | `a2-termin-verschieben-am-telefon` | Einen Termin am Telefon verschieben | `appointments` | `appointments-and-health`, `everyday-life` | Already imported in `roleplays-a1-b2-pilot-v1`; do not duplicate. |

## Generated A2 Packages

| Package | Date | Orders Covered | Scenario Count | Database Status | Notes |
| --- | --- | --- | ---: | --- | --- |
| `roleplays-a2-core-01-v1.json` | 2026-05-26 | 2-5 | 4 | Imported into `darwinlingua_shared` | Covers doctor appointment rescheduling, symptom explanation, pharmacy dosage questions, and school sick notice communication. |
| `roleplays-a2-core-02-v1.json` | 2026-05-26 | 6-8 | 3 | Imported into `darwinlingua_shared` | Covers kindergarten pickup-time changes, Bürgeramt follow-up documents, and public-office form correction. |
| `roleplays-a2-core-03-v1.json` | 2026-05-26 | 9-11 | 3 | Imported into `darwinlingua_shared` | Covers phone status follow-up, Jobcenter appointment scheduling, and a calm supermarket product complaint. |
| `roleplays-a2-core-04-v1.json` | 2026-05-26 | 12-14 | 3 | Imported into `darwinlingua_shared` | Covers clothing size exchange, receipt clarification, and polite restaurant order correction. |
| `roleplays-a2-core-05-v1.json` | 2026-05-26 | 15-17 | 3 | Imported into `darwinlingua_shared` | Covers cafe ingredient questions, phone table reservation, and reporting a hotel room problem. |
| `roleplays-a2-core-06-v1.json` | 2026-05-26 | 18-20 | 3 | Imported into `darwinlingua_shared` | Covers hotel breakfast/checkout questions, train-station connection clarification, and polite train-seat clarification. |
| `roleplays-a2-core-07-v1.json` | 2026-05-26 | 21-23 | 3 | Imported into `darwinlingua_shared` | Covers delay-message wording, route clarification with transfer, and reporting a housing problem to a landlord. |
| `roleplays-a2-core-08-v1.json` | 2026-05-26 | 24-26 | 3 | Imported into `darwinlingua_shared` | Covers polite neighbour noise repair, building hallway rule clarification, and arranging a repair appointment. |
| `roleplays-a2-core-09-v1.json` | 2026-05-26 | 27-29 | 3 | Imported into `darwinlingua_shared` | Covers workplace task clarification, asking a colleague for specific task help, and calling in sick at work. |
| `roleplays-a2-core-10-v1.json` | 2026-05-26 | 30-32 | 3 | Imported into `darwinlingua_shared` | Covers shift-swap negotiation, break-time small talk, and explaining a delay to the team. |
| `roleplays-a2-core-11-v1.json` | 2026-05-26 | 33-36 | 4 | Imported into `darwinlingua_shared` | Covers course date clarification, asking for a grammar explanation, planning partner practice, and suggesting a language-cafe topic. |
| `roleplays-a2-core-12-v1.json` | 2026-05-26 | 37-40 | 4 | Imported into `darwinlingua_shared` | Covers asking for correction, planning a social meeting, declining an invitation with a reason, and repairing a misunderstanding. |

## Candidate Backlog

| Order | Suggested Slug | German Title | Category | Topic Keys | Interaction Mode | Register | Task Type | Skill Focus | Core Learner Task | A2 Complexity Target |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 2 | `a2-arzttermin-wegen-kind-verschieben` | Einen Arzttermin wegen des Kindes verschieben | `appointments` | `appointments-and-health`, `everyday-life` | `phone` | `formal` | `make-appointment` | `speaking`, `roleplay`, `phone-call`, `appointment-management` | Call, explain child-related reason, ask for another time, confirm new appointment. | Give a short reason and choose between two offered times. |
| 3 | `a2-in-der-praxis-beschwerden-kurz-erklaeren` | Beschwerden in der Praxis kurz erklären | `health` | `appointments-and-health`, `everyday-life` | `doctor-office` | `formal` | `explain-problem` | `speaking`, `roleplay`, `formal-conversation` | State symptoms, duration, and answer a follow-up question. | Include duration and simple intensity, not just one symptom. |
| 4 | `a2-in-der-apotheke-nach-einnahme-fragen` | In der Apotheke nach der Einnahme fragen | `health` | `appointments-and-health`, `shopping-and-services` | `service-counter` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `service-interaction` | Ask how often to take medicine and repeat the instruction. | Confirm frequency and time of day. |
| 5 | `a2-in-der-schule-eine-krankmeldung-erklaeren` | In der Schule eine Krankmeldung erklären | `school-kindergarten` | `education-and-training`, `appointments-and-health` | `school-kindergarten` | `formal` | `explain-problem` | `speaking`, `roleplay`, `formal-conversation` | Explain that a child is ill and ask what document is needed. | Include child name, class/group, and expected absence. |
| 6 | `a2-im-kindergarten-abholzeit-aendern` | Im Kindergarten die Abholzeit ändern | `school-kindergarten` | `education-and-training`, `everyday-life` | `school-kindergarten` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `formal-conversation` | Say you will arrive later and ask whether that is okay. | Include new time and reason. |
| 7 | `a2-beim-buergeramt-unterlagen-nachreichen` | Beim Bürgeramt Unterlagen nachreichen | `public-services` | `public-services`, `documents-and-administration` | `government-office` | `formal` | `ask-for-help` | `speaking`, `roleplay`, `formal-conversation` | Submit missing documents and ask whether anything else is needed. | Identify document and ask for confirmation. |
| 8 | `a2-im-amt-einen-fehler-im-formular-klaeren` | Im Amt einen Fehler im Formular klären | `public-services` | `public-services`, `documents-and-administration` | `government-office` | `formal` | `explain-problem` | `speaking`, `roleplay`, `formal-conversation` | Explain a simple form mistake and ask how to correct it. | Clarify field, correction, and next step. |
| 9 | `a2-telefonisch-nach-dem-stand-fragen` | Telefonisch nach dem Stand fragen | `public-services` | `public-services`, `documents-and-administration` | `phone` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `phone-call` | Ask about the status of an application and give reference details. | Include reference number or date and request next step. |
| 10 | `a2-beim-jobcenter-nach-einem-termin-fragen` | Beim Jobcenter nach einem Termin fragen | `public-services` | `public-services`, `work-and-jobs` | `phone` | `formal` | `make-appointment` | `speaking`, `roleplay`, `phone-call`, `appointment-management` | Ask for an appointment and explain availability. | Offer two possible times and confirm documents. |
| 11 | `a2-im-supermarkt-ein-produkt-reklamieren` | Im Supermarkt ein Produkt reklamieren | `shopping-and-services` | `shopping-and-services`, `customer-service` | `service-counter` | `formal` | `explain-problem` | `speaking`, `roleplay`, `service-interaction` | Explain product problem and ask for exchange or refund. | Include purchase date and preferred solution. |
| 12 | `a2-im-laden-eine-groesse-umtauschen` | Im Laden eine Größe umtauschen | `shopping-and-services` | `shopping-and-services`, `customer-service` | `service-counter` | `formal` | `explain-problem` | `speaking`, `roleplay`, `service-interaction` | Say the size does not fit and ask for another size. | Give size, receipt status, and alternative. |
| 13 | `a2-an-der-kasse-einen-bon-klaeren` | An der Kasse einen Bon klären | `shopping-and-services` | `shopping-and-services`, `customer-service` | `service-counter` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `service-interaction` | Ask about a receipt item and clarify price difference. | Mention product and amount. |
| 14 | `a2-im-restaurant-eine-bestellung-korrigieren` | Im Restaurant eine Bestellung korrigieren | `customer-service` | `shopping-and-services`, `everyday-life` | `service-counter` | `neutral` | `explain-problem` | `speaking`, `roleplay`, `service-interaction` | Politely correct a wrong order and ask for replacement. | Use polite repair, not accusation. |
| 15 | `a2-im-cafe-nach-zutaten-fragen` | Im Café nach Zutaten fragen | `customer-service` | `shopping-and-services`, `appointments-and-health` | `service-counter` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `service-interaction` | Ask whether a food contains milk, nuts, or meat. | Include reason such as allergy or preference. |
| 16 | `a2-einen-tisch-reservieren` | Einen Tisch reservieren | `customer-service` | `shopping-and-services`, `everyday-life` | `phone` | `formal` | `make-appointment` | `speaking`, `roleplay`, `phone-call` | Reserve a table by phone with people count and time. | Confirm date, time, name, and phone number. |
| 17 | `a2-im-hotel-ein-problem-melden` | Im Hotel ein Problem melden | `travel` | `transport-and-travel`, `customer-service` | `service-counter` | `formal` | `explain-problem` | `speaking`, `roleplay`, `service-interaction` | Report a room problem and ask what can be done. | Include room number and preferred solution. |
| 18 | `a2-im-hotel-nach-fruehstueck-und-checkout-fragen` | Im Hotel nach Frühstück und Checkout fragen | `travel` | `transport-and-travel`, `customer-service` | `service-counter` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `service-interaction` | Ask about breakfast time and checkout. | Confirm two pieces of information. |
| 19 | `a2-am-bahnhof-eine-verbindung-klaeren` | Am Bahnhof eine Verbindung klären | `transport` | `transport-and-travel`, `everyday-life` | `service-counter` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `service-interaction` | Ask about connection, platform, and delay. | Compare two options and repeat final route. |
| 20 | `a2-im-zug-einen-sitzplatz-klaeren` | Im Zug einen Sitzplatz klären | `transport` | `transport-and-travel`, `social-and-relationships` | `face-to-face` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `informal-conversation` | Politely ask whether a seat is free or reserved. | Clarify ticket/reservation without sounding rude. |
| 21 | `a2-bei-verspaetung-eine-nachricht-formulieren` | Bei Verspätung eine Nachricht formulieren | `transport` | `transport-and-travel`, `social-and-relationships` | `phone` | `informal` | `explain-problem` | `speaking`, `roleplay`, `phone-call` | Say you are late, give reason, and name arrival time. | Include delay length and apology. |
| 22 | `a2-nach-dem-weg-mit-umsteigen-fragen` | Nach dem Weg mit Umsteigen fragen | `directions` | `transport-and-travel`, `everyday-life` | `face-to-face` | `neutral` | `ask-for-directions` | `speaking`, `roleplay`, `service-interaction` | Ask for a route that includes changing bus/train. | Repeat first step and transfer point. |
| 23 | `a2-dem-vermieter-ein-problem-melden` | Dem Vermieter ein Problem melden | `housing` | `housing`, `everyday-life` | `phone` | `formal` | `explain-problem` | `speaking`, `roleplay`, `phone-call` | Report a housing issue and ask for an appointment. | Include urgency, availability, and contact details. |
| 24 | `a2-mit-nachbarn-wegen-laerm-sprechen` | Mit Nachbarn wegen Lärm sprechen | `housing` | `housing`, `social-and-relationships` | `face-to-face` | `neutral` | `explain-problem` | `speaking`, `roleplay`, `informal-conversation` | Politely mention noise and ask for a solution. | Use soft wording and propose a concrete quiet time. |
| 25 | `a2-im-hausflur-eine-regel-klaeren` | Im Hausflur eine Regel klären | `housing` | `housing`, `documents-and-administration` | `face-to-face` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `informal-conversation` | Ask about building rules, bins, laundry, or bikes. | Confirm rule and responsible place/person. |
| 26 | `a2-einen-handwerkertermin-absprechen` | Einen Handwerkertermin absprechen | `housing` | `housing`, `appointments-and-health` | `phone` | `formal` | `make-appointment` | `speaking`, `roleplay`, `phone-call`, `appointment-management` | Arrange a repair appointment and give access information. | Offer time window and confirm address. |
| 27 | `a2-im-buero-eine-aufgabe-klaeren` | Im Büro eine Aufgabe klären | `workplace` | `work-and-jobs`, `business-communication` | `workplace` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `workplace-communication` | Ask what exactly needs to be done and by when. | Clarify task, deadline, and first step. |
| 28 | `a2-im-buero-um-hilfe-bei-einer-aufgabe-bitten` | Im Büro um Hilfe bei einer Aufgabe bitten | `workplace` | `work-and-jobs`, `business-communication` | `workplace` | `neutral` | `ask-for-help` | `speaking`, `roleplay`, `workplace-communication` | Explain where you are stuck and ask for help. | Give attempted step and specific help request. |
| 29 | `a2-eine-krankmeldung-bei-der-arbeit-machen` | Eine Krankmeldung bei der Arbeit machen | `workplace` | `work-and-jobs`, `appointments-and-health` | `phone` | `formal` | `explain-problem` | `speaking`, `roleplay`, `phone-call`, `workplace-communication` | Call in sick, give expected absence, and ask about next steps. | Include apology, duration, and certificate question. |
| 30 | `a2-eine-schicht-tauschen` | Eine Schicht tauschen | `workplace` | `work-and-jobs`, `business-communication` | `workplace` | `neutral` | `explain-problem` | `speaking`, `roleplay`, `workplace-communication` | Ask a colleague to swap shifts and give a reason. | Offer alternative and confirm agreement. |
| 31 | `a2-in-der-pause-ueber-plaene-sprechen` | In der Pause über Pläne sprechen | `workplace` | `work-and-jobs`, `social-and-relationships` | `face-to-face` | `informal` | `ask-for-information` | `speaking`, `roleplay`, `informal-conversation` | Talk about weekend or evening plans and ask back. | Answer with time/place and ask a follow-up question. |
| 32 | `a2-im-team-eine-verspaetung-erklaeren` | Im Team eine Verspätung erklären | `workplace` | `work-and-jobs`, `business-communication` | `workplace` | `neutral` | `explain-problem` | `speaking`, `roleplay`, `workplace-communication` | Explain why you are late and say when you can start. | Include reason, apology, and recovery step. |
| 33 | `a2-im-deutschkurs-einen-termin-klaeren` | Im Deutschkurs einen Termin klären | `classroom` | `education-and-training`, `appointments-and-health` | `classroom` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `formal-conversation` | Ask about test/course date and what to bring. | Confirm date and required material. |
| 34 | `a2-im-kurs-um-eine-erklaerung-bitten` | Im Kurs um eine Erklärung bitten | `classroom` | `education-and-training`, `everyday-life` | `classroom` | `neutral` | `ask-for-help` | `speaking`, `roleplay`, `formal-conversation` | Ask the teacher to explain a grammar point again. | State which part is unclear and request an example. |
| 35 | `a2-mit-einem-lernpartner-ueben-planen` | Mit einem Lernpartner Üben planen | `classroom` | `education-and-training`, `social-and-relationships` | `face-to-face` | `informal` | `make-appointment` | `speaking`, `roleplay`, `informal-conversation` | Plan a short study session with time, place, and topic. | Negotiate one conflict and confirm final plan. |
| 36 | `a2-im-sprachcafe-ein-thema-vorschlagen` | Im Sprachcafé ein Thema vorschlagen | `conversation-cafe` | `social-and-relationships`, `education-and-training` | `face-to-face` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `informal-conversation` | Suggest a conversation topic and ask for opinions. | Give reason why the topic is useful. |
| 37 | `a2-im-sprachcafe-um-korrektur-bitten` | Im Sprachcafé um Korrektur bitten | `conversation-cafe` | `social-and-relationships`, `education-and-training` | `face-to-face` | `neutral` | `ask-for-help` | `speaking`, `roleplay`, `informal-conversation` | Ask a partner to correct one sentence politely. | Accept correction and repeat improved sentence. |
| 38 | `a2-ein-treffen-mit-bekannten-planen` | Ein Treffen mit Bekannten planen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `informal` | `make-appointment` | `speaking`, `roleplay`, `informal-conversation` | Plan a meeting with day, place, and activity. | Choose between two options and confirm. |
| 39 | `a2-eine-einladung-mit-grund-ablehnen` | Eine Einladung mit Grund ablehnen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `informal` | `explain-problem` | `speaking`, `roleplay`, `informal-conversation` | Decline an invitation politely and propose another time. | Give a short reason and alternative. |
| 40 | `a2-nach-einem-missverstaendnis-nachfragen` | Nach einem Missverständnis nachfragen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `informal-conversation` | Say you may have misunderstood and ask for clarification. | Use repair phrase and repeat corrected meaning. |
| 41 | `a2-ein-kompliment-geben-und-reagieren` | Ein Kompliment geben und reagieren | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `informal` | `ask-for-information` | `speaking`, `roleplay`, `informal-conversation` | Give a simple compliment and respond naturally. | Add one reason and answer thanks modestly. |
| 42 | `a2-eine-grenze-freundlich-setzen` | Eine Grenze freundlich setzen | `social` | `social-and-relationships`, `everyday-life` | `face-to-face` | `neutral` | `explain-problem` | `speaking`, `roleplay`, `informal-conversation` | Say that something is not possible and offer a simple alternative. | Use polite refusal, reason, and alternative. |
| 43 | `a2-am-telefon-eine-nachricht-hinterlassen` | Am Telefon eine Nachricht hinterlassen | `phone` | `everyday-life`, `business-communication` | `phone` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `phone-call` | Leave name, reason for call, and callback request. | Include phone number and preferred time. |
| 44 | `a2-einen-rueckruf-vereinbaren` | Einen Rückruf vereinbaren | `phone` | `everyday-life`, `business-communication` | `phone` | `formal` | `make-appointment` | `speaking`, `roleplay`, `phone-call`, `appointment-management` | Ask when someone can call back and confirm time. | Choose one time window and repeat it. |
| 45 | `a2-eine-lieferung-nachfragen` | Eine Lieferung nachfragen | `customer-service` | `shopping-and-services`, `documents-and-administration` | `phone` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `phone-call`, `service-interaction` | Ask about delivery status with order number. | Include order number and expected delivery date. |
| 46 | `a2-einen-service-termin-verschieben` | Einen Service-Termin verschieben | `customer-service` | `shopping-and-services`, `appointments-and-health` | `phone` | `formal` | `make-appointment` | `speaking`, `roleplay`, `phone-call`, `appointment-management` | Reschedule a service appointment and confirm new slot. | Give reason and two alternatives. |
| 47 | `a2-bei-einem-kursanbieter-informationen-erfragen` | Bei einem Kursanbieter Informationen erfragen | `education` | `education-and-training`, `customer-service` | `phone` | `formal` | `ask-for-information` | `speaking`, `roleplay`, `phone-call` | Ask about course time, price, and registration. | Ask two linked questions and summarize answer. |
| 48 | `a2-beim-verein-nach-einem-probetermin-fragen` | Beim Verein nach einem Probetermin fragen | `social` | `social-and-relationships`, `everyday-life` | `phone` | `neutral` | `make-appointment` | `speaking`, `roleplay`, `phone-call` | Ask for a trial appointment at a club. | Mention interest, level, and availability. |
| 49 | `a2-ein-einfaches-feedback-geben` | Einfaches Feedback geben | `workplace` | `work-and-jobs`, `business-communication` | `workplace` | `neutral` | `explain-problem` | `speaking`, `roleplay`, `workplace-communication` | Say what worked well and what needs improvement. | Give one positive point and one concrete request. |
| 50 | `a2-einen-naechsten-schritt-zusammenfassen` | Einen nächsten Schritt zusammenfassen | `workplace` | `work-and-jobs`, `meetings-and-presentations` | `workplace` | `neutral` | `ask-for-information` | `speaking`, `roleplay`, `workplace-communication` | Summarize who does what next after a short discussion. | Name person, task, and deadline. |

## Suggested Small-Batch Order

1. Completed `roleplays-a2-core-01-v1.json` from orders 2-5 with a deliberately smaller first A2 batch for localization and quality control.
2. Completed `roleplays-a2-core-02-v1.json` from orders 6-8 with a deliberately smaller public-office/kindergarten batch for quality control.
3. Completed `roleplays-a2-core-03-v1.json` from orders 9-11 with a deliberately smaller phone/service batch for quality control.
4. Completed `roleplays-a2-core-04-v1.json` from orders 12-14 with a deliberately smaller shopping/restaurant repair batch for quality control.
5. Completed `roleplays-a2-core-05-v1.json` from orders 15-17 with a deliberately smaller cafe/restaurant/hotel batch for quality control.
6. Completed `roleplays-a2-core-06-v1.json` from orders 18-20 with a deliberately smaller travel/transport batch for quality control.
7. Completed `roleplays-a2-core-07-v1.json` from orders 21-23 with a deliberately smaller delay/directions/housing batch for quality control.
8. Completed `roleplays-a2-core-08-v1.json` from orders 24-26 with a deliberately smaller housing-rules/repair batch for quality control.
9. Completed `roleplays-a2-core-09-v1.json` from orders 27-29 with a deliberately smaller workplace task/help/sick-call batch for quality control.
10. Completed `roleplays-a2-core-10-v1.json` from orders 30-32 with a deliberately smaller workplace scheduling/social-repair batch for quality control.
11. Completed `roleplays-a2-core-11-v1.json` from orders 33-36 with a deliberately smaller classroom/language-cafe batch for quality control.
12. Completed `roleplays-a2-core-12-v1.json` from orders 37-40 with a deliberately smaller correction/social-repair batch for quality control.
13. Completed `roleplays-a2-core-13-v1.json` from orders 41-43 with a deliberately smaller social/phone batch for quality control.
14. Completed `roleplays-a2-core-14-v1.json` from orders 44-47 with a deliberately smaller phone/service/education batch for quality control.
15. Completed `roleplays-a2-core-15-v1.json` from orders 48-50 with a deliberately smaller club/workplace-summary batch for quality control.

Status: A2 planned roleplay scenario list is complete. Continue with the next CEFR level only after reviewing the generated A2 coverage and keeping the same validation/import/smoke gate.

## Production Readiness Gate For Each A2 Batch

- Create exactly one package per batch.
- Run a preflight for required fields, active learner languages, controlled values, duplicate slugs, missing answer-choice moments, banned filler answers, and English fallback markers.
- Run ContentOps Roleplay parser/application tests.
- Take a fresh `darwinlingua_shared` backup before import.
- Import with `DarwinLingua.ImportTool --target shared --yes`.
- Verify PostgreSQL row counts and new active slugs.
- Smoke `/roleplays`, at least two detail pages, API detail, search, and admin report where applicable.
- Update this file and add a validation report under `artifacts/validation/`.
