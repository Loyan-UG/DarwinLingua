# A2 Course Lesson Candidates

Date: 2026-05-31

## Purpose

This file preserves the reviewed A2 `CourseLesson` backlog before full multilingual course content generation.

Use it to keep A2 Course content comparable to common modern German coursebooks and A2 exam preparation paths while staying compatible with Darwin Lingua's German-first, linked-content architecture.

## Reference Anchors

The A2 plan aligns with:

- CEFR A2 expectations: familiar everyday matters, immediate needs, simple connected sentences, short social exchanges, and routine information exchange.
- Goethe/telc-style A2 exam needs: short reading/listening tasks, simple forms/messages/emails, everyday services, appointments, health, work, school, housing, travel, and short speaking tasks.
- Existing Darwin Lingua assets: A2 Grammar topics, A2 RoleplayScenario backlog/content, A1-A2 Exercise set, Expressions, Dialogues, Talk Topics, and future Exam Prep links.

## A2 Generation Rules

- Source `title`, `description`, `shortDescription`, `narrative`, `learningGoals`, `reviewSummary`, and `homeworkTask` must be German-first.
- Add learner-helper translations for `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, and `sq`.
- A2 lessons may be longer than A1, but should still use clear short paragraphs and practical examples.
- Each lesson should normally include 3-4 concise German learning goals.
- A2 should add reasons, alternatives, confirmations, short explanations, and simple written communication.
- Course lessons must link to existing content where useful, but must not copy full Grammar/Expression/Dialogues/Roleplay/Exercise content.
- `linkedWordSlugs` may stay empty until a stable server-side word-slug convention exists.
- No sensitive/adult content belongs in this general A2 course path.
- Course imports for an existing path are cumulative: every package update must keep all reviewed lessons produced so far.
- Content production should start only after A1-C2 Course lesson planning files exist and are reviewed.

## Planned A2 Course

| CoursePath | Suggested German Title | Target Lessons | Notes |
| --- | --- | ---: | --- |
| `a2-alltag-und-integration` | A2 Alltag und Integration | 80 | New A2 path after A1 completion. Covers grammar, practical interaction, written communication, public services, work, and A2 exam bridge. |

## Planned Course Modules

| Module Number | Suggested Slug | German Title | Lesson Range | Purpose |
| ---: | --- | --- | --- | --- |
| 1 | `a2-rueckblick-und-alltagserzaehlen` | Rueckblick und Alltagserzaehlen | 1-10 | Bridge from A1, Perfekt, simple past of `sein/haben`, routines, and short personal stories. |
| 2 | `a2-faelle-pronomen-und-orte` | Faelle, Pronomen und Orte | 11-20 | Dative/accusative, pronouns, possessives, and practical prepositions. |
| 3 | `a2-einkaufen-service-und-reisen` | Einkaufen, Service und Reisen | 21-30 | Shopping, complaints, restaurant, hotel, transport, comparison, and route clarification. |
| 4 | `a2-gesundheit-schule-amt-und-wohnen` | Gesundheit, Schule, Amt und Wohnen | 31-40 | Health, pharmacy, school/kindergarten, public offices, forms, housing, and repairs. |
| 5 | `a2-schreiben-telefonieren-und-nachfragen` | Schreiben, Telefonieren und Nachfragen | 41-50 | Emails, messages, phone calls, indirect questions, `dass/weil/wenn`, and practical repair language. |
| 6 | `a2-arbeit-lernen-und-soziales` | Arbeit, Lernen und Soziales | 51-60 | Workplace tasks, sickness calls, study planning, invitations, boundaries, and simple feedback. |
| 7 | `a2-texte-medien-und-alltagskompetenz` | Texte, Medien und Alltagskompetenz | 61-70 | Reading signs/forms/ads, digital appointments, online services, banking basics, and learner strategies. |
| 8 | `a2-pruefung-und-abschluss` | Pruefung und Abschluss | 71-80 | A2 exam skills, integrated speaking/writing/listening/reading practice, common mistakes, and A2 final review. |

## Candidate Backlog

| Order | Suggested Slug | German Title | Module | Category | Topic Keys | Linked Content Focus | Core Lesson Task | A2 Quality Target |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `a2-von-a1-zu-a2-ankommen` | Von A1 zu A2 ankommen | `a2-rueckblick-und-alltagserzaehlen` | orientation | `everyday-life`, `education-and-training` | A1 review map, A2 grammar overview | Review A1 tools and understand what changes at A2. | Clear bridge, not a generic welcome lesson. |
| 2 | `a2-perfekt-mit-haben-im-alltag` | Perfekt mit haben im Alltag | `a2-rueckblick-und-alltagserzaehlen` | grammar-foundation | `everyday-life`, `social-and-relationships` | `a2-perfekt-with-haben`, A2 roleplay delays | Say what you did yesterday or last week. | Past tense in real short stories. |
| 3 | `a2-perfekt-mit-sein-fuer-wege-und-bewegung` | Perfekt mit sein fuer Wege und Bewegung | `a2-rueckblick-und-alltagserzaehlen` | grammar-foundation | `transport-and-travel`, `everyday-life` | `a2-perfekt-with-sein`, transport roleplays | Explain where you went and when you arrived. | Contrast `haben/sein` through movement. |
| 4 | `a2-unregelmaessige-partizipien-merken` | Unregelmaessige Partizipien merken | `a2-rueckblick-und-alltagserzaehlen` | grammar-foundation | `everyday-life`, `education-and-training` | `a2-common-irregular-participles` | Recognize and use frequent irregular participles. | Focus on high-frequency verbs only. |
| 5 | `a2-war-und-hatte-in-kurzen-berichten` | War und hatte in kurzen Berichten | `a2-rueckblick-und-alltagserzaehlen` | grammar-foundation | `everyday-life`, `work-and-jobs` | `a2-praeteritum-of-sein-and-haben` | Use `war` and `hatte` in simple reports. | Avoid full Präteritum overload. |
| 6 | `a2-alltag-und-gewohnheiten-beschreiben` | Alltag und Gewohnheiten beschreiben | `a2-rueckblick-und-alltagserzaehlen` | everyday-life | `everyday-life`, `work-and-jobs` | regular verbs, reflexive intro | Describe routine, workday, and free time. | Add time phrases and sequence markers. |
| 7 | `a2-sich-und-andere-kurz-beschreiben` | Sich und andere kurz beschreiben | `a2-rueckblick-und-alltagserzaehlen` | social | `social-and-relationships`, `everyday-life` | adjective endings intro, pronouns | Describe a person simply and respectfully. | Natural description without stereotypes. |
| 8 | `a2-einfache-erlebnisse-erzaehlen` | Einfache Erlebnisse erzaehlen | `a2-rueckblick-und-alltagserzaehlen` | speaking | `social-and-relationships`, `education-and-training` | Perfekt, connectors | Tell a short weekend or travel experience. | Connect 4-5 simple sentences coherently. |
| 9 | `a2-gruende-mit-denn-und-weil-geben` | Gruende mit denn und weil geben | `a2-rueckblick-und-alltagserzaehlen` | grammar-foundation | `everyday-life`, `social-and-relationships` | `a2-denn-versus-weil`, `a2-weil-clauses` | Give a short reason for a plan or problem. | Introduce verb-final slowly. |
| 10 | `a2-rueckblick-und-alltagserzaehlen-wiederholen` | Rueckblick und Alltagserzaehlen wiederholen | `a2-rueckblick-und-alltagserzaehlen` | review | `everyday-life`, `education-and-training` | module review, A1-A2 exercise set | Review past tense, reasons, routines, and short stories. | First A2 mini-capstone. |
| 11 | `a2-dativ-im-alltag-verstehen` | Dativ im Alltag verstehen | `a2-faelle-pronomen-und-orte` | grammar-foundation | `everyday-life`, `shopping-and-services` | `a2-dative-case-basics` | Use dative in common phrases and service contexts. | Practical dative, not declension tables only. |
| 12 | `a2-akkusativ-und-dativ-unterscheiden` | Akkusativ und Dativ unterscheiden | `a2-faelle-pronomen-und-orte` | grammar-foundation | `everyday-life`, `transport-and-travel` | `a2-accusative-versus-dative-basics` | Decide whether a noun is object or location/recipient. | Use simple visual/context cues. |
| 13 | `a2-dativpronomen-im-gespraech` | Dativpronomen im Gespraech | `a2-faelle-pronomen-und-orte` | grammar-foundation | `social-and-relationships`, `customer-service` | `a2-dative-pronouns` | Say `mir`, `dir`, `Ihnen` in routine exchanges. | Prioritize communication phrases. |
| 14 | `a2-akkusativpronomen-im-alltag` | Akkusativpronomen im Alltag | `a2-faelle-pronomen-und-orte` | grammar-foundation | `shopping-and-services`, `everyday-life` | `a2-accusative-pronouns` | Replace repeated nouns with `ihn`, `sie`, `es`. | Keep gender reference clear. |
| 15 | `a2-possessivpronomen-mit-faellen` | Possessivpronomen mit Faellen | `a2-faelle-pronomen-und-orte` | grammar-foundation | `housing`, `documents-and-administration` | `a2-possessive-pronouns-in-cases` | Talk about your appointment, child, address, or document. | Tie endings to common admin phrases. |
| 16 | `a2-wechselpraepositionen-wo-und-wohin` | Wechselpraepositionen: wo und wohin | `a2-faelle-pronomen-und-orte` | grammar-foundation | `housing`, `transport-and-travel` | `a2-wechselpraepositionen-introduction` | Describe where something is and where it goes. | Keep `wo/wohin` contrast practical. |
| 17 | `a2-praepositionen-mit-dativ` | Praepositionen mit Dativ | `a2-faelle-pronomen-und-orte` | grammar-foundation | `transport-and-travel`, `everyday-life` | `a2-prepositions-with-dative` | Use `mit`, `nach`, `bei`, `von`, `zu`, `aus` in short tasks. | Functional route and appointment language. |
| 18 | `a2-praepositionen-mit-akkusativ` | Praepositionen mit Akkusativ | `a2-faelle-pronomen-und-orte` | grammar-foundation | `everyday-life`, `customer-service` | `a2-prepositions-with-accusative` | Use `fuer`, `ohne`, `durch`, `um` in routine contexts. | Avoid rare preposition overload. |
| 19 | `a2-orte-und-dinge-genau-beschreiben` | Orte und Dinge genau beschreiben | `a2-faelle-pronomen-und-orte` | everyday-life | `housing`, `shopping-and-services` | prepositions, adjective endings | Describe a room, product, or lost item with simple details. | Prepare for service and housing tasks. |
| 20 | `a2-faelle-pronomen-und-orte-wiederholen` | Faelle, Pronomen und Orte wiederholen | `a2-faelle-pronomen-und-orte` | review | `everyday-life`, `education-and-training` | case review, exercise set | Review cases, pronouns, and prepositions. | Second A2 mini-capstone. |
| 21 | `a2-im-laden-groesse-farbe-und-preis` | Im Laden: Groesse, Farbe und Preis | `a2-einkaufen-service-und-reisen` | shopping | `shopping-and-services`, `customer-service` | shopping roleplays, adjective endings | Ask about size, color, price, and availability. | Add choices and comparison. |
| 22 | `a2-einen-artikel-umtauschen` | Einen Artikel umtauschen | `a2-einkaufen-service-und-reisen` | customer-service | `shopping-and-services`, `customer-service` | roleplay `a2-im-laden-eine-groesse-umtauschen` | Explain a simple exchange request. | Include receipt, reason, and solution. |
| 23 | `a2-eine-einfache-reklamation-formulieren` | Eine einfache Reklamation formulieren | `a2-einkaufen-service-und-reisen` | customer-service | `shopping-and-services`, `customer-service` | supermarket complaint roleplay | Explain a product problem and ask politely for help. | Polite complaint without B1 argumentation. |
| 24 | `a2-im-restaurant-korrigieren-und-nachfragen` | Im Restaurant korrigieren und nachfragen | `a2-einkaufen-service-und-reisen` | customer-service | `shopping-and-services`, `appointments-and-health` | restaurant correction, ingredients roleplay | Correct an order and ask about ingredients. | Include repair language, allergy/preference neutral. |
| 25 | `a2-vergleichen-mit-komparativ` | Vergleichen mit Komparativ | `a2-einkaufen-service-und-reisen` | grammar-foundation | `shopping-and-services`, `travel-and-transport` | `a2-comparative-forms` | Compare two products, routes, or appointments. | Use practical choices, not abstract adjectives. |
| 26 | `a2-superlativ-in-alltagsempfehlungen` | Superlativ in Alltagsempfehlungen | `a2-einkaufen-service-und-reisen` | grammar-foundation | `shopping-and-services`, `social-and-relationships` | `a2-superlative-basics` | Say what is best, cheapest, fastest, or closest. | Keep recommendations simple and useful. |
| 27 | `a2-im-hotel-informationen-klaeren` | Im Hotel Informationen klaeren | `a2-einkaufen-service-und-reisen` | travel | `transport-and-travel`, `customer-service` | hotel roleplays | Ask about breakfast, checkout, and room problem. | Confirm two pieces of information. |
| 28 | `a2-am-bahnhof-verbindung-und-gleis-klaeren` | Am Bahnhof Verbindung und Gleis klaeren | `a2-einkaufen-service-und-reisen` | transport | `transport-and-travel`, `customer-service` | station roleplay | Ask about platform, transfer, and delay. | Include route summary. |
| 29 | `a2-bei-verspaetung-richtig-reagieren` | Bei Verspaetung richtig reagieren | `a2-einkaufen-service-und-reisen` | transport | `transport-and-travel`, `social-and-relationships` | delay message roleplay | Say you are late, apologize, and give arrival time. | Combine reason, apology, and new time. |
| 30 | `a2-einkaufen-service-und-reisen-wiederholen` | Einkaufen, Service und Reisen wiederholen | `a2-einkaufen-service-und-reisen` | review | `shopping-and-services`, `transport-and-travel` | module review, exercise set | Review service requests, comparisons, hotel, transport, and delays. | Third A2 mini-capstone. |
| 31 | `a2-beim-arzt-beschwerden-beschreiben` | Beim Arzt Beschwerden beschreiben | `a2-gesundheit-schule-amt-und-wohnen` | health | `appointments-and-health`, `everyday-life` | doctor roleplay, health grammar | Say what hurts, since when, and how strong it is. | Practical and non-diagnostic. |
| 32 | `a2-in-der-apotheke-einnahme-klaeren` | In der Apotheke Einnahme klaeren | `a2-gesundheit-schule-amt-und-wohnen` | health | `appointments-and-health`, `shopping-and-services` | pharmacy roleplay | Ask how often and when to take medicine. | Confirm instruction safely. |
| 33 | `a2-krankmeldung-in-schule-und-arbeit` | Krankmeldung in Schule und Arbeit | `a2-gesundheit-schule-amt-und-wohnen` | health | `appointments-and-health`, `education-and-training`, `work-and-jobs` | school/work sick-call roleplays | Report illness with duration and next step. | Distinguish school/work register. |
| 34 | `a2-schule-und-kindergarten-informationen-geben` | Schule und Kindergarten: Informationen geben | `a2-gesundheit-schule-amt-und-wohnen` | school-kindergarten | `education-and-training`, `everyday-life` | kindergarten/school roleplays | Give child name, group/class, reason, and time. | Include polite formal clarity. |
| 35 | `a2-im-amt-unterlagen-nachreichen` | Im Amt Unterlagen nachreichen | `a2-gesundheit-schule-amt-und-wohnen` | public-services | `public-services`, `documents-and-administration` | Bürgeramt roleplay | Submit missing documents and ask for confirmation. | Document names and next step. |
| 36 | `a2-einen-fehler-im-formular-klaeren` | Einen Fehler im Formular klaeren | `a2-gesundheit-schule-amt-und-wohnen` | public-services | `public-services`, `documents-and-administration` | form correction roleplay | Explain a form mistake and ask how to correct it. | Clear field, correction, and next action. |
| 37 | `a2-nach-dem-stand-eines-antrags-fragen` | Nach dem Stand eines Antrags fragen | `a2-gesundheit-schule-amt-und-wohnen` | public-services | `public-services`, `documents-and-administration` | phone status roleplay | Ask about application status with date/reference. | Include reference details. |
| 38 | `a2-dem-vermieter-ein-problem-melden` | Dem Vermieter ein Problem melden | `a2-gesundheit-schule-amt-und-wohnen` | housing | `housing`, `everyday-life` | landlord problem roleplay | Report a housing issue and request a repair appointment. | Include urgency and availability. |
| 39 | `a2-nachbarschaft-und-hausregeln-klaeren` | Nachbarschaft und Hausregeln klaeren | `a2-gesundheit-schule-amt-und-wohnen` | housing | `housing`, `social-and-relationships` | neighbor/rules roleplays | Ask about noise, bins, bikes, or laundry rules politely. | Avoid confrontation; use soft wording. |
| 40 | `a2-gesundheit-schule-amt-und-wohnen-wiederholen` | Gesundheit, Schule, Amt und Wohnen wiederholen | `a2-gesundheit-schule-amt-und-wohnen` | review | `appointments-and-health`, `public-services`, `housing` | module review, exercise set | Review health, school, office, form, and housing tasks. | Fourth A2 mini-capstone. |
| 41 | `a2-einfache-email-struktur` | Einfache E-Mail-Struktur | `a2-schreiben-telefonieren-und-nachfragen` | writing | `documents-and-administration`, `education-and-training` | `a2-simple-email-grammar` | Write a short email with greeting, reason, request, and closing. | A2 writing template without long formal style. |
| 42 | `a2-kurze-nachrichten-im-alltag` | Kurze Nachrichten im Alltag | `a2-schreiben-telefonieren-und-nachfragen` | writing | `everyday-life`, `social-and-relationships` | delay message roleplay, expressions | Write a short message about delay, plan, or thanks. | Natural and concise. |
| 43 | `a2-telefonieren-mit-name-grund-und-rueckruf` | Telefonieren mit Name, Grund und Rueckruf | `a2-schreiben-telefonieren-und-nachfragen` | phone | `everyday-life`, `business-communication` | phone roleplays | Start a call, leave a message, request callback. | Include name, reason, number, time. |
| 44 | `a2-termin-verschieben-und-bestaetigen` | Termin verschieben und bestaetigen | `a2-schreiben-telefonieren-und-nachfragen` | appointments | `appointments-and-health`, `documents-and-administration` | appointment reschedule roleplays | Reschedule and confirm a new appointment. | Give reason and two options. |
| 45 | `a2-indirekte-fragen-hoeflich-stellen` | Indirekte Fragen hoeflich stellen | `a2-schreiben-telefonieren-und-nachfragen` | grammar-foundation | `public-services`, `customer-service` | `a2-indirect-questions-introduction` | Ask politely for information. | Practical service/public-office use. |
| 46 | `a2-dass-saetze-fuer-informationen` | Dass-Saetze fuer Informationen | `a2-schreiben-telefonieren-und-nachfragen` | grammar-foundation | `everyday-life`, `work-and-jobs` | `a2-dass-clauses` | Report what someone says, thinks, or confirms. | Keep subordinate clauses short. |
| 47 | `a2-weil-und-wenn-fuer-gruende-und-bedingungen` | Weil und wenn fuer Gruende und Bedingungen | `a2-schreiben-telefonieren-und-nachfragen` | grammar-foundation | `everyday-life`, `appointments-and-health` | `a2-weil-clauses`, `a2-wenn-for-conditions` | Explain a reason and a simple condition. | Important A2 connector bridge. |
| 48 | `a2-bevor-und-nachdem-fuer-ablauf` | Bevor und nachdem fuer Ablauf | `a2-schreiben-telefonieren-und-nachfragen` | grammar-foundation | `everyday-life`, `work-and-jobs` | `a2-time-clauses-bevor-and-nachdem-introduction` | Describe sequence in appointments or work steps. | Keep sequence concrete. |
| 49 | `a2-zu-plus-infinitiv-in-plaenen` | Zu plus Infinitiv in Plaenen | `a2-schreiben-telefonieren-und-nachfragen` | grammar-foundation | `education-and-training`, `work-and-jobs` | `a2-zu-plus-infinitive-introduction` | Say what you plan, try, or need to do. | Avoid B1-level complexity. |
| 50 | `a2-schreiben-telefonieren-und-nachfragen-wiederholen` | Schreiben, Telefonieren und Nachfragen wiederholen | `a2-schreiben-telefonieren-und-nachfragen` | review | `documents-and-administration`, `education-and-training` | module review, exercise set | Review short email, phone, appointment, and polite questions. | Fifth A2 mini-capstone. |
| 51 | `a2-im-buero-aufgaben-klaeren` | Im Buero Aufgaben klaeren | `a2-arbeit-lernen-und-soziales` | workplace | `work-and-jobs`, `business-communication` | workplace task roleplay | Ask what to do, by when, and how. | Clarify task, deadline, first step. |
| 52 | `a2-bei-der-arbeit-um-hilfe-bitten` | Bei der Arbeit um Hilfe bitten | `a2-arbeit-lernen-und-soziales` | workplace | `work-and-jobs`, `business-communication` | workplace help roleplay | Explain where you are stuck and ask for help. | Specific request, not vague help. |
| 53 | `a2-schicht-oder-termin-tauschen` | Schicht oder Termin tauschen | `a2-arbeit-lernen-und-soziales` | workplace | `work-and-jobs`, `appointments-and-health` | shift swap roleplay | Ask to swap a shift/appointment and offer an alternative. | Give reason and confirmation. |
| 54 | `a2-eine-verspaetung-im-team-erklaeren` | Eine Verspaetung im Team erklaeren | `a2-arbeit-lernen-und-soziales` | workplace | `work-and-jobs`, `business-communication` | team delay roleplay | Explain delay, apologize, and state recovery step. | Workplace-safe and concise. |
| 55 | `a2-im-deutschkurs-nachfragen` | Im Deutschkurs nachfragen | `a2-arbeit-lernen-und-soziales` | education | `education-and-training`, `everyday-life` | course/class roleplays | Ask for explanation, correction, or course information. | Learner autonomy at A2. |
| 56 | `a2-lernen-mit-partner-planen` | Lernen mit Partner planen | `a2-arbeit-lernen-und-soziales` | education | `education-and-training`, `social-and-relationships` | study partner roleplay | Plan time, place, and topic for practice. | Include negotiation of one conflict. |
| 57 | `a2-ein-treffen-planen-und-absagen` | Ein Treffen planen und absagen | `a2-arbeit-lernen-und-soziales` | social | `social-and-relationships`, `everyday-life` | invitation roleplays | Accept, decline, or propose a new time politely. | Reason plus alternative. |
| 58 | `a2-missverstaendnisse-freundlich-klaeren` | Missverstaendnisse freundlich klaeren | `a2-arbeit-lernen-und-soziales` | social | `social-and-relationships`, `everyday-life` | misunderstanding roleplay, repair expressions | Say you misunderstood and ask again. | Polite repair in social context. |
| 59 | `a2-einfaches-feedback-geben` | Einfaches Feedback geben | `a2-arbeit-lernen-und-soziales` | workplace | `work-and-jobs`, `business-communication` | feedback roleplay | Say what worked and what should change. | Concrete and not too direct. |
| 60 | `a2-arbeit-lernen-und-soziales-wiederholen` | Arbeit, Lernen und Soziales wiederholen | `a2-arbeit-lernen-und-soziales` | review | `work-and-jobs`, `education-and-training`, `social-and-relationships` | module review, exercise set | Review work, learning, social planning, and repair language. | Sixth A2 mini-capstone. |
| 61 | `a2-schilder-und-hinweise-verstehen` | Schilder und Hinweise verstehen | `a2-texte-medien-und-alltagskompetenz` | reading | `public-services`, `transport-and-travel` | signs, public-service language | Understand common signs, notices, and short instructions. | Practical reading skill. |
| 62 | `a2-anzeigen-und-angebote-lesen` | Anzeigen und Angebote lesen | `a2-texte-medien-und-alltagskompetenz` | reading | `shopping-and-services`, `housing`, `work-and-jobs` | ads, comparisons | Read simple ads for rooms, jobs, courses, and products. | Extract key information. |
| 63 | `a2-formulare-und-online-felder-verstehen` | Formulare und Online-Felder verstehen | `a2-texte-medien-und-alltagskompetenz` | public-services | `documents-and-administration`, `public-services` | forms, admin roleplays | Understand and fill common fields. | No legal advice, only language. |
| 64 | `a2-digitale-termine-und-apps-nutzen` | Digitale Termine und Apps nutzen | `a2-texte-medien-und-alltagskompetenz` | digital-life | `everyday-life`, `documents-and-administration` | appointment language, phone/email | Book or change a simple digital appointment. | Modern everyday skill. |
| 65 | `a2-bank-und-zahlung-einfach-klaeren` | Bank und Zahlung einfach klaeren | `a2-texte-medien-und-alltagskompetenz` | everyday-life | `shopping-and-services`, `documents-and-administration` | payment, receipts | Ask about payment, transfer, card, or receipt. | Keep financial detail basic. |
| 66 | `a2-lieferung-und-bestellung-nachfragen` | Lieferung und Bestellung nachfragen | `a2-texte-medien-und-alltagskompetenz` | customer-service | `shopping-and-services`, `customer-service` | delivery phone roleplay | Ask about delivery status with order number. | Include reference number and expected date. |
| 67 | `a2-einfache-mediennachrichten-verstehen` | Einfache Mediennachrichten verstehen | `a2-texte-medien-und-alltagskompetenz` | reading-listening | `everyday-life`, `social-and-relationships` | short texts, announcements | Understand a short public or personal message. | Main point plus one detail. |
| 68 | `a2-informationen-vergleichen-und-auswaehlen` | Informationen vergleichen und auswaehlen | `a2-texte-medien-und-alltagskompetenz` | reading-speaking | `shopping-and-services`, `education-and-training` | comparative/superlative, ads | Compare two options and choose one with reason. | Useful for exams and real life. |
| 69 | `a2-lernstrategien-fuer-a2` | Lernstrategien fuer A2 | `a2-texte-medien-und-alltagskompetenz` | learning-strategy | `education-and-training`, `everyday-life` | repair language, review maps | Use repetition, examples, word families, and self-checks. | Practical learning autonomy. |
| 70 | `a2-texte-medien-und-alltagskompetenz-wiederholen` | Texte, Medien und Alltagskompetenz wiederholen | `a2-texte-medien-und-alltagskompetenz` | review | `documents-and-administration`, `education-and-training` | module review, exercise set | Review signs, ads, forms, online appointments, and choices. | Seventh A2 mini-capstone. |
| 71 | `a2-pruefung-lesen-kurztexte-und-anzeigen` | Pruefung Lesen: Kurztexte und Anzeigen | `a2-pruefung-und-abschluss` | exam | `education-and-training`, `documents-and-administration` | A2 exam prep, reading skills | Read short texts and identify key information. | Goethe/telc-style but not exam-copy content. |
| 72 | `a2-pruefung-hoeren-alltag-und-telefon` | Pruefung Hoeren: Alltag und Telefon | `a2-pruefung-und-abschluss` | exam | `education-and-training`, `everyday-life` | listening tasks, phone roleplays | Listen for time, place, reason, and next step. | Short realistic audio-script preparation. |
| 73 | `a2-pruefung-schreiben-nachricht-und-email` | Pruefung Schreiben: Nachricht und E-Mail | `a2-pruefung-und-abschluss` | exam | `education-and-training`, `documents-and-administration` | simple email grammar | Write a short message or email with required points. | Must cover all bullet points. |
| 74 | `a2-pruefung-sprechen-sich-vorstellen` | Pruefung Sprechen: Sich vorstellen | `a2-pruefung-und-abschluss` | exam | `education-and-training`, `social-and-relationships` | speaking intro, A2 roleplays | Present yourself with place, work/study, hobbies, and reason for learning. | More complete than A1 intro. |
| 75 | `a2-pruefung-sprechen-fragen-stellen` | Pruefung Sprechen: Fragen stellen | `a2-pruefung-und-abschluss` | exam | `education-and-training`, `social-and-relationships` | question patterns, roleplays | Ask and answer simple exam-style questions. | Include follow-up question. |
| 76 | `a2-pruefung-sprechen-etwas-planen` | Pruefung Sprechen: Etwas planen | `a2-pruefung-und-abschluss` | exam | `education-and-training`, `appointments-and-health` | planning roleplays | Plan a meeting/activity with time, place, and reason. | A2 planning, not B1 negotiation. |
| 77 | `a2-haeufige-a2-fehler-vermeiden` | Haeufige A2-Fehler vermeiden | `a2-pruefung-und-abschluss` | review | `education-and-training`, `everyday-life` | `a2-common-a2-mistakes` | Notice common errors in cases, word order, perfect tense, and connectors. | Correction-focused, not shaming. |
| 78 | `a2-verben-faelle-und-konnektoren-wiederholen` | Verben, Faelle und Konnektoren wiederholen | `a2-pruefung-und-abschluss` | review | `education-and-training`, `everyday-life` | verb/case/connectors reviews | Review main A2 grammar systems in context. | Integrated grammar review. |
| 79 | `a2-a2-alltagssituationen-kombinieren` | A2 Alltagssituationen kombinieren | `a2-pruefung-und-abschluss` | review | `everyday-life`, `public-services`, `work-and-jobs` | full A2 linked assets | Combine health, office, work, service, and social tasks. | Final practical capstone. |
| 80 | `a2-a2-abschluss-und-b1-bruecke` | A2 Abschluss und B1-Bruecke | `a2-pruefung-und-abschluss` | review | `education-and-training`, `everyday-life` | A2 review map, B1 preview | Review A2 outcomes and prepare for B1. | Clear next-step guidance. |

## Suggested Small-Batch Order

Course imports are cumulative per path. For `a2-alltag-und-integration`, every package update must include all earlier reviewed lessons for the path.

| Batch | Orders Added | Expected Total Lessons In Package | Focus | Notes |
| --- | --- | ---: | --- | --- |
| A2-01 | 1-5 | 5 | A2 bridge and Perfekt start | Establish path/module 1. |
| A2-02 | 6-10 | 10 | Complete module 1 | Routines, experiences, reasons, review. |
| A2-03 | 11-15 | 15 | Start module 2 | Dative/accusative/pronouns/possessives. |
| A2-04 | 16-20 | 20 | Complete module 2 | Prepositions, descriptions, review. |
| A2-05 | 21-25 | 25 | Start module 3 | Shopping/service/complaint/comparison. |
| A2-06 | 26-30 | 30 | Complete module 3 | Restaurant/hotel/transport/delay/review. |
| A2-07 | 31-35 | 35 | Start module 4 | Health, pharmacy, school, Amt documents. |
| A2-08 | 36-40 | 40 | Complete module 4 | Forms, application status, housing, review. |
| A2-09 | 41-45 | 45 | Start module 5 | Email, messages, phone, appointments, indirect questions. |
| A2-10 | 46-50 | 50 | Complete module 5 | Subordinate clauses, sequence, `zu`, review. |
| A2-11 | 51-55 | 55 | Start module 6 | Workplace tasks/help/schedule/delay/course questions. |
| A2-12 | 56-60 | 60 | Complete module 6 | Learning plan, social planning, feedback, review. |
| A2-13 | 61-65 | 65 | Start module 7 | Signs, ads, forms, digital appointments, payment. |
| A2-14 | 66-70 | 70 | Complete module 7 | Delivery, media messages, comparison, strategies, review. |
| A2-15 | 71-75 | 75 | Start module 8 | A2 exam reading/listening/writing/speaking intro/questions. |
| A2-16 | 76-80 | 80 | Complete A2 | Planning task, common mistakes, integrated review, B1 bridge. |

## Production Readiness Gate

Before generating full A2 lesson content from this plan:

- Confirm A1, A2, B1, B2, C1, and C2 Course planning files exist.
- Confirm the user explicitly starts content generation after planning is complete.
- Use Plan mode for the first content-generation prompt of each level.
- Generate only a small cumulative batch at a time.
- Run JSON/language preflight and Course parser/import validation for each batch.
- Import to `darwinlingua_shared` only after validation.
- Verify PostgreSQL counts after import.
- Run lightweight Web/API smoke for at least one new lesson per batch.
- Take a database backup at the end of each completed CEFR level, not after every small batch.
