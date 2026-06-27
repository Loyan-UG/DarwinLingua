# Multi-Target Platform Refactor Plan

## Purpose

This plan turns the multi-target-language architecture into executable refactor work.
The goal-ready roadmap for the next implementation branch is
`artifacts/planning/multi-target-language-expansion-roadmap.md`.

Darwin Lingua is currently a German-first product. The next product shape must support teaching additional target languages such as English, Spanish, French, Italian, and later languages without treating them as translated copies of German. The system must also support multiple country contexts per target language, future helper-language expansion, and language-specific learning needs.

This is a no-debt plan. Because the product is still pre-customer, old routes, names, package roots, or model assumptions should be cleaned up when they would hide missing target-language or country-context scope.

## Locked Decisions

1. German remains the only public active target-learning language until another target language passes a reviewed activation gate.
2. The language being learned, the UI language, and helper/meaning languages are separate concepts.
3. Helper translations explain source content. They do not replace source content and do not create another target-language content stream.
4. `Life in Germany` is the learner-facing label for the German/Germany stream only. The stable platform feature is `Country Guidance`.
5. Country Guidance is scoped by `(targetLearningLanguageCode, countryContextCode)`.
6. Germany is shown inside German learning now. Later German can also show Austria and Switzerland.
7. English can later show United States, United Kingdom, Australia, and other English-speaking country contexts.
8. The same country can appear under multiple target languages only as separate original source streams, such as `de|CH`, `fr|CH`, and `it|CH`.
9. Current helper languages can expand later. For example, German may move from 10 helper languages to 15 without changing German source content identity.
10. Every level selector must show a compact code plus a learner-friendly label and explanation.
11. Every future target language must have its own capability profile before content generation starts.
12. Direct MAUI/mobile work remains deferred. Web and WebApi must remain future-mobile-compatible through clean APIs and contracts.
13. `Life in Germany` must stay visible inside German learning because Germany is a German-speaking country, but no global platform route, package root, admin label, or model should be named as if every country-guidance stream were Germany.
14. Because there are no active customers, clean canonical behavior wins over compatibility. Old public routes and names should be removed or made to fail after the canonical replacement is green.
15. Helper translations may use helper-language-specific examples when that improves comprehension, but they must avoid stereotypes and must never become a replacement source stream.
16. Future target languages must be planned for their own language-specific needs before content generation starts. German assumptions cannot be copied into English, Spanish, French, Italian, RTL languages, or non-Latin-script languages without review.

## Stable Concepts

### Target Learning Language

The language the learner wants to learn.

Examples:

- `de`: German
- `en`: English
- `es`: Spanish
- `fr`: French

Target learning language scopes source content, packages, imports, search, progress, recommendations, user learning preferences, level metadata, exams, writing conventions, Country Guidance, and linked content.

### UI Language

The language of product chrome: navigation, account pages, settings, admin UI, validation messages, and generic interface labels.

Current UI languages remain:

- `en`
- `de`

UI language does not decide what language is being learned.

### Helper Or Meaning Language

The language used to explain target-language content to the learner.

Current helper languages:

- `en`
- `fa`
- `ar`
- `tr`
- `ru`
- `ckb`
- `kmr`
- `pl`
- `ro`
- `sq`

Future helper-language expansion must be handled through centralized reference data, validation, translation coverage, admin diagnostics, RTL rendering checks, and backup inventories.

### Country Context

The country or region whose daily life, institutions, legal/civic basics, exams, social expectations, and practical systems are being taught.

Examples:

- `DE`: Germany
- `AT`: Austria
- `CH`: Switzerland
- `US`: United States
- `GB`: United Kingdom
- `AU`: Australia

Country Guidance source streams:

| Target language | Country context | Source stream |
| --- | --- | --- |
| `de` | `DE` | German-authored Germany content, displayed as Life in Germany |
| `de` | `AT` | German-authored Austria content |
| `de` | `CH` | German-authored Switzerland content |
| `en` | `US` | English-authored United States content |
| `en` | `GB` | English-authored United Kingdom content |
| `en` | `AU` | English-authored Australia content |
| `fr` | `CH` | French-authored Switzerland content |
| `it` | `CH` | Italian-authored Switzerland content |

No stream above is a translated copy of another stream. Helper translations may adapt explanations for learner clarity, but source content remains scoped to its target language and country context.

### Level System

German currently uses CEFR. Future target languages may use CEFR or another system.

Every level needs:

- stable code
- sort order
- localized display name
- learner-friendly label
- learner-friendly explanation
- optional standard mapping

German baseline labels:

| Code | Label | Learner-facing meaning |
| --- | --- | --- |
| `A1` | Einstieg | First contact with greetings, identity, numbers, and simple everyday actions. |
| `A2` | Grundlagen | Everyday independence with appointments, shopping, messages, and common public-service situations. |
| `B1` | Selbststaendig | Independent daily life, connected speaking/writing, bureaucracy, work, school, health, and basic argumentation. |
| `B2` | Kompetent | Confident communication, detailed explanations, opinions, workplace/study tasks, nuanced reading, and longer writing. |
| `C1` | Souveraen | Advanced academic/professional discourse, implicit meaning, structured argumentation, and flexible register. |
| `C2` | Meisterschaft | Near-native precision, style, rhetorical control, complex synthesis, subtle tone, and expert/public discourse. |

These labels must come from reference data, not hard-coded UI copy.

## Target-Language Capability Profile

Before any target language moves from planned to pilot, it needs a reviewed capability profile.

Each profile must define:

- language code, native name, display name, script, direction, font/input needs
- level system and learner-friendly labels
- search normalization, tokenization, casing, diacritics, spelling variants, transliteration, and word-boundary rules
- grammar model assumptions for validation and exercises
- pronunciation, listening, and TTS expectations
- writing conventions, formality, register, dates, addresses, message length, and country-specific formats
- exam ecosystem and provider/country boundaries
- Country Guidance contexts under that target language
- initial content modules and excluded modules
- helper-language launch set and future helper-language expansion plan
- current-source refresh requirements for legal, civic, medical, financial, or exam-adjacent content
- assumptions from German that do not apply to this language

Minimum design risks by language family:

| Target | Risks that must be modeled |
| --- | --- |
| German | cases, gender, separable verbs, verb-final clauses, formal/informal address, compound words, umlaut search, DACH contexts |
| English | spelling variants, contractions, phrasal verbs, article usage, stress/pronunciation irregularity, US/UK/Australia variants |
| Spanish | gender/number agreement, ser/estar, preterite/imperfect, subjunctive, vosotros/ustedes/voseo, accent handling, regional vocabulary |
| French | gender/agreement, liaison, accent handling, formal register, partitive articles, France/Belgium/Switzerland/Canada variants |
| RTL or non-Latin targets | text direction, mixed-script rendering, fonts, keyboard guidance, punctuation placement, transliteration search |

## Refactor Workstreams

### 1. Reference Data And Status

Goal: make target languages, helper languages, countries, and levels first-class reference data.

Required work:

- Target languages have statuses: `planned`, `pilot`, `active`, and optionally `disabled`.
- German is active.
- English can remain pilot only after diagnostics prove isolation.
- Spanish and French remain planned.
- Public learner routes reject planned/pilot targets unless explicitly allowed by an admin/internal preview.
- Helper languages remain separate from target languages.
- Country contexts are linked to target languages through reference data.
- Level metadata is target-language-specific.

Acceptance:

- No route or API silently falls back from an unsupported target language to German.
- Planned languages can be displayed as planned only with explicit unavailable copy.

### 2. Canonical Routes

Goal: learner content URLs carry target-language scope.

Canonical route shape:

- `/learn/{targetLearningLanguageCode}/courses`
- `/learn/{targetLearningLanguageCode}/grammar`
- `/learn/{targetLearningLanguageCode}/expressions`
- `/learn/{targetLearningLanguageCode}/dialogues`
- `/learn/{targetLearningLanguageCode}/talk-topics`
- `/learn/{targetLearningLanguageCode}/exercises`
- `/learn/{targetLearningLanguageCode}/roleplays`
- `/learn/{targetLearningLanguageCode}/exam-prep`
- `/learn/{targetLearningLanguageCode}/writing-templates`
- `/learn/{targetLearningLanguageCode}/country-guidance/{countryContextCode}`
- `/learn/{targetLearningLanguageCode}/conversation-events`
- `/learn/{targetLearningLanguageCode}/organizers`

Rules:

- Old ambiguous top-level learner routes are not final behavior.
- Temporary redirects are allowed only during an active migration slice.
- Route helper code must generate URLs centrally. Razor/controller hard-coding must be minimized.

Acceptance:

- German canonical routes work.
- Old ambiguous routes fail or redirect only when deliberately still inside a migration slice.
- Route tests prove target language is preserved through links, search results, breadcrumbs, activity targets, and admin preview links.

### 3. Content Contracts And Import

Goal: every package is target-language aware.

Required work:

- Every content package includes `targetLearningLanguageCode`.
- Country Guidance packages also include `countryContextCode`.
- Import replacement is scoped by target language and, for Country Guidance, country context.
- Slug uniqueness is scoped correctly.
- Cross-content links carry target-language assumptions.
- Wrong-language source content is rejected through validation where practical.
- Helper translation validation uses centralized helper-language reference data.

Acceptance:

- A German package cannot accidentally replace an English package.
- A `de|DE` Country Guidance package cannot replace `de|AT` or `en|US`.
- Same slug in two target languages is allowed only if every URL/query/admin report disambiguates by target language.

### 4. Repository, API, Search, And Progress Isolation

Goal: every learning query is scoped.

Target-scoped:

- courses, modules, lessons, and activity blocks
- grammar
- expressions
- dialogues
- talk topics
- exercises and exercise sets
- roleplays
- exam prep
- writing templates
- Country Guidance
- conversation starter packs
- event preparation packs
- conversation events and RSVPs
- progress
- recommendations
- recent activity
- Unified Search
- learning-level support

Intentionally global:

- user identity and security
- organizer identity/directory profile
- operational configuration not tied to learning content

Acceptance:

- Wrong-target fixtures prove content does not leak across target languages.
- Search ranking is target-isolated.
- Progress/recommendations/recent activity are target-isolated.
- Admin reports make global exemptions explicit.

### 5. Source And Helper Projection

Goal: source text and helper text never overwrite each other.

Rules:

- Source fields stay in the target learning language.
- Helper fields are separate learner-support fields.
- API details return both where helper text exists.
- Web can show source plus one or two helper languages without losing source.
- RTL helper text renders with correct direction and wraps safely.

Acceptance:

- Detail projections for every module are audited.
- Admin preview can inspect pilot target content with helper text without making it public.

### 6. Country Guidance Refactor

Goal: make Country Guidance a reusable platform module, while displaying Life in Germany only for `de|DE`.

Required work:

- Stable route, package root, JSON root, admin labels, search result type, and report labels use `country-guidance`.
- Display labels can vary by target/country pair:
  - `de|DE`: Life in Germany
  - `de|AT`: Life in Austria
  - `de|CH`: Life in Switzerland
  - `en|US`: Life in the United States
- Legal/civic/current-source content has a refresh gate before generation.
- Country Guidance remains broader than any one official test.

Acceptance:

- `de|DE` continues to appear inside German.
- No global `Life in Germany` model or route remains as final platform identity.
- Country/civic examples can be culturally clarified in helper translations without changing source stream identity.

### 7. Learner UI And Settings

Goal: learners can understand three independent choices.

UI must clearly separate:

- what language I want to learn
- what language the site UI uses
- what language explains meanings/help

Required surfaces:

- onboarding/settings
- learner shell/navigation
- Course level cards
- filters and search
- progress summaries
- partner/event matching levels
- admin previews

Acceptance:

- German remains default.
- Planned languages can be shown disabled/planned only if explicit.
- Level cards show large compact code plus friendly label and explanation.

### 8. Admin Diagnostics And Backup

Goal: make readiness measurable and restoreable.

Admin reports must include:

- counts by target language
- counts by country context for Country Guidance
- missing helper translations by helper language and module
- unresolved links with target/country scope
- cross-target link issues
- malformed JSON
- unpublished drafts
- package receipts by target/country
- pilot/active/planned status visibility

Backups must include:

- PostgreSQL dump and globals
- restore list and dry-run restore evidence
- repo overlay
- secret/local config bundle
- target-language counts
- country-context counts
- helper-language coverage
- checksums and restore steps

Acceptance:

- GitHub code plus backup folder can restore the exact checkpoint.
- Backup manifests prove target/country/helper state.

## Implementation Order

### Stage 0 - Freeze Decisions

Status: complete for the current architecture checkpoint. The detailed
branch-ready roadmap is
`artifacts/planning/multi-target-language-expansion-roadmap.md`.

Deliverables:

- Architecture document updated.
- This refactor plan added.
- Backlog updated with ordered implementation work.

### Stage 1 - German Baseline Target Scope

Goal: make German prove the target-language architecture without changing the public product.

Work:

- Finish target-scope audit across routes, APIs, repos, search, progress, recommendations, imports, admin reports, and backups.
- Close remaining hidden German fallback behavior.
- Verify all current German learner routes work under canonical target-language URLs.
- Verify level metadata appears on all level-selection surfaces.
- Verify helper-language rendering and admin coverage.

Exit:

- German Web is stable.
- No unsupported target silently returns German content.
- Restore-ready backup exists.

### Stage 2 - English Pilot Review, Not Public Activation

Goal: use English pilot data to prove isolation.

Work:

- Review English capability profile.
- Verify English pilot content is native English source content, not translated German.
- Keep public English hidden unless activation is explicitly approved.
- Verify admin-only preview/search/import/progress diagnostics.

Exit:

- English remains pilot or moves to active only by explicit decision.

### Stage 3 - First Activation Decision

Options:

| Option | Meaning | Recommended when |
| --- | --- | --- |
| A | Expand English depth first | English pilot proves value and tester demand supports it |
| B | Add German Austria/Switzerland Country Guidance | German learners need DACH integration content before English depth |
| C | Start Spanish pilot | Spanish demand is stronger and capability profile is approved |
| D | Start French pilot | French demand or Switzerland/France country guidance is strategically urgent |

Current recommendation:

Finish German baseline target-scope readiness and English pilot diagnostics first. After that, choose between English depth and German DACH Country Guidance based on product priority and tester feedback.

Decision rule:

- If the next goal is platform/business validation, continue English depth in
  small native batches while English stays non-public.
- If the next goal is improving the current German learner experience, add
  German-authored Austria/Switzerland Country Guidance planning and only then a
  small country-context pilot.
- Do not start Spanish or French content generation from a general idea alone.
  Each needs a dedicated goal that approves country/variant strategy,
  language-specific risks, level labels, search behavior, writing conventions,
  and Country Guidance scope.

### Stage 4 - Repeatable Target-Language Launch Pattern

For every new target:

1. Capability profile.
2. Level metadata.
3. Country-context plan.
4. Small native pilot.
5. Helper translation coverage.
6. Import and validation.
7. Search/progress/admin isolation.
8. Web/API smoke.
9. Backup.
10. Activation decision.

## Non-Goals

- Do not generate broad English, Spanish, French, or other target-language content before the readiness gate closes.
- Do not duplicate German content as source content for another target language.
- Do not keep permanent compatibility routes that hide missing scope.
- Do not make Country Guidance only an exam-question trainer.
- Do not resume direct MAUI/mobile work in this phase.

## Completion Criteria

This plan is complete when:

- German remains active and stable under target-scoped architecture.
- Country Guidance supports multiple target/country streams without schema redesign.
- Level metadata is learner-friendly and target-language-specific.
- Helper-language expansion is proven independent from target-language activation.
- Search, progress, recommendations, recent activity, imports, links, admin reports, and backups are target-scoped.
- At least one non-German pilot is reviewed and either kept pilot or explicitly activated.
- A restore-ready backup closes the checkpoint.
