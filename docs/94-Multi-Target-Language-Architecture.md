# Multi-Target Language Architecture

## Purpose

This document defines the architecture for evolving Darwin Lingua from a German-first learning portal into a platform that can teach multiple target languages without mixing content, progress, routes, search, or helper translations.

The current product remains German by default. This architecture prepares the system for future target languages such as English, Spanish, French, and additional languages without treating those languages as translated copies of the German content.

## Stable Terms

### Target Learning Language

`targetLearningLanguage` is the language the learner wants to learn.

Examples:

- `de` for German
- `en` for English
- `es` for Spanish
- `fr` for French

Target learning language controls the source language of authored learning content, vocabulary entries, grammar topics, exercises, courses, exam preparation, writing templates, dialogues, roleplays, search results, progress, and learning recommendations.

Current default:

- `de`

### UI Language

`uiLanguage` is the language of product chrome, navigation, account pages, settings, admin UI, validation messages, and generic interface labels.

Current active UI languages:

- `en`
- `de`

UI language must remain independent from the target learning language. A learner may use the English UI while learning German, Spanish, or another target language.

### Meaning Language

`meaningLanguage` is the learner's helper or explanation language.

Examples:

- `fa`
- `en`
- `ar`
- `tr`

Meaning languages explain target-language content. They do not replace the target-language source text.

Current active helper languages:

- `ar`
- `ckb`
- `en`
- `fa`
- `kmr`
- `pl`
- `ro`
- `ru`
- `sq`
- `tr`

The meaning-language set can grow independently from the target-learning-language set. For example, German may later be learnable with 15 helper languages while English may begin with a smaller reviewed helper-language set.

### Country Context

`countryContext` identifies the country or region whose daily life, civic orientation, legal basics, culture, institutions, exams, or social expectations are being taught.

Examples:

- `DE` for Germany
- `AT` for Austria
- `CH` for Switzerland
- `US` for United States
- `GB` for United Kingdom
- `AU` for Australia

Country context is separate from target learning language, but it is normally displayed within a target-language learning experience.

Examples:

- German target language can show Germany, Austria, and Switzerland guidance.
- English target language can show United States, United Kingdom, and Australia guidance.
- Switzerland can appear under German, French, and Italian when those target languages are active, with separate original content for each target language.

The public feature name may vary by target language and country. For German learners, the Germany context can be presented as `Life in Germany`. The stable product category is `Country Guidance`, because the same model must later support `Life in Austria`, `Life in Switzerland`, `Life in the United States`, and similar country-specific guidance.

### Level System

`levelSystem` defines the learning-level framework used by a target language.

The active German baseline uses CEFR:

- `A1`
- `A2`
- `B1`
- `B2`
- `C1`
- `C2`

Every level must have:

- stable code
- sort order
- localized display title
- learner-friendly label
- short explanation for learners who do not know the standard framework
- optional standard mapping such as CEFR

The UI should not assume every future target language uses CEFR exactly.

For the current German CEFR baseline, the learner-facing level metadata should use this pattern. The exact localized display text can be refined by UI copy review, but the data model and API must support all columns:

| Code | German baseline friendly label | Learner explanation purpose |
| --- | --- | --- |
| `A1` | Einstieg | First contact with the language: greetings, simple personal information, numbers, basic everyday actions. |
| `A2` | Grundlagen | Everyday independence: appointments, shopping, simple messages, basic past events, and common public-service situations. |
| `B1` | Selbststaendig | Independent daily life: connected speaking/writing, practical bureaucracy, work, school, health, and basic argumentation. |
| `B2` | Kompetent | Confident communication: detailed explanations, opinions, workplace/study tasks, nuanced reading, and longer writing. |
| `C1` | Souveraen | Advanced control: academic/professional discourse, implicit meaning, structured argumentation, and flexible register. |
| `C2` | Meisterschaft | Near-native precision: style, rhetorical control, complex synthesis, subtle tone, and expert/public discourse. |

These labels are not only Course-card decoration. They must be available anywhere a learner chooses a level: onboarding, settings, Course browsing, search filters, progress summaries, event/profile matching, and future package manifests.

### Target Language Capability Profile

Each target learning language needs a capability profile before it can become active. The profile describes language-specific behavior that must not be hard-coded from German assumptions.

The profile must define at least:

- writing system and text direction
- level system and learner-friendly level labels
- search normalization rules such as accents, casing, umlauts, apostrophes, or alternative spellings
- morphology and grammar assumptions that affect exercises and validation
- punctuation, quotation, capitalization, and sentence-boundary rules
- keyboard/input guidance for learners
- text-to-speech and pronunciation support expectations
- country contexts that can appear under this target language
- exam-prep providers and countries, if relevant
- writing-template conventions such as formality, letter/email structure, and expected length
- content-type availability for the first public pilot

German is the first active profile. Future English, Spanish, French, and other profiles must be planned with their own needs instead of inheriting German grammar, exams, country guidance, or writing conventions.

## Current Product Decisions

These decisions are locked for the Phase 8 refactor:

1. German remains the active/default target-learning-language until another target language has its own reviewed native content.
2. The product is still pre-customer, so clean route, schema, model, package, and admin changes are preferred over preserving legacy behavior that would create long-term ambiguity.
3. Old top-level learner routes may be removed after the canonical target-language routes are green. Permanent compatibility routes are not required for the current development stage.
4. `Life in Germany` is not the global module concept. It is the German/Germany-facing label for the broader `Country Guidance` model.
5. Country guidance belongs to a target-language and country-context pair. Germany under German, Switzerland under German, Switzerland under French, and Switzerland under Italian are separate source-content streams.
6. Helper translations remain learner support. Adding more helper languages later, such as expanding German from ten helper languages to fifteen, must not require redesigning target-language content.
7. Every learner-facing level selection must show both the compact level code and a plain learner-friendly label/description, because not every learner understands CEFR codes.
8. Every future target language must be planned as a native learning product with its own grammar sequence, examples, cultural contexts, writing conventions, exams, pronunciation expectations, and search behavior.
9. Country Guidance is displayed inside the selected target-learning-language when the country belongs to that learning context. German shows Germany now and can later show Austria and Switzerland. English can later show United States, United Kingdom, Australia, and other contexts. Switzerland may appear under German, French, and Italian as separate original source-content streams.
10. If preserving an old route, JSON root, admin label, or model name would hide missing target-language or country-context scope, it must be removed during this development phase instead of carried as compatibility debt.
11. Country Guidance is not only exam preparation. It may cover official orientation-course and Leben-in-Deutschland concepts, but its broader purpose is to help immigrants understand everyday life, civic expectations, institutions, public processes, communication norms, and integration in the selected country context.
12. Every target-language/country-context pair is a separate authored source stream. For example, Switzerland under German, French, and Italian is three separate source-content sets, each with its own helper translations.
13. Learner-facing level labels are part of the product model, not decoration. A level code such as `A1` or `B2` must always be paired with a plain learner-friendly label and explanation in level-selection surfaces.
14. Current German content remains the only active target-language content until the architecture has completed regression, isolation tests, admin diagnostics, and a restore-ready backup. Planned target languages may be visible as disabled/planned options only when the empty state is explicit.
15. The target-language architecture must support future helper-language growth without schema redesign. A target language may have one reviewed helper-language set at launch and a larger set later.
16. Country Guidance examples may be adapted inside helper translations when a cultural comparison needs a learner-specific explanation, but that does not change the source-content stream. The target-language source remains authored for the selected target language and country context.
17. A new target language is not activated by adding a route or a label. It is activated only after its capability profile, level labels, native content pilot, helper translations, import validation, search isolation, progress isolation, admin diagnostics, and backup gate are complete.

## Decision Addendum: Expansion-Ready Learning Languages

The current German Web baseline must be treated as the first target-language implementation of a broader platform.

The next architecture work should not add English, Spanish, French, or other language content yet. It should first make the German system prove that:

- target learning language is explicit in user preferences, routes, content queries, progress, search, recommendations, admin reports, and package identity
- UI language and helper/meaning languages stay independent from the language being learned
- helper-language expansion, such as adding five more helper languages for German later, does not require changing the target-language model
- Country Guidance can represent multiple country contexts for one target language and one country context under multiple target languages
- content source fields, examples, grammar assumptions, writing conventions, exercise validation, search behavior, and country guidance can vary by target language
- clean canonical routes and model names are preferred over compatibility shims because the product is still pre-customer

This means `de|DE` is only the first Country Guidance stream. Later examples include:

| Target language | Country context | Source-content stream |
| --- | --- | --- |
| `de` | `DE` | German-authored Germany guidance, currently displayed as Life in Germany |
| `de` | `AT` | German-authored Austria guidance |
| `de` | `CH` | German-authored Switzerland guidance |
| `en` | `US` | English-authored United States guidance |
| `en` | `GB` | English-authored United Kingdom guidance |
| `en` | `AU` | English-authored Australia guidance |
| `fr` | `CH` | French-authored Switzerland guidance |
| `it` | `CH` | Italian-authored Switzerland guidance |

Helper translations can explain each stream in the learner's selected helper languages, but helper translations never turn one source stream into another target-language stream.

## Target-Language Support Matrix

This matrix is the planning baseline. A target language can move from `planned` to `active` only after its capability profile, schema/query isolation, route behavior, content contracts, smoke tests, and first reviewed content pilot are complete.

| Target language | Status | Level system | Default country contexts | Initial source-content rule | Notes |
| --- | --- | --- | --- | --- | --- |
| German (`de`) | Active baseline | CEFR A1-C2 | Germany (`DE`), later Austria (`AT`) and Switzerland (`CH`) | German-authored content | Current Web content remains German by default. `Life in Germany` appears inside German learning because Germany is a German-speaking country. |
| English (`en`) | Planned first non-German pilot | CEFR initially, with room for business/academic tracks | United States (`US`), United Kingdom (`GB`), Australia (`AU`) | English-authored content | Do not translate German lessons. English needs its own grammar sequence, pronunciation expectations, writing conventions, exams, and country guidance. |
| Spanish (`es`) | Planned later | CEFR initially | Spain (`ES`), later Latin American country contexts where useful | Spanish-authored content | Must handle regional vocabulary/register differences and accent/diacritic search normalization. |
| French (`fr`) | Planned later | CEFR initially | France (`FR`), later Switzerland (`CH`), Belgium (`BE`), Canada (`CA`) where useful | French-authored content | Switzerland under French is separate original French-target content, not a translation of German/Switzerland content. |
| Italian (`it`) | Future candidate | CEFR initially | Italy (`IT`), Switzerland (`CH`) where useful | Italian-authored content | Switzerland under Italian is separate original Italian-target content if Italian becomes active. |

Helper/meaning languages are not listed as target languages unless they are also activated as languages that can be learned. The active helper-language set can grow independently. For example, German can later be explained in 15 helper languages while English starts with fewer reviewed helper languages.

## Target Language Capability Checklist

Before a target language can move from planned to active, create a capability profile that answers these questions:

- Source language and script: language code, native name, writing direction, alphabet/script variants, punctuation conventions, capitalization behavior, and keyboard/input guidance.
- Level system: CEFR or other framework, learner-friendly labels, placement logic, and any target-language-specific level exceptions.
- Search behavior: accent/diacritic handling, case folding, alternate spellings, compound words, apostrophes, hyphenation, transliteration, and word-boundary rules.
- Grammar model: morphology, conjugation/declension assumptions, gender/case behavior, tense/aspect system, word order, particles, and what answer validation must understand.
- Pronunciation/audio: TTS availability, pronunciation notes, phoneme or spelling-to-sound issues, and whether listening content needs special metadata.
- Writing conventions: email/letter/message structure, formality, register, address formats, date/time formats, expected text length per level, and country-specific variants.
- Country contexts: countries or regions shown under this target language, including whether the same country appears under another target language with separate source content.
- Exam ecosystem: providers, countries, sections, levels, task types, and whether exam prep should be active in the first pilot.
- Content availability: first-pilot modules, minimum counts, linked practice expectations, and modules intentionally excluded from the first pilot.
- Safety/legal refresh needs: legal-adjacent country guidance, official exam names, citizenship/residence topics, public-service processes, and other unstable facts that require source refresh before generation.

The profile is not optional planning prose. It is the activation gate for a new target-learning-language. If a future language has a special writing system, country variants, morphology, placement model, exam ecosystem, or input problem, the system must model that before content generation starts.

Language-specific requirements to check before each activation:

- German: cases, gender, separable verbs, verb-final clauses, formal/informal address, compound words, umlaut search normalization, DACH country contexts, and Goethe/telc/TestDaF/DSH/OeSD exam ecosystem.
- English: spelling variants, contractions, phrasal verbs, article usage, stress/pronunciation irregularity, US/UK/Australia country contexts, academic/business tracks, and country-specific writing conventions.
- Spanish: gender/number agreement, ser/estar, preterite/imperfect, subjunctive sequencing, voseo/ustedes/regional variants, accent search normalization, Spain and later Latin American country contexts.
- French: gender/agreement, liaison/pronunciation issues, formal register, partitive articles, accent handling, France/Belgium/Switzerland/Canada contexts, and country-specific administrative vocabulary.
- Languages with non-Latin or right-to-left scripts: script direction, mixed-script rendering, transliteration search, keyboard guidance, font coverage, punctuation placement, and helper-language display direction must be tested before activation.

## Target Language Activation Gate

Before any target language changes from `planned` to `active`, the following gate must pass:

1. Reference data exists for the target language.
   - Language code, native/display names, text direction, status, default level system, available country contexts, and default helper-language set are defined.
   - The language can be shown as planned/inactive without leaking German content into its routes.
2. Level metadata is complete.
   - Every level has a compact code, learner-friendly label, explanation, sort order, and optional standard mapping.
   - For German CEFR, labels such as `A1 Einstieg`, `A2 Grundlagen`, and `B1 Selbststaendig` are examples of the required pattern, not hard-coded UI decoration.
3. Source-content rules are native to the target language.
   - Grammar order, vocabulary scope, expressions, writing templates, exercises, course flow, exams, pronunciation expectations, and examples are planned for the target language itself.
   - German content is not translated into the new target language as source content.
4. Country Guidance is explicitly scoped.
   - Each country context has a target-language/country-context pair, for example `de|DE`, `de|AT`, `de|CH`, `en|US`, `en|GB`, or `fr|CH`.
   - The same country under different target languages is separate authored content.
5. Helper translations are complete for the selected helper-language launch set.
   - Helper languages explain the source content; they do not replace it.
   - Right-to-left helper languages must render with correct direction in all helper-text surfaces.
6. Data isolation is proven.
   - Routes, API endpoints, repository queries, Unified Search, progress records, recommendations, imports, admin reports, and unresolved-link diagnostics include target-language scope.
   - Same slugs in different target-language namespaces do not collide.
7. Operational evidence exists.
   - Targeted tests pass.
   - Web/API smoke proves the target language routes work and inactive/unsupported target languages do not silently fall back to German.
   - A restore-ready backup records counts by target language, level system, country context, module, and helper-language coverage.

If any gate fails, the language remains `planned`. It can be visible as a disabled/planned option only when the UI clearly says that the language is not available yet.

## German Level Label Baseline

German uses CEFR, but the UI must not rely on learners already understanding CEFR codes. Every level card should show the compact code prominently and pair it with a learner-friendly label and description.

Baseline German labels:

| Code | Learner-friendly label | Practical meaning |
| --- | --- | --- |
| A1 | Einstieg | First contact with German, very short everyday language, greetings, names, numbers, and simple needs. |
| A2 | Grundlagen | More stable everyday communication, appointments, shopping, family, routine, and short messages. |
| B1 | Selbststaendig | Independent everyday use, work, authorities, opinions, problems, and longer practical communication. |
| B2 | Kompetent | Confident communication in work, study, public services, arguments, nuanced reading, and longer writing. |
| C1 | Souveraen | Advanced control in academic/professional contexts, implicit meaning, structured argumentation, and flexible register. |
| C2 | Meisterschaft | Near-native precision with style, rhetoric, synthesis, subtle tone, and demanding expert/public discourse. |

## Level Metadata Contract

Level metadata must be target-language and level-system aware.

Every level definition needs:

- `levelSystemCode`, for example `cefr`
- `targetLearningLanguageCode` or an explicit shared-system marker
- compact learner code, for example `A1`
- learner-friendly label, for example `Einstieg`
- short explanation for learners unfamiliar with the level system
- sort order
- localized UI labels where the UI chrome needs localization
- optional standard mapping, for example CEFR

German can currently use the baseline labels above. Future target languages may reuse CEFR codes, but they still need their own reviewed labels and explanations if the learning path, examples, literacy needs, or local exam ecosystem differs. The UI must read level labels from reference data and must not hard-code German labels into course cards, profile forms, filters, or onboarding.

## Product Rules

1. The German baseline remains the default target-learning-language experience until another language has reviewed, imported, and smoke-tested content.
2. Existing German content must be backfilled as `targetLearningLanguageCode = de`.
3. New target-language content must be authored natively for that target language and its learning needs. It must not be a bulk translation of German packages.
4. Helper translations remain required support content where the relevant contract requires them.
5. A content item belongs to exactly one target learning language unless a future contract explicitly defines a safe shared-content type.
6. Slugs are unique inside a target-language namespace, not globally across every language.
7. Progress, search, recommendations, admin reports, and unresolved-link diagnostics must always include target language in their logic.
8. Country guidance must include country context and target learning language.
9. The product is still pre-customer; clean target-language-aware route and schema changes are preferred over preserving old route compatibility that creates technical debt.
10. Old public route compatibility is not a product requirement during this refactor. Temporary redirects are allowed only as short-lived development aids and must not hide missing target-language context in tests.
11. A future target language cannot be activated only by translating German content. It needs a reviewed capability profile, native source content, helper translations, import validation, search isolation, progress isolation, and admin diagnostics.

## Canonical Route Direction

Future learner routes should make target language explicit.

Recommended shape:

- `/learn/de/courses`
- `/learn/de/courses/{courseSlug}`
- `/learn/de/courses/{courseSlug}/{lessonSlug}`
- `/learn/de/grammar`
- `/learn/de/expressions`
- `/learn/de/exercises`
- `/learn/de/exam-prep`
- `/learn/de/writing-templates`
- `/learn/de/conversation-events`
- `/learn/de/organizers`
- `/learn/de/country-guidance/de`
- `/learn/de/country-guidance/at`
- `/learn/de/country-guidance/ch`

The exact route set can be refined during implementation, but route helpers must not build links that silently cross target-language boundaries.

Current top-level routes such as `/courses`, `/exam-prep`, `/writing-templates`, and `/life-in-germany` should be treated as legacy development routes once canonical `/learn/{targetLanguageCode}/...` routes are implemented. Because the product has no active public customers, the final Web model should prefer explicit target-language routes instead of retaining ambiguous top-level routes indefinitely.

## Content Partitioning

Every content root that participates in learning must be target-language scoped.

Initial scope:

- Word entries
- Grammar topics
- Expressions
- Dialogue lessons
- Talk Topics
- Exercises
- Exercise sets
- Roleplay scenarios
- Course paths
- Course modules
- Course lessons
- Exam profiles
- Exam prep units
- Writing templates
- Life/Country guidance notes
- Conversation starter packs
- Event preparation packs
- Package metadata

Conversation events are language-practice inventory, so they are target-language scoped. Event RSVPs are also target-language scoped because the same future event slug may exist under different learning languages.

Organizer profiles are different: the organizer identity/directory record is global and should not be duplicated per target language. The target-specific parts are the organizer's supported learning levels and active event views. This lets one organizer later support multiple learning languages without creating parallel identity records.

Vocabulary note: `WordEntry.LanguageCode` is the vocabulary target-learning-language code for lexical entries. It should be treated as the target-language scope for words instead of adding a duplicate `TargetLearningLanguageCode` column to `WordEntries`.

Non-vocabulary learning roots must use a concrete `TargetLearningLanguageCode` column with default `de`. Slug uniqueness must move from global slug uniqueness to target-language-scoped uniqueness, for example `(TargetLearningLanguageCode, Slug)`.

## Package Contract Rules

Every future content package must declare:

- `targetLearningLanguageCode`
- `defaultMeaningLanguages` or its renamed helper-language equivalent
- `levelSystemCode` when levels are used
- `countryContextCode` when the package teaches country guidance

Package import must reject:

- missing target learning language
- unsupported or inactive target learning language unless explicitly staging
- helper language codes that are not active helper languages
- source fields that are clearly authored in the wrong target language
- links that cross target-language boundaries without an explicit cross-language rule

## Module Rules

### Vocabulary

Vocabulary entries are target-language source entries. Meanings are helper-language explanations.

### Grammar

Grammar taxonomies, section types, examples, and validation must be target-language aware because grammar systems differ.

### Expressions

Expressions are native pragmatic language content. German idioms must not be translated into English, Spanish, or French expression packages as if they were equivalent content.

### Exercises

Exercise types can be shared, but answer validation, morphology assumptions, casing, punctuation rules, and grammar-specific feedback must be target-language aware.

### Courses

Course paths, modules, lessons, activity blocks, linked targets, and progress must be target-language scoped. The level-system abstraction controls browsing and sequencing.

### Exam Prep

Exam profiles are target-language, country, and provider scoped. Goethe/telc German content must not appear under English or Spanish.

### Writing Templates

Writing conventions, register, template structure, sample filled versions, and expected length vary by target language and country context.

### Life And Country Guidance

The current Life in Germany surface should evolve into target-language-aware country guidance. German target language initially shows Germany content, and later Austria and Switzerland. English can later show United States, United Kingdom, Australia, and other contexts. Switzerland may have separate original content under German, French, and Italian.

Country guidance content must be authored for the target language and country combination. Switzerland under German, French, and Italian is three separate original content streams, not one shared country note with translated source text. Helper translations remain layered on top of each source stream for learner support.

The current Germany content should migrate from the public feature concept `Life in Germany` into the stable model `CountryGuidance` with:

- `targetLearningLanguageCode = de`
- `countryContextCode = DE`
- German source content
- helper translations in active meaning languages
- links only to German target-language learning content

Because the product has no active public customers yet, this migration should be clean rather than compatibility-heavy:

- final learner routes should use Country Guidance under the target-language route context, for example `/learn/de/country-guidance/de`
- `Life in Germany` should remain a German/Germany display label, not the permanent global module name
- old `CulturalNote` public naming should not be preserved indefinitely if it creates model, route, package, or admin ambiguity
- package roots should use `country-guidance` wording and the JSON root array `countryGuidanceNotes`
- admin labels should show the stable model plus the selected country context, for example `Country Guidance - Germany`
- tests should fail loudly when an old route, old package root, or missing country context is still being used after the migration slice is complete
- database backup and restore manifests must report Country Guidance counts by `targetLearningLanguageCode` and `countryContextCode`

Country Guidance content scope:

- It is broader than official exam preparation. For German/Germany, it can help learners understand Orientierungskurs, Leben in Deutschland, civic institutions, everyday law, bureaucracy, social norms, school/work systems, housing, healthcare, and integration topics.
- It must not become only a question-bank trainer. Official or quasi-official fixed questions may be linked or referenced only when legally safe and intentionally modeled. The product-owned content should teach the underlying concepts clearly in the learner's helper language.
- It can contain different examples per helper language when cultural comparison benefits comprehension, but the target-language source stream remains tied to the selected target language and country context.
- Legal-adjacent or official-process content must carry refresh discipline: content generation should verify current source facts before publishing when facts are likely to change.

Implementation progress as of 2026-06-24: Country Guidance now has persisted `countryContextCode` scope, canonical learner/API/admin routes, target/country-scoped repository projection, canonical Unified Search result type `country-guidance`, Course activity target support for `country-guidance`, source packages under `content/learning-portal/country-guidance/packages`, and the JSON root array `countryGuidanceNotes`. The internal model/table rename is complete. The Web shell now exposes a target-learning-language switcher that keeps German active and shows English, Spanish, and French as planned inactive targets, separate from UI language and helper translations. Conversation Starter Packs and Event Preparation Packs now persist `TargetLearningLanguageCode`, use target-scoped indexes, import/replace by package target language, and resolve Web/API list/detail/related lookups by target language. Conversation Events and Event RSVPs now persist `TargetLearningLanguageCode`, use target-aware indexes, and resolve public/admin Web/API calls by target language. Conversation Events and Organizer Profiles now use canonical learner routes under `/learn/{targetLearningLanguageCode}/...`; admin pages expose an explicit active target-language scope selector for operator-managed community resources. Organizer profiles remain global identity records, while supported levels and active event views are target-scoped. Package manifests and package receipt history now carry explicit target-language, level-system, and country-context metadata where applicable. The German baseline regression and restore-ready backup gate passed, and the first English target-language capability profile/pilot plan exists; English remains inactive until a separate reviewed implementation goal starts.

## No-Debt Implementation Plan From Current State

The next execution goal should close Phase 8 before any non-German content generation. The work must stay Web-first and avoid direct MAUI/mobile UX changes.

1. Close the current German target-language UX cleanup.
   - Finish any remaining learner-facing labels that still say "German level" where they should say target-language learning level.
   - Update every level picker and level card to show compact code, friendly label, and short explanation from level-system reference data.
   - Keep target-learning-language, UI language, helper languages, and country context visually separate.
2. Finish clean canonical route and link behavior.
   - Use `/learn/{targetLearningLanguageCode}/...` for learner routes.
   - Consolidate route/link helpers so breadcrumbs, search links, activity links, admin preview links, and generated URLs always carry target-learning-language context.
   - Remove or fail old ambiguous learner routes when the canonical route has replaced them. Do not carry permanent compatibility routes that hide missing target-language scope.
3. Finish API contract cleanup.
   - Make target-learning-language an explicit parameter in public endpoints and client calls where the user-facing route has a target language.
   - Reject unsupported and inactive target languages consistently.
   - Use stored authenticated target-language preference only when no explicit route/request override exists.
4. Finish Country Guidance expansion readiness.
   - Verify the current `de|DE` stream is correctly displayed as Life in Germany inside German.
   - Ensure route, package, import, search, activity, admin, and backup behavior can add `de|AT`, `de|CH`, `en|US`, `en|GB`, `en|AU`, and future `fr|CH` or `it|CH` streams without schema redesign.
   - Ensure official-process and legal-adjacent Country Guidance content has a refresh discipline before future generation.
5. Finish diagnostics and data isolation.
   - Admin reports must show counts and issues by target language, country context, level system, module, and helper-language coverage.
   - Add wrong-language isolation fixtures before activating the first non-German target language.
   - Add diagnostics for cross-language links and same-slug collisions inside and across target-language namespaces.
6. Close German baseline regression.
   - Verify German Web browsing, API detail, helper translations, search, progress, recommendations, admin reports, imports, and Country Guidance under `/learn/de/...`.
   - Verify unsupported target routes/API calls do not show German content.
7. Create a restore-ready backup.
   - Backup manifest must include target-language counts, country-context counts, helper-language coverage, current git state, PostgreSQL dumps, restore list, and dry-run restore evidence.
8. Plan the first non-German pilot.
   - Recommendation remains English first, but content generation starts only after the capability profile and the Phase 8 regression/backup gate are complete.
   - The pilot plan must include a target-language capability profile, country-context plan, level labels, module list, and helper-language coverage plan before any content package is generated.

## Next Execution Plan For Multi-Target Expansion

The next goal should not start broad English, Spanish, or French content generation immediately. It should close the remaining platform readiness gaps that determine whether adding another target language will be clean.

Recommended order:

1. Finish admin diagnostics and report coverage.
   - Show counts by target language, level system, country context, module, and helper language.
   - Add duplicate-slug diagnostics inside each target-language namespace and cross-namespace review diagnostics.
   - Add wrong-target-language unresolved-link diagnostics.
2. Finalize generated/package manifest contracts.
   - Every generated or mobile-facing package manifest must include `targetLearningLanguageCode`, level-system metadata where relevant, and country context where relevant.
   - Direct MAUI/mobile UX work remains deferred, but the exported data contract must not be German-only.
3. Add activation-gate tests.
   - Tests should prove inactive target languages do not display German content.
   - Tests should prove German `de|DE` Country Guidance is visible under German and not as a global module.
   - Tests should prove same-country/multiple-target design can be represented without schema changes.
4. Prepare the first English pilot only after the gate is green.
   - Use the existing English capability profile plan as the starting point.
   - Keep the first pilot small and native: Course, Grammar, Expressions, Exercises, Writing Templates, and one minimal Country Guidance stream such as `en|US`.
   - Do not add English bulk content before the pilot is imported, smoke-tested, reviewed, and backed up.

## Testing Gates

Phase 8 is not complete until tests prove:

- all existing German content is backfilled to `de`
- German browsing still works after the migration
- content for one target language does not appear under another target language
- route helpers preserve target language
- the learner shell shows the active target language separately from UI/helper-language preferences
- package import requires target language
- Unified Search filters by target language
- progress is partitioned by target language
- admin reports show counts and issues by target language
- country guidance is partitioned by target language and country context
- conversation starter packs and event preparation packs are partitioned by target language
- conversation events and RSVPs are partitioned by target language, while global organizer identity data is explicitly documented as an exemption

## First Non-German Pilot Rule

After the architecture is green, the recommended first non-German pilot is English. The pilot must be small, reviewed, and native to English-learning needs. It should not start before the German baseline works under the new target-language architecture.

