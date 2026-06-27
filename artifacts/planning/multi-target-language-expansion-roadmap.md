# Multi-Target Language Expansion Roadmap

Status: readiness checkpoint closed 2026-06-24; future expansion branches remain
product decisions.

## Purpose

This roadmap turns Darwin Lingua from a German-first Web product into a
multi-target language-learning platform. German remains the only public active
target language for now, but the platform must be ready to add English,
Spanish, French, Italian, and later languages without copying German content,
without hidden German fallbacks, and without preserving ambiguous legacy routes.

The product is still pre-customer. That means the correct implementation style
is clean migration, not compatibility-heavy behavior. If an old route, package
root, model name, or UI label hides missing target-language or country-context
scope, the old behavior should be removed or made to fail during development.

## Locked Product Decisions

1. Target learning language, UI language, helper/meaning language, level system,
   and country context are separate concepts.
2. German remains public active until another target language passes an
   explicit activation gate.
3. English is pilot/staging only. Spanish and French remain planned until their
   own goals approve content generation.
4. `Life in Germany` is a display label for the German/Germany stream. The
   stable module is `Country Guidance`.
5. Country Guidance source content is scoped by
   `(targetLearningLanguageCode, countryContextCode)`.
6. German can show Germany, Austria, and Switzerland as separate German-authored
   streams: `de|DE`, `de|AT`, `de|CH`.
7. English can show United States, United Kingdom, Australia, and other
   English-speaking contexts as separate English-authored streams.
8. The same country can appear under multiple target languages only as separate
   original source streams. For example, Switzerland under German, French, and
   Italian is `de|CH`, `fr|CH`, and `it|CH`, not one shared translated note.
9. Helper translations stay as learner support. They can use culturally clearer
   examples, but they do not replace source content and do not create a new
   target-language stream.
10. Helper-language expansion is independent. German may later grow from the
    current helper-language set to a larger set without changing German source
    identity.
11. Every level selector must show a compact code plus a learner-friendly label
    and explanation, because many learners do not understand CEFR labels alone.
12. Every future target language needs a capability profile before content
    generation starts.
13. Direct MAUI/mobile work is deferred. Web and WebApi must remain clean and
    future-mobile-compatible through contracts and APIs.

## Stable Domain Model

### Target Learning Language

The language being learned. It scopes source content, imports, search,
progress, recommendations, level metadata, exams, country guidance, and links.

Examples:

- `de`: German
- `en`: English
- `es`: Spanish
- `fr`: French

### UI Language

The language of product chrome. Current UI languages remain `en` and `de`.
UI language must not decide source-content language.

### Helper Language

The language used to explain source content to the learner. Helper languages
are translation/support coverage, not target-language activation.

### Country Context

The country or region whose everyday systems, public institutions, social
norms, legal/civic basics, exams, and practical life are being taught.

Examples:

- `DE`: Germany
- `AT`: Austria
- `CH`: Switzerland
- `US`: United States
- `GB`: United Kingdom
- `AU`: Australia

## Country Guidance Rules

Country Guidance must remain broader than an exam-prep module. For German and
Germany it should help learners understand Germany, German social expectations,
public systems, civic knowledge, rights, duties, institutions, and everyday
integration. It may help with Orientierungskurs, Test Leben in Deutschland, and
citizenship-related knowledge, but it must not become only a fixed-question
trainer.

For every future country stream:

- source content is authored in the target language for that country context
- helper translations explain that stream
- official, legal-adjacent, civic, residence, healthcare, education, work,
  tax, and exam-adjacent facts need a current-source refresh before publishing
- examples may be adapted per helper language only to improve comprehension,
  never to stereotype a learner group
- links must point only to content in the same target-language namespace unless
  a future cross-language feature is explicitly designed

## Learner-Friendly Level Metadata

Every target language needs level metadata with:

- stable code
- sort order
- learner-friendly label
- learner-friendly explanation
- optional mapping to a standard such as CEFR
- localized display names where UI needs them

German baseline labels:

| Code | Label | Learner meaning |
| --- | --- | --- |
| A1 | Einstieg | First contact with greetings, identity, numbers, and simple everyday actions. |
| A2 | Grundlagen | Everyday independence with appointments, shopping, messages, and common public-service situations. |
| B1 | Selbststaendig | Independent daily life, connected speaking/writing, bureaucracy, work, school, health, and basic argumentation. |
| B2 | Kompetent | Confident communication, detailed explanations, opinions, workplace/study tasks, nuanced reading, and longer writing. |
| C1 | Souveraen | Advanced academic/professional discourse, implicit meaning, structured argumentation, and flexible register. |
| C2 | Meisterschaft | Near-native precision, style, rhetorical control, complex synthesis, subtle tone, and expert/public discourse. |

Future languages may still use CEFR, but their friendly labels and explanations
must be reviewed for that language. Generic UI must not assume that A1-C2 is the
only possible level structure.

## Capability Profile Gate

Before a target language moves from planned to pilot, create or review a
capability profile covering:

- script, direction, font and input needs
- level system and learner-friendly labels
- search normalization, tokenization, casing, diacritics, accents, apostrophes,
  spelling variants, transliteration, and word-boundary rules
- morphology and grammar assumptions for lessons, exercises, validation, and
  generated content
- pronunciation, listening, TTS, stress, rhythm, and accent requirements
- writing conventions, register, message length, dates, addresses, public forms,
  and country-specific formats
- exam ecosystem and provider/country boundaries
- Country Guidance country contexts
- initial content modules and explicitly excluded modules
- helper-language launch set and future helper-language expansion
- current-source refresh needs for legal, civic, medical, financial,
  immigration, residence, exam, or safety-adjacent content
- German assumptions that do not apply to this target language

## Implementation Phases

### Phase A - Close Multi-Target Readiness

Goal: prove the platform can hold multiple target languages without leakage.

Work:

- verify route helpers, WebApi endpoints, repositories, search, progress,
  recommendations, recent activity, imports, links, admin reports, and backups
  are target-language scoped
- keep global data explicit, such as account/security data and organizer
  identity, while target-specific views remain scoped
- fail unsupported or inactive target-language routes instead of falling back to
  German
- keep `/learn/{targetLearningLanguageCode}/...` as the canonical learner route
- keep Country Guidance routes country-scoped, for example
  `/learn/de/country-guidance/de`
- remove ambiguous old top-level routes from final acceptance

Exit:

- German remains stable and public active
- English remains pilot/non-public
- unsupported targets do not show German content
- restore-ready backup exists

### Phase B - Keep English Pilot Non-Public And Decide Branch

Goal: use the imported English A1 pilot as proof that non-German content can be
stored, searched, diagnosed, and previewed without public activation.

Work:

- review English pilot source quality
- review helper translations
- verify admin-only previews and diagnostics
- verify search/progress/recommendation isolation
- keep public `/learn/en/...` unavailable until an explicit activation decision
- decide whether the next business branch is English depth or German DACH
  Country Guidance

Exit:

- English is either explicitly kept as pilot or explicitly activated
- activation cannot happen implicitly through import or route availability

### Phase C - German DACH Country Guidance Option

Goal: improve the current German learner experience with Austria and
Switzerland if that is chosen as the next branch.

Work:

- create `de|AT` and `de|CH` planning profiles
- refresh current official/legal-adjacent sources
- generate a very small reviewed pilot for one country stream first
- import, verify, smoke, admin-check, and back up

Non-goal:

- do not copy `de|DE` Germany notes as Austria or Switzerland source content

### Phase D - English Depth Option

Goal: expand English only if the pilot is accepted as the next business branch.

Work:

- keep English non-public unless activation is explicitly approved
- expand A1 in small reviewed batches
- keep English-native grammar, expressions, exercises, templates, and courses
- defer `en|US` Country Guidance until the English learning-content flow is
  accepted

Non-goal:

- do not translate German packages into English source content

### Phase E - Spanish/French Pilot Options

Goal: start a new target language only through its own goal.

Spanish decisions before content:

- Spain-first or Latin-America-first
- `vosotros`, `ustedes`, `voseo`, regional vocabulary, accents, and exams
- first country guidance stream

French decisions before content:

- France-first or another first country context
- Switzerland, Belgium, Canada handling
- liaison, elision, accents, apostrophes, register, and exams

Exit:

- a small native pilot is imported, smoke-tested, diagnosed, backed up, and
  either kept pilot or activated by explicit decision

### Phase F - Helper-Language Expansion

Goal: add more explanation languages without changing source-content identity.

Work:

- select new helper languages explicitly
- add direction/script/resource metadata
- update import validation and admin missing-coverage diagnostics
- backfill helper translations before exposing the helper language publicly
- run RTL/mixed-script rendering smoke
- include helper coverage in backup manifests

## Goal-Ready Backlog For The Next Goal

The current goal closed the platform readiness and branch-decision checkpoint.
Future goals should not try to finish every future language at once. They should
execute exactly one selected branch.

Completed in this checkpoint:

1. Sync docs/backlog with this roadmap.
2. Confirm German remains active and English remains pilot/non-public.
3. Verify current route/API/search/progress/admin/backup evidence is still
   current.
4. Mark the English pilot as explicitly kept pilot unless activation is chosen.
5. Choose one branch:
   - English depth first
   - German DACH Country Guidance
   - Spanish pilot
   - French pilot
6. Execute only the selected branch in small reviewed batches.
7. Create a restore-ready backup at the branch checkpoint.

Checkpoint result:

- English depth first was selected for the current checkpoint.
- English was expanded once in a controlled A1 staging slice.
- English remains pilot/non-public.
- Restore-ready backup:
  `X:\Projects\DarwinLingua.Backup\20260624-211627-english-a1-expansion-02-branch-checkpoint-pre-activation`

Future branch options:

- activate English publicly only after an explicit activation goal
- continue deeper English content generation in reviewed batches
- add German Austria/Switzerland Country Guidance
- start Spanish or French through dedicated target-language goals

## Completion Criteria

The multi-target expansion readiness work is complete when:

- German remains stable and public active.
- At least one non-German target-language pilot is imported, smoke-tested,
  reviewed, and either activated or explicitly kept in pilot status.
- Country Guidance can hold multiple countries per target language and the same
  country under multiple target languages without schema or route redesign.
- Level metadata is target-language-specific and learner-friendly across Web,
  API, packages, progress, events, profiles, and admin surfaces.
- Helper-language expansion is documented and proven independent from
  target-language activation.
- Search, progress, recommendations, recent activity, admin reports, imports,
  linked-content diagnostics, and backups are target-language/country-context
  scoped where required.
- No permanent ambiguous legacy routes or hidden German fallbacks remain in the
  final acceptance path.
