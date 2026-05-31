# Learning Portal Roadmap And Backlog

## Purpose

This document defines the complete Web-first roadmap for turning Darwin Lingua into a full German-learning portal.

It covers:

- Grammar Guide
- Everyday Expressions, idioms, colloquial phrases, and proverbs
- reusable Exercise Engine
- Course Lessons and structured CEFR learning paths
- Exam Preparation
- Writing Templates and real-life communication support
- Cultural Notes and German-in-Germany guidance
- unified learning search
- learning progress and personalization
- admin and content operations
- Web-first implementation followed by mobile parity
- documentation consolidation rules

This is the single planning source for the learning-portal expansion. Do not create separate roadmap documents for each feature unless implementation proves that a dedicated contract document is required.

---

## Current Implementation Snapshot

Last updated: 2026-05-25.

- Grammar Guide A1-C2 content is generated and validated against the official syllabus and package contract. The latest validation report is `artifacts/validation/grammar-a1-c2-validation-report.md` and records 225 topics with zero issues.
- Post-Grammar Conversation audit blockers are repaired. The latest conversation audit report is `artifacts/validation/conversation-content-audit-report.md` and records zero P0 blockers, zero unresolved Dialogue word references, and zero unresolved Talk Topic word references.
- Existing Conversation content remains protected. Dialogues and Talk Topics were not deleted; only malformed JSON, malformed word references, duplicate useful-word references, and weak active seed dialogue metadata were repaired.
- A small importable Conversation support baseline exists at `content/generated/conversation-support/conversation-support-baseline-v1.json`. It contains 6 Conversation Starter Packs and 8 Event Preparation Packs across A1-C2 with all target learner languages.
- The first real Everyday Expressions pilot package exists at `content/learning-portal/expressions/packages/expressions-a1-a2-core-pilot-v1.json`. It contains 12 A1/A2-focused expressions with all target learner languages and is covered by parser, import-validation, repository/query, Unified Search, Web route/render-structure, admin-report tests, shared PostgreSQL import, and local Web/API/admin smoke validation.
- The next small Everyday Expressions package exists at `content/learning-portal/expressions/packages/expressions-a1-a2-core-01-v1.json`. It contains 25 A1/A2 practical expression candidates with all target learner languages. After the stricter eligibility pass, 23 entries remain published and 2 ordinary-literal entries are unpublished archive findings.
- The A2 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-a2-core-v1.json`. It contains 30 published A2 idioms, semi-idiomatic phrases, and pragmatic formulas with all target learner languages; no ordinary-literal entry is published.
- The B1 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-b1-core-v1.json`. It contains 40 published B1 idioms, semi-idiomatic phrases, pragmatic formulas, and mild-frustration expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The B2 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-b2-core-v1.json`. It contains 40 published B2 idioms, semi-idiomatic phrases, workplace/social nuance expressions, and mild-frustration expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The second B2 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-b2-core-02-v1.json`. It contains 40 additional published B2 idioms and pragmatic expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The third B2 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-b2-core-03-v1.json`. It contains 40 additional published B2 idioms and semi-idiomatic workplace/social nuance expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The C1 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-c1-core-v1.json`. It contains 40 published C1 idioms, cultural phrases, nuanced workplace/social expressions, indirect criticism patterns, and mild-risk expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The second C1 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-c1-core-02-v1.json`. It contains 40 additional published C1 idioms and advanced pragmatic expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The C2 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-c2-core-v1.json`. It contains 40 published C2 idioms, culturally loaded phrases, advanced pragmatic expressions, irony/social-risk expressions, and high-context discussion phrases with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The second C2 Everyday Expressions core package exists at `content/learning-portal/expressions/packages/expressions-c2-core-02-v1.json`. It contains 40 additional published C2 high-context idioms, rhetorical/cultural expressions, and workplace/social-risk expressions with all target learner languages; no ordinary-literal entry is published and no explicit-adult content is included.
- The first mixed Everyday Expressions supplement exists at `content/learning-portal/expressions/packages/expressions-mixed-supplement-01-v1.json`. It contains 14 additional A2-C1 idioms, cultural phrases, and social/workplace expressions that were not already published; no ordinary-literal entry is published and no explicit-adult content is included.
- Everyday Expressions now has a stricter eligibility policy: it is for expressions whose real meaning, tone, or social function is not obvious from literal words. It is not a general sentence bank. Ordinary literal situational sentences belong in Dialogues, Courses, Exercises, Writing Templates, or Grammar examples. `tools/Content/Audit-ExpressionContentQuality.js` generated `artifacts/validation/expression-content-quality-report.md`; the latest pass reports zero issues. Five ordinary-literal entries across earlier packages are unpublished archive findings.
- Everyday Expressions now has a Sensitive Educational Language policy in `docs/85-Sensitive-Educational-Language-Policy.md`. Sensitive educational entries are hidden by default, warning-labeled, opt-in controlled, and non-pornographic. The current system rejects explicit-adult, blocked-illegal, verified-adult-required, and blocked/slur educational entries until a separate legal/access workflow exists.
- Web registration now has a Terms of Use acceptance and Privacy Policy notice acknowledgement path with versioned policy-acceptance records. Sensitive Educational Language remains off by default and is enabled only later through Settings; that preference is not age verification and does not unlock explicit-adult content.
- The Web legal/compliance baseline now exists in `docs/86-Web-Legal-Compliance-Baseline.md`. Public Web pages exist for Terms, Privacy, Legal Notice/Impressum, Cookie/Storage Notice, and Contact, with footer links and a cookie/storage inventory at `artifacts/validation/web-cookie-storage-inventory.md`. Current checked-in Web behavior uses only necessary/preference storage and first-party PWA cache, so no cookie banner is required unless non-essential storage is added later.
- The reviewed Everyday Expressions packages imported into `darwinlingua_shared` on 2026-05-25 with zero import warnings. After the first mixed supplement import, the shared database contains 431 Expression records: 426 active and 5 draft ordinary-literal archive findings. Public-routed Web/API smoke returned HTTP 200 for `/expressions`, `/expressions/hals-und-beinbruch`, `/expressions/die-faeden-ziehen`, `/api/catalog/expressions`, `/api/catalog/expressions/hals-und-beinbruch?primaryMeaningLanguageCode=fa`, and `/api/catalog/search?q=Groschen&resultType=expression`. The public admin report endpoint now requires credentials and returns 401 without them; admin report query tests and the strict content audit remain green, with zero ordinary-literal leakage, zero missing eligibility metadata, zero low example counts, zero missing translations, zero unresolved word/content links, and zero missing risky-content warnings.
- Roleplay terminology is now split into three surfaces: Dialogue-derived Roleplay at `/dialogues/{slug}/roleplay`, Event Preparation `roleplayPrompts`, and standalone `RoleplayScenario` packages at `/roleplays`. Standalone RoleplayScenario infrastructure now has a Web-first pilot-ready implementation slice: parser support, import validation, shared-content persistence, Web API list/detail routes, Web list/detail pages, Unified Search result support, and admin count visibility. The first standalone pilot package exists at `content/learning-portal/roleplays/packages/roleplays-a1-b2-pilot-v1.json` with 10 A1-B2 scenarios, deterministic answer choices/static feedback, and safe image placeholders. It imported into `darwinlingua_shared` on 2026-05-25 with zero warnings and local Web/API/search smoke passed. Bulk roleplay generation remains blocked.
- Transactional email has a Brevo API provider path, sandbox behavior, webhook event handling, delivery logs, diagnostics, and suppression handling. Production launch still requires operational Brevo configuration outside source control, verified sender domain, SPF/DKIM/DMARC, webhook secret, and provider DPA review.
- Mobile/MAUI parity remains later-stage work. Do not modify mobile learning content surfaces until Web/API contracts, imports, rendering, search, admin visibility, and tests are stable.
- Web and WebApi production persistence is PostgreSQL-only. SQLite is allowed only for mobile/local surfaces where SQLite is the actual runtime store; it must not drive Web/API production query behavior.

Immediate next order:

1. Keep validation reports green after any content repair.
2. Keep the Web-side Sensitive Educational Language and registration/legal acknowledgement gates covered by tests before adding more sensitive content.
3. Generate any further Everyday Expressions content only as a small reviewed batch after re-running the strict quality gate, import, Web/API smoke, Unified Search smoke, and admin report checks.
4. Review the first RoleplayScenario pilot in Web and keep any further standalone Roleplay content to small reviewed batches with import and smoke gates.
5. Expand Conversation Starter/Event Preparation only for audit-proven gaps.
6. Start Exercises only after Grammar, Conversation, and Expressions are stable enough to link to.

---

## Product Direction

Darwin Lingua should become a complete practical German-learning portal, not a collection of isolated features.

The platform should help learners:

- learn vocabulary
- understand grammar
- recognize everyday expressions and idioms
- read and discuss real topics
- practice dialogues and roleplay
- complete structured course lessons
- solve exercises
- prepare for German exams
- write practical German messages and emails
- understand German cultural and communication norms
- find conversation events and practice partners
- track progress across all learning surfaces

The Web platform is the primary implementation surface for this expansion. Mobile parity comes after the Web model, Web API contracts, content model, tests, and content operations are stable.

Existing vocabulary, dialogue, Talk Topic, event, organizer, and conversation content must not be deleted. Existing content may be linked, enriched, corrected, or extended only when needed.

---

## Guiding Principles

### No Throwaway MVP

The first implementation of each module should be production-grade.

That means:

- clean domain model
- import-ready content contract
- Web API support
- Web UI support
- admin visibility or management where appropriate
- tests
- localization
- release validation
- mobile parity backlog
- no hardcoded educational content in Razor views or MAUI screens

### Dynamic Content, Static-Like User Experience

Educational content should be dynamic and content-driven, not hardcoded static pages.

The learner-facing experience may feel like static pages because the layouts are stable and cacheable, but the content must come from structured content records.

Allowed hardcoded static pages:

- legal pages
- privacy/contact/about pages
- small operational help pages

Not allowed as hardcoded static pages:

- grammar lessons
- expression entries
- course lessons
- exercises
- exam-preparation content
- writing templates
- cultural notes

### Content References Instead Of Duplication

New modules should reference existing content where possible.

Examples:

- Grammar topics link to exercises, lessons, dialogues, words, and common mistakes.
- Course lessons link to grammar topics, words, expressions, dialogues, Talk Topics, and exercises.
- Expressions link to words but do not duplicate word meanings.
- Exercises link to grammar/topics/lessons instead of owning all teaching content.

### Web First, Mobile Later

Implementation order:

1. Web domain model and persistence
2. Web API contracts
3. Web UI
4. admin/content operations
5. tests and validation
6. content generation
7. mobile package export
8. MAUI refactor and parity

Mobile should not drive the first model design for this expansion. The Web implementation should establish the complete feature model first.

---

## Documentation Consolidation Policy

The documentation set is already large. For this expansion:

- keep this document as the single roadmap/backlog source
- do not create separate planning docs for Grammar, Expressions, Exercises, Courses, and Exam Prep
- add dedicated content-contract docs only when a module is implemented and needs a stable JSON/import contract
- keep `04-Implementation-Backlog.md` as the historic project-wide execution backlog
- keep `71-Web-Test-Backlog.md` for cross-feature test debt
- prefer updating this document over creating new one-off planning files
- consolidate obsolete compatibility pointer docs when safe

Recommended future cleanup:

- keep core docs: vision, scope, phases, backlog, architecture, domain, bounded contexts, engineering standards, release checklist
- keep content contracts that are actively used by importers
- remove or merge obsolete phase-specific worksheets after their release validation is complete
- avoid duplicate documents that repeat the same roadmap or backlog

---

## Proposed Documentation Changes

Add this file:

- `docs/76-Learning-Portal-Roadmap-And-Backlog.md`

Update:

- `docs/00-Documentation-Index.md`
- `docs/03-Product-Phases.md`
- `docs/04-Implementation-Backlog.md`
- `docs/22-Domain-Model.md`
- `docs/26-Bounded-Contexts.md`
- `docs/71-Web-Test-Backlog.md`

Do not create separate documents for each new module yet. Dedicated contract docs should be created only when the corresponding implementation starts.

---

## Module 1: Grammar Guide

### Product Purpose

Grammar Guide is a structured German grammar reference and teaching section. It explains grammar in the learner's selected language while keeping German examples central.

### Recommended UI Name

- Grammar
- Grammar Guide

### Dynamic Content Model

Suggested aggregate:

- `GrammarTopic`

Suggested child concepts:

- `GrammarSection`
- `GrammarExample`
- `GrammarCommonMistake`
- `GrammarRuleSummary`
- `GrammarExceptionNote`
- `GrammarPrerequisiteLink`
- `GrammarRelatedTopicLink`
- `GrammarLinkedWord`
- `GrammarLinkedDialogue`
- `GrammarLinkedTalkTopic`
- `GrammarLinkedExercise`

### Required Metadata

- slug
- title
- short description
- CEFR level
- grammar category
- tags/topic keys
- supported explanation languages
- publication status
- sort order
- prerequisite grammar topics
- next recommended grammar topics

### Grammar Categories

Initial controlled categories:

- articles
- nouns
- gender
- plural
- pronouns
- verbs
- modal-verbs
- tenses
- separable-verbs
- reflexive-verbs
- cases
- nominative
- accusative
- dative
- genitive
- adjective-declension
- prepositions
- word-order
- subordinate-clauses
- connectors
- negation
- questions
- imperative
- passive
- konjunktiv
- reported-speech
- punctuation

### Page Structure

Each grammar detail page should show:

1. title and CEFR metadata
2. short explanation
3. learner-language explanation
4. German examples
5. example translations where useful
6. mini rules
7. common mistakes
8. exceptions
9. linked words
10. linked dialogues
11. linked Talk Topics
12. linked exercises
13. prerequisites
14. next grammar topics

### Localization Rule

Grammar explanations should support learner-selected language where available.

German examples remain German. Explanations may be shown in:

- UI language
- primary meaning language
- secondary meaning language

The exact display mode should follow the existing dual-language UX rules where appropriate.

---

## Module 2: Everyday Expressions

### Product Purpose

Everyday Expressions teaches idioms, colloquial phrases, informal expressions, fixed pragmatic formulas, cultural expressions, false friends, regional phrases, and proverbs.

This module is separate from Words because expression meaning is often not equal to the literal meaning of the individual words.

It is also separate from Dialogues, Courses, Exercises, Writing Templates, and Grammar examples. Do not use it as a bank of ordinary literal sentences such as appointment reports, problem reports, or simple classroom sentences. A literal phrase may belong here only when it is a conventional formula with a social/pragmatic function that needs explanation.

### Recommended UI Names

- Expressions
- Everyday Expressions
- German Expressions

### Suggested Aggregate

- `ExpressionEntry`

### Expression Types

Initial controlled types:

- idiom
- colloquial-phrase
- proverb
- fixed-expression
- slang
- cultural-phrase
- false-friend
- regional-expression
- polite-formula
- warning-phrase

### Required Metadata

- slug
- expression text
- literal meaning text where useful
- actual meaning text
- meaning transparency classification
- usage explanation
- teaching reason
- CEFR level
- expression type
- register
- context/category
- topic keys
- region where applicable
- safety rating
- minimum age
- adult access requirement
- publication status
- sort order

### Register Values

Initial controlled values:

- formal
- informal
- neutral
- colloquial
- slang
- rude
- polite
- workplace-safe
- friends-only
- regional

### Page Structure

Each expression detail page should show:

1. expression
2. type and register
3. CEFR level
4. actual meaning
5. literal meaning where useful
6. usage explanation
7. German example sentences
8. learner-language explanation/translation
9. warnings about tone or context
10. linked words
11. related expressions
12. linked exercises

### Safety Rule

Expressions that are rude, slang-heavy, romantic/social, sexual-educational, discriminatory, politically sensitive, or easy to misuse must have warnings and should be filtered or marked clearly.

Sensitive Educational Language can include common rude words, insults, slang, mild emotional or romantic/social phrases, and neutral non-graphic language needed for comprehension. It is hidden by default and can be shown only after a learner enables the dedicated setting.

This module must not generate pornographic, arousing, graphic sexual, exploitative, coercive, minor-related, illegal, hate-inciting, Nazi-propaganda, or harm-facilitating content. Explicit adult content must not be visible to anonymous users, minors, self-declared-only users, or users without an approved adult-access state. Do not launch explicit adult or pornographic language-learning content in production without legal review and an approved age-verification concept. Store the minimum access state needed; do not store full birthdates unless a reviewed provider workflow requires it.

Follow-up backlog:

- [x] add Expression eligibility validation and quality reporting for ordinary-literal leakage
- [x] add safety metadata fields for `meaningTransparency`, `safetyRating`, `minimumAge`, and `requiresAdultAccess`
- [x] implement a durable Web profile/settings model for rude/slang preferences and adult-content access state (`not-requested`, `self-declared-adult`, `age-verified-adult`, `blocked`)
  - Progress: Web user preferences now persist rude/slang learning preference and adult-content access state. Learner settings may store `self-declared-adult`, but this does not unlock explicit adult content; future provider-backed `age-verified-adult` access remains blocked until legal review and approved age verification.
- [x] hide adult-only Expressions from public list/detail/search by default
- [x] add Sensitive Educational Language metadata validation for safety rating, sensitive kind, opt-in requirement, verified-adult requirement, content warnings, and usage policy
- [x] add profile/settings explanation for "Show sensitive educational language"
- [x] add Web/API/search filtering so sensitive educational entries are hidden by default and only eligible opt-in flows can request allowed educational entries
- [x] add admin report visibility for sensitive content counts, missing warnings, missing usage policy, verified-adult-required entries, and blocked/explicit entries
- [x] exclude sensitive educational entries from mobile package export until mobile has equivalent opt-in filtering and warning rendering
- [x] add admin quality counts for meaning transparency, safety rating, adult-only count, missing teaching reasons, ordinary-literal leakage, and missing warnings
- [ ] add user-eligible list/search/detail filtering once legal adult-access policy is approved
- [ ] ensure mobile export excludes adult-only Expressions until mobile eligibility enforcement is ready
- [ ] add future legal/AVS gate before any explicit adult content is considered

---

## Module 3: Exercise Engine

### Product Purpose

The Exercise Engine is a reusable practice system. Exercises must be usable independently and also attachable to grammar topics, words, expressions, dialogues, Talk Topics, course lessons, and exam-prep units.

### Suggested Aggregates

- `Exercise`
- `ExerciseSet`
- `ExerciseAttempt`

### Exercise Types

Initial supported types:

- multiple-choice
- fill-in-the-blank
- matching
- sentence-ordering
- error-correction
- article-selection
- case-selection
- conjugation
- translation-controlled
- dialogue-completion
- vocabulary-choice
- grammar-choice

Future types:

- dictation
- listening-comprehension
- pronunciation-shadowing
- writing-prompt
- speaking-prompt
- AI-assisted-feedback

### Required Metadata

- slug
- title
- instruction
- CEFR level
- exercise type
- target skill
- linked content owner type
- linked content owner slug/id
- answer key
- feedback model
- publication status
- sort order

### Target Skills

Initial values:

- grammar
- vocabulary
- reading
- listening
- speaking
- writing
- pronunciation
- exam-preparation

### Feedback Requirements

Every exercise must support deterministic feedback before any AI-based feedback is considered.

Required:

- correct/incorrect state
- short explanation
- optional hint
- optional common mistake note

### Relationship Rules

Exercises should link to content, not duplicate teaching material.

Examples:

- a grammar exercise links to `GrammarTopic`
- a dialogue-completion exercise links to `DialogueLesson`
- an idiom exercise links to `ExpressionEntry`
- a course lesson contains references to exercise sets

---

## Module 4: Course Lessons And Learning Paths

### Product Purpose

Course Lessons provide book-like structured learning paths by CEFR level. Lessons combine grammar, vocabulary, expressions, dialogues, Talk Topics, and exercises.

### Recommended UI Names

- Courses
- Learning Path
- Lessons

### Suggested Aggregates

- `CoursePath`
- `CourseModule`
- `CourseLesson`

### Course Structure

Example:

- A1 German
  - A1.1
  - A1.2
- A2 German
- B1 German
- B2 German
- C1 German
- C2 German

### CourseLesson Required Sections

Each lesson should support:

1. lesson title
2. CEFR level
3. module/unit number
4. learning goals
5. short explanation
6. linked grammar topics
7. new word references
8. linked expressions
9. linked dialogue lessons
10. linked Talk Topics
11. linked exercise sets
12. review summary
13. homework/practice task
14. next lesson link

### No Duplication Rule

Course lessons should not copy full grammar articles, word meanings, or expression explanations. They should reference them and provide a short lesson narrative.

### Lesson Metadata

- slug
- course key
- module key
- CEFR level
- lesson number
- estimated minutes
- learning goals
- prerequisite lesson slugs
- publication status
- sort order

---

## Module 5: Exam Preparation

### Product Purpose

Exam Prep helps learners prepare for recognized German exams and exam-like speaking/writing tasks.

### Recommended UI Name

- Exam Prep

### Initial Exam Profiles

- Goethe A1
- Goethe A2
- Goethe B1
- Goethe B2
- telc A1
- telc A2
- telc B1
- telc B2
- DTZ A2-B1
- Berufssprache B1
- Berufssprache B2
- C1 Hochschule
- TestDaF

### Suggested Aggregates

- `ExamProfile`
- `ExamSection`
- `ExamTaskType`
- `ExamPrepUnit`
- `MockExam`

### ExamPrepUnit Should Link To

- dialogues
- Talk Topics
- grammar topics
- expressions
- writing templates
- exercises
- course lessons

### Exam Prep Features

- exam overview
- task explanations
- sample speaking tasks
- sample writing tasks
- useful phrases
- scoring checklist
- mock exam placeholders
- linked practice sets

---

## Module 6: Writing Templates

### Product Purpose

Writing Templates help learners produce practical German texts for real-life situations.

### Initial Template Categories

- email-to-school
- email-to-kindergarten
- message-to-landlord
- doctor-appointment-request
- appointment-reschedule
- sick-note-to-employer
- complaint
- application-email
- cancellation
- insurance-message
- government-office-message

### Suggested Aggregate

- `WritingTemplate`

### Required Fields

- slug
- title
- CEFR level
- category
- situation
- register
- template text
- explanation
- replaceable variables
- sample filled version
- linked grammar topics
- linked words
- linked exercises

---

## Module 7: Cultural Notes

### Product Purpose

Cultural Notes teach German communication behavior, social expectations, and context-specific language use.

### Initial Categories

- du-vs-sie
- politeness
- directness
- small-talk
- workplace-culture
- office-communication
- school-kindergarten
- doctor-visit
- appointments
- punctuality
- complaints
- bureaucracy
- conversation-cafe-etiquette

### Suggested Aggregate

- `CulturalNote`

### Relationship Rules

Cultural Notes may link to:

- dialogues
- grammar topics
- expressions
- Talk Topics
- writing templates
- course lessons

---

## Module 8: Unified Learning Search

### Product Purpose

The learner should be able to search across the full learning portal.

Search targets:

- words
- grammar topics
- expressions
- dialogues
- Talk Topics
- exercises
- course lessons
- exam prep units
- writing templates
- cultural notes
- events and organizers where appropriate

### Search Result Model

Suggested fields:

- result type
- title
- short snippet
- CEFR level
- category/topic
- URL
- relevance score
- matched term information where useful

### Search Requirements

- search by German term
- search by learner-language meaning where applicable
- filter by CEFR level
- filter by content type
- filter by category/topic
- preserve clear ranking rules

---

## Module 9: Progress And Personalization

### Product Purpose

The portal should track learner progress across content types without mixing user state into content entities.

### Suggested Concepts

- `UserContentProgress`
- `UserLessonProgress`
- `UserExerciseAttempt`
- `UserGrammarTopicState`
- `UserExpressionState`
- `UserCourseProgress`

### Progress States

- not-started
- viewed
- in-progress
- completed
- needs-review
- skipped

### Personalization Features

- current CEFR focus
- recommended next lesson
- weak grammar topics
- difficult words
- saved expressions
- recent learning activity
- exam preparation target

---

## Complete Roadmap

### Phase 7.1: Learning Portal Foundation

- [x] define shared learning portal navigation structure
  - Progress: Web learner navigation is grouped as Learn, Practice, Speak, Prepare, and Resources using only implemented routes.
- [ ] add content-type taxonomy for Grammar, Expressions, Exercises, Courses, Exam Prep, Writing Templates, and Cultural Notes
- [ ] define cross-content linking model
- [ ] define reusable linked-word reference model
- [x] define unified CEFR/category/topic filter conventions
  - Progress: Web CEFR filter levels now use a shared convention helper; category/topic conventions remain module-specific until each module starts.
- [x] define Web-first/mobile-later parity rule in docs
- [ ] add admin/system report placeholders for new content families
  - Note: keep placeholders documentation-only until a Phase 7 module has real persisted content to report.
- [x] add test backlog entries for all new modules
  - Progress: `71-Web-Test-Backlog.md` contains Learning Portal Expansion coverage for foundation and future Phase 7 modules.
- [x] document recurring content-generation lessons
  - Progress: `84-Content-Generation-Lessons-Learned.md` records prompt, localization, import, rendering, and validation lessons to check before future bulk content generation.

### Phase 7.2: Grammar Guide

- [x] implement `GrammarTopic` domain model
- [x] implement grammar child sections/examples/common mistakes/rule summaries
- [x] add EF Core mappings and migration
- [x] add import parser and validation support
- [x] add Web API list/detail endpoints
- [x] add Web list/detail pages
- [x] add filters by CEFR/category/topic/search
- [x] add linked words/dialogues/Talk Topics/exercises support
- [x] add admin visibility surface
- [x] add initial parser/navigation/localization tests
- [x] document grammar content contract in `77-Grammar-Content-Package-Contract.md`
- [x] document grammar prompt/content generation lessons in `84-Content-Generation-Lessons-Learned.md`
- [x] support rich localized grammar blocks, localized title/description, content revision diagnostics, image-slot references, and slug-based pilot upsert
- [x] import and render the first reviewed A1 personal-pronouns pilot package
- [x] archive temporary pilot and cross-level validation packages outside the official import package path
- [x] start official Grammar content generation with `grammar-a1-core-v1.json`
  - Progress: the official A1 core package now includes personal pronouns, sein, haben, regular verbs, verb position, yes/no questions, W-questions, definite articles, indefinite articles, noun gender basics, plural basics, nominative case, simple accusative introduction, kein vs nicht basics, possessive pronouns, basic adjective position, basic prepositions, numbers/grammar use, time expressions, word order with time/place, simple modal verbs, polite requests with möchte, imperative basics, separable verbs introduction, simple conjunctions und/aber, pronoun and verb agreement, formal Sie, du versus Sie grammar basics, basic sentence negation, question-answer sentence patterns, articles with food, drinks, and shopping nouns, basic location phrases, basic appointment phrases, common A1 grammar mistakes, and the final A1 grammar review map with 10 learner languages, rich sections, examples, rules, common mistakes, and linked word references.
- [x] start official A2 Grammar content generation with `grammar-a2-core-v1.json`
  - Progress: the official A2 core package now starts with Perfekt with haben, Perfekt with sein, common irregular participles, Präteritum of sein/haben, modal verbs in more detail, dative case basics, accusative vs dative basics, dative pronouns, accusative pronouns, possessive pronouns in cases, Wechselpräpositionen introduction, prepositions with dative, prepositions with accusative, separable verbs in Perfekt, reflexive verbs introduction, dass clauses, weil clauses, wenn for conditions, denn vs weil, sentence order in subordinate clauses, comparative forms, superlative basics, adjective endings introduction, indirect questions introduction, imperative formal/informal, time clauses with bevor/nachdem, zu + infinitive introduction, man as general subject, es gibt, polite forms with würde, simple email grammar, grammar for phone calls, grammar for appointments, grammar for doctor visits, grammar for school/kindergarten communication, common A2 mistakes, the A2 connectors overview, the A2 case review, the A2 verb review, and the final A2 grammar review map, including 10 learner languages, rich sections, examples, rules, common mistakes, and linked word references.
- [x] start official B1 Grammar content generation with `grammar-b1-core-v1.json`
  - Progress: the official B1 core package now starts with relative clauses basics, relative pronouns in nominative and accusative, relative pronouns in dative, Konjunktiv II for polite requests, Konjunktiv II with wäre/hätte/würde, passive voice introduction, werden as auxiliary, infinitive with zu, um ... zu, damit vs um ... zu, weil/obwohl/trotzdem, als vs wenn, nachdem/bevor/während, indirect questions, reported requests/polite questions, adjective declension after definite article, adjective declension after indefinite article, adjective declension without article, genitive introduction, prepositional verbs introduction, verb + preposition combinations, noun-verb phrases, connectors for opinion, connectors for contrast, connectors for cause/effect, sentence order with multiple clauses, formal email sentence structure, complaint sentence patterns, giving reasons clearly, agreeing/disagreeing grammatically, describing experiences in the past, and talking about plans and conditions, including 10 learner languages, rich sections, examples, rules, common mistakes, and linked word references.
- [x] run dedicated Grammar A1-C2 validation and repair pass against the syllabus and package contract
  - Progress: `artifacts/validation/grammar-a1-c2-validation-report.md` records the post-generation coverage pass for all six official Grammar packages. The next content audit should cover existing Dialogues, Talk Topics, and Conversation content before generating more module content. Bulk generation for other modules remains blocked until each module has a stable contract, import validation, Web API validation, Web rendering validation, admin visibility, and release-test coverage.
- [x] repair post-Grammar Conversation audit blockers before new bulk content generation
  - Progress: `artifacts/validation/conversation-content-audit-report.md` now reports zero P0 blockers and zero unresolved Dialogue/Talk Topic word references after JSON repair, linked-word normalization, and duplicate useful-word cleanup. `content/generated/conversation-support/conversation-support-baseline-v1.json` adds a small importable baseline for Conversation Starter Packs and Event Preparation Packs across A1-C2.
- [x] close standalone RoleplayScenario validation before generating the first standalone roleplay pilot package
  - Progress: Web-first infrastructure includes `roleplayScenarios` parser support, application validation, persistence, query services, `/api/catalog/roleplays`, `/roleplays`, Unified Search result support, and admin count-by-type/CEFR visibility. Dedicated import persistence tests, validation failure tests, repository filter/detail tests, WebApi/Web structural tests, Unified Search tests, admin report tests, full solution build, shared PostgreSQL import, and local Web/API/search smoke pass. Existing Dialogue roleplay metadata and Event Preparation `roleplayPrompts` remain separate and are not standalone RoleplayScenario content.
  - Pilot: `content/learning-portal/roleplays/packages/roleplays-a1-b2-pilot-v1.json` contains 10 A1-B2 deterministic scenarios with answer choices, static feedback, and image slots. It imported into `darwinlingua_shared` with zero warnings on 2026-05-25.
- [x] migrate Web/API-critical tests away from SQLite-backed fixtures when they validate PostgreSQL-only production behavior
  - Progress: WebApi service tests, Identity bootstrapper tests, RoleplayScenario repository/search tests, admin-report tests, server-content manifest/delivery tests, and catalog publication lifecycle tests now run against temporary PostgreSQL databases. Web/API production code rejects or avoids SQLite runtime branches. SQLite remains only for mobile/local surfaces where SQLite is the actual runtime store, not as a Web/API dependency.
  - Rule: Do not change production Web/API queries to satisfy SQLite-backed tests. PostgreSQL-specific behavior must be covered with PostgreSQL integration tests.
- [ ] add deeper RoleplayScenario admin quality counters for missing translations, unpublished drafts, missing image assets, and invalid playable sequence after more reviewed packages exist
- [ ] add broader validation coverage for every rich block type after more reviewed pilot packages are available
- [ ] keep mobile parity tracked after Web sign-off

### Phase 7.3: Everyday Expressions

- [x] implement `ExpressionEntry` domain model
- [x] implement expression examples, warnings, related expressions, linked words
- [x] add EF Core mappings and migration
- [x] add import parser and validation support
- [x] add Web API list/detail endpoints
- [x] add Web list/detail pages
- [x] add filters by CEFR/type/register/context/topic
- [x] add safety warnings for risky expressions
- [x] add admin visibility or management surface
- [x] add initial tests for parser contract and Web navigation/localization
- [x] document expression content contract in `78-Expression-Content-Package-Contract.md`
- [x] add broader validation, query, Web API, and Web rendering coverage after first real expression content package is available
  - Progress: the first pilot package imports into shared PostgreSQL, `/expressions`, `/expressions/alles-klar`, `/api/catalog/expressions`, `/api/catalog/expressions/alles-klar`, `/api/catalog/search`, and `/api/admin/catalog/system-report` smoke successfully against local Web/API services. Admin reporting now treats not-yet-created Phase 7 module tables as empty instead of failing the entire report.
- [ ] keep mobile parity tracked after Web sign-off

### Phase 7.4: Exercise Engine

- [x] implement `Exercise` and `ExerciseSet` models
- [x] support initial exercise types
- [x] support deterministic answer keys and feedback
- [x] support links to grammar, words, expressions, dialogues, Talk Topics, and lessons
- [x] add learner exercise attempts
- [x] add Web API endpoints
- [x] add Web exercise runner UI
- [x] add admin/import support
- [x] add initial tests for parser and deterministic answer evaluation
- [x] document exercise content contract in `79-Exercise-Content-Package-Contract.md`
- [x] separate stateless public exercise evaluation from authenticated persisted attempts
- [x] require authenticated user ids for persisted attempts; never store fallback anonymous attempts
- [x] bound and validate submitted-answer JSON before evaluation or persistence
- [x] rate-limit exercise evaluation and attempt endpoints
- [x] broaden runner UI beyond generic JSON submission for initial choice, single-answer, error-correction, sentence-ordering, and matching inputs
- [ ] add tests for every initial exercise type after first real exercise package is available

### Phase 7.5: Course Lessons And Learning Paths

- [x] implement `CoursePath`, `CourseModule`, and `CourseLesson`
- [x] support CEFR-level course tracks
- [x] link lessons to grammar, words, expressions, dialogues, Talk Topics, and exercises
- [x] add Web course/lesson pages
- [ ] add lesson progress tracking
- [x] add admin/import support
- [x] add initial tests for course content parsing and Web navigation/localization
- [x] document course content contract in `80-Course-Content-Package-Contract.md`
- [ ] add broader linked-content projection, Web API, persistence, and progress coverage after the first real course package is available

### Phase 7.6: Exam Preparation

- [x] implement exam profile taxonomy
- [x] implement exam prep unit model
- [x] link exam prep to dialogues, exercises, grammar, expressions, writing templates, Talk Topics, and course lessons
- [x] add Web exam prep pages
- [x] add exam-specific filters and profile routes
- [x] add initial parser/import/navigation/localization tests
- [x] document exam prep content contract in `83-Exam-Prep-Content-Package-Contract.md`
- [ ] add broader Web API, rendering, linked-content, and filter coverage after first real exam-prep package is available

### Phase 7.7: Writing Templates

- [x] implement writing template model
- [x] support template variables and sample filled versions
- [x] link to grammar, words, expressions, and exercises
- [x] add Web list/detail pages
- [x] add initial tests for template parsing, navigation, and localization
- [x] document writing template content contract in `81-Writing-Template-Content-Package-Contract.md`
- [ ] add broader template rendering, Web API, and linked-content coverage after first real writing-template package is available

### Phase 7.8: Cultural Notes

- [x] implement cultural note model
- [x] add Web list/detail pages
- [x] link to dialogues, expressions, writing templates, Talk Topics, and course lessons
- [x] add initial tests for cultural-note parsing, navigation, and localization
- [x] document cultural note content contract in `82-Cultural-Note-Content-Package-Contract.md`
- [ ] add broader filtering, Web API, Web rendering, and linked-content coverage after first real cultural-note package is available

### Phase 7.9: Unified Learning Search

- [x] define unified search result model
- [x] implement cross-content search endpoint
- [x] extend Web search page for all implemented learning content while preserving word-search behavior
- [x] support filters by CEFR/content type/category/topic in the API
- [x] add query validation for empty, too-short, too-long, and unsupported result-type queries
- [x] add PostgreSQL trigram/filter index migration for production bulk-content readiness
- [x] apply PostgreSQL trigram/filter indexes during shared database startup for existing search tables
- [x] rate-limit the public search endpoint
- [x] add initial tests for query validation, result projection, route hardening, and search-index migration coverage
- [ ] add seeded repository, WebApi, Web rendering, and ranking coverage
- [ ] add seeded performance coverage before bulk content generation starts

### Phase 7.10: Progress And Personalization

- [x] implement cross-content progress model
  - Progress: `UserContentProgress` stores user-specific state by controlled owner type and slug without copying content data.
- [x] track viewed/completed states for implemented content
  - Progress: course lesson detail views record a `viewed` state, and the model supports viewed, in-progress, completed, needs-review, and skipped.
- [x] extend learner dashboard/recent activity
  - Progress: Web recent activity includes a compact Learning Portal progress summary.
- [x] add basic deterministic recommendations
  - Progress: recommendation service suggests next incomplete course lessons and grammar topics without AI ranking.
- [x] add tests for progress state transitions
  - Progress: domain/application tests cover state transitions, owner validation, summaries, and completed-content exclusion.
- [ ] add WebApi endpoint tests for authenticated progress workflows
- [ ] add Web rendering tests for progress indicators
- [ ] add weak-exercise and difficult-word recommendation signals

### Phase 7.11: Admin And Operations

- [x] add admin content overview for new modules
  - Progress: Admin system report now includes Learning Portal counts by type, CEFR, category, register, target skill, course, module, and exam profile.
- [x] add content quality reports
  - Progress: Admin Reports includes Learning Portal quality metrics and issue samples.
- [x] add reports for missing links, unresolved words, missing exercises, missing translations, and unpublished drafts
  - Progress: system report summarizes unresolved linked words, unresolved cross-content references, missing translations, unpublished drafts, grammar topics without exercises, and lessons without exercise sets.
- [x] add seed coverage reports per CEFR/module
  - Progress: coverage tables include CEFR and course-module counts for implemented modules.
- [ ] add import validation summaries per Phase 7 module package after import diagnostics are expanded
- [ ] add full unresolved-link drill-down pages with filters and export

### Phase 7.12: Mobile Parity

- [x] define mobile navigation update after Web sign-off
  - Progress: MAUI shell now aligns the primary learner navigation around Learn, Practice, Speak, Prepare, and Resources.
- [x] update mobile content package export for grammar, expressions, exercises, courses, exam prep, writing templates, and cultural notes
  - Progress: full/all mobile packages carry Phase 7 content arrays and linked references; WebApi also publishes `catalog-module` slices for selective module downloads.
- [x] remove Web local SQLite learning/content startup path
  - Progress: DarwinLingua.Web no longer registers the shared SQLite infrastructure initializer or local `darwin-lingua.web.db` path; Web user state uses PostgreSQL through `WebIdentityDbContext`.
- [x] add first-run mobile module selection
  - Progress: MAUI initializes an empty local schema, lets the learner choose content modules from WebApi package manifests, and keeps packaged seed as a legacy fallback outside the default startup path.
- [x] add module-scoped mobile package apply behavior
  - Progress: `ReplaceMode.Module` refreshes selected module tables while preserving unrelated modules and user state.
- [x] refactor MAUI list/detail pages to match Web content model
  - Progress: mobile has dynamic list/detail surfaces backed by local Catalog query services for the implemented Learning Portal modules, plus unified learning search.
- [ ] add mobile exercise runner where appropriate
- [ ] add mobile progress sync where account support is ready
- [ ] add mobile validation worksheets after Web implementation is stable
  - Note: current work adds structural test coverage; target-device validation worksheets still need explicit Phase 7 checklist rows.

---

## First Implementation Order

Recommended build order:

1. Learning Portal Foundation
2. Grammar Guide
3. Everyday Expressions
4. Exercise Engine
5. Course Lessons
6. Exam Preparation
7. Writing Templates
8. Cultural Notes
9. Unified Search
10. Progress and personalization
11. Mobile parity
12. Content generation for new modules

Content generation for new modules should start only after the module contracts, validation rules, and Web rendering are stable.

Foundation convention: Phase 7 Web navigation must not expose dead routes for modules that do not exist yet. New module links should be added only when the Web route, Web API contract, validation path, and release tests exist.

---

## Release Readiness Rules

A new learning-portal module is not release-ready until:

- domain model is implemented
- EF migration exists
- import validation exists
- Web API list/detail endpoints exist
- Web list/detail pages exist
- admin visibility exists where operationally needed
- user-facing strings are localized
- linked content behavior is tested
- unresolved references fail safely
- tests cover import, query, rendering, and validation
- docs are updated
- mobile parity is explicitly completed or backlog-tracked

Current Web hardening status:

- Web and WebApi builds are the release gate for Phase 7 Web sign-off; MAUI parity remains explicitly post-Web.
- Structural hardening tests cover the Phase 7 learner routes, WebApi route registrations, and English/German localization resource keys.
- Admin reports surface persisted Phase 7 coverage and quality metrics, including unresolved links, missing translations, unpublished drafts, and missing exercise coverage.
- Exercise attempt hardening requires authorization for persisted attempts, keeps anonymous evaluation stateless, validates submitted-answer JSON, and applies endpoint rate limits.
- Exercise runner hardening provides structured inputs for initial supported submission shapes and keeps advanced JSON as a fallback.
- Unified Search hardening adds query constraints, endpoint rate limiting, and PostgreSQL trigram/filter indexes for the bulk-content path; target environments must have `pg_trgm` available or allow startup extension creation.
- Bulk content generation must remain blocked until the release checklist gates in `61-Web-Release-Checklist.md` are reviewed against real validated content packages.

---

## Codex Prompt Plan

After this roadmap is accepted, implementation prompts should be produced in this order:

1. Learning Portal Foundation prompt
2. Grammar Guide implementation prompt
3. Everyday Expressions implementation prompt
4. Exercise Engine implementation prompt
5. Course Lessons implementation prompt
6. Exam Prep implementation prompt
7. Writing Templates implementation prompt
8. Cultural Notes implementation prompt
9. Unified Search implementation prompt
10. Progress/personalization implementation prompt
11. Mobile parity prompt
12. Grammar content generation prompt
13. Expressions content generation prompt
14. Exercise content generation prompt
15. Course lesson content generation prompt
16. Exam prep content generation prompt
17. Writing template content generation prompt
18. Cultural notes content generation prompt

Do not generate bulk content before the corresponding implementation and validation rules exist.
