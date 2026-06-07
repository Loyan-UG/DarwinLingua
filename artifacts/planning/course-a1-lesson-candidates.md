# A1 Course Lesson Candidates

Date: 2026-05-31

## Purpose

This file preserves the reviewed A1 `CourseLesson` backlog before further full multilingual course content generation.

Use it to avoid ad hoc lesson creation, English-first metadata, duplicated grammar articles, weak lesson sequencing, missing learner-helper translations, and accidental deletion caused by cumulative CoursePath imports.

## A1 Generation Rules

- Source `title`, `description`, `shortDescription`, `narrative`, `learningGoals`, `reviewSummary`, and `homeworkTask` must be German-first.
- Add learner-helper translations for `en`, `fa`, `ar`, `tr`, `ru`, `ckb`, `kmr`, `pl`, `ro`, and `sq`.
- Keep A1 lessons short, concrete, and directly useful in everyday situations.
- A1 lesson narratives should normally be 2-4 short German sentences, not long articles.
- A1 learning goals should normally be 3 concise German goals.
- Course lessons must link to Grammar, Expressions, Dialogues, Roleplays, Talk Topics, and Exercises where useful, but must not duplicate their full content.
- `linkedWordSlugs` may stay empty until a stable word-slug convention exists.
- Each lesson should have a small review summary and a realistic homework/practice task.
- The first path `a1-einstieg-in-den-alltag` is cumulative: every package import for this path must keep all reviewed lessons produced so far.
- No sensitive/adult content belongs in this general A1 course path.
- Content production should start only after A1-C2 Course lesson planning files exist and are reviewed.

## Existing Imported A1 Course

| CoursePath | Module | Current Lessons | Database Status | Notes |
| --- | --- | ---: | --- | --- |
| `a1-einstieg-in-den-alltag` | `a1-erste-kontakte` | 5 | Imported into `darwinlingua_shared` on 2026-05-31 | Package is cumulative at `content/learning-portal/courses/packages/course-a1-foundation-pilot-v1.json`. |

## Existing Imported A1 Lessons

| Order | Slug | German Title | Module | Topic Keys | Notes |
| ---: | --- | --- | --- | --- | --- |
| 1 | `a1-begruessung-und-name` | Begruessung und Name | `a1-erste-kontakte` | `everyday-life`, `social-and-relationships` | Imported. |
| 2 | `a1-sein-und-haben-im-alltag` | Sein und haben im Alltag | `a1-erste-kontakte` | `everyday-life`, `grammar-foundation` | Imported. |
| 3 | `a1-einfache-fragen-und-antworten` | Einfache Fragen und Antworten | `a1-erste-kontakte` | `everyday-life`, `social-and-relationships` | Imported. |
| 4 | `a1-zahlen-und-kurze-angaben` | Zahlen und kurze Angaben | `a1-erste-kontakte` | `everyday-life`, `appointments-and-health` | Imported. |
| 5 | `a1-artikel-und-dinge-im-alltag` | Artikel und Dinge im Alltag | `a1-erste-kontakte` | `everyday-life`, `shopping-and-services` | Imported. |

## Planned Course Modules

| Module Number | Suggested Slug | German Title | Lesson Range | Purpose |
| ---: | --- | --- | --- | --- |
| 1 | `a1-erste-kontakte` | Erste Kontakte | 1-10 | Greetings, names, basic questions, numbers, articles, and first contact patterns. |
| 2 | `a1-einfache-alltagssaetze` | Einfache Alltagssaetze | 11-20 | Verb position, negation, time/place, simple connectors, and basic personal information. |
| 3 | `a1-einkaufen-und-service` | Einkaufen und Service | 21-30 | Cafe, bakery, supermarket, price, payment, menu, and polite service phrases. |
| 4 | `a1-wege-termine-und-reisen` | Wege, Termine und Reisen | 31-40 | Directions, transport, appointment language, hotel/reception, and simple time coordination. |
| 5 | `a1-gesundheit-schule-und-amt` | Gesundheit, Schule und Amt | 41-50 | Doctor reception, pharmacy, school/kindergarten, forms, and public office basics. |
| 6 | `a1-wohnen-arbeit-und-wiederholung` | Wohnen, Arbeit und Wiederholung | 51-60 | Housing, neighbors, workplace greetings, asking for repetition/help, and A1 review. |

## Candidate Backlog

| Order | Suggested Slug | German Title | Module | Category | Topic Keys | Linked Content Focus | Core Lesson Task | A1 Quality Target |
| ---: | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | `a1-begruessung-und-name` | Begruessung und Name | `a1-erste-kontakte` | social | `everyday-life`, `social-and-relationships` | greetings, basic dialogue, first exercise set | Greet someone and say your name. | Very short, confidence-building first lesson. |
| 2 | `a1-sein-und-haben-im-alltag` | Sein und haben im Alltag | `a1-erste-kontakte` | grammar-foundation | `everyday-life` | `sein`, `haben`, verb position | Use `ich bin` and `ich habe` in short situations. | Keep grammar tied to real sentences. |
| 3 | `a1-einfache-fragen-und-antworten` | Einfache Fragen und Antworten | `a1-erste-kontakte` | social | `everyday-life`, `social-and-relationships` | W-questions, yes/no questions | Ask and answer name, place, and well-being questions. | Avoid abstract grammar-only explanation. |
| 4 | `a1-zahlen-und-kurze-angaben` | Zahlen und kurze Angaben | `a1-erste-kontakte` | everyday-life | `everyday-life`, `appointments-and-health` | numbers, appointment phrases | Give age, phone number, or time in a sentence. | Numbers must appear in practical mini-sentences. |
| 5 | `a1-artikel-und-dinge-im-alltag` | Artikel und Dinge im Alltag | `a1-erste-kontakte` | grammar-foundation | `everyday-life`, `shopping-and-services` | articles, noun gender | Learn everyday nouns with article. | Do not make isolated vocabulary lists. |
| 6 | `a1-personen-und-pronomen` | Personen und Pronomen | `a1-erste-kontakte` | grammar-foundation | `everyday-life`, `social-and-relationships` | personal pronouns, pronoun-verb agreement | Use `ich`, `du`, `Sie`, `er`, `sie` in tiny exchanges. | Keep forms anchored to who is speaking. |
| 7 | `a1-du-und-sie-im-ersten-kontakt` | Du und Sie im ersten Kontakt | `a1-erste-kontakte` | social | `social-and-relationships`, `work-and-jobs` | formal `Sie`, `du` vs `Sie` | Choose polite or informal address in simple situations. | Explain social risk without overloading grammar. |
| 8 | `a1-regelmaessige-verben-im-alltag` | Regelmaessige Verben im Alltag | `a1-erste-kontakte` | grammar-foundation | `everyday-life` | regular present-tense verbs | Say what you learn, live, work, or need. | Use high-frequency verbs only. |
| 9 | `a1-eine-kurze-vorstellung-bauen` | Eine kurze Vorstellung bauen | `a1-erste-kontakte` | social | `social-and-relationships`, `education-and-training` | intro dialogue, expressions, exercise set | Combine greeting, name, place, and one question. | First mini-capstone for module 1. |
| 10 | `a1-erste-kontakte-wiederholen` | Erste Kontakte wiederholen | `a1-erste-kontakte` | review | `everyday-life`, `education-and-training` | module review links | Review greetings, names, questions, numbers, and articles. | Keep review task active, not a summary dump. |
| 11 | `a1-nicht-und-kein-verstehen` | Nicht und kein verstehen | `a1-einfache-alltagssaetze` | grammar-foundation | `everyday-life` | `kein` vs `nicht`, negation | Say what you do not have or do not understand. | Use only simple, high-frequency negation. |
| 12 | `a1-kurze-saetze-mit-zeit` | Kurze Saetze mit Zeit | `a1-einfache-alltagssaetze` | everyday-life | `everyday-life`, `appointments-and-health` | time expressions | Say today, tomorrow, morning, evening, and simple times. | Time phrases must support appointments. |
| 13 | `a1-kurze-saetze-mit-ort` | Kurze Saetze mit Ort | `a1-einfache-alltagssaetze` | everyday-life | `everyday-life`, `transport-and-travel` | basic location phrases | Say where something is or where you are. | Keep prepositions concrete. |
| 14 | `a1-zeit-und-ort-im-satz` | Zeit und Ort im Satz | `a1-einfache-alltagssaetze` | grammar-foundation | `everyday-life`, `transport-and-travel` | word order with time/place | Build short sentences with time and place. | Avoid B1-level word-order complexity. |
| 15 | `a1-und-aber-im-alltag` | Und und aber im Alltag | `a1-einfache-alltagssaetze` | grammar-foundation | `everyday-life`, `social-and-relationships` | simple conjunctions | Connect two short ideas with `und` or `aber`. | Keep clauses independent and clear. |
| 16 | `a1-ich-moechte-hoeflich-sagen` | Ich moechte hoeflich sagen | `a1-einfache-alltagssaetze` | customer-service | `shopping-and-services`, `customer-service` | `moechte`, polite requests | Ask politely for one thing or one appointment. | Useful before service lessons. |
| 17 | `a1-koennen-muessen-wollen-einfach` | Koennen, muessen, wollen einfach | `a1-einfache-alltagssaetze` | grammar-foundation | `everyday-life`, `work-and-jobs` | simple modal verbs | Say a very simple ability, need, or wish. | Limit to one modal per sentence. |
| 18 | `a1-bitte-wiederholen-und-langsamer-sprechen` | Bitte wiederholen und langsamer sprechen | `a1-einfache-alltagssaetze` | learning-strategy | `education-and-training`, `social-and-relationships` | `wie bitte`, repetition expressions, classroom roleplay | Ask for repetition or slower speech. | Must teach repair language early. |
| 19 | `a1-einfache-alltagssaetze-kombinieren` | Einfache Alltagssaetze kombinieren | `a1-einfache-alltagssaetze` | review | `everyday-life`, `education-and-training` | grammar review, exercise set | Combine negation, time, place, and polite request. | Second mini-capstone. |
| 20 | `a1-alltagssaetze-wiederholen` | Alltagssaetze wiederholen | `a1-einfache-alltagssaetze` | review | `everyday-life`, `education-and-training` | module review links | Review simple sentence patterns before service scenarios. | Should prepare for real interactions. |
| 21 | `a1-im-cafe-bestellen` | Im Cafe bestellen | `a1-einkaufen-und-service` | customer-service | `shopping-and-services`, `everyday-life` | cafe dialogue, ordering roleplay, polite expressions | Order one drink and answer a short question. | Keep transactional and natural. |
| 22 | `a1-in-der-baeckerei-kaufen` | In der Baeckerei kaufen | `a1-einkaufen-und-service` | shopping | `shopping-and-services`, `everyday-life` | bakery roleplay, articles, numbers | Ask for one item and understand the price. | Include quantity and `bitte`. |
| 23 | `a1-im-supermarkt-fragen` | Im Supermarkt fragen | `a1-einkaufen-und-service` | shopping | `shopping-and-services`, `customer-service` | product-location roleplay, location grammar | Ask where a product is. | Keep directions short. |
| 24 | `a1-an-der-kasse-bezahlen` | An der Kasse bezahlen | `a1-einkaufen-und-service` | shopping | `shopping-and-services`, `customer-service` | payment roleplay, numbers | Answer cash/card question and close politely. | Include realistic cashier language. |
| 25 | `a1-nach-dem-preis-fragen` | Nach dem Preis fragen | `a1-einkaufen-und-service` | shopping | `shopping-and-services`, `customer-service` | price expressions, numbers | Ask for a price and decide yes/no politely. | Teach refusal without sounding rude. |
| 26 | `a1-im-restaurant-nach-der-karte-fragen` | Im Restaurant nach der Karte fragen | `a1-einkaufen-und-service` | customer-service | `shopping-and-services`, `everyday-life` | restaurant roleplay, polite request | Ask for the menu and one item. | Avoid full dining dialogue. |
| 27 | `a1-ein-getraenk-und-essen-bestellen` | Ein Getraenk und Essen bestellen | `a1-einkaufen-und-service` | customer-service | `shopping-and-services`, `everyday-life` | cafe/restaurant roleplay | Order a drink and simple food. | Keep food nouns article-linked. |
| 28 | `a1-service-nicht-verstehen` | Service nicht verstehen | `a1-einkaufen-und-service` | learning-strategy | `customer-service`, `shopping-and-services` | repetition expressions, service roleplay | Say you did not understand and ask again. | Repair language in public context. |
| 29 | `a1-einkaufen-und-service-kombinieren` | Einkaufen und Service kombinieren | `a1-einkaufen-und-service` | review | `shopping-and-services`, `customer-service` | module links, exercise set | Combine item, price, payment, and polite close. | Third mini-capstone. |
| 30 | `a1-einkaufen-und-service-wiederholen` | Einkaufen und Service wiederholen | `a1-einkaufen-und-service` | review | `shopping-and-services`, `education-and-training` | module review links | Review service phrases and short transactions. | Prepare for appointments and travel. |
| 31 | `a1-nach-dem-weg-fragen` | Nach dem Weg fragen | `a1-wege-termine-und-reisen` | directions | `transport-and-travel`, `everyday-life` | directions dialogue, location grammar | Ask for a simple direction and repeat key information. | No complex route descriptions. |
| 32 | `a1-im-bus-nach-der-haltestelle-fragen` | Im Bus nach der Haltestelle fragen | `a1-wege-termine-und-reisen` | transport | `transport-and-travel`, `everyday-life` | transport roleplay | Ask whether this is the right stop. | Use realistic short passenger language. |
| 33 | `a1-am-bahnhof-nach-dem-gleis-fragen` | Am Bahnhof nach dem Gleis fragen | `a1-wege-termine-und-reisen` | transport | `transport-and-travel`, `customer-service` | station roleplay, numbers | Ask for platform and time. | Keep numbers and location together. |
| 34 | `a1-einen-termin-machen` | Einen Termin machen | `a1-wege-termine-und-reisen` | appointments | `appointments-and-health`, `documents-and-administration` | appointment dialogue, time grammar | Ask for an appointment and state a simple time. | Avoid long phone-call complexity. |
| 35 | `a1-einen-termin-bestaetigen` | Einen Termin bestaetigen | `a1-wege-termine-und-reisen` | appointments | `appointments-and-health`, `documents-and-administration` | appointment roleplay | Confirm day and time and repeat it. | Emphasize listening accuracy. |
| 36 | `a1-telefonisch-kurz-sprechen` | Telefonisch kurz sprechen | `a1-wege-termine-und-reisen` | phone | `everyday-life`, `appointments-and-health` | phone roleplay, repetition expressions | Start a very short phone call and ask for repetition. | Phone language must stay minimal. |
| 37 | `a1-an-der-rezeption-einchecken` | An der Rezeption einchecken | `a1-wege-termine-und-reisen` | travel | `transport-and-travel`, `customer-service` | reception roleplay | Say name and reservation/appointment. | Useful for hotel, course, and office reception. |
| 38 | `a1-unterwegs-nicht-sicher-sein` | Unterwegs nicht sicher sein | `a1-wege-termine-und-reisen` | transport | `transport-and-travel`, `everyday-life` | directions, repetition, yes/no questions | Ask if you are in the right place. | Teach polite uncertainty. |
| 39 | `a1-wege-termine-und-reisen-kombinieren` | Wege, Termine und Reisen kombinieren | `a1-wege-termine-und-reisen` | review | `transport-and-travel`, `appointments-and-health` | module links, exercise set | Combine direction, time, and appointment phrases. | Fourth mini-capstone. |
| 40 | `a1-wege-termine-und-reisen-wiederholen` | Wege, Termine und Reisen wiederholen | `a1-wege-termine-und-reisen` | review | `transport-and-travel`, `education-and-training` | module review links | Review transport and appointment basics. | Prepare for offices and health. |
| 41 | `a1-beim-arzt-anmelden` | Beim Arzt anmelden | `a1-gesundheit-schule-und-amt` | health | `appointments-and-health`, `documents-and-administration` | doctor reception dialogue, appointment phrases | Say name and appointment time at the practice. | Neutral, non-medical detail. |
| 42 | `a1-in-der-apotheke-fragen` | In der Apotheke fragen | `a1-gesundheit-schule-und-amt` | health | `appointments-and-health`, `shopping-and-services` | pharmacy roleplay, polite request | Ask for simple help at the pharmacy. | No medical advice; language only. |
| 43 | `a1-in-der-praxis-warten` | In der Praxis warten | `a1-gesundheit-schule-und-amt` | health | `appointments-and-health`, `everyday-life` | doctor office roleplay | Ask whether to wait and understand a short instruction. | Realistic but very short. |
| 44 | `a1-im-kindergarten-bescheid-sagen` | Im Kindergarten Bescheid sagen | `a1-gesundheit-schule-und-amt` | school-kindergarten | `education-and-training`, `everyday-life` | kindergarten roleplay | Give a short notice and answer one follow-up. | Keep family context neutral. |
| 45 | `a1-in-der-schule-nach-dem-buero-fragen` | In der Schule nach dem Buero fragen | `a1-gesundheit-schule-und-amt` | school-kindergarten | `education-and-training`, `public-services` | school office roleplay, directions | Ask where the office is. | Link directions and formal address. |
| 46 | `a1-im-buergeramt-ankommen` | Im Buergeramt ankommen | `a1-gesundheit-schule-und-amt` | public-services | `public-services`, `documents-and-administration` | government office roleplay | State name, appointment, and document purpose. | Keep admin vocabulary limited. |
| 47 | `a1-ein-formular-abgeben` | Ein Formular abgeben | `a1-gesundheit-schule-und-amt` | public-services | `public-services`, `documents-and-administration` | form submission roleplay | Say that you want to submit a form. | Avoid legal complexity. |
| 48 | `a1-am-empfang-nachfragen` | Am Empfang nachfragen | `a1-gesundheit-schule-und-amt` | public-services | `public-services`, `customer-service` | reception roleplay, repetition expressions | Ask where to go and confirm. | Reuse reception language across contexts. |
| 49 | `a1-gesundheit-schule-und-amt-kombinieren` | Gesundheit, Schule und Amt kombinieren | `a1-gesundheit-schule-und-amt` | review | `appointments-and-health`, `public-services` | module links, exercise set | Combine name, appointment, waiting, and form phrases. | Fifth mini-capstone. |
| 50 | `a1-gesundheit-schule-und-amt-wiederholen` | Gesundheit, Schule und Amt wiederholen | `a1-gesundheit-schule-und-amt` | review | `public-services`, `education-and-training` | module review links | Review formal everyday language. | Prepare for housing and work. |
| 51 | `a1-nachbarn-im-hausflur-gruessen` | Nachbarn im Hausflur gruessen | `a1-wohnen-arbeit-und-wiederholung` | housing | `housing`, `social-and-relationships` | neighbor roleplay, greetings | Greet a neighbor and exchange one polite sentence. | Keep social nuance simple. |
| 52 | `a1-ein-paket-bei-nachbarn-abholen` | Ein Paket bei Nachbarn abholen | `a1-wohnen-arbeit-und-wiederholung` | housing | `housing`, `everyday-life` | package roleplay, polite request | Ask for a package and thank politely. | Practical everyday housing task. |
| 53 | `a1-im-haus-nach-dem-namen-fragen` | Im Haus nach dem Namen fragen | `a1-wohnen-arbeit-und-wiederholung` | housing | `housing`, `social-and-relationships` | neighbor introduction | Ask a neighbor's name and introduce yourself. | Recycle first-contact language in housing. |
| 54 | `a1-im-buero-kollegen-gruessen` | Im Buero Kollegen gruessen | `a1-wohnen-arbeit-und-wiederholung` | workplace | `work-and-jobs`, `social-and-relationships` | workplace greeting roleplay | Greet a colleague and answer small talk. | Workplace-safe and simple. |
| 55 | `a1-im-buero-um-wiederholung-bitten` | Im Buero um Wiederholung bitten | `a1-wohnen-arbeit-und-wiederholung` | workplace | `work-and-jobs`, `business-communication` | repetition expressions, workplace roleplay | Ask someone to repeat slowly at work. | Repair language in workplace setting. |
| 56 | `a1-in-der-pause-smalltalk-machen` | In der Pause Smalltalk machen | `a1-wohnen-arbeit-und-wiederholung` | workplace | `work-and-jobs`, `social-and-relationships` | small talk roleplay | Answer a simple personal question and ask one back. | Avoid B1-level opinions. |
| 57 | `a1-im-kurs-um-hilfe-bitten` | Im Kurs um Hilfe bitten | `a1-wohnen-arbeit-und-wiederholung` | education | `education-and-training`, `everyday-life` | classroom roleplay, help request | Ask for help in class. | Learner survival language. |
| 58 | `a1-eine-einfache-einladung-annehmen-oder-ablehnen` | Eine einfache Einladung annehmen oder ablehnen | `a1-wohnen-arbeit-und-wiederholung` | social | `social-and-relationships`, `everyday-life` | invitation roleplay | Accept or decline a simple invitation politely. | Teach short, non-rude refusal. |
| 59 | `a1-a1-alltag-kombinieren` | A1 Alltag kombinieren | `a1-wohnen-arbeit-und-wiederholung` | review | `everyday-life`, `education-and-training` | full A1 links, exercise set | Combine greetings, questions, service, appointments, and help requests. | Final A1 scenario-style lesson. |
| 60 | `a1-a1-abschluss-und-naechste-schritte` | A1 Abschluss und naechste Schritte | `a1-wohnen-arbeit-und-wiederholung` | review | `everyday-life`, `education-and-training` | A1 review map, next A2 bridge | Review A1 strengths and prepare for A2. | End with clear next-step guidance. |

## Suggested Small-Batch Order

Course imports are cumulative per path. For `a1-einstieg-in-den-alltag`, every package update must include all earlier reviewed lessons.

| Batch | Orders Added | Expected Total Lessons In Package | Focus | Notes |
| --- | --- | ---: | --- | --- |
| Already imported | 1-5 | 5 | First contacts | Current database state. |
| Next | 6-10 | 10 | Complete module 1 | Add pronouns, `du/Sie`, regular verbs, introduction capstone, module review. |
| Then | 11-15 | 15 | Start module 2 | Negation, time, place, time/place order, `und/aber`. |
| Then | 16-20 | 20 | Complete module 2 | Polite `moechte`, modals, repetition language, capstone, review. |
| Then | 21-25 | 25 | Start module 3 | Cafe, bakery, supermarket, checkout, price. |
| Then | 26-30 | 30 | Complete module 3 | Restaurant, ordering, repair language, capstone, review. |
| Then | 31-35 | 35 | Start module 4 | Directions, bus, station, appointment, appointment confirmation. |
| Then | 36-40 | 40 | Complete module 4 | Phone, reception, uncertainty, capstone, review. |
| Then | 41-45 | 45 | Start module 5 | Doctor, pharmacy, waiting, kindergarten, school office. |
| Then | 46-50 | 50 | Complete module 5 | Buergeramt, form, reception, capstone, review. |
| Then | 51-55 | 55 | Start module 6 | Neighbors, package, housing intro, workplace greeting/repetition. |
| Final A1 | 56-60 | 60 | Complete A1 path | Small talk, class help, invitation, A1 capstone, A2 bridge. |

## Production Readiness Gate

Before generating full lesson content from this plan:

- Confirm A1, A2, B1, B2, C1, and C2 Course planning files exist.
- Confirm the user explicitly starts content generation after planning is complete.
- Use Plan mode for the first content-generation prompt of each level.
- Generate only a small cumulative batch at a time.
- Run JSON/language preflight and Course parser/import validation for each batch.
- Import to `darwinlingua_shared` only after validation.
- Verify PostgreSQL counts after import.
- Run lightweight Web/API smoke for at least one new lesson per batch.
- Take a database backup at the end of each completed CEFR level, not after every small batch.
